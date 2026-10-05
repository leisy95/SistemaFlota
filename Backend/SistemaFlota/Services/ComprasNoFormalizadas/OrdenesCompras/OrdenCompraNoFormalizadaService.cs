using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesCompras;
using SistemaFlota.Models.ComprasNoFormalizadas.OrdenesCompras;
using SistemaFlota.Services.Auth;
using SistemaFlota.Services.Consecutivos;
using SistemaFlota.Services.Costos.OrdenCompra;
using SistemaFlota.Services.Email;

namespace SistemaFlota.Services.ComprasNoFormalizadas.OrdenesCompras
{
    public class OrdenCompraNoFormalizadaService : IOrdenCompraNoFormalizadaService
    {
        private readonly AppDbContext _context;
        private readonly IConsecutivoService _consecutivoService;
        private readonly ICurrentUserService _currentUser;
        private readonly IEmailService _emailService;
        private readonly IOrdenCompraPdfService _ordenCompraPdfService;
        private readonly EmailTemplateService _emailTemplateService;

        public OrdenCompraNoFormalizadaService(
            AppDbContext context,
            IConsecutivoService consecutivoService,
            ICurrentUserService currentUser,
            IEmailService emailService,
            IOrdenCompraPdfService ordenCompraPdfService,
            EmailTemplateService emailTemplateService)
        {
            _context = context;
            _consecutivoService = consecutivoService;
            _currentUser = currentUser;
            _emailService = emailService;
            _ordenCompraPdfService = ordenCompraPdfService;
            _emailTemplateService = emailTemplateService;
        }

        private async Task ValidarProveedorAsync(int proveedorNoFormalizadoId)
        {
            var existe = await _context.ProveedoresNoFormalizados
                .AnyAsync(x => x.IdProveedorNoFormalizado == proveedorNoFormalizadoId);

            if (!existe)
                throw new Exception("El proveedor seleccionado no existe.");
        }

        private async Task ValidarMaterialesAsync(
            int proveedorNoFormalizadoId,
            List<CrearOrdenCompraDetalleNoFormalizadaDto> detalles)
        {
            var ids = detalles
                .Select(x => x.MaterialNoFormalizadoId)
                .Distinct()
                .ToList();

            var materiales = await _context.MaterialesNoFormalizados
                .Where(x => ids.Contains(x.IdMaterialNoFormalizado))
                .Select(x => new
                {
                    x.IdMaterialNoFormalizado,
                    x.IdProveedorNoFormalizado
                })
                .ToListAsync();

            var faltantes = ids
                .Except(materiales.Select(x => x.IdMaterialNoFormalizado))
                .ToList();

            if (faltantes.Any())
            {
                throw new Exception(
                    $"No existen los materiales: {string.Join(", ", faltantes)}");
            }

            var materialesOtroProveedor = materiales
                .Where(x => x.IdProveedorNoFormalizado != proveedorNoFormalizadoId)
                .Select(x => x.IdMaterialNoFormalizado)
                .ToList();

            if (materialesOtroProveedor.Any())
            {
                throw new Exception(
                    "Uno o más materiales no pertenecen al proveedor seleccionado.");
            }
        }

        private void ValidarFechas(DateTime fechaOrden, DateTime? fechaEntrega)
        {
            if (!fechaEntrega.HasValue)
                return;

            var fechaMinimaEntrega = fechaOrden.AddMonths(1);

            if (fechaEntrega.Value < fechaMinimaEntrega)
            {
                throw new Exception(
                    $"La fecha de entrega debe ser como mínimo {fechaMinimaEntrega:dd/MM/yyyy}.");
            }
        }

        private (
            decimal totalKg,
            decimal totalBultos,
            decimal subtotal,
            decimal valorImpuesto,
            decimal totalPagar)
        CalcularTotales(
            List<CrearOrdenCompraDetalleNoFormalizadaDto> detalles,
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
                totalPagar);
        }

