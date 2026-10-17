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

        [HttpGet]
        [Permiso("calidad-formatos", "ver")]
        public async Task<IActionResult> Get(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] string? op)
        {
            var query = _context.FormatosFGC008.AsQueryable();

            if (desde.HasValue)
                query = query.Where(f => f.Fecha >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(f => f.Fecha <= hasta.Value.AddDays(1));

            if (!string.IsNullOrWhiteSpace(op))
                query = query.Where(f => f.OrdenProduccion.Contains(op));

            var lista = await query
                .OrderByDescending(f => f.Fecha)
                .ToListAsync();

            // Acumulado: se calcula con TODAS las filas de Bodega de la OP, aunque el filtro oculte algunas
            var ops = lista
                .Where(f => f.Origen == "Bodega")
                .Select(f => f.OrdenProduccion)
                .Distinct()
                .ToList();

            if (ops.Count > 0)
            {
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

                    // Solo se calcula lo que falta si la programada está en la misma unidad que lo recibido
                    var comparable = !string.IsNullOrEmpty(f.UnidadOP)
                        && string.Equals(f.UnidadOP, f.UnidadReal, StringComparison.OrdinalIgnoreCase);

                    f.PorHacer = comparable ? f.CantidadOP - acumulado : null;
                }
            }

            return Ok(lista);
        }

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