using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IFlotaChatDirectorio _flotaChat;

        public VinculacionesFlotaChatController(AppDbContext context, IFlotaChatDirectorio flotaChat)
        {
            _context = context;
            _flotaChat = flotaChat;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? tipoEntidad = null)
        {
            var query = _context.VinculacionesFlotaChat.AsQueryable();
            if (!string.IsNullOrEmpty(tipoEntidad))
                query = query.Where(v => v.TipoEntidad == tipoEntidad);

            return Ok(await query.ToListAsync());
        }

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

        // Usuarios de FlotaChat sin vincular, con sugerencia por teléfono.
        [HttpGet("usuarios-flotachat-pendientes")]
        public async Task<IActionResult> GetUsuariosFlotaChatPendientes([FromQuery] string tipo = "Conductor")
        {
            var usuarios = await _flotaChat.ObtenerUsuariosAsync();
            if (usuarios == null)
                return StatusCode(502, new { mensaje = "No se pudo conectar con FlotaChat" });

            var yaVinculados = await _context.VinculacionesFlotaChat
                .Where(v => v.TipoEntidad == tipo)
                .Select(v => v.FlotaChatUsuarioId)
                .ToListAsync();

            var rolBuscado = tipo == "Conductor" ? "conductor" : null;
            var pendientes = usuarios
                .Where(u => (rolBuscado == null || u.Rol == rolBuscado) && !yaVinculados.Contains(u.Id))
                .ToList();

            if (tipo == "Usuario")
            {
                var usuariosSistema = await _context.Usuarios
                    .Where(u => u.Activo)
                    .Select(u => new { u.Id, u.Username, u.Nombres, u.Apellidos, u.Telefono })
                    .ToListAsync();

                return Ok(pendientes.Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Celular,
                    Sugerencia = usuariosSistema
                        .Where(s => !string.IsNullOrEmpty(s.Telefono) && s.Telefono == u.Celular)
                        .Select(s => new { s.Id, Nombre = NombreUsuario(s.Nombres, s.Apellidos, s.Username), s.Telefono })
                        .FirstOrDefault()
                }));
            }

            if (tipo == "Conductor")
            {
                var conductoresSistema = await _context.Conductores
                    .Select(c => new { c.Id, c.Nombre, c.Telefono })
                    .ToListAsync();

                return Ok(pendientes.Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Celular,
                    Sugerencia = conductoresSistema.FirstOrDefault(c => c.Telefono == u.Celular)
                }));
            }

            var contactosSistema = await _context.ContactosNotificacion
                .Select(c => new { c.Id, c.Nombre, c.NumeroWhatsApp })
                .ToListAsync();

            return Ok(pendientes.Select(u => new
            {
                u.Id,
                u.Nombre,
                u.Celular,
                Sugerencia = contactosSistema.FirstOrDefault(c => c.NumeroWhatsApp == u.Celular)
            }));
        }

        [HttpGet("entidades")]
        public async Task<IActionResult> GetEntidades([FromQuery] string tipo = "Conductor")
        {
            if (tipo == "Usuario")
            {
                var usuarios = await _context.Usuarios
                    .Where(u => u.Activo)
                    .Select(u => new { u.Id, u.Username, u.Nombres, u.Apellidos, u.Telefono })
                    .ToListAsync();

                return Ok(usuarios.Select(u => new { u.Id, Nombre = NombreUsuario(u.Nombres, u.Apellidos, u.Username), u.Telefono }));
            }

            if (tipo == "Conductor")
            {
                return Ok(await _context.Conductores
                    .Select(c => new { c.Id, c.Nombre, c.Telefono })
                    .ToListAsync());
            }

            return Ok(await _context.ContactosNotificacion
                .Select(c => new { c.Id, c.Nombre, Telefono = c.NumeroWhatsApp })
                .ToListAsync());
        }

        // Una persona de FlotaChat puede tener una vinculación por tipo (por ejemplo, contacto de pedidos y usuario).
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearVinculacionDto dto)
        {
            var existe = await _context.VinculacionesFlotaChat
                .AnyAsync(v => v.FlotaChatUsuarioId == dto.FlotaChatUsuarioId && v.TipoEntidad == dto.TipoEntidad);
            if (existe)
                return BadRequest(new { mensaje = "Este usuario de FlotaChat ya tiene una vinculación de este tipo" });

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var vinculacion = await _context.VinculacionesFlotaChat.FindAsync(id);
            if (vinculacion == null) return NotFound();

            _context.VinculacionesFlotaChat.Remove(vinculacion);
            await _context.SaveChangesAsync();
            return Ok();
        }

        public static async Task<int?> ObtenerConductorId(AppDbContext context, int flotaChatUsuarioId)
        {
            var vinculacion = await context.VinculacionesFlotaChat
                .FirstOrDefaultAsync(v => v.FlotaChatUsuarioId == flotaChatUsuarioId && v.TipoEntidad == "Conductor");
            return vinculacion?.EntidadId;
        }

        private static string NombreUsuario(string? nombres, string? apellidos, string username)
        {
            var completo = $"{nombres} {apellidos}".Trim();
            return completo.Length > 0 ? completo : username;
        }
    }
}