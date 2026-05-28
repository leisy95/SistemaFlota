using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;

namespace SistemaFlota
{
    [ApiController]

    [Route("api/[controller]")]

    [Authorize]

    public class VehiculosController
        : ControllerBase
    {
        private readonly AppDbContext
            _context;

        public VehiculosController(
            AppDbContext context
        )
        {
            _context = context;
        }

        // =========================================
        // GET
        // =========================================

        [HttpGet]

        public async Task<IActionResult>
            Get()
        {
            var lista =

                await _context
                .Vehiculos

                .Include(v =>
                    v.Conductor
                )

                .ToListAsync();

            return Ok(lista);
        }

        // =========================================
        // POST
        // =========================================

        [HttpPost]

        public async Task<IActionResult>
            Post(

                [FromForm]
                string Placa,

                [FromForm]
                string Marca,

                [FromForm]
                string Modelo,

                [FromForm(Name = "Año")]
                int anio,

                [FromForm]
                string Color,

                [FromForm]
                string Estado,

                [FromForm]
                int ConductorId,

                IFormFile? foto
            )
        {
            try
            {
                string? nombreArchivo =
                    null;

                // CARPETA

                var carpeta =

                    Path.Combine(

                        Directory
                        .GetCurrentDirectory(),

                        "wwwroot/vehiculos"
                    );

                // CREAR CARPETA

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

                // CREAR

                var vehiculo =

                    new Vehiculo
                    {
                        Placa =
                            Placa,

                        Marca =
                            Marca,

                        Modelo =
                            Modelo,

                        Año =
                            anio,

                        Color =
                            Color,

                        Estado =
                            Estado,

                        ConductorId =
                            ConductorId,

                        Foto =
                            nombreArchivo
                    };

                _context
                    .Vehiculos
                    .Add(vehiculo);

                await _context
                    .SaveChangesAsync();

                return Ok(vehiculo);
            }

            catch (Exception ex)
            {
                return StatusCode(

                    500,

                    ex.InnerException?.Message
                    ??

                    ex.Message
                );
            }
        }

        // =========================================
        // PUT
        // =========================================

        [HttpPut("{id}")]

        public async Task<IActionResult>
            Put(

                int id,

                [FromForm]
                string Placa,

                [FromForm]
                string Marca,

                [FromForm]
                string Modelo,

                [FromForm(Name = "Año")]
                int anio,

                [FromForm]
                string Color,

                [FromForm]
                string Estado,

                [FromForm]
                int ConductorId,

                IFormFile? foto
            )
        {
            try
            {
                var vehiculo =

                    await _context
                    .Vehiculos
                    .FindAsync(id);

                if (vehiculo == null)
                {
                    return NotFound();
                }

                // ACTUALIZAR

                vehiculo.Placa =
                    Placa;

                vehiculo.Marca =
                    Marca;

                vehiculo.Modelo =
                    Modelo;

                vehiculo.Año =
                    anio;

                vehiculo.Color =
                    Color;

                vehiculo.Estado =
                    Estado;

                vehiculo.ConductorId =
                    ConductorId;

                // FOTO

                if (foto != null)
                {
                    string carpeta =

                        Path.Combine(

                            Directory
                            .GetCurrentDirectory(),

                            "wwwroot/vehiculos"
                        );

                    if (
                        !Directory.Exists(
                            carpeta
                        )
                    )
                    {
                        Directory.CreateDirectory(
                            carpeta
                        );
                    }

                    string nombreArchivo =

                        Guid.NewGuid()
                        .ToString()

                        +

                        Path.GetExtension(
                            foto.FileName
                        );

                    string ruta =

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

                    vehiculo.Foto =
                        nombreArchivo;
                }

                await _context
                    .SaveChangesAsync();

                return Ok(vehiculo);
            }

            catch (Exception ex)
            {
                return StatusCode(

                    500,

                    ex.InnerException?.Message
                    ??

                    ex.Message
                );
            }
        }

        // =========================================
        // DELETE
        // =========================================

        [HttpDelete("{id}")]

        public async Task<IActionResult>
            Delete(int id)
        {
            var vehiculo =

                await _context
                .Vehiculos
                .FindAsync(id);

            if (vehiculo == null)
            {
                return NotFound();
            }

            _context
                .Vehiculos
                .Remove(vehiculo);

            await _context
                .SaveChangesAsync();

            return Ok();
        }
    }
}