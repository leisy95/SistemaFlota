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

            if (partes.Length != 3)
                return;

            var modulo = partes[1];
            var accion = partes[2];

            var permiso = await _context.UsuarioPermisos
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.UsuarioId == usuarioId &&
                    p.Modulo == modulo);

            if (permiso == null)
                return;

            var permitido = accion switch
            {
                "ver" => permiso.PuedeVer,
                "crear" => permiso.PuedeCrear,
                "editar" => permiso.PuedeEditar,
                "eliminar" => permiso.PuedeEliminar,
                _ => false
            };

            if (permitido)
            {
                context.Succeed(requirement);
            }
        }
    }
}