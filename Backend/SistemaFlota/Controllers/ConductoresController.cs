using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;

using SistemaFlota.DTOs;

namespace SistemaFlota
{
    [ApiController]

    [Route("api/[controller]")]

    [Authorize]

    public class ConductoresController
        : ControllerBase
    {
        private readonly AppDbContext
            _context;

        public ConductoresController(
            AppDbContext context
        )
        {
            _context = context;
        }

        // =========================
        // HELPERS
        // =========================

       
        private string? NormalizarTelefono(string? telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return telefono;

            var limpio = new string(telefono.Where(char.IsDigit).ToArray());

            if (limpio.Length == 10 && !limpio.StartsWith("57"))
                limpio = "57" + limpio;

            return limpio;
        }

        // =========================
        // GET
        // =========================

        [HttpGet]
        [Authorize(Roles = "Admin,Conductor,Facturacion,jefe,Vendedor,Bodega")]

        public async Task<IActionResult>
            Get()
        {
            var lista =

                await _context
                .Conductores

                .Select(c =>

                    new ConductorDto
                    {
                        Id =
                            c.Id,

                        Nombre =
                            c.Nombre,

                        Licencia =
                            c.Licencia,

                        Telefono =
                            c.Telefono,

                        Email =
                            c.Email,

                        Foto =
                            c.Foto
                    })

                .ToListAsync();

            return Ok(lista);
        }

        // =========================
        // POST
        // =========================

        [HttpPost]

        public async Task<IActionResult>
            Post(

                [FromForm]
                string Nombre,

                [FromForm]
                string Licencia,

                [FromForm]
                string Telefono,

                [FromForm]
                string Email,

                IFormFile? foto

            )
        {
            try
            {
                string? nombreArchivo =
                    null;

                var carpeta =

                    Path.Combine(

                        Directory
                        .GetCurrentDirectory(),

                        "wwwroot/fotos"
                    );

                if (!Directory.Exists(carpeta))
                {
                    Directory.CreateDirectory(
                        carpeta
                    );
                }

                // FOTO

                if (
                    foto != null
                    &&
                    foto.Length > 0
                )
                {
                    nombreArchivo =

                        Guid.NewGuid()
                        .ToString()

                        +

                        Path.GetExtension(
                            foto.FileName
                        );

                    var ruta =

                        Path.Combine(
                            carpeta,
                            nombreArchivo
                        );

                    using (

                        var stream =

                            new FileStream(

                                ruta,

                                FileMode.Create
                            )

                    )
                    {
                        await foto
                            .CopyToAsync(
                                stream
                            );
                    }
                }

                var conductor =

                    new Conductor
                    {
                        Nombre =
                            Nombre,

                        Licencia =
                            Licencia,

                        Telefono =
                            NormalizarTelefono(Telefono),

                        Email =
                            Email,

                        Foto =
                            nombreArchivo
                    };

                _context
                    .Conductores
                    .Add(conductor);

                await _context
                    .SaveChangesAsync();

                return Ok(conductor);
            }

            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    ex.Message
                );
            }
        }

        // =========================
        // PUT
        // =========================

        [HttpPut("{id}")]

        public async Task<IActionResult>
            Put(

                int id,

                [FromForm]
                string Nombre,

                [FromForm]
                string Licencia,

                [FromForm]
                string Telefono,

                [FromForm]
                string Email,

                IFormFile? foto

            )
        {
            try
            {
                var conductor =

                    await _context
                    .Conductores
                    .FindAsync(id);

                if (conductor == null)
                {
                    return NotFound();
                }

                conductor.Nombre =
                    Nombre;

                conductor.Licencia =
                    Licencia;

                conductor.Telefono =
                    NormalizarTelefono(Telefono);

                conductor.Email =
                    Email;

                // FOTO

                if (
                    foto != null
                    &&
                    foto.Length > 0
                )
                {
                    var carpeta =

                        Path.Combine(

                            Directory
                            .GetCurrentDirectory(),

                            "wwwroot/fotos"
                        );

                    if (!Directory.Exists(carpeta))
                    {
                        Directory.CreateDirectory(
                            carpeta
                        );
                    }

                    var nombreArchivo =

                        Guid.NewGuid()
                        .ToString()

                        +

                        Path.GetExtension(
                            foto.FileName
                        );

                    var ruta =

                        Path.Combine(
                            carpeta,
                            nombreArchivo
                        );

                    using (

                        var stream =

                            new FileStream(

                                ruta,

                                FileMode.Create
                            )

                    )
                    {
                        await foto
                            .CopyToAsync(
                                stream
                            );
                    }

                    conductor.Foto =
                        nombreArchivo;
                }

                await _context
                    .SaveChangesAsync();

                return Ok(conductor);
            }

            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    ex.Message
                );
            }
        }

        // =========================
        // DELETE
        // =========================

        [HttpDelete("{id}")]

        public async Task<IActionResult>
            Delete(int id)
        {
            var conductor =

                await _context
                .Conductores
                .FindAsync(id);

            if (conductor == null)
            {
                return NotFound();
            }

            _context
                .Conductores
                .Remove(conductor);

            await _context
                .SaveChangesAsync();

            return Ok();
        }
    }
}