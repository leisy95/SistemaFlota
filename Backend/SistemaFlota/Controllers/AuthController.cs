using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Collections.Concurrent;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly AuditoriaService _auditoria;

        private static readonly ConcurrentDictionary<string, (int intentos, DateTime desde)>
            _intentosFallidos = new();

        private const int MaxIntentos = 5;
        private const int BloqueoMinutos = 3;

        public AuthController(AppDbContext context, IConfiguration config, AuditoriaService auditoria)
        {
            _context = context;
            _config = config;
            _auditoria = auditoria;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Usuario y contraseña son requeridos");

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var ahora = DateTime.UtcNow;

            // ── Control de intentos fallidos ──────────────────────────────────
            if (_intentosFallidos.TryGetValue(ip, out var registro))
            {
                if (ahora - registro.desde > TimeSpan.FromMinutes(BloqueoMinutos))
                    _intentosFallidos.TryRemove(ip, out _);
                else if (registro.intentos >= MaxIntentos)
                {
                    var min = (int)(BloqueoMinutos - (ahora - registro.desde).TotalMinutes);
                    return StatusCode(429, new
                    {
                        error = "Demasiados intentos fallidos",
                        mensaje = $"Espera {min} minuto(s) antes de intentar de nuevo"
                    });
                }
            }

            // ── Buscar usuario activo ─────────────────────────────────────────
            var usuario = await _context.Usuarios
                .Include(u => u.Permisos)
                .FirstOrDefaultAsync(u =>
                    u.Username == request.Username &&
                    u.Activo);

            // ── Verificar contraseña: BCrypt primero, texto plano como fallback ─
            bool passwordValida = false;
            if (usuario != null)
            {
                if (!string.IsNullOrWhiteSpace(usuario.PasswordHash))
                {
                    // Contraseña hasheada con BCrypt
                    passwordValida = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
                }
                else
                {
                    // Contraseña en texto plano (usuarios antiguos) — migrar automáticamente
                    passwordValida = usuario.Password == request.Password;

                    if (passwordValida)
                    {
                        // ── Migración automática al hacer login ───────────────
                        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                        usuario.Password = string.Empty; // limpiar texto plano
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"✅ Contraseña migrada a BCrypt — Usuario: {usuario.Username}");
                    }
                }
            }

            if (usuario == null || !passwordValida)
            {
                _intentosFallidos.AddOrUpdate(ip, (1, ahora),
                    (_, ant) => (ant.intentos + 1, ant.desde));

                await _auditoria.RegistrarAsync(
                    usuario: request.Username, rol: "Desconocido",
                    accion: "Login", modulo: "Auth",
                    detalle: $"Intento fallido desde IP: {ip}", resultado: "Fallido");

                return Unauthorized(new { error = "Usuario o contraseña incorrectos" });
            }

            // ── Validar rol ───────────────────────────────────────────────────
            if (!UsuariosController.RolesValidos.Contains(usuario.Rol))
            {
                await _auditoria.RegistrarAsync(
                    usuario: usuario.Username, rol: usuario.Rol,
                    accion: "Login", modulo: "Auth",
                    detalle: $"Rol inválido: {usuario.Rol}", resultado: "Fallido");

                return Unauthorized(new { error = "Rol de usuario no válido" });
            }

            _intentosFallidos.TryRemove(ip, out _);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name,  usuario.Username),
                new Claim(ClaimTypes.Email, usuario.Email ?? string.Empty),
                new Claim(ClaimTypes.Role,  usuario.Rol),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: creds
            );

            var permisos = usuario.Permisos.Select(p => new
            {
                modulo = p.Modulo,
                puedeVer = p.PuedeVer,
                puedeCrear = p.PuedeCrear,
                puedeEditar = p.PuedeEditar,
                puedeEliminar = p.PuedeEliminar,
                esInicio = p.EsInicio
            }).ToList();

            await _auditoria.RegistrarAsync(
                usuario: usuario.Username, rol: usuario.Rol,
                accion: "Login", modulo: "Auth",
                detalle: $"Login exitoso — Rol: {usuario.Rol} — IP: {ip}",
                resultado: "Exitoso");

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                username = usuario.Username,
                rol = usuario.Rol,
                email = usuario.Email,
                permisos = permisos
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            await _auditoria.RegistrarAsync(
                usuario: request.Username, rol: request.Rol ?? "Desconocido",
                accion: "Logout", modulo: "Auth",
                detalle: "Cierre de sesión", resultado: "Exitoso");
            return Ok();
        }
    }

    public class LoginRequest { public string Username { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
    public class LogoutRequest { public string Username { get; set; } = string.Empty; public string? Rol { get; set; } }
}