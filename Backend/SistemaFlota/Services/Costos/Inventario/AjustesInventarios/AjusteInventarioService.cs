using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.Costos.Inventario;
using SistemaFlota.Models.Costos.Inventario;
using SistemaFlota.Services.Auth;
using SistemaFlota.Services.Consecutivos;

namespace SistemaFlota.Services.Costos.Inventario
{
    public class AjusteInventarioService : IAjusteInventarioService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IConsecutivoService _consecutivoService;

        public AjusteInventarioService(
            AppDbContext context,
            ICurrentUserService currentUser,
            IConsecutivoService consecutivoService)
        {
            _context = context;
            _currentUser = currentUser;
            _consecutivoService = consecutivoService;
        }

        public async Task<AjusteInventarioDto> CrearAsync(CrearAjusteInventarioDto dto)
        {
            var inventario = await _context.Inventarios
                .Include(i => i.Material)
                .FirstOrDefaultAsync(i => i.Id == dto.InventarioId);

            if (inventario == null)
                throw new Exception("El material no existe en el inventario.");

            var stockAnterior = inventario.StockActual;
            var stockNuevo = stockAnterior;

            switch (dto.Tipo)
            {
                case "Entrada":
                    stockNuevo += dto.Cantidad;
                    break;

                case "Salida":

                    if (dto.Cantidad > stockAnterior)
                        throw new Exception("La cantidad supera el stock disponible.");

                    stockNuevo -= dto.Cantidad;
                    break;

                default:
                    throw new Exception("Tipo de ajuste inválido.");
            }

            inventario.StockActual = stockNuevo;
            inventario.ValorInventario =
                stockNuevo * inventario.CostoPromedio;

            inventario.FechaActualizacion = DateTime.Now;

            var numero = await _consecutivoService
                .GenerarAsync("AjusteInventario");

            var ajuste = new AjusteInventario
            {
                NumeroAjuste = numero,
                Fecha = DateTime.Now,
                InventarioId = inventario.Id,
                Tipo = dto.Tipo,
                Cantidad = dto.Cantidad,
                StockAnterior = stockAnterior,
                StockNuevo = stockNuevo,
                CostoPromedio = inventario.CostoPromedio,
                Motivo = dto.Motivo,
                Observaciones = dto.Observaciones,
                UsuarioId = _currentUser.IdUsuario!.Value
            };

            _context.AjustesInventario.Add(ajuste);

            await _context.SaveChangesAsync();

            return new AjusteInventarioDto
            {
                Id = ajuste.Id,
                NumeroAjuste = ajuste.NumeroAjuste,
                Fecha = ajuste.Fecha,
                Material = inventario.Material!.DescripcionCompra ?? "",
                Color = inventario.Color,
                Tipo = ajuste.Tipo,
                Cantidad = ajuste.Cantidad,
                StockAnterior = ajuste.StockAnterior,
                StockNuevo = ajuste.StockNuevo,
                Motivo = ajuste.Motivo,
                Observaciones = ajuste.Observaciones
            };
        }

        public async Task<InventarioAjusteDto> ObtenerInventarioAsync(int inventarioId)
        {
            var inventario = await _context.Inventarios
                .Include(i => i.Material)
                .FirstOrDefaultAsync(i => i.Id == inventarioId);

            if (inventario == null)
                throw new Exception("El inventario no existe.");

            return new InventarioAjusteDto
            {
                InventarioId = inventario.Id,
                Material = inventario.Material!.DescripcionCompra ?? "",
                Color = inventario.Color,
                StockActual = inventario.StockActual,
                CostoPromedio = inventario.CostoPromedio
            };
        }

        public async Task<List<AjusteInventarioDto>> ObtenerHistorialAsync(int inventarioId)
        {
            return await _context.AjustesInventario
                .AsNoTracking()
                .Where(a => a.InventarioId == inventarioId)
                .Include(a => a.Inventario)
                    .ThenInclude(i => i.Material)
                .Include(a => a.Usuario)
                .OrderByDescending(a => a.Fecha)
                .Select(a => new AjusteInventarioDto
                {
                    Id = a.Id,
                    NumeroAjuste = a.NumeroAjuste,
                    Fecha = a.Fecha,
                    Material = a.Inventario.Material!.DescripcionCompra ?? "",
                    Color = a.Inventario.Color,
                    Tipo = a.Tipo,
                    Cantidad = a.Cantidad,
                    StockAnterior = a.StockAnterior,
                    StockNuevo = a.StockNuevo,
                    Motivo = a.Motivo,
                    Observaciones = a.Observaciones,
                    Usuario = a.Usuario.Username
                })
                .ToListAsync();
        }
    }
}