using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesTraslado;
using SistemaFlota.Models.ComprasNoFormalizadas.OrdenesTraslado;
using SistemaFlota.Services.Auth;
using SistemaFlota.Services.ComprasNoFormalizadas.Inventario;
using SistemaFlota.Services.Consecutivos;

namespace SistemaFlota.Services.ComprasNoFormalizadas.OrdenesTraslado
{
    public class OrdenTrasladoNoFormalizadaService : IOrdenTrasladoNoFormalizadaService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IConsecutivoService _consecutivoService;

        public OrdenTrasladoNoFormalizadaService(
            AppDbContext context,
            ICurrentUserService currentUser,
            IConsecutivoService consecutivoService)
        {
            _context = context;
            _currentUser = currentUser;
            _consecutivoService = consecutivoService;
        }

        public async Task<OrdenTrasladoNoFormalizadaDto> CrearAsync(
            CrearOrdenTrasladoNoFormalizadaDto dto)
        {
            if (dto.Materiales == null || !dto.Materiales.Any())
                throw new Exception("La orden de traslado debe tener al menos un material.");

            if (!_currentUser.IdUsuario.HasValue)
                throw new Exception("No fue posible identificar el usuario actual.");

            if (string.IsNullOrWhiteSpace(dto.Destino))
                throw new Exception("El destino es obligatorio.");

            foreach (var item in dto.Materiales)
            {
                if (!item.MaterialNoFormalizadoId.HasValue)
                    throw new Exception("Cada material de la orden de traslado debe tener un material asociado.");

                if (item.CantidadKg <= 0)
                    throw new Exception("La cantidad en KG debe ser mayor a cero.");

                if (item.Bultos <= 0)
                    throw new Exception("La cantidad de bultos debe ser mayor a cero.");

                if (string.IsNullOrWhiteSpace(item.Proveedor))
                    throw new Exception("El proveedor es obligatorio.");

                if (string.IsNullOrWhiteSpace(item.Tipo))
                    throw new Exception("El tipo de material es obligatorio.");

                if (string.IsNullOrWhiteSpace(item.Densidad))
                    throw new Exception("La densidad es obligatoria.");

                if (string.IsNullOrWhiteSpace(item.Color))
                    throw new Exception("El color es obligatorio.");
            }

            await ValidarStockDisponibleAsync(dto.Materiales);

            var numeroOrden = await _consecutivoService.GenerarAsync(
                "OrdenTrasladoNoFormalizada");

            var fechaActual = DateTime.Now;

            var orden = new OrdenTrasladoNoFormalizada
            {
                NumeroOrden = numeroOrden,
                Fecha = fechaActual,
                Destino = dto.Destino.Trim(),
                UsuarioId = _currentUser.IdUsuario.Value,
                Estado = "Pendiente",
                FechaCreacion = fechaActual
            };

            foreach (var item in dto.Materiales)
            {
                orden.Detalles.Add(new OrdenTrasladoDetalleNoFormalizada
                {
                    MaterialNoFormalizadoId = item.MaterialNoFormalizadoId,
                    Proveedor = item.Proveedor.Trim(),
                    Tipo = item.Tipo.Trim(),
                    Densidad = item.Densidad.Trim(),
                    Color = item.Color.Trim(),
                    CantidadKg = item.CantidadKg,
                    Bultos = item.Bultos
                });
            }

            orden.TotalKg = orden.Detalles.Sum(x => x.CantidadKg);
            orden.TotalBultos = orden.Detalles.Sum(x => x.Bultos);

            _context.OrdenesTrasladoNoFormalizadas.Add(orden);
            await _context.SaveChangesAsync();

            return await ObtenerPorIdAsync(orden.Id)
                ?? throw new Exception(
                    "No fue posible obtener la orden de traslado creada.");
        }

