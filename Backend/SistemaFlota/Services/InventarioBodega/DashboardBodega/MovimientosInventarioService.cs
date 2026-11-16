using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.InventarioBodega.DashboardBodega;
using SistemaFlota.Models.InventariosBodega.EntradasBodega;
using SistemaFlota.Services.Auditorias.Base;
using SistemaFlota.Services.Auth;

namespace SistemaFlota.Services.InventarioBodega.DashboardBodega
{
    public class MovimientosInventarioService : IMovimientosInventarioService
    {
        private readonly AppDbContext _context;

        public MovimientosInventarioService(AppDbContext context)
        {
            _context = context;
        }

        // LISTAR MOVIMIENTOS
        public async Task<IEnumerable<MovimientoInventarioResponseDto>>
            ObtenerMovimientosAsync()
        {
            return await _context.MovimientosInventarioBodega
                .Select(m => new MovimientoInventarioResponseDto
                {
                    Id = m.Id,
                    Referencia = m.Referencia,
                    ReferenciaPeso = m.ReferenciaPeso,
                    Peso = m.Peso,
                    Fecha = m.Fecha,
                    Tipo = m.Tipo,
                    Usuario = m.Usuario,
                    ReferenciaImpresa = m.InventarioBodega.ReferenciaImpresa,
                    InventarioNombre = m.InventarioBodega.Referencia,
                    InventarioItemCodigo = m.InventarioBodegaItem.ReferenciaPeso
                })
                .ToListAsync();
        }

        // OBTENER MOVIMIENTO POR ID
        public async Task<MovimientoInventarioResponseDto?>
            ObtenerMovimientoPorIdAsync(int id)
        {
            return await _context.MovimientosInventarioBodega
                .Where(m => m.Id == id)
                .Select(m => new MovimientoInventarioResponseDto
                {
                    Id = m.Id,
                    Referencia = m.Referencia,
                    ReferenciaPeso = m.ReferenciaPeso,
                    Peso = m.Peso,
                    Fecha = m.Fecha,
                    Tipo = m.Tipo,
                    Usuario = m.Usuario,
                    ReferenciaImpresa = m.InventarioBodega.ReferenciaImpresa,
                    InventarioNombre = m.InventarioBodega.Referencia,
                    InventarioItemCodigo = m.InventarioBodegaItem.ReferenciaPeso
                })
                .FirstOrDefaultAsync();
        }

        // AUDITORÍA
        public async Task<MovimientoInventarioAuditoriaCompletaDto>
            ObtenerAuditoriaAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var query = _context.MovimientosInventarioBodega.AsQueryable();

            if (fechaInicio.HasValue)
            {
                query = query.Where(m =>
                    m.Fecha >= fechaInicio.Value.Date.ToUniversalTime());
            }

            if (fechaFin.HasValue)
            {
                query = query.Where(m =>
                    m.Fecha <= fechaFin.Value.Date
                        .AddDays(1)
                        .AddTicks(-1)
                        .ToUniversalTime());
            }

            var movimientosList = await query
                .Select(m => new MovimientoInventarioAuditoriaDto
                {
                    Referencia = m.Referencia,
                    ReferenciaPeso = m.ReferenciaPeso,
                    Tipo = m.Tipo,
                    Peso = m.Peso,
                    Fecha = m.Fecha,
                    Usuario = m.Usuario,
                    Cliente = m.Cliente != null
                        ? m.Cliente.Nombre
                        : null,
                    ReferenciaImpresa = m.InventarioBodega.ReferenciaImpresa,
                    Observacion = m.Observacion
                })
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();

            var inventarioInicial = movimientosList
                .Where(m => m.Tipo == "Inicial").ToList();

            var entradas = movimientosList
                .Where(m => m.Tipo == "Entrada").ToList();

            var devoluciones = movimientosList
                .Where(m => m.Tipo == "Devolucion").ToList();

            var salidas = movimientosList
                .Where(m => m.Tipo == "Salida").ToList();

            var eliminados = movimientosList
                .Where(m => m.Tipo == "Eliminado").ToList();

            var ajustes = movimientosList
                .Where(m => m.Tipo == "Ajuste").ToList();

            // EXISTENCIAS
            var existenciasCantidad =
                inventarioInicial.Count
                + entradas.Count
                + devoluciones.Count
                - ajustes.Count
                - salidas.Count
                - eliminados.Count;

