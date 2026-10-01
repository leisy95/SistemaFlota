using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.ComprasNoFormalizadas.Inventario.CortesInventario;
using SistemaFlota.Models.ComprasNoFormalizadas.Inventario.CortesInventario;
using SistemaFlota.Services.Auth;

namespace SistemaFlota.Services.ComprasNoFormalizadas.Inventario.CortesInventario
{
    public class CorteInventarioNoFormalizadoService
        : ICorteInventarioNoFormalizadoService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public CorteInventarioNoFormalizadoService(
            AppDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<CorteInventarioNoFormalizadoDto>> ObtenerCorteAsync()
        {
            return await _context.InventariosNoFormalizados
                .Include(x => x.Material)
                    .ThenInclude(x => x!.ProveedorNoFormalizado)
                .Select(x => new CorteInventarioNoFormalizadoDto
                {
                    InventarioId = x.Id,

                    MaterialId = x.MaterialId,

                    Material = x.Material!.NombreMaterial,

                    Proveedor = x.Material.ProveedorNoFormalizado != null
                        ? x.Material.ProveedorNoFormalizado.Nombre
                        : string.Empty,

                    Color = x.Color,

                    Sistema = x.StockActual,

                    Conteo = 0
                })
                .ToListAsync();
        }

        public async Task GuardarCorteAsync(
            CrearCorteInventarioNoFormalizadoDto dto)
        {
            var corte = new CorteInventarioNoFormalizado
            {
                Fecha = DateTime.Now,
                Estado = "Pendiente",
                UsuarioId = _currentUser.IdUsuario!.Value
            };

            foreach (var item in dto.Detalles)
            {
                if (item.Conteo < 0)
                {
                    throw new ArgumentException(
                        $"El conteo físico no puede ser negativo para el material {item.MaterialId}."
                    );
                }

                var inventario = await _context.InventariosNoFormalizados
                    .FirstOrDefaultAsync(x =>
                        x.MaterialId == item.MaterialId &&
                        x.Color == item.Color
                    );

                if (inventario == null)
                {
                    throw new Exception(
                        $"No se encontró inventario para MaterialId {item.MaterialId}"
                    );
                }

                var detalle = new DetalleCorteInventarioNoFormalizado
                {
                    MaterialId = item.MaterialId,
                    Color = inventario.Color,
                    StockSistema = inventario.StockActual,
                    ConteoFisico = item.Conteo
                };

                corte.Detalles.Add(detalle);

                inventario.StockActual = item.Conteo;

                inventario.ValorInventario =
                    inventario.StockActual * inventario.CostoPromedio;

                inventario.FechaActualizacion = DateTime.Now;
            }

            _context.CortesInventarioNoFormalizados.Add(corte);

            await _context.SaveChangesAsync();
        }

        public async Task<List<HistorialCorteInventarioNoFormalizadoDto>>
            ObtenerHistorialAsync()
        {
            return await _context.CortesInventarioNoFormalizados
                .Join(
                    _context.Usuarios,
                    corte => corte.UsuarioId,
                    usuario => usuario.Id,
                    (corte, usuario) =>
                        new HistorialCorteInventarioNoFormalizadoDto
                        {
                            Id = corte.Id,
                            Fecha = corte.Fecha,
                            Estado = corte.Estado,
                            Usuario = usuario.Username,
                            CantidadDetalles = corte.Detalles.Count()
                        })
                .OrderByDescending(x => x.Fecha)
                .ToListAsync();
        }

        public async Task<HistorialCorteDetalleNoFormalizadoDto?>
            ObtenerDetalleAsync(int id)
        {
            return await _context.CortesInventarioNoFormalizados
                .Where(x => x.Id == id)
                .Join(
                    _context.Usuarios,
                    corte => corte.UsuarioId,
                    usuario => usuario.Id,
                    (corte, usuario) =>
                        new HistorialCorteDetalleNoFormalizadoDto
                        {
                            Id = corte.Id,
                            Fecha = corte.Fecha,
                            Estado = corte.Estado,
                            Usuario = usuario.Username,

                            Detalles = corte.Detalles
                                .Select(d =>
                                    new DetalleHistorialCorteNoFormalizadoDto
                                    {
                                        MaterialId = d.MaterialId,

                                        Material =
                                            d.Material.NombreMaterial,

                                        Proveedor =
                                            d.Material.ProveedorNoFormalizado != null
                                                ? d.Material.ProveedorNoFormalizado.Nombre
                                                : string.Empty,

                                        Color = d.Color,

                                        StockSistema = d.StockSistema,

                                        ConteoFisico = d.ConteoFisico,

                                        Diferencia =
                                            d.ConteoFisico -
                                            d.StockSistema
                                    })
                                .ToList()
                        })
                .FirstOrDefaultAsync();
        }
    }
}