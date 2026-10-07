using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SistemaFlota.DTOs.Usuarios;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;
        private static readonly string[] UsuariosOcultos = { "maestro_sf" };

        public static readonly string[] RolesValidos =
        {
            "Admin", "Auxiliar", "Conductor", "Jefe", "Facturacion", "Bodega",
            "Porteria", "RecursosHumanos", "PESV", "Vendedor", "Impresion",
            "Calidad", "SST", "Compras", "Precorte", "Extrusion", "Sellado",
            "Produccion"
        };

        public UsuariosController(AppDbContext context, AuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        private string GetUsuario() => User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";
        private string GetRol() => User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

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
                    (u.Email != null && u.Email.Contains(buscar)) ||
                    (u.Nombres != null && u.Nombres.Contains(buscar)) ||
                    (u.Apellidos != null && u.Apellidos.Contains(buscar)) ||
                    (u.Telefono != null && u.Telefono.Contains(buscar)));

            var total = await query.CountAsync();

            var lista = await query
                .OrderBy(u => u.Username)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Nombres,
                    u.Apellidos,
                    u.Telefono,
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
                        p.PuedeEliminar,
                        p.PuedeEnviarCorreo,
                        p.PuedeVerDatosNumericos
                    }).ToList()
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

        [HttpGet("destinatarios")]
        [Authorize]
        public async Task<IActionResult> GetDestinatarios()
        {
            var usuarios = await _context.Usuarios
                .Where(u => u.Activo &&
                            !string.IsNullOrWhiteSpace(u.Email) &&
                            !UsuariosOcultos.Contains(u.Username))
                .OrderBy(u => u.Username)
                .Select(u => new { u.Id, u.Username, u.Email, u.Rol })
                .ToListAsync();

            return Ok(usuarios);
        }

        [HttpGet("roles")]
        [Authorize(Roles = "Admin,RecursosHumanos,PESV")]
        public IActionResult GetRoles() => Ok(RolesValidos);

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
                    u.Nombres,
                    u.Apellidos,
                    u.Telefono,
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
                        p.PuedeEliminar,
                        p.PuedeEnviarCorreo,
                        p.PuedeVerDatosNumericos
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return usuario == null ? NotFound() : Ok(usuario);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,RecursosHumanos")]
        public async Task<IActionResult> Post([FromBody] CrearUsuarioDto dto)
        {
            if (UsuariosOcultos.Contains(dto.Username))
                return BadRequest("Nombre de usuario no permitido");

            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Usuario y contraseña son requeridos");

            if (dto.Password.Length < 6)
                return BadRequest("La contraseña debe tener al menos 6 caracteres");

            if (!RolesValidos.Contains(dto.Rol))
                return BadRequest($"Rol inválido. Roles permitidos: {string.Join(", ", RolesValidos)}");

            if (await _context.Usuarios.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("El nombre de usuario ya existe");

            var usuario = new Usuario
            {
                Username = dto.Username,
                Password = string.Empty,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Nombres = dto.Nombres,
                Apellidos = dto.Apellidos,
                Telefono = dto.Telefono,
                Rol = dto.Rol,
                Email = dto.Email,
                Activo = true
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            if (dto.Permisos != null && dto.Permisos.Count > 0)
            {
                bool inicioAsignado = false;

                foreach (var p in dto.Permisos)
                {
                    _context.UsuarioPermisos.Add(new UsuarioPermiso
                    {
                        UsuarioId = usuario.Id,
                        Modulo = p.Modulo,
                        PuedeVer = p.PuedeVer,
                        PuedeCrear = p.PuedeCrear,
                        PuedeEditar = p.PuedeEditar,
                        PuedeEliminar = p.PuedeEliminar,
                        PuedeEnviarCorreo = p.PuedeEnviarCorreo,
                        PuedeVerDatosNumericos = p.PuedeVerDatosNumericos,
                        EsInicio = p.PuedeVer && !inicioAsignado
                    });

                    if (p.PuedeVer && !inicioAsignado)
                        inicioAsignado = true;
                }

                await _context.SaveChangesAsync();
            }

            await _auditoria.RegistrarAsync(
                GetUsuario(), GetRol(), "Crear", "Usuarios",
                $"Usuario creado — Username: {dto.Username}, Rol: {dto.Rol}",
                usuario.Id);

            return Ok(new
            {
                usuario.Id,
                usuario.Username,
                usuario.Nombres,
                usuario.Apellidos,
                usuario.Telefono,
                usuario.Rol,
                usuario.Email,
                usuario.Activo,
                Permisos = dto.Permisos
            });
        }

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
            usuario.Nombres = dto.Nombres;
            usuario.Apellidos = dto.Apellidos;
            usuario.Telefono = dto.Telefono;
            usuario.Rol = dto.Rol;
            usuario.Email = dto.Email;
            usuario.Activo = dto.Activo;

            if (!string.IsNullOrEmpty(dto.Password))
            {
                if (dto.Password.Length < 6)
                    return BadRequest("La contraseña debe tener al menos 6 caracteres");

                usuario.Password = string.Empty;
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            _context.UsuarioPermisos.RemoveRange(usuario.Permisos);

            if (dto.Permisos != null && dto.Permisos.Count > 0)
            {
                bool inicioAsignado = false;

                foreach (var p in dto.Permisos)
                {
                    _context.UsuarioPermisos.Add(new UsuarioPermiso
                    {
                        UsuarioId = usuario.Id,
                        Modulo = p.Modulo,
                        PuedeVer = p.PuedeVer,
                        PuedeCrear = p.PuedeCrear,
                        PuedeEditar = p.PuedeEditar,
                        PuedeEliminar = p.PuedeEliminar,
                        PuedeEnviarCorreo = p.PuedeEnviarCorreo,
                        PuedeVerDatosNumericos = p.PuedeVerDatosNumericos,
                        EsInicio = p.PuedeVer && !inicioAsignado
                    });

                    if (p.PuedeVer && !inicioAsignado)
                        inicioAsignado = true;
                }
            }

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                GetUsuario(), GetRol(), "Editar", "Usuarios",
                $"Usuario editado — Username: {usernameAnterior}, Nuevo rol: {dto.Rol}",
                id);

            return Ok(new
            {
                usuario.Id,
                usuario.Username,
                usuario.Nombres,
                usuario.Apellidos,
                usuario.Telefono,
                usuario.Rol,
                usuario.Email,
                usuario.Activo,
                Permisos = dto.Permisos
            });
        }

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
                GetUsuario(), GetRol(), "Eliminar", "Usuarios",
                $"Usuario eliminado — Username: {nombreUsuario}",
                id);

            return Ok(new { mensaje = "Usuario eliminado correctamente" });
        }

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
                GetUsuario(), GetRol(), "Editar", "Usuarios",
                $"Estado cambiado — Username: {usuario.Username}, Activo: {usuario.Activo}",
                id);

            return Ok(new { usuario.Id, usuario.Username, usuario.Activo });
        }

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
                usuario.Username, usuario.Rol,
                "RecuperarPassword", "Usuarios",
                $"Solicitud de recuperación — Email: {dto.Email}");

            return Ok(new
            {
                mensaje = "Token generado correctamente",
                token,
                expira = usuario.TokenExpiracion
            });
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
            if (usuario.TokenExpiracion < DateTime.Now)
                return BadRequest("Token expirado");

            usuario.Password = string.Empty;
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaPassword);
            usuario.TokenRecuperacion = null;
            usuario.TokenExpiracion = null;

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario.Username, usuario.Rol,
                "CambiarPassword", "Usuarios",
                $"Contraseña cambiada — Email: {dto.Email}");

            return Ok(new { mensaje = "Contraseña cambiada correctamente" });
        }

        [HttpPut("{id}/password-modulo")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetPasswordModulo(
            int id,
            [FromBody] PasswordModuloDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null) return NotFound();

            usuario.PasswordConductores =
                BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Contrasena actualizada" });
        }

        [HttpPost("verificar-modulo")]
        [AllowAnonymous]
        public async Task<IActionResult> VerificarModulo(
            [FromBody] VerificarModuloDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Username == dto.Username && u.Activo);

            if (usuario == null)
                return Unauthorized(new { error = "Usuario no encontrado" });

            if (string.IsNullOrWhiteSpace(usuario.PasswordConductores))
                return Unauthorized(new { error = "Sin contrasena de modulo" });

            var ok = BCrypt.Net.BCrypt.Verify(
                dto.Password,
                usuario.PasswordConductores);

            if (!ok)
                return Unauthorized(new { error = "Contrasena incorrecta" });

            await _auditoria.RegistrarAsync(
                usuario.Username,
                usuario.Rol,
                "Acceso",
                "Conductores",
                "Acceso modulo Conductores",
                resultado: "Exitoso");

            return Ok(new { mensaje = "Acceso permitido" });
        }

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
                p.PuedeEliminar,
                p.PuedeEnviarCorreo,
                p.PuedeVerDatosNumericos
            }).ToList();

            return Ok(new { permisos });
        }
    }
}