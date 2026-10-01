using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.ComprasNoFormalizadas.RecepcionMercancias;
using SistemaFlota.Models.ComprasNoFormalizadas.RecepcionMercancias;
using SistemaFlota.Services.Auth;
using SistemaFlota.Services.ComprasNoFormalizadas.Inventario;
using SistemaFlota.Services.Consecutivos;
using SistemaFlota.Services.Notificaciones;

namespace SistemaFlota.Services.ComprasNoFormalizadas.RecepcionMercancia
{
    public class RecepcionMercanciaNoFormalizadaService
        : IRecepcionMercanciaNoFormalizadaService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IConsecutivoService _consecutivoService;
        private readonly INotificacionRecepcionService _notificacion;
        private readonly IInventarioNoFormalizadoService _inventarioService;

        public RecepcionMercanciaNoFormalizadaService(
            AppDbContext context,
            ICurrentUserService currentUser,
            IConsecutivoService consecutivoService,
            INotificacionRecepcionService notificacion,
             IInventarioNoFormalizadoService inventarioService)
        {
            _context = context;
            _currentUser = currentUser;
            _consecutivoService = consecutivoService;
            _notificacion = notificacion;
            _inventarioService = inventarioService;
        }

        public async Task<RecepcionMercanciaNoFormalizadaPaginadoDto> ObtenerAsync(
            string? search,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? proveedorId,
            int page,
            int pageSize)
        {
            throw new NotImplementedException();
        }

        public async Task<RecepcionMercanciaNoFormalizadaDto?> ObtenerPorIdAsync(
            int id)
        {
            var recepcion =
                await _context.RecepcionesMercanciasNoFormalizadas
                    .Include(r => r.OrdenCompraNoFormalizada)
                        .ThenInclude(o => o.ProveedorNoFormalizado)
                    .Include(r => r.Detalles)
                        .ThenInclude(d =>
                            d.OrdenCompraDetalleNoFormalizada)
                        .ThenInclude(d => d.MaterialNoFormalizado)
                    .FirstOrDefaultAsync(r => r.Id == id);

            if (recepcion == null)
                return null;

            return new RecepcionMercanciaNoFormalizadaDto
            {
                Id = recepcion.Id,

                ConsecutivoEntrada =
                    recepcion.NumeroRecepcion,

                OrdenCompraNoFormalizadaId =
                    recepcion.OrdenCompraNoFormalizadaId,

                NumeroOrden =
                    recepcion.OrdenCompraNoFormalizada!.Numero,

                Proveedor =
                    recepcion.OrdenCompraNoFormalizada
                        .ProveedorNoFormalizado!.Nombre,

                FechaRecepcion =
                    recepcion.FechaRecepcion,

                Conductor =
                    recepcion.Conductor,

                Transportadora =
                    recepcion.Transportadora,

                EmbalajeAdecuado =
                    recepcion.EmbalajeAdecuado,

                TotalKg =
                    recepcion.Detalles
                        .Sum(x => x.CantidadRecibida),

                TotalBultos =
                    recepcion.Detalles
                        .Sum(x => x.BultosRecibidos),

                Detalles =
                    recepcion.Detalles
                        .Select(x =>
                            new RecepcionDetalleConsultaNoFormalizadaDto
                            {
                                Material =
                                    x.OrdenCompraDetalleNoFormalizada!
                                        .MaterialNoFormalizado!
                                        .NombreMaterial,

                                CantidadRecibida =
                                    x.CantidadRecibida,

                                BultosRecibidos =
                                    x.BultosRecibidos,

                                LoteProveedor =
                                    x.LoteProveedor,

                                EstadoMaterial =
                                    x.EstadoMaterial,

                                Observaciones =
                                    x.Observaciones
                            })
                        .ToList()
            };
        }