            var existenciasPeso =
                inventarioInicial.Sum(x => x.Peso)
                + entradas.Sum(x => x.Peso)
                + devoluciones.Sum(x => x.Peso)
                - ajustes.Sum(x => x.Peso)
                - salidas.Sum(x => x.Peso)
                - eliminados.Sum(x => x.Peso);

            // RESUMEN
            var resumen = new MovimientoInventarioAuditoriaResumenDto
            {
                InventarioInicial = new MovimientoInventarioTotalesDto
                {
                    Cantidad = inventarioInicial.Count,
                    Peso = inventarioInicial.Sum(x => x.Peso)
                },
                Entradas = new MovimientoInventarioTotalesDto
                {
                    Cantidad = entradas.Count,
                    Peso = entradas.Sum(x => x.Peso)
                },
                Salidas = new MovimientoInventarioTotalesDto
                {
                    Cantidad = salidas.Count,
                    Peso = salidas.Sum(x => x.Peso)
                },
                Devoluciones = new MovimientoInventarioTotalesDto
                {
                    Cantidad = devoluciones.Count,
                    Peso = devoluciones.Sum(x => x.Peso)
                },
                Eliminados = new MovimientoInventarioTotalesDto
                {
                    Cantidad = eliminados.Count,
                    Peso = eliminados.Sum(x => x.Peso)
                },
                Ajustes = new MovimientoInventarioTotalesDto
                {
                    Cantidad = ajustes.Count,
                    Peso = ajustes.Sum(x => x.Peso)
                },
                Existencias = new MovimientoInventarioTotalesDto
                {
                    Cantidad = existenciasCantidad,
                    Peso = existenciasPeso
                }
            };

            // ESTADÍSTICAS POR TIPO
            var totalesPorTipo = movimientosList
                .GroupBy(m => m.Tipo)
                .Select(g => new MovimientoInventarioEstadisticaDto
                {
                    Tipo = g.Key,
                    Total = g.Count()
                })
                .ToList();

            // REFERENCIAS MÁS MOVIDAS
            var referenciasMasMovidas = movimientosList
                .GroupBy(m => m.Referencia)
                .Select(g => new ReferenciaMasMovidaDto
                {
                    Referencia = g.Key,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToList();

            // USUARIOS MÁS ACTIVOS
            var usuariosMasActivos = movimientosList
                .GroupBy(m => m.Usuario)
                .Select(g => new UsuarioMasActivoDto
                {
                    Usuario = g.Key,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToList();

            return new MovimientoInventarioAuditoriaCompletaDto
            {
                Resumen = resumen,
                Movimientos = movimientosList,
                TotalesPorTipo = totalesPorTipo,
                ReferenciasMasMovidas = referenciasMasMovidas,
                UsuariosMasActivos = usuariosMasActivos
            };
        }

        // REGISTRAR MOVIMIENTO
        public async Task<MovimientoInventarioResponseDto>
            RegistrarMovimientoAsync(MovimientoInventarioRequestDto request)
        {
            var movimiento = new MovimientoInventarioBodega
            {
                Referencia = request.Referencia,
                ReferenciaPeso = request.ReferenciaPeso,
                Peso = request.Peso,
                Fecha = request.Fecha,
                Tipo = request.Tipo,
                Usuario = request.Usuario,
                IdInventarioBodega = request.IdInventarioBodega,
                IdInventarioBodegaItem = request.IdInventarioBodegaItem,
                ClienteId = request.ClienteId,
                Observacion = request.Observacion
            };

            _context.MovimientosInventarioBodega.Add(movimiento);

            await _context.SaveChangesAsync();

            return await _context.MovimientosInventarioBodega
                .Where(m => m.Id == movimiento.Id)
                .Select(m => new MovimientoInventarioResponseDto
                {
                    Id = m.Id,
                    Referencia = m.Referencia,
                    ReferenciaPeso = m.ReferenciaPeso,
                    Peso = m.Peso,
                    Fecha = m.Fecha,
                    Tipo = m.Tipo,
                    Usuario = m.Usuario,
                    ReferenciaImpresa = m.InventarioBodega.ReferenciaImpresa,
                    InventarioNombre = m.InventarioBodega.Referencia,
                    InventarioItemCodigo = m.InventarioBodegaItem.ReferenciaPeso
                })
                .FirstAsync();
        }
    }
}
