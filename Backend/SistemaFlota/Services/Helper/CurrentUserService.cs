using System.Security.Claims;

namespace SistemaFlota.Services.Auth;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? UsuarioActual =>
        _httpContextAccessor.HttpContext?.User;

    public int? IdUsuario
    {
        get
        {
            var claims = UsuarioActual?.Claims;

            foreach (var claim in claims ?? [])
            {
                Console.WriteLine($"{claim.Type} => {claim.Value}");
            }

            var valor = UsuarioActual?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(valor, out var id)
                ? id
                : null;
        }
    }

    public string? Usuario =>
        UsuarioActual?
            .FindFirst(ClaimTypes.Name)?.Value;

    public string? Rol =>
    UsuarioActual?
        .FindFirst(ClaimTypes.Role)?.Value;

    public string? Email =>
        UsuarioActual?
            .FindFirst(ClaimTypes.Email)?.Value;

    public Claim? ObtenerClaim(string tipo)
    {
        return UsuarioActual?.FindFirst(tipo);
    }
}