        public async Task<OrdenCompraNoFormalizadaPaginadoDto> ObtenerAsync(
            string? search,
            string? estado,
            int? proveedorNoFormalizadoId,
            string? formaPago,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int page,
            int pageSize)
        {
            var query = _context.OrdenesCompraNoFormalizadas
                .AsNoTracking()
                .Include(x => x.ProveedorNoFormalizado)
                .Include(x => x.UsuarioCreacion)
                .Include(x => x.UsuarioActualizacion)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Numero.Contains(search) ||
                    x.ProveedorNoFormalizado!.Nombre.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(x => x.Estado == estado);

            if (proveedorNoFormalizadoId.HasValue)
                query = query.Where(x =>
                    x.ProveedorNoFormalizadoId == proveedorNoFormalizadoId);

            if (!string.IsNullOrWhiteSpace(formaPago))
                query = query.Where(x => x.FormaPago == formaPago);

            if (fechaInicio.HasValue)
                query = query.Where(x => x.FechaOrden >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(x => x.FechaOrden <= fechaFin.Value);

            var totalRegistros = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.FechaCreacion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OrdenCompraNoFormalizadaDto
                {
                    Id = x.Id,
                    Numero = x.Numero,
                    ProveedorNoFormalizadoId = x.ProveedorNoFormalizadoId,
                    Proveedor = x.ProveedorNoFormalizado!.Nombre,
                    FechaOrden = x.FechaOrden,
                    FechaEntrega = x.FechaEntrega,
                    FormaPago = x.FormaPago,
                    LugarEntrega = x.LugarEntrega,
                    TotalItems = x.TotalItems,
                    TotalKg = x.TotalKg,
                    TotalBultos = x.TotalBultos,
                    Subtotal = x.Subtotal,
                    TipoImpuesto = x.TipoImpuesto,
                    PorcentajeImpuesto = x.PorcentajeImpuesto,
                    ValorImpuesto = x.ValorImpuesto,
                    TotalPagar = x.TotalPagar,
                    Estado = x.Estado,
                    RecepcionId = x.RecepcionesMercancia
                        .OrderByDescending(r => r.FechaRecepcion)
                        .Select(r => (int?)r.Id)
                        .FirstOrDefault(),
                    Observaciones = x.Observaciones,
                    UsuarioCreacion = x.UsuarioCreacion != null
                        ? x.UsuarioCreacion.Username
                        : "",
                    FechaCreacion = x.FechaCreacion,
                    UsuarioActualizacion = x.UsuarioActualizacion != null
                        ? x.UsuarioActualizacion.Username
                        : "",
                    FechaActualizacion = x.FechaActualizacion,
                    KgRecibidos = _context.RecepcionesMercanciasNoFormalizadas
                        .Where(r => r.OrdenCompraNoFormalizadaId == x.Id)
                        .SelectMany(r => r.Detalles)
                        .Sum(d => (decimal?)d.CantidadRecibida) ?? 0,
                    BultosRecibidos = _context.RecepcionesMercanciasNoFormalizadas
                        .Where(r => r.OrdenCompraNoFormalizadaId == x.Id)
                        .SelectMany(r => r.Detalles)
                        .Sum(d => (decimal?)d.BultosRecibidos) ?? 0
                })
                .ToListAsync();

            foreach (var item in items)
            {
                item.KgRecibidos = Math.Max(0, item.KgRecibidos);
                item.BultosRecibidos = Math.Max(0, item.BultosRecibidos);

                item.KgPendientes = item.Estado == "Parcial"
                    ? Math.Max(0, item.TotalKg - item.KgRecibidos)
                    : 0;

                item.BultosPendientes = item.Estado == "Parcial"
                    ? Math.Max(0, item.TotalBultos - item.BultosRecibidos)
                    : 0;
            }

            foreach (var item in items)
            {
                if (item.KgRecibidos < 0)
                    item.KgRecibidos = 0;

                if (item.BultosRecibidos < 0)
                    item.BultosRecibidos = 0;

                if (item.KgPendientes < 0)
                    item.KgPendientes = 0;

                if (item.BultosPendientes < 0)
                    item.BultosPendientes = 0;
            }

            return new OrdenCompraNoFormalizadaPaginadoDto
            {
                Items = items,
                Total = totalRegistros,
                Pagina = page,
                PageSize = pageSize
            };
        }

