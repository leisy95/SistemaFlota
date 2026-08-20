using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.Costos.RecepcionMercancia;
using SistemaFlota.Models.Costos.RecepcionMercancias;
using SistemaFlota.Services.Auth;
using SistemaFlota.Services.Consecutivos;
using SistemaFlota.Services.Costos.Inventario;
using SistemaFlota.Services.Notificaciones;

namespace SistemaFlota.Services.Costos.RecepcionMercancia
{
    public class RecepcionMercanciaService : IRecepcionMercanciaService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IConsecutivoService _consecutivoService;
        private readonly INotificacionRecepcionService _notificacion;
        private readonly IInventarioService _inventarioService;

        public RecepcionMercanciaService(
            AppDbContext context,
             ICurrentUserService currentUser,
             IConsecutivoService consecutivoService,
             INotificacionRecepcionService notificacion,
             IInventarioService inventarioService)
        {
            _context = context;
            _currentUser = currentUser;
            _consecutivoService = consecutivoService;
            _notificacion = notificacion;
            _inventarioService = inventarioService;
        }

        public async Task<RecepcionMercanciaPaginadoDto> ObtenerAsync(
            string? search,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? proveedorId,
            int page,
            int pageSize)
        {
            throw new NotImplementedException();
        }

        public async Task<RecepcionMercanciaDto?> ObtenerPorIdAsync(int id)
        {
            var recepcion = await _context.RecepcionesMercancias
                .Include(r => r.OrdenCompra)
                    .ThenInclude(o => o.Proveedor)
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.OrdenCompraDetalle)
                        .ThenInclude(o => o.Material)
                .FirstOrDefaultAsync(r => r.Id == id);


            if (recepcion == null)
                return null;