        private async Task ValidarStockDisponibleAsync(
            List<CrearOrdenTrasladoNoFormalizadaDetalleDto> materiales)
        {
            var solicitudes = materiales
                .Where(x => x.MaterialNoFormalizadoId.HasValue)
                .GroupBy(x => new
                {
                    MaterialNoFormalizadoId = x.MaterialNoFormalizadoId!.Value,
                    Color = x.Color.Trim()
                })
                .Select(g => new
                {
                    MaterialNoFormalizadoId = g.Key.MaterialNoFormalizadoId,
                    Color = g.Key.Color,
                    CantidadSolicitada = g.Sum(x => x.CantidadKg),
                    Tipo = g.First().Tipo
                })
                .ToList();

            foreach (var solicitud in solicitudes)
            {
                var inventario = await _context.InventariosNoFormalizados
                    .FirstOrDefaultAsync(x =>
                        x.MaterialId == solicitud.MaterialNoFormalizadoId &&
                        x.Color == solicitud.Color);

                if (inventario == null)
                {
                    throw new Exception(
                        $"No existe inventario para el material '{solicitud.Tipo}' " +
                        $"con color '{solicitud.Color}'.");
                }

                var cantidadComprometida =
                    await _context.OrdenesTrasladoNoFormalizadas
                        .Where(o =>
                            o.Estado == "Pendiente" ||
                            o.Estado == "Verificando")
                        .SelectMany(o => o.Detalles)
                        .Where(d =>
                            d.MaterialNoFormalizadoId ==
                                solicitud.MaterialNoFormalizadoId &&
                            d.Color == solicitud.Color)
                        .SumAsync(d => (decimal?)d.CantidadKg) ?? 0m;

                var stockDisponible =
                    inventario.StockActual - cantidadComprometida;

                if (stockDisponible < 0)
                    stockDisponible = 0;

                if (stockDisponible < solicitud.CantidadSolicitada)
                {
                    throw new Exception(
                        $"Stock insuficiente para el material '{solicitud.Tipo}' " +
                        $"con color '{solicitud.Color}'. " +
                        $"Stock actual: {inventario.StockActual:N2} KG. " +
                        $"Comprometido en otras órdenes: {cantidadComprometida:N2} KG. " +
                        $"Disponible: {stockDisponible:N2} KG. " +
                        $"Solicitado: {solicitud.CantidadSolicitada:N2} KG.");
                }
            }
        }

        public async Task<OrdenTrasladoNoFormalizadaDto?> ObtenerPorIdAsync(int id)
        {
            var orden = await _context.OrdenesTrasladoNoFormalizadas
                .AsNoTracking()
                .Include(x => x.Usuario)
                .Include(x => x.UsuarioVerificacion)
                .Include(x => x.UsuarioConfirmacion)
                .Include(x => x.Detalles)
                    .ThenInclude(x => x.MaterialNoFormalizado)
                .FirstOrDefaultAsync(x => x.Id == id);

            return orden == null ? null : MapearOrden(orden);
        }

        public async Task<OrdenTrasladoNoFormalizadaPaginadoDto> ObtenerTodosAsync(
            string? search,
            string? estado,
            string? destino,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int pagina = 1,
            int tamanoPagina = 10)
        {
            pagina = pagina < 1 ? 1 : pagina;

            tamanoPagina = tamanoPagina switch
            {
                <= 0 => 10,
                > 100 => 100,
                _ => tamanoPagina
            };

            var query = _context.OrdenesTrasladoNoFormalizadas
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.NumeroOrden.Contains(search) ||
                    x.Destino.Contains(search) ||
                    (x.Usuario != null &&
                     x.Usuario.Username.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(x => x.Estado == estado);

            if (!string.IsNullOrWhiteSpace(destino))
                query = query.Where(x => x.Destino == destino);

            if (fechaInicio.HasValue)
            {
                var inicio = fechaInicio.Value.Date;
                query = query.Where(x => x.Fecha >= inicio);
            }

            if (fechaFin.HasValue)
            {
                var fin = fechaFin.Value.Date.AddDays(1);
                query = query.Where(x => x.Fecha < fin);
            }

            var totalRegistros = await query.CountAsync();

            var ordenes = await query
                .OrderByDescending(x => x.Fecha)
                .ThenByDescending(x => x.Id)
                .Include(x => x.Usuario)
                .Include(x => x.UsuarioVerificacion)
                .Include(x => x.UsuarioConfirmacion)
                .Include(x => x.Detalles)
                    .ThenInclude(x => x.MaterialNoFormalizado)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            var totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)tamanoPagina);

