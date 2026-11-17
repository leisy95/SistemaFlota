
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.InventarioBodega;
using SistemaFlota.DTOs.InventarioBodega.Inventario;
using SistemaFlota.Models.InventariosBodega.EntradasBodega;
using SistemaFlota.Services.Auditorias.Base;
using SistemaFlota.Services.Auth;
using SistemaFlota.Services.Helper.InventariosBodega;

using InventarioBodegaModel =
    SistemaFlota.Models.InventariosBodega.Inventario.InventarioBodega;

namespace SistemaFlota.Services.InventarioBodega.InventarioBodegaInfo
{
    public class InventarioBodegaService : BaseService,
        IInventarioBodegaService
    {
        private readonly AppDbContext _context;

        public InventarioBodegaService(
            AppDbContext context,
            ICurrentUserService currentUser,
            SistemaFlota.AuditoriaService auditoria)
            : base(auditoria, currentUser)
        {
            _context = context;
        }

        // OBTENER TODOS
        public async Task<List<InventarioBodegaModel>> ObtenerAsync()
        {
            return await _context.InventariosBodega
                .AsNoTracking()
                .OrderBy(i => i.Referencia)
                .ToListAsync();
        }

        // OBTENER SUGERENCIAS DE REFERENCIAS
        public async Task<List<string>> ObtenerSugerenciasAsync(string? buscar)
        {
            var consulta = _context.InventariosBodega
                .AsNoTracking()
                .Where(i => i.Referencia != null && i.Referencia != "");

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();

                consulta = consulta.Where(i =>
                    i.Referencia.StartsWith(texto));
            }

            return await consulta
                .OrderBy(i => i.Referencia)
                .Select(i => i.Referencia)
                .Take(5)
                .ToListAsync();
        }


        // OBTENER INVENTARIO PAGINADO CON FILTROS Y RESUMEN
        public async Task<InventarioBodegaPaginadoResponseDto>
            ObtenerPaginadoAsync(
                string? buscar,
                string? material,
                string? color,
                int pagina,
                int pageSize)
        {
            pagina = Math.Max(1, pagina);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var consultaBase = _context.InventariosBodega
                .AsNoTracking();

            // RESUMEN GENERAL
            var totalProductos = await consultaBase.CountAsync();

            var pesoTotal = await consultaBase
                .SumAsync(i => (decimal?)i.Peso) ?? 0;

            var cantidadTotal = await consultaBase
                .SumAsync(i => (int?)i.Cantidad) ?? 0;

            var totalColores = await consultaBase
                .Where(i => i.Color != null && i.Color != "")
                .Select(i => i.Color)
                .Distinct()
                .CountAsync();

            var totalMateriales = await consultaBase
                .Where(i => i.TipoMaterial != null &&
                            i.TipoMaterial != "")
                .Select(i => i.TipoMaterial)
                .Distinct()
                .CountAsync();

            var totalReferencias = await consultaBase
                .Where(i => i.Referencia != null &&
                            i.Referencia != "")
                .Select(i => i.Referencia)
                .Distinct()
                .CountAsync();

            var resumen = new ResumenInventarioDto
            {
                TotalProductos = totalProductos,
                PesoTotal = pesoTotal,
                CantidadTotal = cantidadTotal,
                TotalColores = totalColores,
                TotalMateriales = totalMateriales,
                TotalReferencias = totalReferencias
            };

            // FILTROS DISPONIBLES
            var materiales = await consultaBase
                .Where(i => i.TipoMaterial != null &&
                            i.TipoMaterial != "")
                .Select(i => i.TipoMaterial!)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            var colores = await consultaBase
                .Where(i => i.Color != null && i.Color != "")
                .Select(i => i.Color!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var filtros = new FiltrosInventarioDto
            {
                Materiales = materiales,
                Colores = colores
            };

            // BÚSQUEDA Y FILTROS
            var consultaFiltrada = consultaBase.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();

                consultaFiltrada = consultaFiltrada.Where(i =>
                    (i.Referencia ?? "").Contains(texto) ||
                    (i.Descripcion ?? "").Contains(texto) ||
                    (i.TipoBolsa ?? "").Contains(texto) ||
                    (i.TipoMaterial ?? "").Contains(texto) ||
                    (i.Color ?? "").Contains(texto) ||
                    (i.ReferenciaImpresa ?? "").Contains(texto)
                );
            }

            if (!string.IsNullOrWhiteSpace(material))
            {
                consultaFiltrada = consultaFiltrada.Where(i =>
                    i.TipoMaterial == material);
            }

            if (!string.IsNullOrWhiteSpace(color))
            {
                consultaFiltrada = consultaFiltrada.Where(i =>
                    i.Color == color);
            }

            // TOTAL DE REGISTROS
            var total = await consultaFiltrada.CountAsync();

            var totalPaginas = (int)Math.Ceiling(
                total / (double)pageSize);

            // EVITAR PÁGINAS FUERA DEL RANGO
            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            // REGISTROS DE LA PÁGINA
            var items = await consultaFiltrada
                .OrderBy(i => i.Referencia)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new InventarioBodegaPaginadoResponseDto
            {
                Resumen = resumen,
                Filtros = filtros,
                Items = items,
                Total = total,
                Pagina = pagina,
                PageSize = pageSize,
                TotalPaginas = totalPaginas
            };
        }

