using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EncuestaFatigaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;

        public EncuestaFatigaController(
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
        // GET TODAS
        // =====================================

        [HttpGet]
        public async Task<IActionResult> Get(
    [FromQuery] int pagina = 1,
    [FromQuery] int porPagina = 20,
    [FromQuery] string? buscar = null,
    [FromQuery] string? resultado = null,
    [FromQuery] int? conductorId = null)
        {
            var query = _context.EncuestasFatiga
                .Include(e => e.Conductor)
                .Include(e => e.Vehiculo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var q = buscar.ToLower();
                query = query.Where(e =>
                    e.Conductor!.Nombre.ToLower().Contains(q) ||
                    e.Vehiculo!.Placa.ToLower().Contains(q));
            }

            if (!string.IsNullOrWhiteSpace(resultado))
                query = query.Where(e => e.Resultado == resultado);

            if (conductorId.HasValue)
                query = query.Where(e => e.ConductorId == conductorId.Value);

            var total = await query.CountAsync();

            var lista = await query
                .OrderByDescending(e => e.Fecha)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .Select(e => new
                {
                    e.Id,
                    e.Fecha,
                    e.DurmioMenos7Horas,
                    e.SienteCansancio,
                    e.DespertoVariasVeces,
                    e.MedicamentoSueno,
                    e.DificultadConcentracion,
                    e.OtraObservacion,
                    e.Resultado,
                    e.RegistradoPor,
                    e.Observaciones,
                    Conductor = new { e.Conductor!.Id, e.Conductor.Nombre, e.Conductor.Licencia },
                    Vehiculo = new { e.Vehiculo!.Id, e.Vehiculo.Placa }
                })
                .ToListAsync();

            return Ok(new
            {
                data = lista,
                total,
                pagina,
                porPagina,
                totalPaginas = (int)Math.Ceiling((double)total / porPagina)
            });
        }

        // =====================================
        // GET POR CONDUCTOR
        // =====================================

        [HttpGet("conductor/{conductorId}")]
        public async Task<IActionResult> GetPorConductor(int conductorId)
        {
            var lista = await _context.EncuestasFatiga
                .Include(e => e.Conductor)
                .Include(e => e.Vehiculo)
                .Where(e => e.ConductorId == conductorId)
                .OrderByDescending(e => e.Fecha)
                .ToListAsync();

            return Ok(lista);
        }

        // =====================================
        // GET POR ID
        // =====================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var encuesta = await _context.EncuestasFatiga
                .Include(e => e.Conductor)
                .Include(e => e.Vehiculo)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (encuesta == null) return NotFound();
            return Ok(encuesta);
        }

        // =====================================
        // POST — CREAR
        // =====================================

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CrearEncuestaFatigaDto dto)
        {
            // CALCULAR RESULTADO
            int respuestasSi =
                (dto.DurmioMenos7Horas ? 1 : 0) +
                (dto.SienteCansancio ? 1 : 0) +
                (dto.DespertoVariasVeces ? 1 : 0) +
                (dto.MedicamentoSueno ? 1 : 0) +
                (dto.DificultadConcentracion ? 1 : 0);

            // Si tiene 2 o más respuestas SI → No Apto
            string resultado = respuestasSi >= 2 ? "No Apto" : "Apto";

            var encuesta = new EncuestaFatiga
            {
                ConductorId = dto.ConductorId,
                VehiculoId = dto.VehiculoId,
                DurmioMenos7Horas = dto.DurmioMenos7Horas,
                SienteCansancio = dto.SienteCansancio,
                DespertoVariasVeces = dto.DespertoVariasVeces,
                MedicamentoSueno = dto.MedicamentoSueno,
                DificultadConcentracion = dto.DificultadConcentracion,
                OtraObservacion = dto.OtraObservacion,
                Resultado = resultado,
                RegistradoPor = dto.RegistradoPor,
                Observaciones = dto.Observaciones,
                Fecha = DateTime.Now
            };

            _context.EncuestasFatiga.Add(encuesta);
            await _context.SaveChangesAsync();

            var conductor = await _context.Conductores.FindAsync(dto.ConductorId);
            var vehiculo = await _context.Vehiculos.FindAsync(dto.VehiculoId);

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(),
                rol: GetRol(),
                accion: "Crear",
                modulo: "EncuestaFatiga",
                detalle: $"Encuesta fatiga — Conductor: {conductor?.Nombre ?? "-"}, Resultado: {resultado}",
                registroId: encuesta.Id
            );

            return Ok(new { encuesta, resultado, respuestasSi });
        }

        // =====================================
        // DELETE
        // =====================================

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var encuesta = await _context.EncuestasFatiga.FindAsync(id);
            if (encuesta == null) return NotFound();

            _context.EncuestasFatiga.Remove(encuesta);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(),
                rol: GetRol(),
                accion: "Eliminar",
                modulo: "EncuestaFatiga",
                detalle: $"Encuesta fatiga #{id} eliminada",
                registroId: id
            );

            return Ok();
        }

        // =====================================
        // GET ESTADÍSTICAS
        // =====================================

        [HttpGet("estadisticas")]
        public async Task<IActionResult> GetEstadisticas()
        {
            var total = await _context.EncuestasFatiga.CountAsync();
            var aptos = await _context.EncuestasFatiga.CountAsync(e => e.Resultado == "Apto");
            var noAptos = await _context.EncuestasFatiga.CountAsync(e => e.Resultado == "No Apto");
            var hoy = DateTime.Today;
            var encuestasHoy = await _context.EncuestasFatiga
                .CountAsync(e => e.Fecha >= hoy);

            return Ok(new { total, aptos, noAptos, encuestasHoy });
        }
    }

    // =====================================
    // DTO
    // =====================================

    public class CrearEncuestaFatigaDto
    {
        public int ConductorId { get; set; }
        public int VehiculoId { get; set; }
        public bool DurmioMenos7Horas { get; set; }
        public bool SienteCansancio { get; set; }
        public bool DespertoVariasVeces { get; set; }
        public bool MedicamentoSueno { get; set; }
        public bool DificultadConcentracion { get; set; }
        public string? OtraObservacion { get; set; }
        public string? RegistradoPor { get; set; }
        public string? Observaciones { get; set; }
    }
}