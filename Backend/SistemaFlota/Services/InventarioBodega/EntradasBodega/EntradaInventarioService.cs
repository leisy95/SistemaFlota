using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.InventarioBodega.Inventario;
using SistemaFlota.Models.InventarioBodega;
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

    }
}
