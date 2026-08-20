using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.Costos.Inventario.CortesInventario;
using SistemaFlota.Migrations;
using SistemaFlota.Models.Costos.Inventario.CortesInventario;
using SistemaFlota.Services.Auth;

namespace SistemaFlota.Services.Costos.Inventario.CortesInventario
{
    public class CorteInventarioService : ICorteInventarioService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public CorteInventarioService(
            AppDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }


        public async Task<List<CorteInventarioDto>> ObtenerCorteAsync()
        {
            return await _context.Inventarios
                .Include(x => x.Material)
                .ThenInclude(x => x.Proveedor)
                .Select(x => new CorteInventarioDto
                {
                    MaterialId = x.MaterialId,

                    Material = x.Material!.NombreMaterial,

                    Proveedor = x.Material.Proveedor != null
                        ? x.Material.Proveedor.Nombre
                        : string.Empty,

                    Color = x.Color,

                    Sistema = x.StockActual,

                    Conteo = 0

                })
                .ToListAsync();
        }


        public async Task GuardarCorteAsync(CrearCorteInventarioDto dto)
        {
            var corte = new CorteInventario
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

                var inventario = await _context.Inventarios
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

                var detalle = new DetalleCorteInventario
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

            _context.CortesInventario.Add(corte);
            await _context.SaveChangesAsync();
        }
        public async Task<List<HistorialCorteInventarioDto>> ObtenerHistorialAsync()
        {
            return await _context.CortesInventario
                .Join(
                    _context.Usuarios,
                    corte => corte.UsuarioId,
                    usuario => usuario.Id,
                    (corte, usuario) => new HistorialCorteInventarioDto
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

        public async Task<HistorialCorteDetalleDto?> ObtenerDetalleAsync(int id)
        {
            return await _context.CortesInventario
                .Where(x => x.Id == id)
                .Join(
                    _context.Usuarios,
                    corte => corte.UsuarioId,
                    usuario => usuario.Id,
                    (corte, usuario) => new HistorialCorteDetalleDto
                    {
                        Id = corte.Id,
                        Fecha = corte.Fecha,
                        Estado = corte.Estado,
                        Usuario = usuario.Username,

                        Detalles = corte.Detalles.Select(d => new DetalleHistorialCorteDto
                        {
                            MaterialId = d.MaterialId,
                            Material = d.Material.NombreMaterial,
                            Proveedor = d.Material.Proveedor != null
                                ? d.Material.Proveedor.Nombre
                                : string.Empty,
                            Color = d.Color,
                            StockSistema = d.StockSistema,
                            ConteoFisico = d.ConteoFisico,
                            Diferencia = d.ConteoFisico - d.StockSistema
                        }).ToList()
                    })
                .FirstOrDefaultAsync();
        }
    }
}