        // BUSCAR POR REFERENCIA
        public async Task<InventarioBodegaModel?>
            ObtenerPorReferenciaAsync(string referencia)
        {
            var referenciaNormalizada =
                ReferenciaHelper.Normalize(referencia);

            return await _context.InventariosBodega
                .AsNoTracking()
                .FirstOrDefaultAsync(i =>
                    i.ReferenciaNormalizada == referenciaNormalizada);
        }

        // REGISTRAR PRODUCTO
        public async Task<RegistrarItemResponseDto>
            RegistrarItemAsync(RegistrarItemDto request)
        {
            var referenciaNormalizada =
                ReferenciaHelper.Normalize(request.Referencia);

            var existe = await _context.InventariosBodega
                .AnyAsync(i =>
                    i.ReferenciaNormalizada == referenciaNormalizada);

            if (existe)
            {
                throw new InvalidOperationException(
                    "La referencia ya se encuentra registrada.");
            }

            var inventario = new InventarioBodegaModel
            {
                Referencia = request.Referencia,
                ReferenciaNormalizada = referenciaNormalizada,
                Descripcion = request.Descripcion,
                TipoBolsa = request.TipoBolsa,
                TipoMaterial = request.TipoMaterial,
                Densidad = request.Densidad,
                Color = request.Color,
                FuelleLateral = request.FuelleLateral ?? 0,
                FuelleFondo = request.FuelleFondo ?? 0,
                FuelleSuperior = request.FuelleSuperior ?? 0,
                SegundoColor = request.SegundoColor,
                ImpresoNo = request.ImpresoNo,
                Ancho = request.Ancho ?? 0,
                Alto = request.Alto ?? 0,
                Calibre = request.Calibre ?? 0,
                Troquel = request.Troquel,
                ReferenciaImpresa = request.ReferenciaImpresa,
                PaqueteNumero = request.PaqueteNumero ?? 0,
                PaqueteCantidad = request.PaqueteCantidad ?? 0,
                Peso = 0,
                Cantidad = 0
            };

            _context.InventariosBodega.Add(inventario);

            await _context.SaveChangesAsync();

            await AuditarAsync(
                "Crear",
                "InventarioBodega",
                $"Se registró un nuevo producto en el inventario. " +
                $"Referencia: {inventario.Referencia}. " +
                $"Descripción: {inventario.Descripcion}.",
                inventario.Id);

            return new RegistrarItemResponseDto
            {
                IdItemGenerado = inventario.Id,
                Referencia = inventario.Referencia,
                Message = "Producto registrado correctamente en Inventario"
            };
        }

        // OBTENER BULTOS DISPONIBLES EN ALMACÉN
        public async Task<List<InventarioBodegaItem>>
            ObtenerItemsPorInventarioAsync(int idInventarioBodega)
        {
            return await _context.InventariosBodegaItems
                .AsNoTracking()
                .Where(i =>
                    i.IdInventarioBodega == idInventarioBodega &&
                    i.Estado == "EN_ALMACEN")
                .OrderByDescending(i => i.FechaRegistroItem)
                .ToListAsync();
        }

        // EDITAR INVENTARIO
        public async Task<bool> EditarInventarioAsync(
            int id,
            ActualizarInventarioBodegaDto request)
        {
            var inventario = await _context.InventariosBodega
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventario == null)
                return false;

