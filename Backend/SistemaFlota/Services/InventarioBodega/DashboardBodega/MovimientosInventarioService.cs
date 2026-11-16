using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.InventarioBodega.DashboardBodega;
using SistemaFlota.Models.InventariosBodega.EntradasBodega;

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
                    Cliente = m.Cliente != null
                        ? m.Cliente.Nombre
                        : null,
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
                    Cliente = m.Cliente != null
                        ? m.Cliente.Nombre
                        : null,
                    InventarioNombre = m.InventarioBodega.Referencia,
                    InventarioItemCodigo = m.InventarioBodegaItem.ReferenciaPeso
                })
                .FirstOrDefaultAsync();
        }

        // AUDITORÍA

        public async Task<MovimientoInventarioAuditoriaCompletaDto>
            ObtenerAuditoriaAsync(MovimientoInventarioFiltroDto filtros)
        {
            // Normalizar paginación
            var pagina = Math.Max(1, filtros.Pagina);
            var pageSize = Math.Clamp(filtros.PageSize, 1, 100);

            // Consulta base
            var query = _context.MovimientosInventarioBodega
                .AsNoTracking()
                .AsQueryable();

            // FILTRO POR FECHAS
            if (filtros.FechaInicio.HasValue)
            {
                var fechaInicio = filtros.FechaInicio.Value.Date.ToUniversalTime();

                query = query.Where(m => m.Fecha >= fechaInicio);
            }

            if (filtros.FechaFin.HasValue)
            {
                var fechaFin = filtros.FechaFin.Value.Date
                    .AddDays(1)
                    .AddTicks(-1)
                    .ToUniversalTime();

                query = query.Where(m => m.Fecha <= fechaFin);
            }

            // FILTRO POR REFERENCIA
            if (!string.IsNullOrWhiteSpace(filtros.Referencia))
            {
                var referencia = filtros.Referencia.Trim();

                query = query.Where(m =>
                    m.Referencia.Contains(referencia));
            }

            // FILTRO POR USUARIO
            if (!string.IsNullOrWhiteSpace(filtros.Usuario))
            {
                var usuario = filtros.Usuario.Trim();

                query = query.Where(m =>
                    m.Usuario != null &&
                    m.Usuario.Contains(usuario));
            }

            // FILTRO POR CLIENTE
            if (!string.IsNullOrWhiteSpace(filtros.Cliente))
            {
                var cliente = filtros.Cliente.Trim();

                query = query.Where(m =>
                    m.Cliente != null &&
                    m.Cliente.Nombre.Contains(cliente));
            }

            // FILTRO POR TIPO
            if (!string.IsNullOrWhiteSpace(filtros.Tipo))
            {
                var tipo = filtros.Tipo.Trim();

                query = query.Where(m =>
                    m.Tipo.Contains(tipo));
            }

            // FILTRO POR REFERENCIA IMPRESA
            if (!string.IsNullOrWhiteSpace(filtros.ReferenciaImpresa))
            {
                var referenciaImpresa = filtros.ReferenciaImpresa.Trim();

                query = query.Where(m =>
                    m.InventarioBodega != null &&
                    m.InventarioBodega.ReferenciaImpresa != null &&
                    m.InventarioBodega.ReferenciaImpresa.Contains(referenciaImpresa));
            }

            // TOTAL DE REGISTROS FILTRADOS
            var totalRegistros = await query.CountAsync();

            var totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)pageSize
            );

            // RESUMEN AGRUPADO POR TIPO
            // Se calcula sobre todos los registros filtrados.
            var agrupados = await query
                .GroupBy(m => m.Tipo)
                .Select(g => new
                {
                    Tipo = g.Key,
                    Cantidad = g.Count(),
                    Peso = g.Sum(m => m.Peso)
                })
                .ToListAsync();

            var inicial = agrupados.FirstOrDefault(x => x.Tipo == "Inicial");
            var entradas = agrupados.FirstOrDefault(x => x.Tipo == "Entrada");
            var salidas = agrupados.FirstOrDefault(x => x.Tipo == "Salida");
            var devoluciones = agrupados.FirstOrDefault(x => x.Tipo == "Devolucion");
            var eliminados = agrupados.FirstOrDefault(x => x.Tipo == "Eliminado");
            var ajustes = agrupados.FirstOrDefault(x => x.Tipo == "Ajuste");

            // EXISTENCIAS
            var existenciasCantidad =
                (inicial?.Cantidad ?? 0)
                + (entradas?.Cantidad ?? 0)
                + (devoluciones?.Cantidad ?? 0)
                - (ajustes?.Cantidad ?? 0)
                - (salidas?.Cantidad ?? 0)
                - (eliminados?.Cantidad ?? 0);

            var existenciasPeso =
                (inicial?.Peso ?? 0)
                + (entradas?.Peso ?? 0)
                + (devoluciones?.Peso ?? 0)
                - (ajustes?.Peso ?? 0)
                - (salidas?.Peso ?? 0)
                - (eliminados?.Peso ?? 0);

            // RESUMEN
            var resumen = new MovimientoInventarioAuditoriaResumenDto
            {
                InventarioInicial = new MovimientoInventarioTotalesDto
                {
                    Cantidad = inicial?.Cantidad ?? 0,
                    Peso = inicial?.Peso ?? 0
                },

                Entradas = new MovimientoInventarioTotalesDto
                {
                    Cantidad = entradas?.Cantidad ?? 0,
                    Peso = entradas?.Peso ?? 0
                },

                Salidas = new MovimientoInventarioTotalesDto
                {
                    Cantidad = salidas?.Cantidad ?? 0,
                    Peso = salidas?.Peso ?? 0
                },

                Devoluciones = new MovimientoInventarioTotalesDto
                {
                    Cantidad = devoluciones?.Cantidad ?? 0,
                    Peso = devoluciones?.Peso ?? 0
                },

                Eliminados = new MovimientoInventarioTotalesDto
                {
                    Cantidad = eliminados?.Cantidad ?? 0,
                    Peso = eliminados?.Peso ?? 0
                },

                Ajustes = new MovimientoInventarioTotalesDto
                {
                    Cantidad = ajustes?.Cantidad ?? 0,
                    Peso = ajustes?.Peso ?? 0
                },

                Existencias = new MovimientoInventarioTotalesDto
                {
                    Cantidad = existenciasCantidad,
                    Peso = existenciasPeso
                }
            };

            // TOTALES POR TIPO
            var totalesPorTipo = agrupados
                .Select(g => new MovimientoInventarioEstadisticaDto
                {
                    Tipo = g.Tipo,
                    Total = g.Cantidad
                })
                .ToList();

            // REFERENCIAS MÁS MOVIDAS
            var referenciasMasMovidas = await query
                .GroupBy(m => m.Referencia)
                .Select(g => new ReferenciaMasMovidaDto
                {
                    Referencia = g.Key,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToListAsync();

            // USUARIOS MÁS ACTIVOS
            var usuariosMasActivos = await query
                .GroupBy(m => m.Usuario)
                .Select(g => new UsuarioMasActivoDto
                {
                    Usuario = g.Key ?? "Sin usuario",
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToListAsync();

            // MOVIMIENTOS PAGINADOS
            var movimientos = await query
                .OrderByDescending(m => m.Fecha)
                .ThenByDescending(m => m.Id)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
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
                .ToListAsync();

            return new MovimientoInventarioAuditoriaCompletaDto
            {
                Resumen = resumen,
                Movimientos = movimientos,
                TotalesPorTipo = totalesPorTipo,
                ReferenciasMasMovidas = referenciasMasMovidas,
                UsuariosMasActivos = usuariosMasActivos,

                Pagina = pagina,
                PageSize = pageSize,
                TotalRegistros = totalRegistros,
                TotalPaginas = totalPaginas
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
                    Cliente = m.Cliente != null
                        ? m.Cliente.Nombre
                        : null,
                    ReferenciaImpresa = m.InventarioBodega.ReferenciaImpresa,
                    InventarioNombre = m.InventarioBodega.Referencia,
                    InventarioItemCodigo = m.InventarioBodegaItem.ReferenciaPeso
                })
                .FirstAsync();
        }
    }
}
