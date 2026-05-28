using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;

        public DocumentosController(
            AppDbContext context,
            AuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        private string GetUsuario() =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";
        private string GetRol() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

        // =====================================
        // DOCUMENTOS VEHÍCULO — GET
        // =====================================

        [HttpGet("vehiculo/{vehiculoId}")]
        public async Task<IActionResult> GetDocumentosVehiculo(int vehiculoId)
        {
            var docs = await _context.DocumentosVehiculo
                .Include(d => d.Vehiculo)
                .Where(d => d.VehiculoId == vehiculoId && d.Activo)
                .OrderByDescending(d => d.FechaSubida)
                .ToListAsync();

            return Ok(docs);
        }

        // =====================================
        // DOCUMENTOS VEHÍCULO — POST
        // =====================================

        [HttpPost("vehiculo")]
        public async Task<IActionResult> SubirDocumentoVehiculo(
            [FromForm] int VehiculoId,
            [FromForm] string TipoDocumento,
            [FromForm] string Nombre,
            [FromForm] string? Descripcion,
            [FromForm] DateTime? FechaVencimiento,
            IFormFile Archivo
        )
        {
            try
            {
                var carpeta = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/documentos/vehiculos"
                );

                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                var extension = Path.GetExtension(Archivo.FileName);
                var nombreArchivo = Guid.NewGuid().ToString() + extension;
                var ruta = Path.Combine(carpeta, nombreArchivo);

                using var stream = new FileStream(ruta, FileMode.Create);
                await Archivo.CopyToAsync(stream);

                var doc = new DocumentoVehiculo
                {
                    VehiculoId = VehiculoId,
                    TipoDocumento = TipoDocumento,
                    Nombre = Nombre,
                    Descripcion = Descripcion,
                    ArchivoUrl = nombreArchivo,
                    Extension = extension,
                    FechaSubida = DateTime.Now,
                    FechaVencimiento = FechaVencimiento,
                    Activo = true
                };

                _context.DocumentosVehiculo.Add(doc);
                await _context.SaveChangesAsync();

                var vehiculo = await _context.Vehiculos.FindAsync(VehiculoId);

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(),
                    rol: GetRol(),
                    accion: "Subir",
                    modulo: "Documentos",
                    detalle: $"Documento vehículo subido — Vehículo: {vehiculo?.Placa ?? "-"}, Tipo: {TipoDocumento}, Nombre: {Nombre}",
                    registroId: doc.Id
                );

                return Ok(doc);
            }
            catch (Exception ex)
            {
                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(),
                    rol: GetRol(),
                    accion: "Subir",
                    modulo: "Documentos",
                    detalle: $"Error subiendo documento vehículo: {ex.Message}",
                    resultado: "Fallido"
                );
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // =====================================
        // DOCUMENTOS VEHÍCULO — DELETE
        // =====================================

        [HttpDelete("vehiculo/{id}")]
        public async Task<IActionResult> EliminarDocumentoVehiculo(int id)
        {
            var doc = await _context.DocumentosVehiculo
                .Include(d => d.Vehiculo)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc == null) return NotFound();

            doc.Activo = false;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(),
                rol: GetRol(),
                accion: "Eliminar",
                modulo: "Documentos",
                detalle: $"Documento vehículo eliminado — Nombre: {doc.Nombre}, Vehículo: {doc.Vehiculo?.Placa ?? "-"}",
                registroId: id
            );

            return Ok();
        }

        // =====================================
        // DOCUMENTOS GENERALES — GET
        // =====================================

        [HttpGet("generales")]
        public async Task<IActionResult> GetDocumentosGenerales(
            [FromQuery] string? categoria)
        {
            var query = _context.DocumentosGenerales
                .Where(d => d.Activo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(categoria))
                query = query.Where(d => d.Categoria == categoria);

            var docs = await query
                .OrderByDescending(d => d.FechaSubida)
                .ToListAsync();

            return Ok(docs);
        }

        // =====================================
        // DOCUMENTOS GENERALES — POST
        // =====================================

        [HttpPost("generales")]
        public async Task<IActionResult> SubirDocumentoGeneral(
            [FromForm] string Nombre,
            [FromForm] string? Descripcion,
            [FromForm] string Categoria,
            [FromForm] DateTime? FechaVencimiento,
            IFormFile Archivo
        )
        {
            try
            {
                var carpeta = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/documentos/generales"
                );

                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                var extension = Path.GetExtension(Archivo.FileName);
                var nombreArchivo = Guid.NewGuid().ToString() + extension;
                var ruta = Path.Combine(carpeta, nombreArchivo);

                using var stream = new FileStream(ruta, FileMode.Create);
                await Archivo.CopyToAsync(stream);

                var doc = new DocumentoGeneral
                {
                    Nombre = Nombre,
                    Descripcion = Descripcion,
                    Categoria = Categoria,
                    ArchivoUrl = nombreArchivo,
                    Extension = extension,
                    TamanoBytes = Archivo.Length,
                    FechaSubida = DateTime.Now,
                    FechaVencimiento = FechaVencimiento,
                    Activo = true
                };

                _context.DocumentosGenerales.Add(doc);
                await _context.SaveChangesAsync();

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(),
                    rol: GetRol(),
                    accion: "Subir",
                    modulo: "Documentos",
                    detalle: $"Documento general subido — Nombre: {Nombre}, Categoría: {Categoria}",
                    registroId: doc.Id
                );

                return Ok(doc);
            }
            catch (Exception ex)
            {
                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(),
                    rol: GetRol(),
                    accion: "Subir",
                    modulo: "Documentos",
                    detalle: $"Error subiendo documento general: {ex.Message}",
                    resultado: "Fallido"
                );
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // =====================================
        // DOCUMENTOS GENERALES — DELETE
        // =====================================

        [HttpDelete("generales/{id}")]
        public async Task<IActionResult> EliminarDocumentoGeneral(int id)
        {
            var doc = await _context.DocumentosGenerales.FindAsync(id);
            if (doc == null) return NotFound();

            var nombre = doc.Nombre;
            doc.Activo = false;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(),
                rol: GetRol(),
                accion: "Eliminar",
                modulo: "Documentos",
                detalle: $"Documento general eliminado — Nombre: {nombre}",
                registroId: id
            );

            return Ok();
        }

        // =====================================
        // DOCUMENTOS POR VENCER
        // =====================================

        [HttpGet("por-vencer")]
        public async Task<IActionResult> GetPorVencer()
        {
            var fecha = DateTime.Now.AddDays(30);

            var docsVehiculo = await _context.DocumentosVehiculo
                .Include(d => d.Vehiculo)
                .Where(d =>
                    d.Activo &&
                    d.FechaVencimiento != null &&
                    d.FechaVencimiento <= fecha)
                .Select(d => new
                {
                    d.Id,
                    d.Nombre,
                    d.TipoDocumento,
                    d.FechaVencimiento,
                    Tipo = "vehiculo",
                    Vehiculo = d.Vehiculo!.Placa
                })
                .ToListAsync();

            var docsGenerales = await _context.DocumentosGenerales
                .Where(d =>
                    d.Activo &&
                    d.FechaVencimiento != null &&
                    d.FechaVencimiento <= fecha)
                .Select(d => new
                {
                    d.Id,
                    d.Nombre,
                    TipoDocumento = d.Categoria,
                    d.FechaVencimiento,
                    Tipo = "general",
                    Vehiculo = ""
                })
                .ToListAsync();

            return Ok(new
            {
                vehiculo = docsVehiculo,
                generales = docsGenerales,
                total = docsVehiculo.Count + docsGenerales.Count
            });
        }
    }
}