using System.Security.Claims;

namespace SistemaFlota.Services.Auth;

public interface ICurrentUserService
{
    int? IdUsuario { get; }
    string? Usuario { get; }
    string? Email { get; }
    string? Rol { get; }

    Claim? ObtenerClaim(string tipo);
}