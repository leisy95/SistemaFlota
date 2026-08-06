using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.Costos.OrdenCompra;
using SistemaFlota.Migrations;
using SistemaFlota.Models.Costos.OrdenesCompras;
using SistemaFlota.Services.Auth;
using SistemaFlota.Services.Consecutivos;

namespace SistemaFlota.Services.Costos.OrdenCompra
{
    public class OrdenCompraService : IOrdenCompraService
    {
        private readonly AppDbContext _context;
        private readonly IConsecutivoService _consecutivoService;
        private readonly ICurrentUserService _currentUser;

        public OrdenCompraService(
            AppDbContext context,
            IConsecutivoService consecutivoService,
            ICurrentUserService currentUser)
        {
            _context = context;
            _consecutivoService = consecutivoService;
            _currentUser = currentUser;
        }

        private async Task ValidarProveedorAsync(int proveedorId)
        {
            var existe = await _context.Proveedores
                .AnyAsync(x => x.IdProveedor == proveedorId);

            if (!existe)
                throw new Exception("El proveedor seleccionado no existe.");
        }

        private async Task ValidarMaterialesAsync(List<CrearOrdenCompraDetalleDto> detalles)
        {
            var ids = detalles
                .Select(x => x.MaterialId)
                .Distinct()
                .ToList();

            var existentes = await _context.Materiales
                .Where(x => ids.Contains(x.IdMaterial))
                .Select(x => x.IdMaterial)
                .ToListAsync();

            var faltantes = ids.Except(existentes).ToList();

            if (faltantes.Any())
            {
                throw new Exception(
                    $"No existen los materiales: {string.Join(", ", faltantes)}");
            }
        }

        private (
            decimal totalKg,
            decimal totalBultos,
            decimal subtotal,
            decimal valorImpuesto,
            decimal totalPagar)
        CalcularTotales(
            List<CrearOrdenCompraDetalleDto> detalles,
            decimal porcentajeImpuesto)
        {
            decimal totalKg = 0;
            decimal totalBultos = 0;
            decimal subtotal = 0;

            foreach (var item in detalles)
            {
                var bultos = item.KgPorBulto > 0
                    ? item.CantidadKg / item.KgPorBulto
                    : 0;

                var subtotalItem = item.CantidadKg * item.CostoKg;

                totalKg += item.CantidadKg;
                totalBultos += bultos;
                subtotal += subtotalItem;
            }

            var valorImpuesto = subtotal * (porcentajeImpuesto / 100m);

            var totalPagar = subtotal + valorImpuesto;

            return (
                totalKg,
                totalBultos,
                subtotal,
                valorImpuesto,
                totalPagar
            );
        }

        public async Task<OrdenCompraPaginadoDto> ObtenerAsync(
             string? search,
             string? estado,
             int? proveedorId,
             string? formaPago,
             DateTime? fechaInicio,
             DateTime? fechaFin,
             int page,
             int pageSize)
                {
            var query = _context.OrdenesCompra
                .AsNoTracking()
                .Include(x => x.Proveedor)
                .Include(x => x.UsuarioCreacion)
                 .Include(x => x.RecepcionMercancia)
                .Include(x => x.UsuarioActualizacion)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Numero.Contains(search) ||
                    x.Proveedor.Nombre.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(x => x.Estado == estado);
            }

            if (proveedorId.HasValue)
            {
                query = query.Where(x => x.ProveedorId == proveedorId);
            }

            if (!string.IsNullOrWhiteSpace(formaPago))
            {
                query = query.Where(x => x.FormaPago == formaPago);
            }

