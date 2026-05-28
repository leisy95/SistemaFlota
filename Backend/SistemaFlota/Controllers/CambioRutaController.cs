using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CambioRutaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;

        public CambioRutaController(
            AppDbContext context,
            AuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        private string GetUsuario() =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";
        private string GetRol() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

        // =====================================
        // GET TODOS
        // =====================================

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lista = await _context.CambiosRuta
                .Include(c => c.Conductor)
                .Include(c => c.Vehiculo)
                .Include(c => c.Autorizacion)
                .OrderByDescending(c => c.FechaSolicitud)
                .Select(c => new
                {
                    c.Id,
                    c.FechaSolicitud,
                    c.RutaOriginal,
                    c.NuevaRuta,
                    c.MotivoCambio,
                    c.Estado,
                    c.AutorizadoPor,
                    c.ObservacionAut,
                    c.FechaAutorizacion,
                    c.AutorizacionId,
                    Conductor = new
                    {
                        c.Conductor!.Id,
                        c.Conductor.Nombre,
                        c.Conductor.Telefono
                    },
                    Vehiculo = new
                    {
                        c.Vehiculo!.Id,
                        c.Vehiculo.Placa
                    }
                })
                .ToListAsync();

            return Ok(lista);
        }

        // =====================================
        // GET PENDIENTES
        // =====================================

        [HttpGet("pendientes")]
        public async Task<IActionResult> GetPendientes()
        {
            var lista = await _context.CambiosRuta
                .Include(c => c.Conductor)
                .Include(c => c.Vehiculo)
                .Where(c => c.Estado == "Pendiente")
                .OrderByDescending(c => c.FechaSolicitud)
                .ToListAsync();

            return Ok(lista);
        }

        // =====================================
        // GET POR ID
        // =====================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cambio = await _context.CambiosRuta
                .Include(c => c.Conductor)
                .Include(c => c.Vehiculo)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cambio == null) return NotFound();
            return Ok(cambio);
        }

        // =====================================
        // POST — SOLICITAR CAMBIO
        // =====================================

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CrearCambioRutaDto dto)
        {
            var cambio = new CambioRuta
            {
                AutorizacionId = dto.AutorizacionId,
                ConductorId = dto.ConductorId,
                VehiculoId = dto.VehiculoId,
                RutaOriginal = dto.RutaOriginal,
                NuevaRuta = dto.NuevaRuta,
                MotivoCambio = dto.MotivoCambio,
                Estado = "Pendiente",
                FechaSolicitud = DateTime.Now
            };

            _context.CambiosRuta.Add(cambio);
            await _context.SaveChangesAsync();

            var conductor = await _context.Conductores.FindAsync(dto.ConductorId);
            var vehiculo = await _context.Vehiculos.FindAsync(dto.VehiculoId);

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(),
                rol: GetRol(),
                accion: "Crear",
                modulo: "CambioRuta",
                detalle: $"Cambio de ruta solicitado — Conductor: {conductor?.Nombre ?? "-"}, Nueva ruta: {dto.NuevaRuta}",
                registroId: cambio.Id
            );

            return Ok(cambio);
        }

        // =====================================
        // PUT — AUTORIZAR
        // =====================================

        [HttpPut("{id}/autorizar")]
        public async Task<IActionResult> Autorizar(
            int id,
            [FromBody] AutorizarCambioDto dto)
        {
            var cambio = await _context.CambiosRuta
                .Include(c => c.Conductor)
                .Include(c => c.Vehiculo)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cambio == null) return NotFound();

            cambio.Estado = "Autorizado";
            cambio.AutorizadoPor = dto.AutorizadoPor;
            cambio.ObservacionAut = dto.Observacion;
            cambio.FechaAutorizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(),
                rol: GetRol(),
                accion: "Autorizar",
                modulo: "CambioRuta",
                detalle: $"Cambio de ruta #{id} AUTORIZADO por: {dto.AutorizadoPor}",
                registroId: id
            );

            return Ok(cambio);
        }

        // =====================================
        // PUT — RECHAZAR
        // =====================================

        [HttpPut("{id}/rechazar")]
        public async Task<IActionResult> Rechazar(
            int id,
            [FromBody] AutorizarCambioDto dto)
        {
            var cambio = await _context.CambiosRuta.FindAsync(id);
            if (cambio == null) return NotFound();

            cambio.Estado = "Rechazado";
            cambio.AutorizadoPor = dto.AutorizadoPor;
            cambio.ObservacionAut = dto.Observacion;
            cambio.FechaAutorizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(),
                rol: GetRol(),
                accion: "Rechazar",
                modulo: "CambioRuta",
                detalle: $"Cambio de ruta #{id} RECHAZADO por: {dto.AutorizadoPor}",
                registroId: id
            );

            return Ok(cambio);
        }

        // =====================================
        // DELETE
        // =====================================

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var cambio = await _context.CambiosRuta.FindAsync(id);
            if (cambio == null) return NotFound();

            _context.CambiosRuta.Remove(cambio);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(),
                rol: GetRol(),
                accion: "Eliminar",
                modulo: "CambioRuta",
                detalle: $"Cambio de ruta #{id} eliminado",
                registroId: id
            );

            return Ok();
        }
    }

    // =====================================
    // DTOs
    // =====================================

    public class CrearCambioRutaDto
    {
        public int? AutorizacionId { get; set; }
        public int ConductorId { get; set; }
        public int VehiculoId { get; set; }
        public string RutaOriginal { get; set; } = string.Empty;
        public string NuevaRuta { get; set; } = string.Empty;
        public string MotivoCambio { get; set; } = string.Empty;
    }

    public class AutorizarCambioDto
    {
        public string AutorizadoPor { get; set; } = string.Empty;
        public string? Observacion { get; set; }
    }
}