            return new RecepcionMercanciaDto
            {
                Id = recepcion.Id,
                ConsecutivoEntrada = recepcion.NumeroRecepcion,
                NumeroOrden = recepcion.OrdenCompra!.Numero,
                Proveedor = recepcion.OrdenCompra.Proveedor!.Nombre,
                FechaRecepcion = recepcion.FechaRecepcion,
                Conductor = recepcion.Conductor,
                Transportadora = recepcion.Transportadora,
                EmbalajeAdecuado = recepcion.EmbalajeAdecuado,

                TotalKg = recepcion.Detalles.Sum(x => x.CantidadRecibida),
                TotalBultos = recepcion.Detalles.Sum(x => x.BultosRecibidos),

                Detalles = recepcion.Detalles.Select(x => new RecepcionDetalleConsultaDto
                {
                    Material = x.OrdenCompraDetalle!.Material!.NombreMaterial,
                    CantidadRecibida = x.CantidadRecibida,
                    BultosRecibidos = x.BultosRecibidos,
                    LoteProveedor = x.LoteProveedor,
                    EstadoMaterial = x.EstadoMaterial

                }).ToList()
            };
        }

        public async Task<RecepcionFormularioDto?> ObtenerFormularioAsync(int ordenCompraId)
        {
            var orden = await _context.OrdenesCompra
                .Include(o => o.Proveedor)
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Material)
                .FirstOrDefaultAsync(o => o.Id == ordenCompraId);

            if (orden == null)
                return null;

            var recepcionesAnteriores = await _context.RecepcionesMercancias
                .Where(r => r.OrdenCompraId == ordenCompraId)
                .SelectMany(r => r.Detalles)
                .ToListAsync();

            var items = orden.Detalles
                .Select(d =>
                {
                    var cantidadRecibida = recepcionesAnteriores
                        .Where(r => r.OrdenCompraDetalleId == d.Id)
                        .Sum(r => r.CantidadRecibida);

                    var bultosRecibidos = recepcionesAnteriores
                        .Where(r => r.OrdenCompraDetalleId == d.Id)
                        .Sum(r => r.BultosRecibidos);

                    var cantidadPendiente = Math.Max(0, d.CantidadKg - cantidadRecibida);
                    var bultosPendientes = Math.Max(0, d.Bultos - bultosRecibidos);

                    return new RecepcionMercanciaDetalleFormularioDto
                    {
                        OrdenCompraDetalleId = d.Id,
                        MaterialId = d.MaterialId,
                        Material = d.Material?.NombreMaterial ?? string.Empty,
                        Cantidad = d.CantidadKg,
                        Bultos = d.Bultos,
                        CantidadRecibida = cantidadRecibida,
                        BultosRecibidos = bultosRecibidos,
                        CantidadPendiente = cantidadPendiente,
                        BultosPendientes = bultosPendientes
                    };
                })
                .Where(x => x.CantidadPendiente > 0 || x.BultosPendientes > 0)
                .ToList();

            return new RecepcionFormularioDto
            {
                OrdenCompraId = orden.Id,
                NumeroOrden = orden.Numero,
                Proveedor = orden.Proveedor?.Nombre ?? string.Empty,
                FechaOrden = orden.FechaOrden,
                Recibe = _currentUser.Usuario,
                Cargo = _currentUser.Rol,
                Items = items
            };
        }

        public async Task<RecepcionMercanciaDto> CrearAsync(
             CrearRecepcionMercanciaDto dto)
        {
            var orden = await _context.OrdenesCompra
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Material)
                .Include(o => o.Proveedor)
                .FirstOrDefaultAsync(o => o.Id == dto.OrdenCompraId);

            if (orden == null)
                throw new Exception("La orden de compra no existe");

            var recepcionesAnteriores = await _context.RecepcionesMercancias
                .Where(r => r.OrdenCompraId == dto.OrdenCompraId)
                .SelectMany(r => r.Detalles)
                .ToListAsync();

            var numeroRecepcion = await _consecutivoService.GenerarAsync("RecepcionMercancia");

            var recepcion = new Models.Costos.RecepcionMercancias.RecepcionMercancia
            {
                OrdenCompraId = dto.OrdenCompraId,
                NumeroRecepcion = numeroRecepcion,
                Conductor = dto.Conductor,
                Transportadora = dto.Transportadora,
                TipoDocumento = dto.TipoDocumento,
                EmbalajeAdecuado = dto.EmbalajeAdecuado,
                Recibe = dto.Recibe,
                Cargo = dto.Cargo,
                Observaciones = dto.Observaciones,
                FechaRecepcion = DateTime.Now
            };


            foreach (var item in dto.Detalles)
            {
                var detalleOrden = orden.Detalles
                    .FirstOrDefault(d => d.Id == item.OrdenCompraDetalleId);

                if (detalleOrden == null)
                    throw new Exception(
                        $"El detalle {item.OrdenCompraDetalleId} no pertenece a la orden de compra."
                    );

                var cantidadRecibidaAnterior = recepcionesAnteriores
                    .Where(r => r.OrdenCompraDetalleId == item.OrdenCompraDetalleId)
                    .Sum(r => r.CantidadRecibida);

                var bultosRecibidosAnterior = recepcionesAnteriores
                    .Where(r => r.OrdenCompraDetalleId == item.OrdenCompraDetalleId)
                    .Sum(r => r.BultosRecibidos);

                var cantidadPendiente = Math.Max(
                    0,
                    detalleOrden.CantidadKg - cantidadRecibidaAnterior
                );

                var bultosPendientes = Math.Max(
                    0,
                    detalleOrden.Bultos - bultosRecibidosAnterior
                );

                if (item.CantidadRecibida < 0)
                    throw new Exception(
                        $"La cantidad recibida para {detalleOrden.Material?.NombreMaterial} no puede ser negativa."
                    );

                if (item.BultosRecibidos < 0)
                    throw new Exception(
                        $"Los bultos recibidos para {detalleOrden.Material?.NombreMaterial} no pueden ser negativos."
                    );

                if (item.CantidadRecibida > cantidadPendiente)
                    throw new Exception(
                        $"La cantidad recibida de {detalleOrden.Material?.NombreMaterial} " +
                        $"supera la cantidad pendiente. " +
                        $"Pendiente: {cantidadPendiente} kg."
                    );

                if (item.BultosRecibidos > bultosPendientes)
                    throw new Exception(
                        $"Los bultos recibidos de {detalleOrden.Material?.NombreMaterial} " +
                        $"superan los bultos pendientes. " +
                        $"Pendientes: {bultosPendientes}."
                    );

                if (item.CantidadRecibida == 0 && item.BultosRecibidos == 0)
                    continue;

                var detalle = new RecepcionMercanciaDetalle
                {
                    OrdenCompraDetalleId = item.OrdenCompraDetalleId,
                    CantidadRecibida = item.CantidadRecibida,
                    BultosRecibidos = item.BultosRecibidos,
                    LoteProveedor = item.LoteProveedor,
                    EstadoMaterial = item.EstadoMaterial,
                    Observaciones = item.Observaciones
                };

                recepcion.Detalles.Add(detalle);
            }

            if (!recepcion.Detalles.Any())
                throw new Exception("Debe ingresar al menos una cantidad o bulto recibido.");

            _context.RecepcionesMercancias.Add(recepcion);

            await _context.SaveChangesAsync();

            var todosLosDetallesRecibidos = await _context.RecepcionesMercancias
                .Where(r => r.OrdenCompraId == dto.OrdenCompraId)
                .SelectMany(r => r.Detalles)
                .ToListAsync();

            var ordenCompleta = orden.Detalles.All(d =>
            {
                var cantidadRecibida = todosLosDetallesRecibidos
                    .Where(r => r.OrdenCompraDetalleId == d.Id)
                    .Sum(r => r.CantidadRecibida);

                var bultosRecibidos = todosLosDetallesRecibidos
                    .Where(r => r.OrdenCompraDetalleId == d.Id)
                    .Sum(r => r.BultosRecibidos);

                return cantidadRecibida >= d.CantidadKg &&
                       bultosRecibidos >= d.Bultos;
            });

            orden.Estado = ordenCompleta
                ? "Recepcionada"
                : "Parcial";

            await _context.SaveChangesAsync();

            await _notificacion.EnviarRecepcionMercanciaAsync(
                recepcion.Id,
                dto.Usuarios
            );

            return new RecepcionMercanciaDto
            {
                Id = recepcion.Id,
                ConsecutivoEntrada = recepcion.NumeroRecepcion,
                OrdenCompraId = recepcion.OrdenCompraId,
                NumeroOrden = orden.Numero,
                Proveedor = orden.Proveedor?.Nombre ?? "",
                FechaRecepcion = recepcion.FechaRecepcion,
                Conductor = recepcion.Conductor,
                Transportadora = recepcion.Transportadora,
                EmbalajeAdecuado = recepcion.EmbalajeAdecuado,
                TotalKg = recepcion.Detalles.Sum(x => x.CantidadRecibida),
                TotalBultos = recepcion.Detalles.Sum(x => x.BultosRecibidos)
            };
        }

        public async Task ConfirmarRecepcionAsync(int id)
        {
            try
            {
                var recepcion = await _context.RecepcionesMercancias
                    .Include(r => r.OrdenCompra)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (recepcion == null)
                    throw new Exception("La recepción no existe.");

                if (recepcion.FechaConfirmacion.HasValue)
                    throw new Exception("Esta recepción ya fue confirmada.");

                if (recepcion.OrdenCompra == null)
                    throw new Exception("La orden de compra no existe.");

                if (recepcion.OrdenCompra.Estado != "Parcial" && recepcion.OrdenCompra.Estado != "Recepcionada")
                    throw new Exception("La orden de compra no está en un estado válido para confirmar la recepción.");

                recepcion.FechaConfirmacion = DateTime.Now;
                recepcion.UsuarioConfirmacionId = _currentUser.IdUsuario!.Value;

                await _inventarioService.ProcesarRecepcionAsync(id);

                var recepcionesPendientes = await _context.RecepcionesMercancias
                    .Where(r => r.OrdenCompraId == recepcion.OrdenCompraId && r.FechaConfirmacion == null)
                    .AnyAsync();

                if (!recepcionesPendientes)
                {
                    var detallesOrden = await _context.OrdenesCompra
                        .Where(o => o.Id == recepcion.OrdenCompraId)
                        .SelectMany(o => o.Detalles)
                        .ToListAsync();

                    var detallesRecibidos = await _context.RecepcionesMercancias
                        .Where(r => r.OrdenCompraId == recepcion.OrdenCompraId)
                        .SelectMany(r => r.Detalles)
                        .ToListAsync();

                    var ordenCompleta = detallesOrden.All(d =>
                    {
                        var kgRecibidos = detallesRecibidos
                            .Where(x => x.OrdenCompraDetalleId == d.Id)
                            .Sum(x => x.CantidadRecibida);

                        var bultosRecibidos = detallesRecibidos
                            .Where(x => x.OrdenCompraDetalleId == d.Id)
                            .Sum(x => x.BultosRecibidos);

                        return kgRecibidos >= d.CantidadKg && bultosRecibidos >= d.Bultos;
                    });

                    recepcion.OrdenCompra.Estado = ordenCompleta ? "Confirmada" : "Parcial";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task<bool> ActualizarAsync(int id, ActualizarRecepcionMercanciaDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> EliminarAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<FiltrosRecepcionMercanciaDto> ObtenerFiltrosAsync()
        {
            throw new NotImplementedException();
        }
    }
}