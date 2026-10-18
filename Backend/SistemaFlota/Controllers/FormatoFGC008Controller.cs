using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FormatoFGC008Controller : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;
        private readonly EmpresaOrdenesService _empresaOrdenes;

        public FormatoFGC008Controller(
            AppDbContext context,
            AuditoriaService auditoria,
            EmpresaOrdenesService empresaOrdenes)
        {
            _context = context;
            _auditoria = auditoria;
            _empresaOrdenes = empresaOrdenes;
        }

        private string GetUsuario() =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";

        private string GetRol() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

        // ── Ayudas ──────────────────────────────────────────────────────────

        private IQueryable<FormatoFGC008> Filtrar(DateTime? desde, DateTime? hasta, string? op)
        {
            var query = _context.FormatosFGC008.AsQueryable();

            if (desde.HasValue)
                query = query.Where(f => f.Fecha >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(f => f.Fecha <= hasta.Value.AddDays(1));

            if (!string.IsNullOrWhiteSpace(op))
                query = query.Where(f => f.OrdenProduccion.Contains(op));

            return query;
        }

        // Calcula acumulado, por hacer, diferencia y nombre del revisor para las filas que se van a mostrar
        private async Task EnriquecerAsync(List<FormatoFGC008> lista)
        {
            var ops = lista
                .Where(f => f.Origen == "Bodega")
                .Select(f => f.OrdenProduccion)
                .Distinct()
                .ToList();

            if (ops.Count > 0)
            {
                // El acumulado usa TODAS las filas de Bodega de la OP, aunque el filtro o la página oculten algunas
                var todas = await _context.FormatosFGC008
                    .Where(f => f.Origen == "Bodega" && ops.Contains(f.OrdenProduccion))
                    .Select(f => new { f.OrdenProduccion, f.UnidadReal, f.DiaEntrada, f.CantidadReal })
                    .ToListAsync();

                foreach (var f in lista.Where(x => x.Origen == "Bodega"))
                {
                    var acumulado = todas
                        .Where(x => x.OrdenProduccion == f.OrdenProduccion
                                 && x.UnidadReal == f.UnidadReal
                                 && x.DiaEntrada <= f.DiaEntrada)
                        .Sum(x => x.CantidadReal);

                    f.AcumuladoOP = acumulado;

                    if (f.CantidadOP > 0)
                    {
                        f.PorHacer = f.CantidadOP - acumulado;
                        f.DiferenciaPct = Math.Round((acumulado - f.CantidadOP) / f.CantidadOP * 100, 1);
                    }
                }
            }

            // Nombre de quien revisó, tomado del usuario
            var revisores = lista
                .Where(f => !string.IsNullOrEmpty(f.RevisadoPor))
                .Select(f => f.RevisadoPor!)
                .Distinct()
                .ToList();

            if (revisores.Count > 0)
            {
                var datos = await _context.Usuarios
                    .Where(u => revisores.Contains(u.Username))
                    .Select(u => new { u.Username, u.Nombres, u.Apellidos })
                    .ToListAsync();

                var nombres = datos
                    .GroupBy(d => d.Username, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => ((g.First().Nombres ?? "") + " " + (g.First().Apellidos ?? "")).Trim(),
                        StringComparer.OrdinalIgnoreCase);

                foreach (var f in lista)
                {
                    if (f.RevisadoPor != null
                        && nombres.TryGetValue(f.RevisadoPor, out var nombre)
                        && !string.IsNullOrWhiteSpace(nombre))
                        f.RevisadoPorNombre = nombre;
                }
            }
        }

        // ── Consultas ───────────────────────────────────────────────────────

        // Sin paginar: lo usa el PDF, que necesita todas las filas
        [HttpGet]
        [Permiso("calidad-formatos", "ver")]
        public async Task<IActionResult> Get(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] string? op)
        {
            var lista = await Filtrar(desde, hasta, op)
            .Include(f => f.FotosEvidencia)
            .OrderByDescending(f => f.Fecha)
            .ToListAsync();

            await EnriquecerAsync(lista);
            return Ok(lista);
        }

        // Paginado: lo usa la lista en pantalla
        [HttpGet("paginado")]
        [Permiso("calidad-formatos", "ver")]
        public async Task<IActionResult> GetPaginado(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] string? op,
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 25)
        {
            pagina = Math.Max(1, pagina);
            porPagina = Math.Clamp(porPagina, 5, 100);

            var query = Filtrar(desde, hasta, op);
            var total = await query.CountAsync();

            var items = await query
    .Include(f => f.FotosEvidencia)
    .OrderByDescending(f => f.Fecha)
    .ThenByDescending(f => f.Id)
    .Skip((pagina - 1) * porPagina)
    .Take(porPagina)
    .ToListAsync();

            await EnriquecerAsync(items);

            return Ok(new
            {
                items,
                total,
                pagina,
                porPagina,
                totalPaginas = (int)Math.Ceiling(total / (double)porPagina)
            });
        }

        // Tarjetas de resumen: se calculan sobre todo el filtro, no solo sobre la página
        [HttpGet("resumen")]
        [Permiso("calidad-formatos", "ver")]
        public async Task<IActionResult> GetResumen(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] string? op)
        {
            var query = Filtrar(desde, hasta, op);

            var total = await query.CountAsync();
            var conDefectos = await query.CountAsync(f => f.DefectosSI);
            var pendientesDespacho = await query.CountAsync(f => f.Despachado == null || f.Despachado == "");

            var opsQuery = query.Select(f => f.OrdenProduccion);
            var todas = await _context.FormatosFGC008
                .Where(f => opsQuery.Contains(f.OrdenProduccion))
                .Select(f => new { f.OrdenProduccion, f.Origen, f.CantidadOP, f.CantidadReal })
                .ToListAsync();

            // Por OP: lo programado una sola vez y lo recibido sin pasarse de lo programado
            decimal programada = 0, real = 0;
            foreach (var g in todas.GroupBy(x => x.OrdenProduccion))
            {
                var bodega = g.Where(x => x.Origen == "Bodega").ToList();
                var fuente = bodega.Count > 0 ? bodega : g.ToList();
                var prog = fuente.Max(x => x.CantidadOP);
                var rea = fuente.Sum(x => x.CantidadReal);
                programada += prog;
                real += Math.Min(rea, prog);
            }

            var porcentaje = programada == 0 ? 0 : (int)Math.Round(real / programada * 100);

            return Ok(new { total, conDefectos, pendientesDespacho, porcentajeCompletado = porcentaje });
        }

        [HttpGet("op/{op}")]
        [Permiso("calidad-formatos", "ver")]
        public async Task<IActionResult> GetPorOP(string op)
        {
            var registros = await _context.FormatosFGC008
                .Where(f => f.OrdenProduccion == op)
                .OrderByDescending(f => f.Fecha)
                .ToListAsync();

            if (!registros.Any())
                return NotFound();

            return Ok(new
            {
                cantidadOP = registros.First().CantidadOP,
                totalEntradas = registros.Count,
                totalReal = registros.Sum(f => f.CantidadReal)
            });
        }

        // ── Escritura ───────────────────────────────────────────────────────

        [HttpPost]
        [Permiso("calidad-formatos", "crear")]
        public async Task<IActionResult> Post(
            [FromForm] FormatoFGC008Dto dto,
            IFormFile? foto)
        {
            // Protección contra el doble clic: mismo usuario, misma OP, mismo minuto
            var usuario = GetUsuario();
            var hace = DateTime.Now.AddMinutes(-1);

            var duplicado = await _context.FormatosFGC008.AnyAsync(f =>
                f.OrdenProduccion == dto.OrdenProduccion
                && f.Origen == "Manual"
                && f.RevisadoPor == usuario
                && f.FechaRevision != null
                && f.FechaRevision >= hace);

            if (duplicado)
                return Conflict(new { mensaje = "Ese registro ya se guardó hace un momento" });

            string? nombreFoto = null;

            if (foto != null)
            {
                var carpeta = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/formatos");

                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                nombreFoto = Guid.NewGuid() + Path.GetExtension(foto.FileName);

                using var stream = new FileStream(
                    Path.Combine(carpeta, nombreFoto),
                    FileMode.Create);

                await foto.CopyToAsync(stream);
            }

            var registro = new FormatoFGC008
            {
                OrdenProduccion = dto.OrdenProduccion,
                Cliente = dto.Cliente,
                Referencia = dto.Referencia,
                EtiquetasSI = dto.EtiquetasSI,
                EmbalajeSI = dto.EmbalajeSI,
                DefectosSI = dto.DefectosSI,
                CantidadOP = dto.CantidadOP,
                CantidadReal = dto.CantidadReal,
                ListoBodega = dto.ListoBodega,
                Despachado = dto.Despachado,
                AccionesTomadas = dto.AccionesTomadas,
                FotoEvidencia = nombreFoto,
                FirmaDigital = dto.FirmaDigital,
                RevisadoPor = usuario,
                FechaRevision = DateTime.Now
            };

            _context.FormatosFGC008.Add(registro);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                GetUsuario(),
                GetRol(),
                "Crear",
                "FormatoFGC008",
                $"Registro OP: {dto.OrdenProduccion}",
                registro.Id);

            return Ok(registro);
        }

        [HttpPut("{id}")]
        [Permiso("calidad-formatos", "editar")]
        public async Task<IActionResult> Put(
            int id,
            [FromForm] FormatoFGC008Dto dto,
            IFormFile? foto)
        {
            var r = await _context.FormatosFGC008.FindAsync(id);

            if (r == null)
                return NotFound();

            var esBodega = r.Origen == "Bodega";

            // En las filas que vienen de Bodega no se tocan los datos ni las cantidades: solo la verificación
            if (!esBodega)
            {
                r.OrdenProduccion = dto.OrdenProduccion;
                r.Cliente = dto.Cliente;
                r.Referencia = dto.Referencia;
                r.CantidadOP = dto.CantidadOP;
                r.CantidadReal = dto.CantidadReal;
            }

            r.EtiquetasSI = dto.EtiquetasSI;
            r.EmbalajeSI = dto.EmbalajeSI;
            r.DefectosSI = dto.DefectosSI;
            r.ListoBodega = dto.ListoBodega;
            r.Despachado = dto.Despachado;
            r.AccionesTomadas = dto.AccionesTomadas;
            r.FirmaDigital = dto.FirmaDigital;

            if (foto != null)
            {
                var carpeta = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/formatos");

                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                var nombreFoto = Guid.NewGuid() + Path.GetExtension(foto.FileName);

                using var stream = new FileStream(
                    Path.Combine(carpeta, nombreFoto),
                    FileMode.Create);

                await foto.CopyToAsync(stream);

                r.FotoEvidencia = nombreFoto;
            }

            // Al guardar la verificación de una fila de Bodega, queda revisada y se apaga el aviso
            if (esBodega)
            {
                r.RevisadoPor = GetUsuario();
                r.FechaRevision = DateTime.Now;
                r.CambioDespuesDeRevisar = false;
            }

            await _context.SaveChangesAsync();

            return Ok(r);
        }
        // ── Fotos de evidencia (hasta 5 por registro, contando la foto antigua de FotoEvidencia) ──

        private const int MaxFotos = 5;
        private const long MaxBytesPorFoto = 8 * 1024 * 1024;
        private static readonly string[] ExtensionesFoto = { ".jpg", ".jpeg", ".png", ".webp" };

        [HttpPost("{id}/fotos")]
        [Permiso("calidad-formatos", "crear")]
        public async Task<IActionResult> SubirFotos(int id, [FromForm] List<IFormFile> fotos)
        {
            if (fotos == null || fotos.Count == 0)
                return BadRequest(new { mensaje = "No se recibieron fotos" });

            var registro = await _context.FormatosFGC008
                .Include(f => f.FotosEvidencia)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (registro == null)
                return NotFound();

            var existentes = registro.FotosEvidencia.Count
                + (string.IsNullOrEmpty(registro.FotoEvidencia) ? 0 : 1);
            var disponibles = MaxFotos - existentes;

            if (disponibles <= 0)
                return BadRequest(new { mensaje = $"Este registro ya tiene el máximo de {MaxFotos} fotos" });

            if (fotos.Count > disponibles)
                return BadRequest(new { mensaje = $"Solo puede agregar {disponibles} foto(s) más (máximo {MaxFotos} por registro)" });

            // Se valida todo antes de guardar nada, para no dejar la subida a medias
            foreach (var f in fotos)
            {
                var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
                if (!ExtensionesFoto.Contains(ext) || !f.ContentType.StartsWith("image/"))
                    return BadRequest(new { mensaje = $"El archivo '{f.FileName}' no es una imagen válida (JPG, PNG o WEBP)" });

                if (f.Length == 0 || f.Length > MaxBytesPorFoto)
                    return BadRequest(new { mensaje = $"El archivo '{f.FileName}' es demasiado grande (máximo 8 MB)" });
            }

            var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/formatos");
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            var usuario = GetUsuario();

            foreach (var f in fotos)
            {
                var nombre = Guid.NewGuid() + Path.GetExtension(f.FileName).ToLowerInvariant();

                using (var stream = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create))
                    await f.CopyToAsync(stream);

                registro.FotosEvidencia.Add(new FormatoFGC008Foto
                {
                    Archivo = nombre,
                    SubidaPor = usuario
                });
            }

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario,
                GetRol(),
                "Subir-Fotos",
                "FormatoFGC008",
                $"{fotos.Count} foto(s) en OP {registro.OrdenProduccion}",
                registro.Id);

            return Ok(registro.FotosEvidencia);
        }

        [HttpDelete("{id}/fotos/{fotoId}")]
        [Permiso("calidad-formatos", "crear")]
        public async Task<IActionResult> EliminarFoto(int id, int fotoId)
        {
            var foto = await _context.FormatoFGC008Fotos
                .FirstOrDefaultAsync(x => x.Id == fotoId && x.FormatoFGC008Id == id);

            if (foto == null)
                return NotFound();

            _context.FormatoFGC008Fotos.Remove(foto);
            await _context.SaveChangesAsync();

            // El archivo se borra después de la base; si falla, queda suelto pero no se ve en pantalla
            var ruta = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/formatos",
                Path.GetFileName(foto.Archivo));

            if (System.IO.File.Exists(ruta))
            {
                try { System.IO.File.Delete(ruta); } catch { }
            }

            return Ok();
        }

        [HttpPost("sincronizar/{op}")]
        [Permiso("calidad-formatos", "crear")]
        public async Task<IActionResult> Sincronizar(
            string op,
            [FromServices] SincronizacionFgc008Service sincronizacion)
        {
            var orden = await _empresaOrdenes.BuscarPorNumero(op);

            if (orden == null)
                return NotFound(new { mensaje = "No se encontró esa orden en producción" });

            var resultado = await sincronizacion.SincronizarOpAsync(op);

            if (resultado.EntradasLeidas == 0)
                return Ok(new
                {
                    mensaje = "La orden existe, pero Bodega todavía no tiene entradas para ella",
                    resultado
                });

            await _auditoria.RegistrarAsync(
                GetUsuario(),
                GetRol(),
                "Sincronizar",
                "FormatoFGC008",
                $"OP {op}: {resultado.FilasCreadas} filas creadas, {resultado.FilasActualizadas} actualizadas");

            var filas = await _context.FormatosFGC008
    .Include(f => f.FotosEvidencia)
    .Where(f => f.OrdenProduccion == op)
    .OrderByDescending(f => f.Fecha)
    .ToListAsync();

            return Ok(new { resultado, filas });
        }

        [HttpDelete("{id}")]
        [Permiso("calidad-formatos", "eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _context.FormatosFGC008.FindAsync(id);

            if (r == null)
                return NotFound();

            _context.FormatosFGC008.Remove(r);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}