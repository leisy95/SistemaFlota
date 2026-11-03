using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.Costos.RecepcionMercancia;
using SistemaFlota.Migrations;
using SistemaFlota.Models.Costos.RecepcionMercancias;
using SistemaFlota.Services.Auditorias.Base;
using SistemaFlota.Services.Auth;
using SistemaFlota.Services.Consecutivos;
using SistemaFlota.Services.Costos.Inventario;
using SistemaFlota.Services.Notificaciones;

namespace SistemaFlota.Services.Costos.RecepcionMercancia
{
    public class RecepcionMercanciaService : BaseService, IRecepcionMercanciaService
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
            IInventarioService inventarioService,
            SistemaFlota.AuditoriaService auditoria)
            : base(auditoria, currentUser)
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
                    EstadoMaterial = x.EstadoMaterial,
                    NumeroEntrega = x.NumeroEntrega,
                    FechaEntrega = x.FechaEntrega,
                    ProcesadoInventario = x.ProcesadoInventario

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

            if (orden.Estado?.Equals("Anulada", StringComparison.OrdinalIgnoreCase) == true)
                throw new InvalidOperationException(
                    "No se puede registrar una recepción para una orden de compra anulada.");

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

            // Consultar el usuario autenticado para obtener su cargo.
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == _currentUser.IdUsuario);

            return new RecepcionFormularioDto
            {
                OrdenCompraId = orden.Id,
                NumeroOrden = orden.Numero,
                Proveedor = orden.Proveedor?.Nombre ?? string.Empty,
                FechaOrden = orden.FechaOrden,
                Recibe = _currentUser.Nombre ?? string.Empty,
                Cargo = usuario?.Cargo ?? string.Empty,
                Items = items
            };
        }

        public async Task<RecepcionMercanciaDto> CrearAsync(CrearRecepcionMercanciaDto dto)
        {
            var usuarioId = _currentUser.IdUsuario;
            var nombreUsuario = _currentUser.Nombre;

            if (usuarioId == null)
            {
                throw new UnauthorizedAccessException(
                    "No se pudo identificar al usuario autenticado.");
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == usuarioId.Value);

            var cargoUsuario = usuario?.Cargo ?? string.Empty;

            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new UnauthorizedAccessException(
                    "El usuario autenticado no tiene un nombre configurado.");
            }

            if (dto.Detalles == null || !dto.Detalles.Any())
            {
                throw new InvalidOperationException(
                    "Debe ingresar al menos un lote para registrar la recepción.");
            }

            var orden = await _context.OrdenesCompra
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Material)
                .Include(o => o.Proveedor)
                .FirstOrDefaultAsync(o => o.Id == dto.OrdenCompraId);

            if (orden == null)
                throw new Exception("La orden de compra no existe.");

            var estadosNoRecepcionables = new[]
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

            // Una sola recepción por orden de compra.
            var recepcion = await _context.RecepcionesMercancias
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r => r.OrdenCompraId == dto.OrdenCompraId);

            // Crear la recepción si todavía no existe.
            if (recepcion == null)
            {
                var numeroRecepcion = await _consecutivoService
                    .GenerarAsync("RecepcionMercancia");

                recepcion = new Models.Costos.RecepcionMercancias.RecepcionMercancia
                {
                    OrdenCompraId = dto.OrdenCompraId,
                    NumeroRecepcion = numeroRecepcion,
                    Conductor = dto.Conductor,
                    Transportadora = dto.Transportadora,
                    TipoDocumento = dto.TipoDocumento,
                    EmbalajeAdecuado = dto.EmbalajeAdecuado,
                    Recibe = nombreUsuario,
                    Cargo = cargoUsuario,
                    Observaciones = dto.Observaciones,
                    FechaRecepcion = DateTime.Now,
                    NumeroUltimaEntrega = 0
                };

                _context.RecepcionesMercancias.Add(recepcion);
            }
            else
            {
                // Actualizar información general de la recepción.
                recepcion.Conductor = dto.Conductor;
                recepcion.Transportadora = dto.Transportadora;
                recepcion.TipoDocumento = dto.TipoDocumento;
                recepcion.EmbalajeAdecuado = dto.EmbalajeAdecuado;
                recepcion.Recibe = nombreUsuario;
                recepcion.Cargo = cargoUsuario;
                recepcion.Observaciones = dto.Observaciones;
            }

            // Número y fecha de esta entrega física.
            var numeroEntrega = recepcion.NumeroUltimaEntrega + 1;
            var fechaEntrega = DateTime.Now;

            // Detalles recibidos anteriormente para esta orden.
            var recepcionesAnteriores = await _context.RecepcionesMercancias
                .Where(r => r.OrdenCompraId == dto.OrdenCompraId)
                .SelectMany(r => r.Detalles)
                .ToListAsync();

            // IDs de los detalles que pertenecen a esta orden.
            var detallesOrdenIds = orden.Detalles
                .Select(d => d.Id)
                .ToHashSet();

            // Validar cada lote individualmente.
            foreach (var item in dto.Detalles)
            {
                if (!detallesOrdenIds.Contains(item.OrdenCompraDetalleId))
                {
                    throw new InvalidOperationException(
                        $"El detalle {item.OrdenCompraDetalleId} no pertenece a la orden de compra.");
                }

                var detalleOrden = orden.Detalles
                    .First(d => d.Id == item.OrdenCompraDetalleId);

                if (item.CantidadRecibida <= 0)
                {
                    throw new InvalidOperationException(
                        $"La cantidad recibida de {detalleOrden.Material?.NombreMaterial} debe ser mayor a cero.");
                }

                if (item.BultosRecibidos <= 0)
                {
                    throw new InvalidOperationException(
                        $"Los bultos recibidos de {detalleOrden.Material?.NombreMaterial} deben ser mayores a cero.");
                }

                // Los bultos deben ser números enteros.
                if (item.BultosRecibidos != decimal.Truncate(item.BultosRecibidos))
                {
                    throw new InvalidOperationException(
                        $"Los bultos recibidos de {detalleOrden.Material?.NombreMaterial} deben ser un número entero.");
                }

                if (string.IsNullOrWhiteSpace(item.LoteProveedor))
                {
                    throw new InvalidOperationException(
                        $"Debe ingresar el lote del proveedor para {detalleOrden.Material?.NombreMaterial}.");
                }

                if (string.IsNullOrWhiteSpace(item.EstadoMaterial))
                {
                    throw new InvalidOperationException(
                        $"Debe seleccionar el estado del material {detalleOrden.Material?.NombreMaterial}.");
                }
            }

            // Agrupar los lotes de esta entrega por detalle de orden.
            // Así se valida el total de KG y bultos de todos los lotes del material.
            var cantidadesNuevaEntrega = dto.Detalles
                .GroupBy(x => x.OrdenCompraDetalleId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.CantidadRecibida)
                );

            var bultosNuevaEntrega = dto.Detalles
                .GroupBy(x => x.OrdenCompraDetalleId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.BultosRecibidos)
                );

            // Validar los acumulados contra lo pendiente de la orden.
            foreach (var grupo in cantidadesNuevaEntrega)
            {
                var detalleOrden = orden.Detalles
                    .First(d => d.Id == grupo.Key);

                var cantidadRecibidaAnterior = recepcionesAnteriores
                    .Where(r => r.OrdenCompraDetalleId == grupo.Key)
                    .Sum(r => r.CantidadRecibida);

                var bultosRecibidosAnterior = recepcionesAnteriores
                    .Where(r => r.OrdenCompraDetalleId == grupo.Key)
                    .Sum(r => r.BultosRecibidos);

                var cantidadPendiente = Math.Max(
                    0,
                    detalleOrden.CantidadKg - cantidadRecibidaAnterior
                );

                var bultosPendientes = Math.Max(
                    0,
                    detalleOrden.Bultos - bultosRecibidosAnterior
                );

                var cantidadNueva = grupo.Value;
                var bultosNuevos = bultosNuevaEntrega[grupo.Key];

                if (cantidadNueva > cantidadPendiente)
                {
                    throw new InvalidOperationException(
                        $"La suma de los lotes de {detalleOrden.Material?.NombreMaterial} " +
                        $"supera los KG pendientes. Pendiente: {cantidadPendiente:N2} kg. " +
                        $"Ingresado: {cantidadNueva:N2} kg."
                    );
                }

                if (bultosNuevos > bultosPendientes)
                {
                    throw new InvalidOperationException(
                        $"La suma de los lotes de {detalleOrden.Material?.NombreMaterial} " +
                        $"supera los bultos pendientes. Pendientes: {bultosPendientes:N0}. " +
                        $"Ingresados: {bultosNuevos:N0}."
                    );
                }
            }

            // Crear un registro independiente por cada lote.
            foreach (var item in dto.Detalles)
            {
                var detalle = new RecepcionMercanciaDetalle
                {
                    OrdenCompraDetalleId = item.OrdenCompraDetalleId,
                    CantidadRecibida = item.CantidadRecibida,
                    BultosRecibidos = item.BultosRecibidos,
                    LoteProveedor = item.LoteProveedor.Trim(),
                    EstadoMaterial = item.EstadoMaterial.Trim(),
                    Observaciones = item.Observaciones,
                    NumeroEntrega = numeroEntrega,
                    FechaEntrega = fechaEntrega,
                    ProcesadoInventario = false
                };

                recepcion.Detalles.Add(detalle);
            }

            // Cantidad de lotes creados en esta entrega.
            var cantidadDetallesNuevos = dto.Detalles.Count;

            // Guardar el número de esta entrega.
            recepcion.NumeroUltimaEntrega = numeroEntrega;

            await _context.SaveChangesAsync();

            // Obtener todos los detalles recibidos de la orden.
            var todosLosDetallesRecibidos = await _context.RecepcionesMercancias
                .Where(r => r.OrdenCompraId == dto.OrdenCompraId)
                .SelectMany(r => r.Detalles)
                .ToListAsync();

            // Determinar si ya llegó toda la orden.
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

            await AuditarAsync(
                "Crear",
                "RecepcionMercancia",
                $"Se registró la recepción {recepcion.NumeroRecepcion} " +
                $"para la orden de compra {orden.Numero}. " +
                $"Proveedor: {orden.Proveedor?.Nombre ?? "No especificado"}. " +
                $"Entrega número: {numeroEntrega}. " +
                $"Cantidad de lotes recibidos en esta entrega: {cantidadDetallesNuevos}. " +
                $"Estado de la orden: {orden.Estado}.",
                recepcion.Id);

            await _notificacion.EnviarRecepcionMercanciaAsync(
                recepcion.Id,
                dto.Usuarios
            );

            // Retornar los totales de toda la recepción.
            return new RecepcionMercanciaDto
            {
                Id = recepcion.Id,
                ConsecutivoEntrada = recepcion.NumeroRecepcion,
                OrdenCompraId = recepcion.OrdenCompraId,
                NumeroOrden = orden.Numero,
                Proveedor = orden.Proveedor?.Nombre ?? string.Empty,
                FechaRecepcion = recepcion.FechaRecepcion,
                Conductor = recepcion.Conductor,
                Transportadora = recepcion.Transportadora,
                EmbalajeAdecuado = recepcion.EmbalajeAdecuado,
                TotalKg = todosLosDetallesRecibidos.Sum(x => x.CantidadRecibida),
                TotalBultos = todosLosDetallesRecibidos.Sum(x => x.BultosRecibidos)
            };
        }

        public async Task ConfirmarRecepcionAsync(int id)
        {
            var recepcion = await _context.RecepcionesMercancias
                .Include(r => r.OrdenCompra)
                    .ThenInclude(o => o.Detalles)
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recepcion == null)
                throw new Exception("La recepción no existe.");

            if (recepcion.OrdenCompra == null)
                throw new Exception("La orden de compra no existe.");

            // Buscar únicamente los detalles de esta recepción
            // que todavía no han sido procesados en inventario.
            var detallesPendientes = recepcion.Detalles
                .Where(d => !d.ProcesadoInventario)
                .ToList();

            if (!detallesPendientes.Any())
                throw new Exception(
                    "No existen materiales pendientes por ingresar al inventario."
                );

            // Procesar solamente los detalles nuevos de esta entrega.
            await _inventarioService.ProcesarRecepcionAsync(id);

            // Consultar nuevamente todos los detalles recibidos de la orden.
            var detallesRecibidos = await _context.RecepcionesMercancias
                .Where(r => r.OrdenCompraId == recepcion.OrdenCompraId)
                .SelectMany(r => r.Detalles)
                .ToListAsync();

            // Verificar si la orden ya quedó completamente recibida.
            var ordenCompleta = recepcion.OrdenCompra.Detalles.All(d =>
            {
                var kgRecibidos = detallesRecibidos
                    .Where(x => x.OrdenCompraDetalleId == d.Id)
                    .Sum(x => x.CantidadRecibida);

                var bultosRecibidos = detallesRecibidos
                    .Where(x => x.OrdenCompraDetalleId == d.Id)
                    .Sum(x => x.BultosRecibidos);

                return kgRecibidos >= d.CantidadKg &&
                       bultosRecibidos >= d.Bultos;
            });

            if (ordenCompleta)
            {
                // Ya llegó toda la mercancía y toda la recepción
                // pendiente fue procesada en inventario.
                recepcion.OrdenCompra.Estado = "Confirmada";
                recepcion.FechaConfirmacion = DateTime.Now;
                recepcion.UsuarioConfirmacionId = _currentUser.IdUsuario!.Value;
            }
            else
            {
                // La entrega actual ya entró a inventario,
                // pero todavía faltan materiales por recibir.
                recepcion.OrdenCompra.Estado = "Parcial";
            }

            await _context.SaveChangesAsync();

            var totalKgProcesados = detallesPendientes
                .Sum(d => d.CantidadRecibida);

            var totalBultosProcesados = detallesPendientes
                .Sum(d => d.BultosRecibidos);

            await AuditarAsync(
                "Confirmar",
                "RecepcionMercancia",
                $"Se confirmó el ingreso al inventario de la recepción " +
                $"{recepcion.NumeroRecepcion}, correspondiente a la orden " +
                $"{recepcion.OrdenCompra.Numero}. " +
                $"Se ingresaron {totalKgProcesados:N2} kg distribuidos en " +
                $"{detallesPendientes.Count} " +
                $"{(detallesPendientes.Count == 1 ? "detalle" : "detalles")}, " +
                $"correspondientes a {totalBultosProcesados:N2} bultos. " +
                $"Estado final de la orden: {recepcion.OrdenCompra.Estado}.",
                recepcion.Id);
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