        public async Task<OrdenCompraNoFormalizadaDto?> ObtenerPorIdAsync(int id)
        {
            return await _context.OrdenesCompraNoFormalizadas
                .AsNoTracking()
                .Include(x => x.ProveedorNoFormalizado)
                .Include(x => x.Detalles)
                    .ThenInclude(d => d.MaterialNoFormalizado)
                .Where(x => x.Id == id)
                .Select(x => new OrdenCompraNoFormalizadaDto
                {
                    Id = x.Id,
                    Numero = x.Numero,
                    ProveedorNoFormalizadoId = x.ProveedorNoFormalizadoId,
                    Proveedor = x.ProveedorNoFormalizado!.Nombre,
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
                    Detalles = x.Detalles.Select(d => new OrdenCompraDetalleNoFormalizadaDto
                    {
                        Id = d.Id,
                        MaterialNoFormalizadoId = d.MaterialNoFormalizadoId,
                        Material = d.MaterialNoFormalizado != null
                            ? d.MaterialNoFormalizado.DescripcionCompra ?? ""
                            : "",
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

        public async Task<OrdenCompraNoFormalizadaDto> CrearAsync(
            CrearOrdenCompraNoFormalizadaDto dto)
        {
            await ValidarProveedorAsync(dto.ProveedorNoFormalizadoId);

            await ValidarMaterialesAsync(
                dto.ProveedorNoFormalizadoId,
                dto.Detalles);

            ValidarFechas(dto.FechaOrden, dto.FechaEntrega);

            var (
                totalKg,
                totalBultos,
                subtotal,
                valorImpuesto,
                totalPagar) = CalcularTotales(
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
                        .GenerarAsync("OrdenCompraNoFormalizada");

                    var idUsuario = _currentUser.IdUsuario;

                    if (idUsuario == null)
                    {
                        throw new UnauthorizedAccessException(
                            "No se pudo identificar el usuario.");
                    }

                    var orden = new OrdenCompraNoFormalizada
                    {
                        Numero = numero,
                        ProveedorNoFormalizadoId = dto.ProveedorNoFormalizadoId,
                        FechaOrden = dto.FechaOrden,
                        FechaEntrega = dto.FechaEntrega,
                        FormaPago = dto.FormaPago,
                        LugarEntrega = dto.LugarEntrega,
                        Observaciones = dto.Observaciones,
                        Estado = "Pendiente",
                        Activo = true,
                        FechaCreacion = DateTime.Now,
                        UsuarioCreacionId = idUsuario.Value,
                        TotalItems = dto.Detalles.Count,
                        TotalKg = totalKg,
                        TotalBultos = totalBultos,
                        Subtotal = subtotal,
                        TipoImpuesto = dto.TipoImpuesto,
                        PorcentajeImpuesto = dto.PorcentajeImpuesto,
                        ValorImpuesto = valorImpuesto,
                        TotalPagar = totalPagar
                    };

                    foreach (var item in dto.Detalles)
                    {
                        orden.Detalles.Add(new OrdenCompraDetalleNoFormalizada
                        {
                            MaterialNoFormalizadoId = item.MaterialNoFormalizadoId,
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

                    _context.OrdenesCompraNoFormalizadas.Add(orden);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new OrdenCompraNoFormalizadaDto
                    {
                        Id = orden.Id,
                        Numero = orden.Numero,
                        ProveedorNoFormalizadoId = orden.ProveedorNoFormalizadoId,
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

        public async Task<bool> ActualizarAsync(
            int id,
            ActualizarOrdenCompraNoFormalizadaDto dto)
        {
            await ValidarProveedorAsync(dto.ProveedorNoFormalizadoId);

            await ValidarMaterialesAsync(
                dto.ProveedorNoFormalizadoId,
                dto.Detalles);

            ValidarFechas(dto.FechaOrden, dto.FechaEntrega);

            var orden = await _context.OrdenesCompraNoFormalizadas
                .Include(x => x.Detalles)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (orden == null)
                throw new Exception("La orden de compra no existe.");

            var idUsuario = _currentUser.IdUsuario;

            if (idUsuario == null)
            {
                throw new UnauthorizedAccessException(
                    "No se pudo identificar el usuario.");
            }

            var (
                totalKg,
                totalBultos,
                subtotal,
                valorImpuesto,
                totalPagar) = CalcularTotales(
                    dto.Detalles,
                    dto.PorcentajeImpuesto);

            orden.ProveedorNoFormalizadoId = dto.ProveedorNoFormalizadoId;
            orden.FechaOrden = dto.FechaOrden;
            orden.FechaEntrega = dto.FechaEntrega;
            orden.FormaPago = dto.FormaPago;
            orden.LugarEntrega = dto.LugarEntrega;
            orden.Observaciones = dto.Observaciones;
            orden.TotalItems = dto.Detalles.Count;
            orden.TotalKg = totalKg;
            orden.TotalBultos = totalBultos;
            orden.UsuarioActualizacionId = idUsuario.Value;
            orden.Subtotal = subtotal;
            orden.TipoImpuesto = dto.TipoImpuesto;
            orden.PorcentajeImpuesto = dto.PorcentajeImpuesto;
            orden.ValorImpuesto = valorImpuesto;
            orden.TotalPagar = totalPagar;

            _context.OrdenesCompraDetalleNoFormalizadas
                .RemoveRange(orden.Detalles);

            foreach (var item in dto.Detalles)
            {
                orden.Detalles.Add(new OrdenCompraDetalleNoFormalizada
                {
                    MaterialNoFormalizadoId = item.MaterialNoFormalizadoId,
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

        public async Task<bool> EnviarPorCorreoAsync(int id)
        {
            var orden = await _context.OrdenesCompraNoFormalizadas
                .Include(x => x.ProveedorNoFormalizado)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (orden == null)
                throw new Exception("La orden de compra no existe.");

            if (orden.Estado == "Anulada")
                throw new Exception("No se puede enviar una orden anulada.");

            if (string.IsNullOrWhiteSpace(
                orden.ProveedorNoFormalizado?.CorreoElectronico))
            {
                throw new Exception(
                    "El proveedor no tiene un correo configurado.");
            }

            byte[] pdf = await _ordenCompraPdfService.GenerarPdfAsync(id);

            string html = _emailTemplateService.OrdenCompra(
                orden.Numero,
                orden.ProveedorNoFormalizado.Nombre,
                orden.FechaOrden);

            await _emailService.EnviarAsync(
                orden.ProveedorNoFormalizado.CorreoElectronico,
                $"Orden de compra {orden.Numero}",
                html,
                pdf,
                $"OrdenCompra-{orden.Numero}.pdf");

            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<FiltrosOrdenCompraNoFormalizadaDto> ObtenerFiltrosAsync()
        {
            var estados = await _context.OrdenesCompraNoFormalizadas
                .AsNoTracking()
                .Where(x => !string.IsNullOrWhiteSpace(x.Estado))
                .Select(x => x.Estado)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var proveedores = await _context.OrdenesCompraNoFormalizadas
                .AsNoTracking()
                .Where(x => x.ProveedorNoFormalizado != null)
                .Select(x => new ProveedorOrdenCompraNoFormalizadaFiltroDto
                {
                    Id = x.ProveedorNoFormalizadoId,
                    Nombre = x.ProveedorNoFormalizado!.Nombre
                })
                .Distinct()
                .OrderBy(x => x.Nombre)
                .ToListAsync();

            var formasPago = await _context.OrdenesCompraNoFormalizadas
                .AsNoTracking()
                .Where(x => !string.IsNullOrWhiteSpace(x.FormaPago))
                .Select(x => x.FormaPago)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            return new FiltrosOrdenCompraNoFormalizadaDto
            {
                Estados = estados,
                Proveedores = proveedores,
                FormasPago = formasPago
            };
        }
    }
}
