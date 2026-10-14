
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SistemaFlota.Authorization
{
    public class PermisoAuthorizationHandler
        : AuthorizationHandler<PermisoRequirement>
    {
        private readonly AppDbContext _context;

        public PermisoAuthorizationHandler(AppDbContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermisoRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
                return;

            var rol = context.User.FindFirst(
                ClaimTypes.Role)?.Value;

            // Admin tiene acceso total
            if (rol == "Admin")
            {
                context.Succeed(requirement);
                return;
            }

            var idClaim = context.User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idClaim, out var usuarioId))
                return;

            var partes = requirement.PolicyName.Split(':');

            // Permiso normal: Permiso:modulo:accion
            if (partes.Length == 3 &&
                partes[0] == "Permiso")
            {
                var modulo = partes[1];
                var accion = partes[2];

                var permiso = await _context.UsuarioPermisos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p =>
                        p.UsuarioId == usuarioId &&
                        p.Modulo == modulo);

                if (permiso != null &&
                    AccionPermitida(permiso, accion))
                {
                    context.Succeed(requirement);
                }

                return;
            }

            // Permiso alternativo:
            // PermisoAlternativo:modulo1:modulo2:accion
            if (partes.Length == 4 &&
                partes[0] == "PermisoAlternativo")
            {
                var modulo1 = partes[1];
                var modulo2 = partes[2];
                var accion = partes[3];

                var permisos = await _context.UsuarioPermisos
                    .AsNoTracking()
                    .Where(p =>
                        p.UsuarioId == usuarioId &&
                        (p.Modulo == modulo1 ||
                         p.Modulo == modulo2))
                    .ToListAsync();

                // Basta con que tenga el permiso en uno
                // de los dos módulos.
                var permitido = permisos.Any(
                    p => AccionPermitida(p, accion));

                if (permitido)
                {
                    context.Succeed(requirement);
                }
            }
        }

        private static bool AccionPermitida(
            UsuarioPermiso permiso,
            string accion)
        {
            return accion switch
            {
                "ver" => permiso.PuedeVer,
                "crear" => permiso.PuedeCrear,
                "editar" => permiso.PuedeEditar,
                "eliminar" => permiso.PuedeEliminar,
                "enviar-correo" => permiso.PuedeEnviarCorreo,
                _ => false
            };
        }
    }
}