            if (fechaInicio.HasValue)
            {
                query = query.Where(x => x.FechaOrden >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                query = query.Where(x => x.FechaOrden <= fechaFin.Value);
            }

            var totalRegistros = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.FechaCreacion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OrdenCompraDto
                {
                    Id = x.Id,
                    Numero = x.Numero,
                    ProveedorId = x.ProveedorId,
                    Proveedor = x.Proveedor.Nombre,
                    FechaOrden = x.FechaOrden,
                    FechaEntrega = x.FechaEntrega,
                    FormaPago = x.FormaPago,
                    LugarEntrega = x.LugarEntrega,
                    TotalItems = x.TotalItems,
                    TotalKg = x.TotalKg,
                    TotalBultos = x.TotalBultos,
                    TotalPagar = x.TotalPagar,
                    Estado = x.Estado,
                    RecepcionId = x.RecepcionMercancia != null
                        ? x.RecepcionMercancia.Id
                        : null,
                    Observaciones = x.Observaciones,

                    UsuarioCreacion = x.UsuarioCreacion != null
                        ? x.UsuarioCreacion.Username
                        : "",

                    FechaCreacion = x.FechaCreacion,

                    UsuarioActualizacion = x.UsuarioActualizacion != null
                        ? x.UsuarioActualizacion.Username
                        : "",

                    FechaActualizacion = x.FechaActualizacion,
                })
                .ToListAsync();

            return new OrdenCompraPaginadoDto
            {
                Items = items,
                Total = totalRegistros,
                Pagina = page,
                PageSize = pageSize
            };
        }