            var referenciaNormalizada =
                ReferenciaHelper.Normalize(request.Referencia);

            var existe = await _context.InventariosBodega
                .AnyAsync(i =>
                    i.Id != id &&
                    i.ReferenciaNormalizada == referenciaNormalizada);

            if (existe)
            {
                throw new InvalidOperationException(
                    "La referencia ya está registrada.");
            }

            inventario.Referencia = request.Referencia;
            inventario.ReferenciaNormalizada = referenciaNormalizada;
            inventario.Descripcion = request.Descripcion;
            inventario.TipoBolsa = request.TipoBolsa;
            inventario.TipoMaterial = request.TipoMaterial;
            inventario.Densidad = request.Densidad;
            inventario.Color = request.Color;
            inventario.SegundoColor = request.SegundoColor;
            inventario.ImpresoNo = request.ImpresoNo;
            inventario.Ancho = request.Ancho;
            inventario.Alto = request.Alto;
            inventario.Calibre = request.Calibre;
            inventario.FuelleLateral = request.FuelleLateral;
            inventario.FuelleFondo = request.FuelleFondo;
            inventario.FuelleSuperior = request.FuelleSuperior;
            inventario.Troquel = request.Troquel;
            inventario.ReferenciaImpresa = request.ReferenciaImpresa;
            inventario.PaqueteNumero = request.PaqueteNumero;
            inventario.PaqueteCantidad = request.PaqueteCantidad;

            await _context.SaveChangesAsync();

            await AuditarAsync(
                "Actualizar",
                "InventarioBodega",
                $"Se actualizó el producto del inventario. " +
                $"ID: {inventario.Id}. " +
                $"Referencia: {inventario.Referencia}. " +
                $"Descripción: {inventario.Descripcion}.",
                inventario.Id);

            return true;
        }

        // RECALCULAR INVENTARIO
        public async Task<RecalcularInventarioBodegaResponseDto>
            RecalcularInventarioAsync()
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var inventarios = await _context.InventariosBodega
                    .ToListAsync();

                if (!inventarios.Any())
                {
                    throw new KeyNotFoundException(
                        "No hay inventario para recalcular.");
                }

                // RESUMEN REAL DESDE ITEMS EN ALMACÉN
                var resumen = await _context.InventariosBodegaItems
                    .Where(i => i.Estado == "EN_ALMACEN")
                    .GroupBy(i => i.IdInventarioBodega)
                    .Select(g => new
                    {
                        IdInventarioBodega = g.Key,
                        Cantidad = g.Count(),
                        Peso = g.Sum(x => x.PesoActual)
                    })
                    .ToListAsync();

                var mapa = resumen.ToDictionary(
                    x => x.IdInventarioBodega,
                    x => x);

                int actualizados = 0;

                foreach (var inventario in inventarios)
                {
                    mapa.TryGetValue(
                        inventario.Id,
                        out var resultado);

                    var cantidad = resultado?.Cantidad ?? 0;
                    var peso = resultado?.Peso ?? 0;

                    if (inventario.Cantidad != cantidad ||
                        inventario.Peso != peso)
                    {
                        inventario.Cantidad = cantidad;
                        inventario.Peso = peso;

                        actualizados++;
                    }
                }

                await _context.SaveChangesAsync();

                await AuditarAsync(
                    "Recalcular",
                    "InventarioBodega",
                    $"Se ejecutó el recálculo general del inventario. " +
                    $"Total de inventarios: {inventarios.Count}. " +
                    $"Inventarios actualizados: {actualizados}.",
                    null);

                await transaction.CommitAsync();

                return new RecalcularInventarioBodegaResponseDto
                {
                    Message = "Inventario recalculado correctamente.",
                    TotalInventarios = inventarios.Count,
                    InventariosActualizados = actualizados
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ELIMINAR INVENTARIO
        public async Task<bool> EliminarInventarioAsync(int id)
        {
            var inventario = await _context.InventariosBodega
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventario == null)
                return false;

            var referencia = inventario.Referencia;
            var descripcion = inventario.Descripcion;

            _context.InventariosBodega.Remove(inventario);

            await _context.SaveChangesAsync();

            await AuditarAsync(
                "Eliminar",
                "InventarioBodega",
                $"Se eliminó un producto del inventario. " +
                $"Referencia: {referencia}. " +
                $"Descripción: {descripcion}.",
                id);

            return true;
        }
    }
}
