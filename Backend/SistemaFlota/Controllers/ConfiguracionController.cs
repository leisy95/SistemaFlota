using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConfiguracionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ConfiguracionController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================
        // GET CONFIGURACIÓN
        // =====================================

        [HttpGet]
        [AllowAnonymous]  // ← agrega esta línea
        public async Task<IActionResult> Get()
        {
            var config = await _context.ConfiguracionEmpresa
                .FirstOrDefaultAsync();

            if (config == null)
            {
                return Ok(new ConfiguracionEmpresa
                {
                    Id = 0,
                    NombreEmpresa = "",
                    NIT = "",
                    Direccion = "",
                    Telefono = "",
                    Email = "",
                    ColorCorporativo = "#15803d"
                });
            }

            return Ok(config);
        }

        // =====================================
        // POST / PUT — GUARDAR CONFIGURACIÓN
        // =====================================

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Guardar(
            [FromForm] string NombreEmpresa,
            [FromForm] string NIT,
            [FromForm] string Direccion,
            [FromForm] string Telefono,
            [FromForm] string Email,
            [FromForm] string ColorCorporativo,
            [FromForm] string? SitioWeb,
            [FromForm] string? Descripcion,
            IFormFile? Logo
        )
        {
            try
            {
                var config = await _context.ConfiguracionEmpresa
                    .FirstOrDefaultAsync();

                // LOGO
                string? nombreLogo = config?.Logo;

                if (Logo != null)
                {
                    var carpeta = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/config"
                    );

                    if (!Directory.Exists(carpeta))
                        Directory.CreateDirectory(carpeta);

                    nombreLogo = "logo" + Path.GetExtension(Logo.FileName);
                    var ruta = Path.Combine(carpeta, nombreLogo);

                    using var stream = new FileStream(ruta, FileMode.Create);
                    await Logo.CopyToAsync(stream);
                }

                if (config == null)
                {
                    config = new ConfiguracionEmpresa
                    {
                        NombreEmpresa = NombreEmpresa,
                        NIT = NIT,
                        Direccion = Direccion,
                        Telefono = Telefono,
                        Email = Email,
                        ColorCorporativo = ColorCorporativo,
                        SitioWeb = SitioWeb,
                        Descripcion = Descripcion,
                        Logo = nombreLogo
                    };
                    _context.ConfiguracionEmpresa.Add(config);
                }
                else
                {
                    config.NombreEmpresa = NombreEmpresa;
                    config.NIT = NIT;
                    config.Direccion = Direccion;
                    config.Telefono = Telefono;
                    config.Email = Email;
                    config.ColorCorporativo = ColorCorporativo;
                    config.SitioWeb = SitioWeb;
                    config.Descripcion = Descripcion;
                    if (nombreLogo != null)
                        config.Logo = nombreLogo;
                }

                await _context.SaveChangesAsync();
                return Ok(config);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}