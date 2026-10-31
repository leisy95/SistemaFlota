using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.Calidad.FormatoFGC008;
using SistemaFlota.Models.Calidad.FormatoFGC008;

namespace SistemaFlota.Services.Calidad.FormatoFGC008
{
    public class FormatoFGC008Service : IFormatoFGC008Service
    {
        private readonly AppDbContext _context;

        private const int MaxFotos = 5;
        private const long MaxBytesPorFoto = 8 * 1024 * 1024;

        private static readonly string[] ExtensionesFoto =
            { ".jpg", ".jpeg", ".png", ".webp" };

        public FormatoFGC008Service(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<Models.Calidad.FormatoFGC008.FormatoFGC008> Filtrar(
            DateTime? desde, DateTime? hasta, string? op)
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

        private async Task EnriquecerAsync(
            List<Models.Calidad.FormatoFGC008.FormatoFGC008> lista)
        {
            var ops = lista
                .Where(f => f.Origen == "Bodega")
                .Select(f => f.OrdenProduccion)
                .Distinct()
                .ToList();

            if (ops.Count > 0)
            {
                var todas = await _context.FormatosFGC008
                    .Where(f => f.Origen == "Bodega"
                             && ops.Contains(f.OrdenProduccion))
                    .Select(f => new
                    {
                        f.OrdenProduccion,
                        f.UnidadReal,
                        f.DiaEntrada,
                        f.CantidadReal
                    })
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

                        f.DiferenciaPct = Math.Round(
                            (acumulado - f.CantidadOP) / f.CantidadOP * 100, 1);
                    }
                }
            }

            var revisores = lista
                .Where(f => !string.IsNullOrEmpty(f.RevisadoPor))
                .Select(f => f.RevisadoPor!)
                .Distinct()
                .ToList();

            if (revisores.Count == 0)
                return;

            var datos = await _context.Usuarios
                .Where(u => revisores.Contains(u.Username))
                .Select(u => new
                {
                    u.Username,
                    u.Nombres,
                    u.Apellidos
                })
                .ToListAsync();

            var nombres = datos
                .GroupBy(d => d.Username, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => ((g.First().Nombres ?? "") + " " +
                          (g.First().Apellidos ?? "")).Trim(),
                    StringComparer.OrdinalIgnoreCase);

            foreach (var f in lista)
            {
                if (f.RevisadoPor != null
                    && nombres.TryGetValue(f.RevisadoPor, out var nombre)
                    && !string.IsNullOrWhiteSpace(nombre))
                {
                    f.RevisadoPorNombre = nombre;
                }
            }
        }

        public async Task<List<Models.Calidad.FormatoFGC008.FormatoFGC008>> ObtenerTodosAsync(
            DateTime? desde, DateTime? hasta, string? op)
        {
            var lista = await Filtrar(desde, hasta, op)
                .Include(f => f.FotosEvidencia)
                .OrderByDescending(f => f.Fecha)
                .ToListAsync();

            await EnriquecerAsync(lista);
            return lista;
        }

        public async Task<FormatoFGC008PaginadoDto> ObtenerPaginadoAsync(
            DateTime? desde, DateTime? hasta, string? op,
            int pagina, int porPagina)
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