        public async Task<OrdenCompraDto?> ObtenerPorIdAsync(int id)
        {
            return await _context.OrdenesCompra
                .AsNoTracking()
                .Include(x => x.Proveedor)
                .Include(x => x.Detalles)
                    .ThenInclude(d => d.Material)
                .Where(x => x.Id == id)
                .Select(x => new OrdenCompraDto
                {
                    Id = x.Id,
                    Numero = x.Numero,
                    ProveedorId = x.ProveedorId,
                    Proveedor = x.Proveedor.Nombre,
                    FechaOrden = x.FechaOrden,
                    FechaEntrega = x.FechaEntrega,
                    FormaPago = x.FormaPago,
                    LugarEntrega = x.LugarEntrega,
                    TotalItems = x.TotalItems,
                    TotalKg = x.TotalKg,

                    Subtotal = x.Subtotal,
                    TipoImpuesto = x.TipoImpuesto,
                    PorcentajeImpuesto = x.PorcentajeImpuesto,
                    ValorImpuesto = x.ValorImpuesto,

                    TotalBultos = x.TotalBultos,
                    TotalPagar = x.TotalPagar,
                    Estado = x.Estado,
                    Observaciones = x.Observaciones,

                    Detalles = x.Detalles.Select(d => new OrdenCompraDetalleDto
                    {
                        Id = d.Id,
                        MaterialId = d.MaterialId,
                        Material = d.Material != null ? d.Material.DescripcionCompra ?? "" : "",
                        Color = d.Color,
                        CantidadKg = d.CantidadKg,
                        KgPorBulto = d.KgPorBulto,
                        Bultos = d.Bultos,
                        CostoKg = d.CostoKg,
                        Subtotal = d.Subtotal
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }


        // Crear Una orden de compra
        public async Task<OrdenCompraDto> CrearAsync(CrearOrdenCompraDto dto)
        {
            await ValidarProveedorAsync(dto.ProveedorId);

            await ValidarMaterialesAsync(dto.Detalles);

            var (
                 totalKg,
                 totalBultos,
                 subtotal,
                 valorImpuesto,
                 totalPagar
             ) = CalcularTotales(
                 dto.Detalles,
                 dto.PorcentajeImpuesto);

            var strategy = _context.Database.CreateExecutionStrategy();


            return await strategy.ExecuteAsync(async () =>
            {

                await using var transaction =
                    await _context.Database.BeginTransactionAsync();


                try
                {

                    var numero = await _consecutivoService
                        .GenerarAsync("OrdenCompra");

                    var idUsuario = _currentUser.IdUsuario;

                    if (idUsuario == null)
                    {
                        throw new UnauthorizedAccessException("No se pudo identificar el usuario.");
                    }


                    var orden = new Models.Costos.OrdenesCompras.OrdenCompra
                    {
                        Numero = numero,
                        ProveedorId = dto.ProveedorId,
                        FechaOrden = dto.FechaOrden,
                        FechaEntrega = dto.FechaEntrega,
                        FormaPago = dto.FormaPago,
                        LugarEntrega = dto.LugarEntrega,
                        Observaciones = dto.Observaciones,
                        Estado = "Pendiente",
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        UsuarioCreacionId = _currentUser.IdUsuario!.Value,

                        TotalItems = dto.Detalles.Count,
                        TotalKg = totalKg,
                        TotalBultos = totalBultos,

                        Subtotal = subtotal,

                        TipoImpuesto = dto.TipoImpuesto,
                        PorcentajeImpuesto = dto.PorcentajeImpuesto,
                        ValorImpuesto = valorImpuesto,

                        TotalPagar = totalPagar,
                    };


                    foreach (var item in dto.Detalles)
                    {
                        orden.Detalles.Add(new OrdenCompraDetalle
                        {
                            MaterialId = item.MaterialId,
                            Color = item.Color,
                            CantidadKg = item.CantidadKg,
                            KgPorBulto = item.KgPorBulto,
                            Bultos = item.CantidadKg / item.KgPorBulto,
                            CostoKg = item.CostoKg,
                            Subtotal = item.CantidadKg * item.CostoKg
                        });
                    }


                    _context.OrdenesCompra.Add(orden);

                    await _context.SaveChangesAsync();


                    await transaction.CommitAsync();


                    return new OrdenCompraDto
                    {
                        Id = orden.Id,
                        Numero = orden.Numero,
                        ProveedorId = orden.ProveedorId,
                        TotalKg = orden.TotalKg,
                        TotalBultos = orden.TotalBultos,

                        FormaPago = orden.FormaPago,
                        LugarEntrega = orden.LugarEntrega,

                        Subtotal = orden.Subtotal,

                        TipoImpuesto = orden.TipoImpuesto,
                        PorcentajeImpuesto = orden.PorcentajeImpuesto,
                        ValorImpuesto = orden.ValorImpuesto,

                        TotalPagar = orden.TotalPagar,
                        Estado = orden.Estado
                    };

                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }

            });
        }
        public async Task<bool> ActualizarAsync(int id, ActualizarOrdenCompraDto dto)
        {
            await ValidarProveedorAsync(dto.ProveedorId);

            await ValidarMaterialesAsync(dto.Detalles);

            var orden = await _context.OrdenesCompra
                .Include(x => x.Detalles)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (orden == null)
                throw new Exception("La orden de compra no existe.");

            var idUsuario = _currentUser.IdUsuario;

            if (idUsuario == null)
            {
                throw new UnauthorizedAccessException("No se pudo identificar el usuario.");
            }

            var (
                totalKg,
                totalBultos,
                subtotal,
                valorImpuesto,
                totalPagar
            ) = CalcularTotales(
                dto.Detalles,
                dto.PorcentajeImpuesto);

            orden.ProveedorId = dto.ProveedorId;
            orden.FechaOrden = dto.FechaOrden;
            orden.FechaEntrega = dto.FechaEntrega;
            orden.FormaPago = dto.FormaPago;

            orden.Observaciones = dto.Observaciones;
            orden.Estado = dto.Estado;

            orden.TotalItems = dto.Detalles.Count;
            orden.TotalKg = totalKg;
            orden.TotalBultos = totalBultos;

            orden.UsuarioActualizacionId = idUsuario.Value;

            orden.Subtotal = subtotal;
            orden.TipoImpuesto = dto.TipoImpuesto;
            orden.PorcentajeImpuesto = dto.PorcentajeImpuesto;
            orden.ValorImpuesto = valorImpuesto;
            orden.TotalPagar = totalPagar;

            _context.OrdenesCompraDetalle.RemoveRange(orden.Detalles);

            foreach (var item in dto.Detalles)
            {
                orden.Detalles.Add(new OrdenCompraDetalle
                {
                    MaterialId = item.MaterialId,
                    Color = item.Color,
                    CantidadKg = item.CantidadKg,
                    KgPorBulto = item.KgPorBulto,
                    Bultos = item.KgPorBulto > 0
                        ? item.CantidadKg / item.KgPorBulto
                        : 0,
                    CostoKg = item.CostoKg,
                    Subtotal = item.CantidadKg * item.CostoKg
                });
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<FiltrosOrdenCompraDto> ObtenerFiltrosAsync()
        {
            throw new NotImplementedException();
        }
    }
}