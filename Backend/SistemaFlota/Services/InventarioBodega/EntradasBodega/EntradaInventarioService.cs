using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.InventarioBodega.Devoluciones;
using SistemaFlota.DTOs.InventarioBodega.EntradasBodega;
using SistemaFlota.DTOs.InventarioBodega.Inventario;
using SistemaFlota.Models.InventariosBodega.EntradasBodega;
using SistemaFlota.Services.Auditorias.Base;
using SistemaFlota.Services.Auth;
using SistemaFlota.Services.Helper.InventariosBodega;
using System.Globalization;

namespace SistemaFlota.Services.InventarioBodega.EntradasBodega
{
    public class EntradaInventarioService : BaseService,
       IEntradaInventarioService
    {
        private readonly AppDbContext _context;

        public EntradaInventarioService(
            AppDbContext context,
            ICurrentUserService currentUser,
            SistemaFlota.AuditoriaService auditoria)
            : base(auditoria, currentUser)
        {
            _context = context;
        }

        // OBTENER ENTRADAS DE INVENTARIO
        public async Task<List<MostrarEntradaDto>> ObtenerEntradasAsync()
        {
            return await _context.InventariosBodegaItems
                .AsNoTracking()
                .OrderByDescending(i => i.FechaRegistroItem)
                .Select(i => new MostrarEntradaDto
                {
                    Id = i.Id,
                    Referencia = i.InventarioBodega.Referencia,
                    ReferenciaPeso = i.ReferenciaPeso,
                    FechaRegistroItem = i.FechaRegistroItem,
                    PesoActual = i.PesoActual,
                    Estado = i.Estado
                })
                .ToListAsync();
        }

        // REGISTRAR ENTRADA DE PESO
        public async Task<IngresarPesoResponseDto> IngresarPesoAsync(
            IngresarPesoDto request,
            string usuario)
        {
            if (request.Peso <= 0)
                throw new ArgumentException("El peso debe ser mayor que cero.");

            if (string.IsNullOrWhiteSpace(request.Referencia))
                throw new ArgumentException("La referencia es obligatoria.");

            var referenciaNormalizada =
                ReferenciaHelper.Normalize(request.Referencia);

            // GENERAR REFERENCIA INDIVIDUAL
            var caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var random2 = new string(
                Enumerable.Range(0, 2)
                    .Select(_ => caracteres[Random.Shared.Next(caracteres.Length)])
                    .ToArray());

            // UTILIZAR LA ESTRATEGIA DE EJECUCIÓN DE MYSQL
            var estrategia = _context.Database.CreateExecutionStrategy();

            return await estrategia.ExecuteAsync(async () =>
            {
                // BUSCAR PRODUCTO
                var producto = await _context.InventariosBodega
                    .FirstOrDefaultAsync(p =>
                        p.ReferenciaNormalizada == referenciaNormalizada);

                if (producto == null)
                    throw new KeyNotFoundException("Referencia no encontrada.");

                // GENERAR REFERENCIA CON PESO
                var ref4 = producto.ReferenciaNormalizada.Length >= 4
                    ? producto.ReferenciaNormalizada.Substring(0, 4)
                    : producto.ReferenciaNormalizada;

                var pesoFormateado = request.Peso
                    .ToString("0.##", CultureInfo.InvariantCulture)
                    .Replace(".", "")
                    .Replace(",", "");

                var letraImpresa =
                    string.IsNullOrWhiteSpace(producto.ReferenciaImpresa)
                        ? "X"
                        : producto.ReferenciaImpresa[0]
                            .ToString()
                            .ToUpperInvariant();

                var referenciaConPeso =
                    $"{ref4}{pesoFormateado}{random2}{letraImpresa}";

                // INICIAR TRANSACCIÓN DENTRO DE LA ESTRATEGIA
                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                try
                {
                    Console.WriteLine("INICIO DEL REGISTRO");

                    // CREAR ITEM
                    var nuevoItem = new InventarioBodegaItem
                    {
                        IdInventarioBodega = producto.Id,
                        PesoActual = request.Peso,
                        FechaRegistroItem = DateTime.UtcNow,
                        Estado = "EN_ALMACEN",
                        ReferenciaPeso = referenciaConPeso
                    };

                    _context.InventariosBodegaItems.Add(nuevoItem);

                    // ACTUALIZAR INVENTARIO
                    producto.Peso += request.Peso;
                    producto.Cantidad += 1;

                    Console.WriteLine("GUARDANDO ITEM E INVENTARIO");

                    await _context.SaveChangesAsync();

                    Console.WriteLine("PRIMER GUARDADO OK");

                    // REGISTRAR MOVIMIENTO
                    var nuevoMovimiento = new MovimientoInventarioBodega
                    {
                        Referencia = producto.ReferenciaNormalizada,
                        ReferenciaPeso = nuevoItem.ReferenciaPeso,
                        Peso = request.Peso,
                        Fecha = DateTime.UtcNow,
                        Tipo = "Entrada",
                        Usuario = usuario,
                        IdInventarioBodega = producto.Id,
                        IdInventarioBodegaItem = nuevoItem.Id
                    };

                    _context.MovimientosInventarioBodega.Add(nuevoMovimiento);

                    Console.WriteLine("GUARDANDO MOVIMIENTO");

                    await _context.SaveChangesAsync();

                    Console.WriteLine("SEGUNDO GUARDADO OK");

                    await transaction.CommitAsync();

                    Console.WriteLine("TRANSACCION CONFIRMADA");

                    return new IngresarPesoResponseDto
                    {
                        Referencia = producto.Referencia,
                        ReferenciaPeso = nuevoItem.ReferenciaPeso,
                        Peso = producto.Peso,
                        Cantidad = producto.Cantidad,
                        Descripcion = producto.Descripcion,
                        Ancho = producto.Ancho,
                        Alto = producto.Alto,
                        Calibre = producto.Calibre,
                        Color = producto.Color,
                        Material = producto.TipoMaterial,
                        FuelleLateral = producto.FuelleLateral,
                        FuelleFondo = producto.FuelleFondo,
                        FuelleSuperior = producto.FuelleSuperior,
                        Troquel = producto.Troquel,
                        ReferenciaImpresa = producto.ReferenciaImpresa,
                        PaqueteNumero = producto.PaqueteNumero,
                        PaqueteCantidad = producto.PaqueteCantidad
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        // REGISTRAR DEVOLUCIÓN
        public async Task<string> RegistrarDevolucionAsync(
            DevolucionEntradaDto request,
            string usuario)
        {
            if (request.InventarioBodegaItemId <= 0)
                throw new ArgumentException("El item seleccionado no es válido.");

            var estrategia = _context.Database.CreateExecutionStrategy();

            return await estrategia.ExecuteAsync(async () =>
            {
                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                try
                {
                    var item = await _context.InventariosBodegaItems
                        .Include(i => i.InventarioBodega)
                        .FirstOrDefaultAsync(i =>
                            i.Id == request.InventarioBodegaItemId);

                    if (item == null)
                        throw new KeyNotFoundException("No se encontró el item seleccionado.");

                    if (item.Estado != "SALIDA")
                        throw new InvalidOperationException(
                            "Solo se pueden devolver items que tengan estado SALIDA.");

                    var inventario = item.InventarioBodega;

                    if (inventario == null)
                        throw new InvalidOperationException(
                            "El item no tiene un inventario asociado.");

                    // DEVOLVER ITEM AL ALMACÉN
                    item.Estado = "EN_ALMACEN";

                    // ACTUALIZAR INVENTARIO PRINCIPAL
                    inventario.Peso += item.PesoActual;
                    inventario.Cantidad += 1;

                    // REGISTRAR MOVIMIENTO
                    var movimiento = new MovimientoInventarioBodega
                    {
                        Referencia = inventario.ReferenciaNormalizada
                            ?? inventario.Referencia,

                        ReferenciaPeso = item.ReferenciaPeso,
                        Peso = item.PesoActual,
                        Fecha = DateTime.UtcNow,
                        Tipo = "Devolucion",
                        Usuario = usuario,
                        IdInventarioBodega = inventario.Id,
                        IdInventarioBodegaItem = item.Id,
                        Observacion = request.Observacion
                    };

                    _context.MovimientosInventarioBodega.Add(movimiento);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return "Devolución registrada correctamente.";
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        // AJUSTE GENERAL DE INVENTARIO
        public async Task<string> AjusteGeneralAsync(
            AjusteGeneralEntradaDto request,
            string usuario)
        {
            if (request.ItemIds == null || !request.ItemIds.Any())
                throw new ArgumentException(
                    "Debe seleccionar al menos un item.");

            if (string.IsNullOrWhiteSpace(request.Observacion))
                throw new ArgumentException(
                    "La observación es obligatoria para realizar el ajuste.");

            // VALIDAR IDS DUPLICADOS
            var ids = request.ItemIds.Distinct().ToList();

            if (ids.Count != request.ItemIds.Count)
                throw new ArgumentException(
                    "La selección contiene items duplicados.");

            var estrategia = _context.Database.CreateExecutionStrategy();

            return await estrategia.ExecuteAsync(async () =>
            {
                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                try
                {
                    var items = await _context.InventariosBodegaItems
                        .Include(i => i.InventarioBodega)
                        .Where(i =>
                            ids.Contains(i.Id) &&
                            i.Estado == "EN_ALMACEN")
                        .ToListAsync();

                    // EXIGIR QUE TODOS LOS ITEMS SEAN VÁLIDOS
                    if (items.Count != ids.Count)
                    {
                        throw new InvalidOperationException(
                            "Uno o más items no existen o no se encuentran disponibles en almacén.");
                    }

                    // AGRUPAR POR INVENTARIO PARA ACTUALIZAR SUS TOTALES
                    var grupos = items.GroupBy(i => i.IdInventarioBodega);

                    foreach (var grupo in grupos)
                    {
                        var inventario = grupo.First().InventarioBodega;

                        if (inventario == null)
                            throw new InvalidOperationException(
                                "Uno de los items no tiene un inventario asociado.");

                        var pesoAjustar = grupo.Sum(i => i.PesoActual);
                        var cantidadAjustar = grupo.Count();

                        // EVITAR DEJAR EXISTENCIAS NEGATIVAS
                        if (inventario.Peso < pesoAjustar ||
                            inventario.Cantidad < cantidadAjustar)
                        {
                            throw new InvalidOperationException(
                                $"Las existencias del inventario {inventario.Referencia} no coinciden con los items seleccionados.");
                        }

                        inventario.Peso -= pesoAjustar;
                        inventario.Cantidad -= cantidadAjustar;
                    }

                    // CAMBIAR ESTADO Y REGISTRAR MOVIMIENTOS
                    foreach (var item in items)
                    {
                        item.Estado = "AJUSTADO";

                        var inventario = item.InventarioBodega;

                        var movimiento = new MovimientoInventarioBodega
                        {
                            Referencia = inventario.ReferenciaNormalizada
                                ?? inventario.Referencia,

                            ReferenciaPeso = item.ReferenciaPeso,
                            Peso = item.PesoActual,
                            Fecha = DateTime.UtcNow,
                            Tipo = "Ajuste",
                            Usuario = usuario,
                            IdInventarioBodega = inventario.Id,
                            IdInventarioBodegaItem = item.Id,
                            Observacion = request.Observacion.Trim()
                        };

                        _context.MovimientosInventarioBodega.Add(movimiento);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return $"Ajuste general aplicado correctamente a {items.Count} items.";
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

    }
}
