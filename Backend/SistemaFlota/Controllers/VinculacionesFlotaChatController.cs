using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs;

namespace SistemaFlota.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VinculacionesFlotaChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public VinculacionesFlotaChatController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ── GET: listar vinculaciones existentes por tipo ──────────────────
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? tipoEntidad = null)
        {
            var query = _context.VinculacionesFlotaChat.AsQueryable();
            if (!string.IsNullOrEmpty(tipoEntidad))
                query = query.Where(v => v.TipoEntidad == tipoEntidad);

            var lista = await query.ToListAsync();
            return Ok(lista);
        }

        // ── GET: conductores sin vincular (comparando con FlotaChat) ───────
        [HttpGet("conductores-pendientes")]
        public async Task<IActionResult> GetConductoresPendientes()
        {
            var conductoresVinculados = await _context.VinculacionesFlotaChat
                .Where(v => v.TipoEntidad == "Conductor")
                .Select(v => v.EntidadId)
                .ToListAsync();

            var conductores = await _context.Conductores
                .Where(c => !conductoresVinculados.Contains(c.Id))
                .Select(c => new { c.Id, c.Nombre, c.Telefono })
                .ToListAsync();

            return Ok(conductores);
        }

        // ── GET: usuarios de FlotaChat pendientes, con sugerencia de match ──
        [HttpGet("usuarios-flotachat-pendientes")]
        public async Task<IActionResult> GetUsuariosFlotaChatPendientes([FromQuery] string tipo = "Conductor")
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("X-Api-Key",
                _config["FlotaChat:ApiKey"] ?? "FlotaChat_API_Key_2026_Seguro_XYZ789");

            var apiUrl = _config["FlotaChat:ApiUrl"] ?? "https://apichat.gecobagsci.com";
            var response = await http.GetAsync($"{apiUrl}/api/Externo/usuarios");

            if (!response.IsSuccessStatusCode)
                return StatusCode(502, new { mensaje = "No se pudo conectar con FlotaChat" });

            var json = await response.Content.ReadAsStringAsync();
            var usuarios = System.Text.Json.JsonSerializer.Deserialize<List<UsuarioFlotaChatDto>>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

            var yaVinculados = await _context.VinculacionesFlotaChat
                .Where(v => v.TipoEntidad == tipo)
                .Select(v => v.FlotaChatUsuarioId)
                .ToListAsync();

            // Filtra por rol: conductores para "Conductor", cualquier otro rol para "ContactoNotificacion"
            var rolBuscado = tipo == "Conductor" ? "conductor" : null;
            var pendientes = usuarios
                .Where(u => (rolBuscado == null || u.Rol == rolBuscado) && !yaVinculados.Contains(u.Id))
                .ToList();

            if (tipo == "Conductor")
            {
                var conductoresSistema = await _context.Conductores
                    .Select(c => new { c.Id, c.Nombre, c.Telefono })
                    .ToListAsync();

                var resultadoConductores = pendientes.Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Celular,
                    Sugerencia = conductoresSistema.FirstOrDefault(c => c.Telefono == u.Celular)
                });
                return Ok(resultadoConductores);
            }
            else
            {
                var contactosSistema = await _context.ContactosNotificacion
                    .Select(c => new { c.Id, c.Nombre, c.NumeroWhatsApp })
                    .ToListAsync();

                var resultadoContactos = pendientes.Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Celular,
                    Sugerencia = contactosSistema.FirstOrDefault(c => c.NumeroWhatsApp == u.Celular)
                });
                return Ok(resultadoContactos);
            }
        }

        // ── GET: lista completa de Conductores o Contactos (para el <select>) ──
        [HttpGet("entidades")]
        public async Task<IActionResult> GetEntidades([FromQuery] string tipo = "Conductor")
        {
            if (tipo == "Conductor")
            {
                var conductores = await _context.Conductores
                    .Select(c => new { c.Id, c.Nombre, c.Telefono })
                    .ToListAsync();
                return Ok(conductores);
            }
            else
            {
                var contactos = await _context.ContactosNotificacion
                    .Select(c => new { c.Id, c.Nombre, Telefono = c.NumeroWhatsApp })
                    .ToListAsync();
                return Ok(contactos);
            }
        }

        // ── POST: crear vinculación ─────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearVinculacionDto dto)
        {
            var existe = await _context.VinculacionesFlotaChat
                .AnyAsync(v => v.FlotaChatUsuarioId == dto.FlotaChatUsuarioId);
            if (existe)
                return BadRequest(new { mensaje = "Este usuario de FlotaChat ya está vinculado" });

            var vinculacion = new VinculacionFlotaChat
            {
                FlotaChatUsuarioId = dto.FlotaChatUsuarioId,
                TipoEntidad = dto.TipoEntidad,
                EntidadId = dto.EntidadId,
                Telefono = dto.Telefono,
                FechaVinculacion = DateTime.Now
            };

            _context.VinculacionesFlotaChat.Add(vinculacion);
            await _context.SaveChangesAsync();

            return Ok(vinculacion);
        }

        // ── DELETE: quitar vinculación ──────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var vinculacion = await _context.VinculacionesFlotaChat.FindAsync(id);
            if (vinculacion == null) return NotFound();

            _context.VinculacionesFlotaChat.Remove(vinculacion);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ── Helper interno: buscar Conductor por FlotaChatUsuarioId ─────────
        public static async Task<int?> ObtenerConductorId(AppDbContext context, int flotaChatUsuarioId)
        {
            var vinculacion = await context.VinculacionesFlotaChat
                .FirstOrDefaultAsync(v => v.FlotaChatUsuarioId == flotaChatUsuarioId && v.TipoEntidad == "Conductor");
            return vinculacion?.EntidadId;
        }
    }
}