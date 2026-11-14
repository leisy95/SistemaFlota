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
        public async Task<List<MostrarEntradaDto>> ObtenerEntradasAsync(
            string? estado = null)
        {
            var query = _context.InventariosBodegaItems
                .AsNoTracking()
                .AsQueryable();

            // POR DEFECTO: MOSTRAR SOLO ITEMS EN ALMACÉN
            if (string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(i => i.Estado == "EN_ALMACEN");
            }
            else
            {
                query = query.Where(i => i.Estado == estado.Trim().ToUpper());
            }

            return await query
                .OrderByDescending(i => i.FechaRegistroItem)
                .ThenByDescending(i => i.Id)
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

        // FILTRAR ENTRADAS CON PAGINACIÓN DESDE EL BACKEND
        public async Task<EntradasBodegaPaginadasDto> FiltrarEntradasAsync(
            FiltroEntradaBodegaDto filtros)
        {
            if (filtros.Pagina < 1)
                filtros.Pagina = 1;

            if (filtros.PageSize < 1)
                filtros.PageSize = 10;

            if (filtros.PageSize > 100)
                filtros.PageSize = 100;

            var query = _context.InventariosBodegaItems
                .AsNoTracking()
                .AsQueryable();

            // FILTRO POR ESTADO
            if (!string.IsNullOrWhiteSpace(filtros.Estado) &&
                !filtros.Estado.Trim().Equals(
                    "TODOS",
                    StringComparison.OrdinalIgnoreCase))
            {
                var estado = filtros.Estado.Trim().ToUpperInvariant();

                query = query.Where(i => i.Estado == estado);
            }
            else if (string.IsNullOrWhiteSpace(filtros.Estado))
            {
                // POR DEFECTO SOLO EN ALMACÉN
                query = query.Where(i => i.Estado == "EN_ALMACEN");
            }

            // BÚSQUEDA POR REFERENCIA
            if (!string.IsNullOrWhiteSpace(filtros.Buscar))
            {
                var buscar = filtros.Buscar.Trim();

                query = query.Where(i =>
                    i.InventarioBodega.Referencia.Contains(buscar) ||
                    i.ReferenciaPeso.Contains(buscar));
            }

            // FECHA INICIAL
            if (filtros.FechaInicio.HasValue)
            {
                var fechaInicio = filtros.FechaInicio.Value.Date;

                query = query.Where(i =>
                    i.FechaRegistroItem >= fechaInicio);
            }

            // FECHA FINAL: INCLUYE TODO EL DÍA
            if (filtros.FechaFin.HasValue)
            {
                var fechaFin = filtros.FechaFin.Value.Date.AddDays(1);

                query = query.Where(i =>
                    i.FechaRegistroItem < fechaFin);
            }

            // TOTAL DE REGISTROS FILTRADOS
            var total = await query.CountAsync();

            // PAGINACIÓN EN BASE DE DATOS
            var items = await query
                .OrderByDescending(i => i.FechaRegistroItem)
                .ThenByDescending(i => i.Id)
                .Skip((filtros.Pagina - 1) * filtros.PageSize)
                .Take(filtros.PageSize)
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

            return new EntradasBodegaPaginadasDto
            {
                Items = items,
                Total = total,
                Pagina = filtros.Pagina,
                PageSize = filtros.PageSize,
                TotalPaginas = (int)Math.Ceiling(
                    total / (double)filtros.PageSize)
            };
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

                    await _context.SaveChangesAsync();

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

                    await _context.SaveChangesAsync();

                    // AUDITORÍA
                    await AuditarAsync(
                        "Crear",
                        "InventarioBodega",
                        $"Se registró una entrada de {request.Peso} kg. " +
                        $"Referencia: {producto.Referencia}. " +
                        $"Referencia individual: {nuevoItem.ReferenciaPeso}.",
                        nuevoItem.Id);

                    await transaction.CommitAsync();

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

                    // AUDITORÍA
                    await AuditarAsync(
                        "Devolucion",
                        "InventarioBodega",
                        $"Se devolvió el item {item.ReferenciaPeso} al almacén. " +
                        $"Referencia: {inventario.Referencia}. " +
                        $"Peso devuelto: {item.PesoActual} kg. " +
                        $"Observación: {request.Observacion}.",
                        item.Id);

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

                    // AUDITORÍA
                    await AuditarAsync(
                        "AjusteGeneral",
                        "InventarioBodega",
                        $"Se aplicó un ajuste general a {items.Count} items. " +
                        $"IDs: {string.Join(", ", items.Select(i => i.Id))}. " +
                        $"Observación: {request.Observacion}.",
                        null);

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