        public async Task<RecepcionFormularioNoFormalizadaDto?>
            ObtenerFormularioAsync(int ordenCompraId)
        {
            var orden =
                await _context.OrdenesCompraNoFormalizadas
                    .Include(o => o.ProveedorNoFormalizado)
                    .Include(o => o.Detalles)
                        .ThenInclude(d => d.MaterialNoFormalizado)
                    .FirstOrDefaultAsync(o => o.Id == ordenCompraId);

            if (orden == null)
                return null;

            var recepcionesAnteriores =
                await _context.RecepcionesMercanciasNoFormalizadas
                    .Where(r =>
                        r.OrdenCompraNoFormalizadaId == ordenCompraId)
                    .SelectMany(r => r.Detalles)
                    .ToListAsync();

            var items = orden.Detalles
                .Select(d =>
                {
                    var cantidadRecibida =
                        recepcionesAnteriores
                            .Where(r =>
                                r.OrdenCompraDetalleNoFormalizadaId ==
                                d.Id)
                            .Sum(r => r.CantidadRecibida);

                    var bultosRecibidos =
                        recepcionesAnteriores
                            .Where(r =>
                                r.OrdenCompraDetalleNoFormalizadaId ==
                                d.Id)
                            .Sum(r => r.BultosRecibidos);

                    var cantidadPendiente =
                        Math.Max(
                            0,
                            d.CantidadKg - cantidadRecibida);

                    var bultosPendientes =
                        Math.Max(
                            0,
                            d.Bultos - bultosRecibidos);

                    return
                        new RecepcionMercanciaDetalleFormularioNoFormalizadaDto
                        {
                            OrdenCompraDetalleNoFormalizadaId =
                                d.Id,

                            MaterialNoFormalizadoId =
                                d.MaterialNoFormalizadoId,

                            Material =
                                d.MaterialNoFormalizado?.NombreMaterial
                                ?? string.Empty,

                            Cantidad =
                                d.CantidadKg,

                            Bultos =
                                d.Bultos,

                            CantidadRecibida =
                                cantidadRecibida,

                            BultosRecibidos =
                                bultosRecibidos,

                            CantidadPendiente =
                                cantidadPendiente,

                            BultosPendientes =
                                bultosPendientes
                        };
                })
                .Where(x =>
                    x.CantidadPendiente > 0 ||
                    x.BultosPendientes > 0)
                .ToList();

            return new RecepcionFormularioNoFormalizadaDto
            {
                OrdenCompraNoFormalizadaId =
                    orden.Id,

                NumeroOrden =
                    orden.Numero,

                Proveedor =
                    orden.ProveedorNoFormalizado?.Nombre
                    ?? string.Empty,

                FechaOrden =
                    orden.FechaOrden,

                Recibe =
                    _currentUser.Usuario,

                Cargo =
                    _currentUser.Rol,

                Items =
                    items
            };
        }

