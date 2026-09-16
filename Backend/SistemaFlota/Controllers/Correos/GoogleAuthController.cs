using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Services.Auth;
using SistemaFlota.Services.Correos;

namespace SistemaFlota.Controllers.Correos;

[ApiController]
[Route("api/[controller]")]
public class GoogleAuthController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly ICurrentUserService _currentUser;

    public GoogleAuthController(
        IGoogleAuthService googleAuthService,
        ICurrentUserService currentUser)
    {
        _googleAuthService = googleAuthService;
        _currentUser = currentUser;
    }

    [HttpGet("conectar")]
    [Authorize]
    public async Task<IActionResult> Conectar()
    {
        if (_currentUser.IdUsuario == null)
        {
            return Unauthorized(new
            {
                mensaje = "No se pudo identificar al usuario."
            });
        }

        var url = await _googleAuthService
            .GenerarUrlAutorizacionAsync(
                _currentUser.IdUsuario.Value);

        return Redirect(url);
    }

    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback(
        [FromQuery] string code,
        [FromQuery] string state)
    {
        await _googleAuthService.ProcesarCallbackAsync(
            code,
            state);

        return Ok(new
        {
            mensaje = "Cuenta de Gmail autorizada correctamente."
        });
    }
}