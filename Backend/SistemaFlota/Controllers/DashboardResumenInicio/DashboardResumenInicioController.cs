using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.DashboardResumenInicio;

namespace SistemaFlota.Controllers.DashboardResumenInicio
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardResumenInicioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardResumenInicioController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardResumenInicioResponse>> ObtenerResumen()
        {
            var fechaActual = DateTime.Now;
            var fechaLimite = fechaActual.AddDays(30);

            // RESUMEN
            var vehiculos = await _context.Vehiculos
                .CountAsync(x => x.Estado == "Activo");

            var ordenesCompra = await _context.OrdenesCompra
                .CountAsync(x =>
                    x.Activo &&
                    (
                        x.Estado == "Pendiente" ||
                        x.Estado == "Confirmada" ||
                        x.Estado == "Parcial"
                    ));

            var ordenesPendientes = await _context.OrdenesCompra
                .CountAsync(x =>
                    x.Activo &&
                    x.Estado == "Pendiente");

            var mantenimientos = await _context.Mantenimientos
                .CountAsync(x => x.Estado == "EnTaller");

            var mantenimientosProximos = await _context.Mantenimientos
                .CountAsync(x =>
                    x.FechaSiguiente.HasValue &&
                    x.FechaSiguiente.Value >= fechaActual &&
                    x.FechaSiguiente.Value <= fechaLimite);


            // ACTIVIDADES

            var actividadesOrdenes = await _context.OrdenesCompra
                .Where(x => x.Activo)
                .OrderByDescending(x => x.FechaCreacion)
                .Take(10)
                .Select(x => new DashboardActividadResponse
                {
                    Tipo = "orden",
                    Descripcion = $"Orden de compra {x.Numero} creada",
                    Fecha = x.FechaCreacion
                })
                .ToListAsync();

            var actividadesMantenimientos = await _context.Mantenimientos
                .OrderByDescending(x => x.FechaEntrada)
                .Take(10)
                .Select(x => new DashboardActividadResponse
                {
                    Tipo = "mantenimiento",
                    Descripcion = "Vehículo enviado a mantenimiento",
                    Fecha = x.FechaEntrada
                })
                .ToListAsync();


            var actividades = actividadesOrdenes
                .Concat(actividadesMantenimientos)
                .OrderByDescending(x => x.Fecha)
                .Take(5)
                .ToList();

            // VENCIMIENTOS

            var mantenimientosVencimientos = await _context.Mantenimientos
                .Where(x =>
                    x.FechaSiguiente.HasValue &&
                    x.FechaSiguiente.Value >= fechaActual &&
                    x.FechaSiguiente.Value <= fechaLimite)
                .OrderBy(x => x.FechaSiguiente)
                .Take(5)
                .ToListAsync();

            var vencimientos = mantenimientosVencimientos
                .Select(x => new DashboardVencimientoResponse
                {
                    Tipo = "Mantenimiento",
                    Descripcion = "Mantenimiento camión",
                    Dias = (int)Math.Ceiling(
                        (x.FechaSiguiente!.Value - fechaActual).TotalDays
                    )
                })
                .ToList();

            // RESPUESTA
            var respuesta = new DashboardResumenInicioResponse
            {
                Vehiculos = vehiculos,
                Empleados = 0,

                OrdenesCompra = ordenesCompra,
                OrdenesPendientes = ordenesPendientes,

                Mantenimientos = mantenimientos,
                MantenimientosProximos = mantenimientosProximos,

                Actividades = actividades,
                Vencimientos = vencimientos
            };

            return Ok(respuesta);
        }
    }
}