        public async Task<RecepcionMercanciaNoFormalizadaDto> CrearAsync(
            CrearRecepcionMercanciaNoFormalizadaDto dto)
        {
            var orden =
                await _context.OrdenesCompraNoFormalizadas
                    .Include(o => o.Detalles)
                        .ThenInclude(d => d.MaterialNoFormalizado)
                    .Include(o => o.ProveedorNoFormalizado)
                    .FirstOrDefaultAsync(o =>
                        o.Id == dto.OrdenCompraNoFormalizadaId);

            if (orden == null)
                throw new Exception(
                    "La orden de compra no existe.");

            // Buscar una recepción pendiente existente.
            // Si existe, se reutiliza para acumular
            // entregas parciales.
            var recepcion =
                await _context.RecepcionesMercanciasNoFormalizadas
                    .Include(r => r.Detalles)
                    .FirstOrDefaultAsync(r =>
                        r.OrdenCompraNoFormalizadaId ==
                            dto.OrdenCompraNoFormalizadaId &&
                        r.FechaConfirmacion == null);

            // Si no existe recepción pendiente,
            // crear una nueva.
            if (recepcion == null)
            {
                var numeroRecepcion =
                    await _consecutivoService.GenerarAsync(
                        "RecepcionMercanciaNoFormalizada");

                recepcion =
                    new RecepcionMercanciaNoFormalizada
                    {
                        OrdenCompraNoFormalizadaId =
                            dto.OrdenCompraNoFormalizadaId,

                        NumeroRecepcion =
                            numeroRecepcion,

                        Conductor =
                            dto.Conductor,

                        Transportadora =
                            dto.Transportadora,

                        TipoDocumento =
                            dto.TipoDocumento,

                        EmbalajeAdecuado =
                            dto.EmbalajeAdecuado,

                        Recibe =
                            dto.Recibe,

                        Cargo =
                            dto.Cargo,

                        Observaciones =
                            dto.Observaciones,

                        FechaRecepcion =
                            DateTime.Now
                    };

                _context.RecepcionesMercanciasNoFormalizadas
                    .Add(recepcion);
            }

            // Todos los detalles recibidos anteriormente
            // para esta orden.
            var recepcionesAnteriores =
                await _context.RecepcionesMercanciasNoFormalizadas
                    .Where(r =>
                        r.OrdenCompraNoFormalizadaId ==
                        dto.OrdenCompraNoFormalizadaId)
                    .SelectMany(r => r.Detalles)
                    .ToListAsync();

            // Acumuladores de la entrega actual.
            var cantidadesNuevaEntrega =
                new Dictionary<int, decimal>();

            var bultosNuevaEntrega =
                new Dictionary<int, decimal>();

            bool agregoDetalle = false;

            foreach (var item in dto.Detalles)
            {
                var detalleOrden =
                    orden.Detalles
                        .FirstOrDefault(d =>
                            d.Id ==
                            item.OrdenCompraDetalleNoFormalizadaId);

                if (detalleOrden == null)
                {
                    throw new Exception(
                        $"El detalle " +
                        $"{item.OrdenCompraDetalleNoFormalizadaId} " +
                        $"no pertenece a la orden de compra."
                    );
                }

                if (item.CantidadRecibida < 0)
                {
                    throw new Exception(
                        $"La cantidad recibida de " +
                        $"{detalleOrden.MaterialNoFormalizado?.NombreMaterial} " +
                        $"no puede ser negativa."
                    );
                }

                if (item.BultosRecibidos < 0)
                {
                    throw new Exception(
                        $"Los bultos recibidos de " +
                        $"{detalleOrden.MaterialNoFormalizado?.NombreMaterial} " +
                        $"no pueden ser negativos."
                    );
                }

                // Si no recibió nada, no se agrega.
                if (item.CantidadRecibida == 0 &&
                    item.BultosRecibidos == 0)
                {
                    continue;
                }

                agregoDetalle = true;

                if (!cantidadesNuevaEntrega.ContainsKey(
                        item.OrdenCompraDetalleNoFormalizadaId))
                {
                    cantidadesNuevaEntrega[
                        item.OrdenCompraDetalleNoFormalizadaId] = 0;

                    bultosNuevaEntrega[
                        item.OrdenCompraDetalleNoFormalizadaId] = 0;
                }

                cantidadesNuevaEntrega[
                    item.OrdenCompraDetalleNoFormalizadaId] +=
                    item.CantidadRecibida;

                bultosNuevaEntrega[
                    item.OrdenCompraDetalleNoFormalizadaId] +=
                    item.BultosRecibidos;

                // Total recibido anteriormente.
                var cantidadRecibidaAnterior =
                    recepcionesAnteriores
                        .Where(r =>
                            r.OrdenCompraDetalleNoFormalizadaId ==
                            item.OrdenCompraDetalleNoFormalizadaId)
                        .Sum(r => r.CantidadRecibida);

                var bultosRecibidosAnterior =
                    recepcionesAnteriores
                        .Where(r =>
                            r.OrdenCompraDetalleNoFormalizadaId ==
                            item.OrdenCompraDetalleNoFormalizadaId)
                        .Sum(r => r.BultosRecibidos);

                // Cantidad pendiente.
                var cantidadPendiente =
                    Math.Max(
                        0,
                        detalleOrden.CantidadKg -
                        cantidadRecibidaAnterior);

                var bultosPendientes =
                    Math.Max(
                        0,
                        detalleOrden.Bultos -
                        bultosRecibidosAnterior);

                // Cantidad de esta entrega.
                var cantidadNueva =
                    cantidadesNuevaEntrega[
                        item.OrdenCompraDetalleNoFormalizadaId];

                var bultosNuevos =
                    bultosNuevaEntrega[
                        item.OrdenCompraDetalleNoFormalizadaId];

                if (cantidadNueva > cantidadPendiente)
                {
                    throw new Exception(
                        $"La cantidad recibida de " +
                        $"{detalleOrden.MaterialNoFormalizado?.NombreMaterial} " +
                        $"supera la cantidad pendiente. " +
                        $"Pendiente: {cantidadPendiente} kg."
                    );
                }

                if (bultosNuevos > bultosPendientes)
                {
                    throw new Exception(
                        $"Los bultos recibidos de " +
                        $"{detalleOrden.MaterialNoFormalizado?.NombreMaterial} " +
                        $"superan los bultos pendientes. " +
                        $"Pendientes: {bultosPendientes}."
                    );
                }

                var detalle =
                    new RecepcionMercanciaDetalleNoFormalizada
                    {
                        RecepcionMercanciaNoFormalizada =
                            recepcion,

                        OrdenCompraDetalleNoFormalizadaId =
                            item.OrdenCompraDetalleNoFormalizadaId,

                        CantidadRecibida =
                            item.CantidadRecibida,

                        BultosRecibidos =
                            item.BultosRecibidos,

                        LoteProveedor =
                            item.LoteProveedor,

                        EstadoMaterial =
                            item.EstadoMaterial,

                        Observaciones =
                            item.Observaciones
                    };

                recepcion.Detalles.Add(detalle);
            }

            if (!agregoDetalle)
            {
                throw new Exception(
                    "Debe ingresar al menos una cantidad o bulto recibido."
                );
            }

            await _context.SaveChangesAsync();

            // Volver a consultar todo lo recibido.
            var todosLosDetallesRecibidos =
                await _context.RecepcionesMercanciasNoFormalizadas
                    .Where(r =>
                        r.OrdenCompraNoFormalizadaId ==
                        dto.OrdenCompraNoFormalizadaId)
                    .SelectMany(r => r.Detalles)
                    .ToListAsync();

            var ordenCompleta =
                orden.Detalles.All(d =>
                {
                    var cantidadRecibida =
                        todosLosDetallesRecibidos
                            .Where(r =>
                                r.OrdenCompraDetalleNoFormalizadaId ==
                                d.Id)
                            .Sum(r => r.CantidadRecibida);

                    var bultosRecibidos =
                        todosLosDetallesRecibidos
                            .Where(r =>
                                r.OrdenCompraDetalleNoFormalizadaId ==
                                d.Id)
                            .Sum(r => r.BultosRecibidos);

                    return cantidadRecibida >= d.CantidadKg &&
                           bultosRecibidos >= d.Bultos;
                });

            if (ordenCompleta)
            {
                orden.Estado = "Recepcionada";
            }
            else
            {
                orden.Estado = "Parcial";
            }

            await _context.SaveChangesAsync();

            await _notificacion.EnviarRecepcionMercanciaNoFormalizadaAsync(
                recepcion.Id,
                dto.Usuarios
            );

            return new RecepcionMercanciaNoFormalizadaDto
            {
                Id =
                    recepcion.Id,

                ConsecutivoEntrada =
                    recepcion.NumeroRecepcion,

                OrdenCompraNoFormalizadaId =
                    recepcion.OrdenCompraNoFormalizadaId,

                NumeroOrden =
                    orden.Numero,

                Proveedor =
                    orden.ProveedorNoFormalizado?.Nombre
                    ?? string.Empty,

                FechaRecepcion =
                    recepcion.FechaRecepcion,

                Conductor =
                    recepcion.Conductor,

                Transportadora =
                    recepcion.Transportadora,

                EmbalajeAdecuado =
                    recepcion.EmbalajeAdecuado,

                TotalKg =
                    recepcion.Detalles
                        .Sum(x => x.CantidadRecibida),

                TotalBultos =
                    recepcion.Detalles
                        .Sum(x => x.BultosRecibidos)
            };
        }