            return new FormatoFGC008PaginadoDto
            {
                Items = items,
                Total = total,
                Pagina = pagina,
                PorPagina = porPagina,
                TotalPaginas = (int)Math.Ceiling(
                    total / (double)porPagina)
            };
        }

        public async Task<FormatoFGC008ResumenDto> ObtenerResumenAsync(
            DateTime? desde, DateTime? hasta, string? op)
        {
            var query = Filtrar(desde, hasta, op);

            var total = await query.CountAsync();
            var conDefectos = await query.CountAsync(f => f.DefectosSI);

            var pendientesDespacho = await query.CountAsync(
                f => f.Despachado == null || f.Despachado == "");

            var opsQuery = query.Select(f => f.OrdenProduccion);

            var todas = await _context.FormatosFGC008
                .Where(f => opsQuery.Contains(f.OrdenProduccion))
                .Select(f => new
                {
                    f.OrdenProduccion,
                    f.Origen,
                    f.CantidadOP,
                    f.CantidadReal
                })
                .ToListAsync();

            decimal programada = 0;
            decimal real = 0;

            foreach (var grupo in todas.GroupBy(x => x.OrdenProduccion))
            {
                var bodega = grupo
                    .Where(x => x.Origen == "Bodega")
                    .ToList();

                var fuente = bodega.Count > 0 ? bodega : grupo.ToList();

                var prog = fuente.Max(x => x.CantidadOP);
                var rea = fuente.Sum(x => x.CantidadReal);

                programada += prog;
                real += Math.Min(rea, prog);
            }

            var porcentaje = programada == 0
                ? 0
                : (int)Math.Round(real / programada * 100);

            return new FormatoFGC008ResumenDto
            {
                Total = total,
                ConDefectos = conDefectos,
                PendientesDespacho = pendientesDespacho,
                PorcentajeCompletado = porcentaje
            };
        }

        public async Task<List<Models.Calidad.FormatoFGC008.FormatoFGC008>> ObtenerPorOPAsync(
            string op)
        {
            return await _context.FormatosFGC008
                .Where(f => f.OrdenProduccion == op)
                .OrderByDescending(f => f.Fecha)
                .ToListAsync();
        }

        public async Task<Models.Calidad.FormatoFGC008.FormatoFGC008?> ObtenerPorIdAsync(
            int id)
        {
            return await _context.FormatosFGC008.FindAsync(id);
        }

        public async Task<(
            Models.Calidad.FormatoFGC008.FormatoFGC008? Registro,
            bool Duplicado)> CrearAsync(
                FormatoFGC008Dto dto,
                string usuario,
                IFormFile? foto)
        {
            var hace = DateTime.Now.AddMinutes(-1);

            var duplicado = await _context.FormatosFGC008.AnyAsync(f =>
                f.OrdenProduccion == dto.OrdenProduccion
                && f.Origen == "Manual"
                && f.RevisadoPor == usuario
                && f.FechaRevision != null
                && f.FechaRevision >= hace);

            if (duplicado)
                return (null, true);

            string? nombreFoto = null;

            if (foto != null)
            {
                var carpeta = ObtenerCarpetaFotos();
                Directory.CreateDirectory(carpeta);

                nombreFoto = Guid.NewGuid()
                    + Path.GetExtension(foto.FileName);

                using var stream = new FileStream(
                    Path.Combine(carpeta, nombreFoto),
                    FileMode.Create);

                await foto.CopyToAsync(stream);
            }

            var registro = new Models.Calidad.FormatoFGC008.FormatoFGC008
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

            return (registro, false);
        }

        public async Task<bool> ActualizarAsync(
            int id,
            FormatoFGC008Dto dto,
            string usuario,
            IFormFile? foto)
        {
            var r = await _context.FormatosFGC008.FindAsync(id);

            if (r == null)
                return false;

            var esBodega = r.Origen == "Bodega";

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
                var carpeta = ObtenerCarpetaFotos();
                Directory.CreateDirectory(carpeta);

                var nombreFoto = Guid.NewGuid()
                    + Path.GetExtension(foto.FileName);

                using var stream = new FileStream(
                    Path.Combine(carpeta, nombreFoto),
                    FileMode.Create);

                await foto.CopyToAsync(stream);
                r.FotoEvidencia = nombreFoto;
            }

            if (esBodega)
            {
                r.RevisadoPor = usuario;
                r.FechaRevision = DateTime.Now;
                r.CambioDespuesDeRevisar = false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Exito, string? Mensaje,
            List<FormatoFGC008Foto>? Fotos)> SubirFotosAsync(
                int id, List<IFormFile> fotos, string usuario)
        {
            if (fotos == null || fotos.Count == 0)
                return (false, "No se recibieron fotos", null);

            var registro = await _context.FormatosFGC008
                .Include(f => f.FotosEvidencia)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (registro == null)
                return (false, null, null);

            var existentes = registro.FotosEvidencia.Count
                + (string.IsNullOrEmpty(registro.FotoEvidencia) ? 0 : 1);

            var disponibles = MaxFotos - existentes;

            if (disponibles <= 0)
                return (false,
                    $"Este registro ya tiene el máximo de {MaxFotos} fotos",
                    null);

            if (fotos.Count > disponibles)
                return (false,
                    $"Solo puede agregar {disponibles} foto(s) más (máximo {MaxFotos} por registro)",
                    null);

            foreach (var f in fotos)
            {
                var ext = Path.GetExtension(f.FileName).ToLowerInvariant();

                if (!ExtensionesFoto.Contains(ext)
                    || string.IsNullOrEmpty(f.ContentType)
                    || !f.ContentType.StartsWith("image/"))
                {
                    return (false,
                        $"El archivo '{f.FileName}' no es una imagen válida (JPG, PNG o WEBP)",
                        null);
                }

                if (f.Length == 0 || f.Length > MaxBytesPorFoto)
                    return (false,
                        $"El archivo '{f.FileName}' es demasiado grande (máximo 8 MB)",
                        null);
            }

            var carpeta = ObtenerCarpetaFotos();
            Directory.CreateDirectory(carpeta);

            foreach (var f in fotos)
            {
                var nombre = Guid.NewGuid()
                    + Path.GetExtension(f.FileName).ToLowerInvariant();

                using var stream = new FileStream(
                    Path.Combine(carpeta, nombre),
                    FileMode.Create);

                await f.CopyToAsync(stream);

                registro.FotosEvidencia.Add(
                    new FormatoFGC008Foto
                    {
                        Archivo = nombre,
                        SubidaPor = usuario
                    });
            }

            await _context.SaveChangesAsync();

            return (true, null, registro.FotosEvidencia.ToList());
        }

        public async Task<bool> EliminarFotoAsync(int id, int fotoId)
        {
            var foto = await _context.FormatoFGC008Fotos
                .FirstOrDefaultAsync(x =>
                    x.Id == fotoId && x.FormatoFGC008Id == id);

            if (foto == null)
                return false;

            _context.FormatoFGC008Fotos.Remove(foto);
            await _context.SaveChangesAsync();

            var ruta = Path.Combine(
                ObtenerCarpetaFotos(),
                Path.GetFileName(foto.Archivo));

            if (File.Exists(ruta))
            {
                try
                {
                    File.Delete(ruta);
                }
                catch
                {
                    // La fila ya fue eliminada de la base de datos.
                }
            }

            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var registro = await _context.FormatosFGC008.FindAsync(id);

            if (registro == null)
                return false;

            _context.FormatosFGC008.Remove(registro);
            await _context.SaveChangesAsync();

            return true;
        }

        private static string ObtenerCarpetaFotos()
        {
            return Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "formatos");
        }
    }
}