using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private const string V = "Impresion";
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;

        private static readonly string[] UsuariosOcultos = { "maestro_sf" };

        // ── Roles válidos del sistema ─────────────────────────────────────────
        public static readonly string[] RolesValidos = {
        "Admin", "Auxiliar", "Conductor", "Jefe",
        "Facturacion", "Bodega", "Porteria",
        "RecursosHumanos", "PESV", "Vendedor", "Impresion"
};

        public UsuariosController(AppDbContext context, AuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        private string GetUsuario() =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";
        private string GetRol() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

        // =====================================
        // GET TODOS — con paginación
        // =====================================
        [HttpGet]
        [Authorize(Roles = "Admin,RecursosHumanos,PESV")]
        public async Task<IActionResult> Get(
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 20,
            [FromQuery] string? buscar = null)
        {
            var query = _context.Usuarios
                .Include(u => u.Permisos)
                .Where(u => !UsuariosOcultos.Contains(u.Username))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
                query = query.Where(u =>
                    u.Username.Contains(buscar) ||
                    (u.Email != null && u.Email.Contains(buscar)));

            var total = await query.CountAsync();

            var lista = await query
                .OrderBy(u => u.Username)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Rol,
                    u.Email,
                    u.Activo,
                    Permisos = u.Permisos.Select(p => new
                    {
                        p.Id,
                        p.Modulo,
                        p.PuedeVer,
                        p.PuedeCrear,
                        p.PuedeEditar,
                        p.PuedeEliminar
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new
            {
                data = lista,
                total = total,
                pagina = pagina,
                porPagina = porPagina,
                totalPaginas = (int)Math.Ceiling((double)total / porPagina)
            });
        }

        // =====================================
        // GET ROLES VÁLIDOS — para el frontend
        // =====================================
        [HttpGet("roles")]
        [Authorize(Roles = "Admin,RecursosHumanos,PESV")]
        public IActionResult GetRoles()
        {
            return Ok(RolesValidos);
        }

        // =====================================
        // GET POR ID
        // =====================================
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,RecursosHumanos,PESV")]
        public async Task<IActionResult> GetById(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Permisos)
                .Where(u => u.Id == id && !UsuariosOcultos.Contains(u.Username))
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Rol,
                    u.Email,
                    u.Activo,
                    Permisos = u.Permisos.Select(p => new
                    {
                        p.Id,
                        p.Modulo,
                        p.PuedeVer,
                        p.PuedeCrear,
                        p.PuedeEditar,
                        p.PuedeEliminar
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (usuario == null) return NotFound();
            return Ok(usuario);
        }

        // =====================================
        // POST — CREAR USUARIO
        // =====================================
        [HttpPost]
        [Authorize(Roles = "Admin,RecursosHumanos")]
        public async Task<IActionResult> Post([FromBody] CrearUsuarioDto dto)
        {
            if (UsuariosOcultos.Contains(dto.Username))
                return BadRequest("Nombre de usuario no permitido");

            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Usuario y contraseña son requeridos");

            if (dto.Password.Length < 6)
                return BadRequest("La contraseña debe tener al menos 6 caracteres");

            // Validar que el rol sea válido
            if (!RolesValidos.Contains(dto.Rol))
                return BadRequest($"Rol inválido. Roles permitidos: {string.Join(", ", RolesValidos)}");

            var existe = await _context.Usuarios
                .AnyAsync(u => u.Username == dto.Username);

            if (existe)
                return BadRequest("El nombre de usuario ya existe");

            var usuario = new Usuario
            {
                Username = dto.Username,
                Password = dto.Password,
                Rol = dto.Rol,
                Email = dto.Email,
                Activo = true
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            if (dto.Permisos != null && dto.Permisos.Count > 0)
            {
                foreach (var p in dto.Permisos)
                {
                    _context.UsuarioPermisos.Add(new UsuarioPermiso
                    {
                        UsuarioId = usuario.Id,
                        Modulo = p.Modulo,
                        PuedeVer = p.PuedeVer,
                        PuedeCrear = p.PuedeCrear,
                        PuedeEditar = p.PuedeEditar,
                        PuedeEliminar = p.PuedeEliminar
                    });
                }
                await _context.SaveChangesAsync();
            }

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Crear", modulo: "Usuarios",
                detalle: $"Usuario creado — Username: {dto.Username}, Rol: {dto.Rol}",
                registroId: usuario.Id
            );

            return Ok(new
            {
                usuario.Id,
                usuario.Username,
                usuario.Rol,
                usuario.Email,
                usuario.Activo,
                Permisos = dto.Permisos
            });
        }

        // =====================================
        // PUT — EDITAR USUARIO
        // =====================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,RecursosHumanos")]
        public async Task<IActionResult> Put(int id, [FromBody] CrearUsuarioDto dto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Permisos)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null) return NotFound();
            if (UsuariosOcultos.Contains(usuario.Username)) return Forbid();

            if (!RolesValidos.Contains(dto.Rol))
                return BadRequest($"Rol inválido. Roles permitidos: {string.Join(", ", RolesValidos)}");

            var usernameAnterior = usuario.Username;
            usuario.Username = dto.Username;
            usuario.Rol = dto.Rol;
            usuario.Email = dto.Email;
            usuario.Activo = dto.Activo;

            if (!string.IsNullOrEmpty(dto.Password))
            {
                if (dto.Password.Length < 6)
                    return BadRequest("La contraseña debe tener al menos 6 caracteres");
                usuario.Password = dto.Password;
            }

            _context.UsuarioPermisos.RemoveRange(usuario.Permisos);

            if (dto.Permisos != null && dto.Permisos.Count > 0)
            {
                foreach (var p in dto.Permisos)
                {
                    _context.UsuarioPermisos.Add(new UsuarioPermiso
                    {
                        UsuarioId = usuario.Id,
                        Modulo = p.Modulo,
                        PuedeVer = p.PuedeVer,
                        PuedeCrear = p.PuedeCrear,
                        PuedeEditar = p.PuedeEditar,
                        PuedeEliminar = p.PuedeEliminar
                    });
                }
            }

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Editar", modulo: "Usuarios",
                detalle: $"Usuario editado — Username: {usernameAnterior}, Nuevo rol: {dto.Rol}",
                registroId: id
            );

            return Ok(new
            {
                usuario.Id,
                usuario.Username,
                usuario.Rol,
                usuario.Email,
                usuario.Activo,
                Permisos = dto.Permisos
            });
        }

        // =====================================
        // DELETE
        // =====================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Permisos)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null) return NotFound();
            if (UsuariosOcultos.Contains(usuario.Username)) return Forbid();

            var nombreUsuario = usuario.Username;
            _context.UsuarioPermisos.RemoveRange(usuario.Permisos);
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Eliminar", modulo: "Usuarios",
                detalle: $"Usuario eliminado — Username: {nombreUsuario}",
                registroId: id
            );

            return Ok(new { mensaje = "Usuario eliminado correctamente" });
        }

        // =====================================
        // CAMBIAR ESTADO
        // =====================================
        [HttpPut("{id}/estado")]
        [Authorize(Roles = "Admin,RecursosHumanos")]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            if (UsuariosOcultos.Contains(usuario.Username)) return Forbid();

            usuario.Activo = !usuario.Activo;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Editar", modulo: "Usuarios",
                detalle: $"Estado cambiado — Username: {usuario.Username}, Activo: {usuario.Activo}",
                registroId: id
            );

            return Ok(new { usuario.Id, usuario.Username, usuario.Activo });
        }

        // =====================================
        // RECUPERAR CONTRASEÑA
        // =====================================
        [HttpPost("recuperar")]
        [AllowAnonymous]
        public async Task<IActionResult> SolicitarRecuperacion([FromBody] RecuperarDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario == null)
                return Ok(new { mensaje = "Si el correo existe, recibirás el token" });

            var token = Guid.NewGuid().ToString("N")[..8].ToUpper();
            usuario.TokenRecuperacion = token;
            usuario.TokenExpiracion = DateTime.Now.AddHours(1);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: usuario.Username, rol: usuario.Rol,
                accion: "RecuperarPassword", modulo: "Usuarios",
                detalle: $"Solicitud de recuperación — Email: {dto.Email}"
            );

            return Ok(new { mensaje = "Token generado correctamente", token, expira = usuario.TokenExpiracion });
        }

        [HttpPost("cambiar-password")]
        [AllowAnonymous]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
        {
            if (dto.NuevaPassword.Length < 6)
                return BadRequest("La contraseña debe tener al menos 6 caracteres");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Email == dto.Email &&
                    u.TokenRecuperacion == dto.Token);

            if (usuario == null) return BadRequest("Token inválido");
            if (usuario.TokenExpiracion < DateTime.Now) return BadRequest("Token expirado");

            usuario.Password = dto.NuevaPassword;
            usuario.TokenRecuperacion = null;
            usuario.TokenExpiracion = null;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: usuario.Username, rol: usuario.Rol,
                accion: "CambiarPassword", modulo: "Usuarios",
                detalle: $"Contraseña cambiada — Email: {dto.Email}"
            );

            return Ok(new { mensaje = "Contraseña cambiada correctamente" });
        }

        // =====================================
        // MIS PERMISOS
        // =====================================
        [HttpGet("mis-permisos")]
        [Authorize]
        public async Task<IActionResult> MisPermisos()
        {
            var username = User.Identity?.Name;
            var usuario = await _context.Usuarios
                .Include(u => u.Permisos)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (usuario == null) return NotFound();

            var permisos = usuario.Permisos.Select(p => new
            {
                p.Modulo,
                p.PuedeVer,
                p.PuedeCrear,
                p.PuedeEditar,
                p.PuedeEliminar
            }).ToList();

            return Ok(new { permisos });
        }
    }

    // DTOs
    public class CrearUsuarioDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool Activo { get; set; } = true;
        public List<PermisoDto>? Permisos { get; set; }
    }

    public class PermisoDto
    {
        public string Modulo { get; set; } = string.Empty;
        public bool PuedeVer { get; set; } = true;
        public bool PuedeCrear { get; set; } = false;
        public bool PuedeEditar { get; set; } = false;
        public bool PuedeEliminar { get; set; } = false;
    }

    public class RecuperarDto { public string Email { get; set; } = string.Empty; }
    public class CambiarPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NuevaPassword { get; set; } = string.Empty;
    }
}