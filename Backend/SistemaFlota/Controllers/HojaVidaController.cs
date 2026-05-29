using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HojaVidaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;
        private readonly IWebHostEnvironment _env;

        public HojaVidaController(AppDbContext context, AuditoriaService auditoria, IWebHostEnvironment env)
        {
            _context = context;
            _auditoria = auditoria;
            _env = env;
        }

        private string GetUsuario() => User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";
        private string GetRol() => User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

        // =====================================
        // GET HOJA DE VIDA COMPLETA
        // =====================================
        [HttpGet("{conductorId}")]
        public async Task<IActionResult> GetHojaVida(int conductorId)
        {
            var conductor = await _context.Conductores
                .Include(c => c.ExamenesMedicos.OrderByDescending(e => e.FechaExamen))
                .Include(c => c.Capacitaciones.OrderByDescending(c => c.FechaInicio))
                .Include(c => c.Infracciones.OrderByDescending(i => i.FechaInfraccion))
                .FirstOrDefaultAsync(c => c.Id == conductorId);

            if (conductor == null) return NotFound();
            return Ok(conductor);
        }

        // =====================================
        // PUT — ACTUALIZAR DATOS CONDUCTOR
        // =====================================
        [HttpPut("{conductorId}")]
        public async Task<IActionResult> ActualizarConductor(int conductorId, [FromBody] ActualizarConductorDto dto)
        {
            var conductor = await _context.Conductores.FindAsync(conductorId);
            if (conductor == null) return NotFound();

            conductor.Nombre = dto.Nombre;
            conductor.Telefono = dto.Telefono;
            conductor.Email = dto.Email;
            conductor.Cedula = dto.Cedula;
            conductor.FechaNacimiento = dto.FechaNacimiento;
            conductor.Direccion = dto.Direccion;
            conductor.TipoSangre = dto.TipoSangre;
            conductor.Eps = dto.Eps;
            conductor.Arl = dto.Arl;
            conductor.FondoPension = dto.FondoPension;
            conductor.ContactoEmergencia = dto.ContactoEmergencia;
            conductor.TelefonoEmergencia = dto.TelefonoEmergencia;
            conductor.Licencia = dto.Licencia;
            conductor.CategoriaLicencia = dto.CategoriaLicencia;
            conductor.FechaVencimientoLicencia = dto.FechaVencimientoLicencia;
            conductor.FechaExpedicionLicencia = dto.FechaExpedicionLicencia;
            conductor.Estado = dto.Estado;

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(GetUsuario(), GetRol(), "Editar", "HojaVida",
                $"Datos conductor actualizados — {conductor.Nombre}", conductorId);

            return Ok(conductor);
        }

        // =====================================
        // EXÁMENES MÉDICOS
        // =====================================
        [HttpPost("{conductorId}/examenes")]
        public async Task<IActionResult> AgregarExamen(int conductorId, [FromForm] ExamenMedicoDto dto)
        {
            var examen = new ExamenMedico
            {
                ConductorId = conductorId,
                TipoExamen = dto.TipoExamen,
                FechaExamen = dto.FechaExamen,
                FechaVencimiento = dto.FechaVencimiento,
                Resultado = dto.Resultado,
                Observaciones = dto.Observaciones,
                Medico = dto.Medico,
                Entidad = dto.Entidad,
                FechaRegistro = DateTime.Now
            };

            if (dto.Documento != null)
            {
                var carpeta = Path.Combine(_env.WebRootPath, "hojavida", "examenes");
                Directory.CreateDirectory(carpeta);
                var nombre = $"examen_{conductorId}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(dto.Documento.FileName)}";
                using var stream = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create);
                await dto.Documento.CopyToAsync(stream);
                examen.Documento = nombre;
            }

            _context.ExamenesMedicos.Add(examen);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(GetUsuario(), GetRol(), "Crear", "HojaVida",
                $"Examen médico agregado — Conductor #{conductorId}, Tipo: {dto.TipoExamen}", examen.Id);

            return Ok(examen);
        }

        [HttpDelete("examenes/{id}")]
        public async Task<IActionResult> EliminarExamen(int id)
        {
            var examen = await _context.ExamenesMedicos.FindAsync(id);
            if (examen == null) return NotFound();
            _context.ExamenesMedicos.Remove(examen);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // =====================================
        // CAPACITACIONES
        // =====================================
        [HttpPost("{conductorId}/capacitaciones")]
        public async Task<IActionResult> AgregarCapacitacion(int conductorId, [FromForm] CapacitacionDto dto)
        {
            var cap = new Capacitacion
            {
                ConductorId = conductorId,
                Nombre = dto.Nombre,
                Tipo = dto.Tipo,
                Entidad = dto.Entidad,
                Instructor = dto.Instructor,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                DuracionHoras = dto.DuracionHoras,
                Aprobado = dto.Aprobado,
                Observaciones = dto.Observaciones,
                FechaRegistro = DateTime.Now
            };

            if (dto.Documento != null)
            {
                var carpeta = Path.Combine(_env.WebRootPath, "hojavida", "capacitaciones");
                Directory.CreateDirectory(carpeta);
                var nombre = $"cap_{conductorId}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(dto.Documento.FileName)}";
                using var stream = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create);
                await dto.Documento.CopyToAsync(stream);
                cap.Documento = nombre;
            }

            _context.Capacitaciones.Add(cap);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(GetUsuario(), GetRol(), "Crear", "HojaVida",
                $"Capacitación agregada — Conductor #{conductorId}, {dto.Nombre}", cap.Id);

            return Ok(cap);
        }

        [HttpDelete("capacitaciones/{id}")]
        public async Task<IActionResult> EliminarCapacitacion(int id)
        {
            var cap = await _context.Capacitaciones.FindAsync(id);
            if (cap == null) return NotFound();
            _context.Capacitaciones.Remove(cap);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // =====================================
        // INFRACCIONES
        // =====================================
        [HttpPost("{conductorId}/infracciones")]
        public async Task<IActionResult> AgregarInfraccion(int conductorId, [FromForm] InfraccionDto dto)
        {
            var inf = new InfraccionConductor
            {
                ConductorId = conductorId,
                FechaInfraccion = dto.FechaInfraccion,
                TipoInfraccion = dto.TipoInfraccion,
                Descripcion = dto.Descripcion,
                Lugar = dto.Lugar,
                Valor = dto.Valor,
                Estado = dto.Estado,
                NumeroComparendo = dto.NumeroComparendo,
                Observaciones = dto.Observaciones,
                FechaRegistro = DateTime.Now
            };

            if (dto.Documento != null)
            {
                var carpeta = Path.Combine(_env.WebRootPath, "hojavida", "infracciones");
                Directory.CreateDirectory(carpeta);
                var nombre = $"inf_{conductorId}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(dto.Documento.FileName)}";
                using var stream = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create);
                await dto.Documento.CopyToAsync(stream);
                inf.Documento = nombre;
            }

            _context.Infracciones.Add(inf);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(GetUsuario(), GetRol(), "Crear", "HojaVida",
                $"Infracción agregada — Conductor #{conductorId}, Tipo: {dto.TipoInfraccion}", inf.Id);

            return Ok(inf);
        }

        [HttpPut("infracciones/{id}/estado")]
        public async Task<IActionResult> ActualizarEstadoInfraccion(int id, [FromBody] EstadoInfraccionDto dto)
        {
            var inf = await _context.Infracciones.FindAsync(id);
            if (inf == null) return NotFound();
            inf.Estado = dto.Estado;
            await _context.SaveChangesAsync();
            return Ok(inf);
        }

        [HttpDelete("infracciones/{id}")]
        public async Task<IActionResult> EliminarInfraccion(int id)
        {
            var inf = await _context.Infracciones.FindAsync(id);
            if (inf == null) return NotFound();
            _context.Infracciones.Remove(inf);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    // =====================================
    // DTOs
    // =====================================
    public class ActualizarConductorDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Cedula { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Direccion { get; set; }
        public string? TipoSangre { get; set; }
        public string? Eps { get; set; }
        public string? Arl { get; set; }
        public string? FondoPension { get; set; }
        public string? ContactoEmergencia { get; set; }
        public string? TelefonoEmergencia { get; set; }
        public string Licencia { get; set; } = string.Empty;
        public string? CategoriaLicencia { get; set; }
        public DateTime? FechaVencimientoLicencia { get; set; }
        public DateTime? FechaExpedicionLicencia { get; set; }
        public string Estado { get; set; } = "Activo";
    }

    public class ExamenMedicoDto
    {
        public string TipoExamen { get; set; } = string.Empty;
        public DateTime FechaExamen { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string Resultado { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public string? Medico { get; set; }
        public string? Entidad { get; set; }
        public IFormFile? Documento { get; set; }
    }

    public class CapacitacionDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string? Entidad { get; set; }
        public string? Instructor { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? DuracionHoras { get; set; }
        public bool Aprobado { get; set; } = true;
        public string? Observaciones { get; set; }
        public IFormFile? Documento { get; set; }
    }

    public class InfraccionDto
    {
        public DateTime FechaInfraccion { get; set; }
        public string TipoInfraccion { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Lugar { get; set; }
        public decimal? Valor { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public string? NumeroComparendo { get; set; }
        public string? Observaciones { get; set; }
        public IFormFile? Documento { get; set; }
    }

    public class EstadoInfraccionDto
    {
        public string Estado { get; set; } = string.Empty;
    }
}