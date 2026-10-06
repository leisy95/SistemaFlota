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

        // OBTENER RECEPCIONES
        public async Task<RecepcionMercanciaNoFormalizadaPaginadoDto> ObtenerAsync(
            string? search,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? proveedorId,
            int page,
            int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = _context.RecepcionesMercanciasNoFormalizadas
                .AsNoTracking()
                .Include(r => r.OrdenCompraNoFormalizada)
                    .ThenInclude(o => o.ProveedorNoFormalizado)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(r =>
                    r.NumeroRecepcion.Contains(search) ||
                    r.OrdenCompraNoFormalizada!.Numero.Contains(search) ||
                    r.OrdenCompraNoFormalizada!
                        .ProveedorNoFormalizado!.Nombre.Contains(search));
            }

            if (fechaInicio.HasValue)
            {
                query = query.Where(r =>
                    r.FechaRecepcion >= fechaInicio.Value.Date);
            }

            if (fechaFin.HasValue)
            {
                var fechaFinAjustada =
                    fechaFin.Value.Date.AddDays(1);

                query = query.Where(r =>
                    r.FechaRecepcion < fechaFinAjustada);
            }

            if (proveedorId.HasValue)
            {
                query = query.Where(r =>
                    r.OrdenCompraNoFormalizada!
                        .ProveedorNoFormalizadoId ==
                    proveedorId.Value);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.FechaRecepcion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RecepcionMercanciaNoFormalizadaDto
                {
                    Id = r.Id,

                    ConsecutivoEntrada =
                        r.NumeroRecepcion,

                    OrdenCompraNoFormalizadaId =
                        r.OrdenCompraNoFormalizadaId,

                    NumeroOrden =
                        r.OrdenCompraNoFormalizada!.Numero,

                    Proveedor =
                        r.OrdenCompraNoFormalizada!
                            .ProveedorNoFormalizado!.Nombre,

                    FechaRecepcion =
                        r.FechaRecepcion,

                    Conductor =
                        r.Conductor,

                    Transportadora =
                        r.Transportadora,

                    EmbalajeAdecuado =
                        r.EmbalajeAdecuado,

                    TotalKg =
                        r.Detalles.Sum(x => x.CantidadRecibida),

                    TotalBultos =
                        r.Detalles.Sum(x => x.BultosRecibidos)
                })
                .ToListAsync();

            return new RecepcionMercanciaNoFormalizadaPaginadoDto
            {
                Data = items,
                TotalRegistros = total,
                PaginaActual = page,
                TotalPaginas = (int)Math.Ceiling((double)total / pageSize)
            };
        }

        // OBTENER POR ID

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
                        .ThenInclude(d =>
                            d.MaterialNoFormalizado)

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
                                    x.Observaciones,

                                NumeroEntrega =
                                    x.NumeroEntrega,

                                FechaEntrega =
                                    x.FechaEntrega,

                                ProcesadoInventario =
                                    x.ProcesadoInventario
                            })
                        .ToList()
            };
        }

        // FORMULARIO

        public async Task<RecepcionFormularioNoFormalizadaDto?>
            ObtenerFormularioAsync(int ordenCompraId)
        {
            var orden =
                await _context.OrdenesCompraNoFormalizadas
                    .Include(o => o.ProveedorNoFormalizado)
                    .Include(o => o.Detalles)
                        .ThenInclude(d => d.MaterialNoFormalizado)
                    .FirstOrDefaultAsync(o =>
                        o.Id == ordenCompraId);

            if (orden == null)
                return null;

            if (orden.Estado?.Equals(
                    "Anulada",
                    StringComparison.OrdinalIgnoreCase) == true)
            {
                throw new InvalidOperationException(
                    "No se puede registrar una recepción para una orden de compra anulada.");
            }

            var recepcionesAnteriores =
                await _context.RecepcionesMercanciasNoFormalizadas
                    .Where(r =>
                        r.OrdenCompraNoFormalizadaId ==
                        ordenCompraId)
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
                            d.CantidadKg -
                            cantidadRecibida);

                    var bultosPendientes =
                        Math.Max(
                            0,
                            d.Bultos -
                            bultosRecibidos);

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

        // CREAR RECEPCIÓN

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

            var estadosNoRecepcionables =
                new[]
                {
                    "Anulada",
                    "Recepcionada",
                    "Confirmada"
                };

            if (estadosNoRecepcionables.Contains(
                    orden.Estado ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"No se puede registrar una recepción para una orden en estado '{orden.Estado}'.");
            }

            var recepcion =
                await _context.RecepcionesMercanciasNoFormalizadas
                    .Include(r => r.Detalles)
                    .FirstOrDefaultAsync(r =>
                        r.OrdenCompraNoFormalizadaId ==
                            dto.OrdenCompraNoFormalizadaId &&
                        r.FechaConfirmacion == null);

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
                            DateTime.Now,

                        NumeroUltimaEntrega = 0
                    };

                _context.RecepcionesMercanciasNoFormalizadas
                    .Add(recepcion);
            }
            else
            {
                recepcion.Conductor =
                    dto.Conductor;

                recepcion.Transportadora =
                    dto.Transportadora;

                recepcion.TipoDocumento =
                    dto.TipoDocumento;

                recepcion.EmbalajeAdecuado =
                    dto.EmbalajeAdecuado;

                recepcion.Recibe =
                    dto.Recibe;

                recepcion.Cargo =
                    dto.Cargo;

                recepcion.Observaciones =
                    dto.Observaciones;
            }

            var numeroEntrega =
                recepcion.NumeroUltimaEntrega + 1;

            var fechaEntrega =
                DateTime.Now;

            var recepcionesAnteriores =
                await _context.RecepcionesMercanciasNoFormalizadas
                    .Where(r =>
                        r.OrdenCompraNoFormalizadaId ==
                        dto.OrdenCompraNoFormalizadaId)
                    .SelectMany(r => r.Detalles)
                    .ToListAsync();

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
                        $"no pertenece a la orden de compra.");
                }

                if (item.CantidadRecibida < 0)
                {
                    throw new Exception(
                        $"La cantidad recibida de " +
                        $"{detalleOrden.MaterialNoFormalizado?.NombreMaterial} " +
                        $"no puede ser negativa.");
                }

                if (item.BultosRecibidos < 0)
                {
                    throw new Exception(
                        $"Los bultos recibidos de " +
                        $"{detalleOrden.MaterialNoFormalizado?.NombreMaterial} " +
                        $"no pueden ser negativos.");
                }

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
                        $"Pendiente: {cantidadPendiente} kg.");
                }

                if (bultosNuevos > bultosPendientes)
                {
                    throw new Exception(
                        $"Los bultos recibidos de " +
                        $"{detalleOrden.MaterialNoFormalizado?.NombreMaterial} " +
                        $"superan los bultos pendientes. " +
                        $"Pendientes: {bultosPendientes}.");
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
                            item.Observaciones,

                        NumeroEntrega =
                            numeroEntrega,

                        FechaEntrega =
                            fechaEntrega,

                        ProcesadoInventario =
                            false
                    };

                recepcion.Detalles.Add(detalle);
            }

            if (!agregoDetalle)
            {
                throw new Exception(
                    "Debe ingresar al menos una cantidad o bulto recibido.");
            }

            recepcion.NumeroUltimaEntrega =
                numeroEntrega;

            await _context.SaveChangesAsync();

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

            orden.Estado =
                ordenCompleta
                    ? "Recepcionada"
                    : "Parcial";

            await _context.SaveChangesAsync();

            await _notificacion
                .EnviarRecepcionMercanciaNoFormalizadaAsync(
                    recepcion.Id,
                    dto.Usuarios);

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

        // CONFIRMAR RECEPCIÓN

        public async Task ConfirmarRecepcionAsync(int id)
        {
            var recepcion = await _context.RecepcionesMercanciasNoFormalizadas
                .Include(r => r.OrdenCompraNoFormalizada)
                    .ThenInclude(o => o.Detalles)
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recepcion == null)
                throw new Exception("La recepción no existe.");

            if (recepcion.OrdenCompraNoFormalizada == null)
                throw new Exception("La orden de compra no existe.");

            // Buscar únicamente los detalles de esta recepción
            // que todavía no han sido procesados en inventario.
            var detallesPendientes = recepcion.Detalles
                .Where(d => !d.ProcesadoInventario)
                .ToList();

            if (!detallesPendientes.Any())
            {
                throw new Exception(
                    "No existen materiales pendientes por ingresar al inventario."
                );
            }

            // Procesar únicamente los detalles pendientes
            // de esta recepción.
            await _inventarioService.ProcesarRecepcionAsync(id);

            // Consultar nuevamente todos los detalles recibidos
            // para esta orden.
            var detallesRecibidos =
                await _context.RecepcionesMercanciasNoFormalizadas
                    .Where(r =>
                        r.OrdenCompraNoFormalizadaId ==
                        recepcion.OrdenCompraNoFormalizadaId)
                    .SelectMany(r => r.Detalles)
                    .ToListAsync();

            // Verificar si la orden ya quedó completamente recibida.
            var ordenCompleta =
                recepcion.OrdenCompraNoFormalizada.Detalles.All(d =>
                {
                    var kgRecibidos = detallesRecibidos
                        .Where(x =>
                            x.OrdenCompraDetalleNoFormalizadaId == d.Id)
                        .Sum(x => x.CantidadRecibida);

                    var bultosRecibidos = detallesRecibidos
                        .Where(x =>
                            x.OrdenCompraDetalleNoFormalizadaId == d.Id)
                        .Sum(x => x.BultosRecibidos);

                    return kgRecibidos >= d.CantidadKg &&
                           bultosRecibidos >= d.Bultos;
                });

            if (ordenCompleta)
            {
                // Ya llegó toda la mercancía y la recepción
                // pendiente fue procesada en inventario.
                recepcion.OrdenCompraNoFormalizada.Estado = "Confirmada";

                recepcion.FechaConfirmacion = DateTime.Now;

                recepcion.UsuarioConfirmacionId =
                    _currentUser.IdUsuario!.Value;
            }
            else
            {
                // La mercancía recibida ya entró a inventario,
                // pero todavía falta mercancía por recibir.
                recepcion.OrdenCompraNoFormalizada.Estado = "Parcial";
            }

            await _context.SaveChangesAsync();
        }

        //ACTUALIZAR

        public async Task<bool> ActualizarAsync(
            int id,
            ActualizarRecepcionMercanciaNoFormalizadaDto dto)
        {
            var recepcion =
                await _context.RecepcionesMercanciasNoFormalizadas
                    .Include(r => r.Detalles)
                    .FirstOrDefaultAsync(r => r.Id == id);

            if (recepcion == null)
                return false;

            if (recepcion.FechaConfirmacion.HasValue)
            {
                throw new InvalidOperationException(
                    "No se puede modificar una recepción ya confirmada.");
            }

            if (recepcion.Detalles.Any(d =>
                    d.ProcesadoInventario))
            {
                throw new InvalidOperationException(
                    "No se puede modificar una recepción cuyos materiales ya ingresaron al inventario.");
            }

            recepcion.Conductor =
                dto.Conductor;

            recepcion.Transportadora =
                dto.Transportadora;

            recepcion.TipoDocumento =
                dto.TipoDocumento;

            recepcion.EmbalajeAdecuado =
                dto.EmbalajeAdecuado;

            recepcion.Recibe =
                dto.Recibe;

            recepcion.Cargo =
                dto.Cargo;

            recepcion.Observaciones =
                dto.Observaciones;

            await _context.SaveChangesAsync();

            return true;
        }

        // ELIMINAR
        public async Task<bool> EliminarAsync(int id)
        {
            var recepcion =
                await _context.RecepcionesMercanciasNoFormalizadas
                    .Include(r => r.Detalles)
                    .FirstOrDefaultAsync(r => r.Id == id);

            if (recepcion == null)
                return false;

            if (recepcion.FechaConfirmacion.HasValue)
            {
                throw new InvalidOperationException(
                    "No se puede eliminar una recepción ya confirmada.");
            }

            if (recepcion.Detalles.Any(d =>
                    d.ProcesadoInventario))
            {
                throw new InvalidOperationException(
                    "No se puede eliminar una recepción cuyos materiales ya ingresaron al inventario.");
            }

            _context.RecepcionesMercanciasNoFormalizadas
                .Remove(recepcion);

            await _context.SaveChangesAsync();

            return true;
        }

        // FILTROS

        public async Task<FiltrosRecepcionMercanciaNoFormalizadaDto>
            ObtenerFiltrosAsync()
        {
            var proveedores =
                await _context.ProveedoresNoFormalizados
                    .AsNoTracking()
                    .Where(p => p.Activo)
                    .OrderBy(p => p.Nombre)
                    .Select(p => new
                    {
                        Id = p.IdProveedorNoFormalizado,
                        Nombre = p.Nombre
                    })
                    .ToListAsync();

            return new FiltrosRecepcionMercanciaNoFormalizadaDto
            {
                Proveedores = proveedores
                    .Select(p => new FiltroProveedorNoFormalizadoDto
                    {
                        Id = p.Id,
                        Nombre = p.Nombre
                    })
                    .ToList()
            };
        }
    }
}