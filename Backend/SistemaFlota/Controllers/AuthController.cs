using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly AuditoriaService _auditoria;

        public AuthController(
            AppDbContext context,
            IConfiguration config,
            AuditoriaService auditoria)
        {
            _context = context;
            _config = config;
            _auditoria = auditoria;
        }

        // =====================================
        // LOGIN
        // =====================================

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Permisos)
                .FirstOrDefaultAsync(u =>
                    u.Username == request.Username &&
                    u.Password == request.Password &&
                    u.Activo
                );

            if (usuario == null)
            {
                await _auditoria.RegistrarAsync(
                    usuario: request.Username,
                    rol: "Desconocido",
                    accion: "Login",
                    modulo: "Auth",
                    detalle: "Intento de login fallido",
                    resultado: "Fallido"
                );
                return Unauthorized("Credenciales inválidas");
            }

            // CLAIMS
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds
            );

            // PERMISOS GRANULARES COMPLETOS
            var permisos = usuario.Permisos.Select(p => new
            {
                modulo = p.Modulo,
                puedeVer = p.PuedeVer,
                puedeCrear = p.PuedeCrear,
                puedeEditar = p.PuedeEditar,
                puedeEliminar = p.PuedeEliminar
            }).ToList();

            await _auditoria.RegistrarAsync(
                usuario: usuario.Username,
                rol: usuario.Rol,
                accion: "Login",
                modulo: "Auth",
                detalle: $"Login exitoso — Rol: {usuario.Rol}",
                resultado: "Exitoso"
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                username = usuario.Username,
                rol = usuario.Rol,
                email = usuario.Email,
                permisos = permisos
            });
        }

        // =====================================
        // LOGOUT
        // =====================================

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            await _auditoria.RegistrarAsync(
                usuario: request.Username,
                rol: request.Rol ?? "Desconocido",
                accion: "Logout",
                modulo: "Auth",
                detalle: "Cierre de sesión",
                resultado: "Exitoso"
            );
            return Ok();
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LogoutRequest
    {
        public string Username { get; set; } = string.Empty;
        public string? Rol { get; set; }
    }
}