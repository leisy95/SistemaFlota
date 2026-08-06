using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.Costos.RecepcionMercancia;
using SistemaFlota.Migrations;
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
            var existeRecepcion = await _context.RecepcionesMercancias
                .AnyAsync(x => x.OrdenCompraId == ordenCompraId);

            if (existeRecepcion)
                throw new Exception("Esta orden de compra ya tiene una recepción registrada.");

            var orden = await _context.OrdenesCompra
                .Include(o => o.Proveedor)
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Material)
                .FirstOrDefaultAsync(o => o.Id == ordenCompraId);

            if (orden == null)
                return null;


            return new RecepcionFormularioDto
            {
                OrdenCompraId = orden.Id,
                NumeroOrden = orden.Numero,
                Proveedor = orden.Proveedor?.Nombre ?? string.Empty,
                FechaOrden = orden.FechaOrden,
                Recibe = _currentUser.Usuario,
                Cargo = _currentUser.Rol,

                Items = orden.Detalles
                    .Select(d => new RecepcionMercanciaDetalleFormularioDto
                    {
                        OrdenCompraDetalleId = d.Id,
                        MaterialId = d.MaterialId,
                        Material = d.Material?.NombreMaterial ?? string.Empty,
                        Cantidad = d.CantidadKg,
                        Bultos = d.Bultos
                    })
                    .ToList()
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

            var existeRecepcion = await _context.RecepcionesMercancias
                .AnyAsync(x => x.OrdenCompraId == dto.OrdenCompraId);

            if (existeRecepcion)
                throw new Exception("Esta orden de compra ya fue recepcionada.");

            var numeroRecepcion = await _consecutivoService
                .GenerarAsync("RecepcionMercancia");

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

            orden.Estado = "Recepcionada";

            _context.RecepcionesMercancias.Add(recepcion);

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

                if (recepcion.OrdenCompra!.Estado != "Recepcionada")
                    throw new Exception("Solo se pueden confirmar recepciones en estado Recepcionada.");

                recepcion.OrdenCompra.Estado = "Confirmada";
                recepcion.FechaConfirmacion = DateTime.Now;
                recepcion.UsuarioConfirmacionId = _currentUser.IdUsuario!.Value;

                await _inventarioService.ProcesarRecepcionAsync(id);

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