            return new OrdenTrasladoNoFormalizadaPaginadoDto
            {
                Datos = ordenes.Select(MapearOrden).ToList(),
                TotalRegistros = totalRegistros,
                Pagina = pagina,
                TamanoPagina = tamanoPagina,
                TotalPaginas = totalPaginas
            };
        }

        public async Task<OrdenTrasladoNoFormalizadaDto> VerificarAsync(
            VerificarOrdenTrasladoNoFormalizadaDto dto)
        {
            if (!_currentUser.IdUsuario.HasValue)
                throw new Exception("No fue posible identificar el usuario actual.");

            var orden = await _context.OrdenesTrasladoNoFormalizadas
                .Include(x => x.Detalles)
                .FirstOrDefaultAsync(
                    x => x.Id == dto.OrdenTrasladoNoFormalizadaId);

            if (orden == null)
                throw new Exception("La orden de traslado no existe.");

            if (orden.Estado != "Pendiente")
            {
                throw new Exception(
                    $"La orden no puede ser verificada porque actualmente está en estado '{orden.Estado}'.");
            }

            if (dto.Materiales == null || !dto.Materiales.Any())
                throw new Exception("Debe verificar al menos un material.");

            foreach (var item in dto.Materiales)
            {
                var detalle = orden.Detalles
                    .FirstOrDefault(x => x.Id == item.DetalleId);

                if (detalle == null)
                {
                    throw new Exception(
                        $"No se encontró el detalle de material con ID {item.DetalleId}.");
                }

                if (item.CantidadVerificadaKg < 0)
                {
                    throw new Exception(
                        $"La cantidad verificada de {detalle.Tipo} no puede ser negativa.");
                }

                if (item.BultosVerificados < 0)
                {
                    throw new Exception(
                        $"Los bultos verificados de {detalle.Tipo} no pueden ser negativos.");
                }

                if (item.CantidadVerificadaKg > detalle.CantidadKg)
                {
                    throw new Exception(
                        $"La cantidad verificada de {detalle.Tipo} " +
                        "no puede superar la cantidad solicitada.");
                }

                if (item.BultosVerificados > detalle.Bultos)
                {
                    throw new Exception(
                        $"Los bultos verificados de {detalle.Tipo} " +
                        "no pueden superar los bultos solicitados.");
                }

                detalle.CantidadVerificadaKg =
                    item.CantidadVerificadaKg;

                detalle.BultosVerificados =
                    item.BultosVerificados;

                if (item.CantidadVerificadaKg == 0)
                {
                    detalle.EstadoVerificacion = "NoDisponible";
                }
                else if (item.CantidadVerificadaKg < detalle.CantidadKg)
                {
                    detalle.EstadoVerificacion = "Parcial";
                }
                else
                {
                    detalle.EstadoVerificacion = "Completo";
                }
            }

            orden.Estado = "Verificando";
            orden.FechaVerificacion = DateTime.Now;
            orden.UsuarioVerificacionId = _currentUser.IdUsuario.Value;

            await _context.SaveChangesAsync();

            return await ObtenerPorIdAsync(orden.Id)
                ?? throw new Exception(
                    "No fue posible obtener la orden verificada.");
        }

        public async Task<OrdenTrasladoNoFormalizadaDto> ConfirmarAsync(int id)
        {
            if (!_currentUser.IdUsuario.HasValue)
                throw new Exception("No fue posible identificar el usuario actual.");

            var orden = await _context.OrdenesTrasladoNoFormalizadas
                .Include(x => x.Detalles)
                    .ThenInclude(x => x.MaterialNoFormalizado)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (orden == null)
                throw new Exception("La orden de traslado no existe.");

            if (orden.Estado != "Verificando")
            {
                throw new Exception(
                    $"La orden no puede ser confirmada porque actualmente está en estado '{orden.Estado}'.");
            }

            var materialesIncompletos = orden.Detalles
                .Where(x => x.EstadoVerificacion != "Completo")
                .ToList();

            if (materialesIncompletos.Any())
            {
                throw new Exception(
                    "No se puede confirmar la orden porque existen " +
                    "materiales parciales o no disponibles.");
            }

            foreach (var detalle in orden.Detalles)
            {
                if (!detalle.MaterialNoFormalizadoId.HasValue)
                {
                    throw new Exception(
                        $"El detalle de material {detalle.Id} " +
                        "no tiene un material asociado.");
                }

                var inventario = await _context.InventariosNoFormalizados
                    .FirstOrDefaultAsync(x =>
                        x.MaterialId ==
                            detalle.MaterialNoFormalizadoId.Value &&
                        x.Color == detalle.Color);

                if (inventario == null)
                {
                    throw new Exception(
                        $"No existe inventario para el material '{detalle.Tipo}' " +
                        $"con color '{detalle.Color}'.");
                }

                var cantidadDescontar =
                    detalle.CantidadVerificadaKg ?? 0m;

                if (inventario.StockActual < cantidadDescontar)
                {
                    throw new Exception(
                        $"Inventario insuficiente para el material '{detalle.Tipo}' " +
                        $"color '{detalle.Color}'. " +
                        $"Disponible: {inventario.StockActual:N2} KG. " +
                        $"Requerido: {cantidadDescontar:N2} KG.");
                }

                inventario.StockActual -= cantidadDescontar;

                inventario.ValorInventario =
                    inventario.StockActual * inventario.CostoPromedio;

                inventario.FechaActualizacion = DateTime.Now;
            }

            orden.Estado = "Confirmado";
            orden.FechaConfirmacion = DateTime.Now;
            orden.UsuarioConfirmacionId = _currentUser.IdUsuario.Value;

            await _context.SaveChangesAsync();

            return await ObtenerPorIdAsync(orden.Id)
                ?? throw new Exception(
                    "No fue posible obtener la orden confirmada.");
        }

        private static OrdenTrasladoNoFormalizadaDto MapearOrden(
            OrdenTrasladoNoFormalizada orden)
        {
            return new OrdenTrasladoNoFormalizadaDto
            {
                Id = orden.Id,
                NumeroOrden = orden.NumeroOrden,
                Fecha = orden.Fecha,
                Destino = orden.Destino,
                Estado = orden.Estado,
                UsuarioId = orden.UsuarioId,
                Usuario = orden.Usuario?.Username ?? string.Empty,
                TotalKg = orden.TotalKg,
                TotalBultos = orden.TotalBultos,
                FechaVerificacion = orden.FechaVerificacion,
                UsuarioVerificacionId = orden.UsuarioVerificacionId,
                UsuarioVerificacion =
                    orden.UsuarioVerificacion?.Username ?? string.Empty,
                FechaConfirmacion = orden.FechaConfirmacion,
                UsuarioConfirmacionId = orden.UsuarioConfirmacionId,
                UsuarioConfirmacion =
                    orden.UsuarioConfirmacion?.Username ?? string.Empty,
                Materiales = orden.Detalles
                    .Select(x => new OrdenTrasladoDetalleNoFormalizadaDto
                    {
                        Id = x.Id,
                        MaterialNoFormalizadoId =
                            x.MaterialNoFormalizadoId,
                        Material = x.MaterialNoFormalizado?.NombreMaterial ?? string.Empty,
                        Proveedor = x.Proveedor,
                        Tipo = x.Tipo,
                        Densidad = x.Densidad,
                        Color = x.Color,
                        CantidadKg = x.CantidadKg,
                        Bultos = x.Bultos,
                        CantidadVerificadaKg =
                            x.CantidadVerificadaKg,
                        BultosVerificados =
                            x.BultosVerificados,
                        EstadoVerificacion =
                            x.EstadoVerificacion
                    })
                    .ToList()
            };
        }
    }
}