        public async Task ConfirmarRecepcionAsync(int id)
        {
            var recepcion =
                await _context.RecepcionesMercanciasNoFormalizadas
                    .Include(r => r.OrdenCompraNoFormalizada)
                        .ThenInclude(o => o.Detalles)
                    .FirstOrDefaultAsync(r => r.Id == id);

            if (recepcion == null)
                throw new Exception(
                    "La recepción no existe.");

            if (recepcion.FechaConfirmacion.HasValue)
                throw new Exception(
                    "Esta recepción ya fue confirmada.");

            if (recepcion.OrdenCompraNoFormalizada == null)
                throw new Exception(
                    "La orden de compra no existe.");

            // Traer todo lo recibido para la orden.
            var detallesRecibidos =
                await _context.RecepcionesMercanciasNoFormalizadas
                    .Where(r =>
                        r.OrdenCompraNoFormalizadaId ==
                        recepcion.OrdenCompraNoFormalizadaId)
                    .SelectMany(r => r.Detalles)
                    .ToListAsync();

            // Verificar que la orden esté completamente recibida.
            var ordenCompleta =
                recepcion.OrdenCompraNoFormalizada
                    .Detalles
                    .All(d =>
                    {
                        var kgRecibidos =
                            detallesRecibidos
                                .Where(x =>
                                    x.OrdenCompraDetalleNoFormalizadaId ==
                                    d.Id)
                                .Sum(x => x.CantidadRecibida);

                        var bultosRecibidos =
                            detallesRecibidos
                                .Where(x =>
                                    x.OrdenCompraDetalleNoFormalizadaId ==
                                    d.Id)
                                .Sum(x => x.BultosRecibidos);

                        return kgRecibidos >= d.CantidadKg &&
                               bultosRecibidos >= d.Bultos;
                    });

            if (!ordenCompleta)
            {
                throw new Exception(
                    "La recepción todavía está incompleta. " +
                    "Debe recibirse toda la mercancía antes de confirmar."
                );
            }

            // se envia al inventario
            await _inventarioService.ProcesarRecepcionAsync(id);

            // si fue exitoso enviar
            recepcion.FechaConfirmacion =
                DateTime.Now;

            recepcion.UsuarioConfirmacionId =
                _currentUser.IdUsuario!.Value;

            recepcion.OrdenCompraNoFormalizada.Estado =
                "Confirmada";

            await _context.SaveChangesAsync();
        }

        public async Task<bool> ActualizarAsync(
            int id,
            ActualizarRecepcionMercanciaNoFormalizadaDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> EliminarAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<FiltrosRecepcionMercanciaNoFormalizadaDto>
            ObtenerFiltrosAsync()
        {
            throw new NotImplementedException();
        }
    }
}
