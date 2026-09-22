using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SistemaFlota.DTOs.Trazabilidad;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TrazabilidadController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;

        public TrazabilidadController(
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
        // SINCRONIZAR CON COSTOS FLETES
        // =====================================
        private async Task SincronizarCostoFleteAsync(int trazabilidadId, int? autorizacionId, decimal? valorFlete)
        {
            if (valorFlete == null || valorFlete <= 0) return;

            CosteFlete? existente;

            if (autorizacionId != null)
            {
                // Caso flota propia: se relaciona por Autorización
                existente = await _context.CostosFletes
                    .FirstOrDefaultAsync(c => c.AutorizacionId == autorizacionId);
            }
            else
            {
                // Caso transportadora externa: se relaciona directo por Trazabilidad
                existente = await _context.CostosFletes
                    .FirstOrDefaultAsync(c => c.TrazabilidadId == trazabilidadId);
            }

            if (existente != null)
            {
                existente.Total = valorFlete.Value;
            }
            else
            {
                _context.CostosFletes.Add(new CosteFlete
                {
                    AutorizacionId = autorizacionId,
                    TrazabilidadId = autorizacionId == null ? trazabilidadId : null,
                    FechaRegistro = DateTime.Now,
                    Peajes = 0,
                    Combustible = 0,
                    Parqueos = 0,
                    DescarguesMcia = 0,
                    CargueMateriales = 0,
                    Alimentacion = 0,
                    Hospedaje = 0,
                    Varios = 0,
                    Total = valorFlete.Value,
                    Estado = "Pendiente"
                });
            }

            await _context.SaveChangesAsync();
        }

        // =====================================
        // GET TODAS — con paginación y filtros
        // =====================================
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 50,
            [FromQuery] string? buscar = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? entregada = null,
            [FromQuery] string? tipo = null)
        {
            var query = _context.TrazabilidadFacturas
                .Include(t => t.Autorizacion)
                    .ThenInclude(a => a != null ? a.Conductor : null)
                .Include(t => t.Autorizacion)
                    .ThenInclude(a => a != null ? a.Vehiculo : null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var q = buscar.ToLower();
                query = query.Where(t =>
                    t.FacturaRemision.ToLower().Contains(q) ||
                    t.Cliente.ToLower().Contains(q) ||
                    t.Conductor.ToLower().Contains(q) ||
                    (t.Vehiculo != null && t.Vehiculo.ToLower().Contains(q)) ||
                    (t.Guia != null && t.Guia.ToLower().Contains(q)));
            }

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(t => t.Estado == estado);

            if (entregada == "si")
                query = query.Where(t => t.FacturaEntregada);
            else if (entregada == "no")
                query = query.Where(t => !t.FacturaEntregada);

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                var tipoUpper = tipo.ToUpper();
                var alternativo = tipoUpper == "CT" ? "COT" : tipoUpper == "RM" ? "RE" : tipoUpper == "NCE" ? "NC" : tipoUpper == "COT" ? "CT" : tipoUpper == "RE" ? "RM" : tipoUpper == "NC" ? "NCE" : "";
                query = query.Where(t => t.FacturaRemision.ToUpper().StartsWith(tipoUpper) || (alternativo != "" && t.FacturaRemision.ToUpper().StartsWith(alternativo)));
            }

            var total = await query.CountAsync();

            var lista = await query
                .OrderByDescending(t => t.FechaRegistro).ThenByDescending(t => t.Id)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .ToListAsync();

            return Ok(new
            {
                data = lista,
                total,
                pagina,
                porPagina,
                totalPaginas = (int)Math.Ceiling((double)total / porPagina)
            });
        }

        [HttpGet("resumen")]
        public async Task<IActionResult> GetResumen(
    [FromQuery] string? buscar = null,
    [FromQuery] string? estado = null,
    [FromQuery] string? entregada = null,
    [FromQuery] string? tipo = null)
        {
            var query = _context.TrazabilidadFacturas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var q = buscar.ToLower();
                query = query.Where(t =>
                    t.FacturaRemision.ToLower().Contains(q) ||
                    t.Cliente.ToLower().Contains(q) ||
                    t.Conductor.ToLower().Contains(q) ||
                    (t.Vehiculo != null && t.Vehiculo.ToLower().Contains(q)) ||
                    (t.Guia != null && t.Guia.ToLower().Contains(q)));
            }

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(t => t.Estado == estado);

            if (entregada == "si")
                query = query.Where(t => t.FacturaEntregada);
            else if (entregada == "no")
                query = query.Where(t => !t.FacturaEntregada);

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                var tipoUpper = tipo.ToUpper();
                var alternativo = tipoUpper == "CT" ? "COT" : tipoUpper == "RM" ? "RE" : tipoUpper == "NCE" ? "NC" : tipoUpper == "COT" ? "CT" : tipoUpper == "RE" ? "RM" : tipoUpper == "NC" ? "NCE" : "";
                query = query.Where(t => t.FacturaRemision.ToUpper().StartsWith(tipoUpper) || (alternativo != "" && t.FacturaRemision.ToUpper().StartsWith(alternativo)));
            }

            var total = await query.CountAsync();
            var entregadas = await query.CountAsync(t => t.FacturaEntregada);
            var pendientesEntrega = total - entregadas;
            var totalFlete = await query.SumAsync(t => t.ValorFlete ?? 0);

            return Ok(new { total, entregadas, pendientesEntrega, totalFlete });
        }

        // =====================================
        // GET POR ID
        // =====================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var t = await _context.TrazabilidadFacturas
                .Include(t => t.Autorizacion)
                    .ThenInclude(a => a != null ? a.Conductor : null)
                .Include(t => t.Autorizacion)
                    .ThenInclude(a => a != null ? a.Vehiculo : null)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (t == null) return NotFound();
            return Ok(t);
        }

        // =====================================
        // POST — CREAR
        // =====================================
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CrearTrazabilidadDto dto)
        {
            var trazabilidad = new TrazabilidadFactura
            {
                AutorizacionId = dto.AutorizacionId,
                FacturaRemision = dto.FacturaRemision,
                Cliente = dto.Cliente,
                Conductor = dto.Conductor,
                Transportadora = dto.Transportadora,
                Guia = dto.Guia,
                Vehiculo = dto.Vehiculo,
                PesoKilos = dto.PesoKilos,
                ValorFlete = dto.ValorFlete,
                AjusteRecibido = dto.AjusteRecibido,
                FacturaEntregada = dto.FacturaEntregada,
                FechaEntrega = dto.FacturaEntregada ? DateTime.Now : null,
                Novedad = dto.Novedad,
                Estado = dto.Estado ?? "Pendiente",
                FechaRegistro = DateTime.Now
            };

            _context.TrazabilidadFacturas.Add(trazabilidad);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Crear", modulo: "Trazabilidad",
                detalle: $"Trazabilidad creada — Factura: {dto.FacturaRemision}, Cliente: {dto.Cliente}",
                registroId: trazabilidad.Id
            );

            await SincronizarCostoFleteAsync(trazabilidad.Id, trazabilidad.AutorizacionId, trazabilidad.ValorFlete);

            return Ok(trazabilidad);
        }

        // =====================================
        // PUT — EDITAR
        // =====================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] CrearTrazabilidadDto dto)
        {
            var t = await _context.TrazabilidadFacturas.FindAsync(id);
            if (t == null) return NotFound();

            t.FacturaRemision = dto.FacturaRemision;
            t.Cliente = dto.Cliente;
            t.Conductor = dto.Conductor;
            t.Transportadora = dto.Transportadora;
            t.Guia = dto.Guia;
            t.Vehiculo = dto.Vehiculo;
            t.PesoKilos = dto.PesoKilos;
            t.ValorFlete = dto.ValorFlete;
            t.AjusteRecibido = dto.AjusteRecibido;
            t.Novedad = dto.Novedad;
            t.Estado = dto.Estado ?? t.Estado;

            if (dto.FacturaEntregada && !t.FacturaEntregada)
                t.FechaEntrega = DateTime.Now;

            t.FacturaEntregada = dto.FacturaEntregada;

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Editar", modulo: "Trazabilidad",
                detalle: $"Trazabilidad #{id} editada — Factura: {dto.FacturaRemision}",
                registroId: id
            );

            await SincronizarCostoFleteAsync(t.Id, t.AutorizacionId, t.ValorFlete);

            return Ok(t);
        }

        // =====================================
        // DELETE
        // =====================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await _context.TrazabilidadFacturas.FindAsync(id);
            if (t == null) return NotFound();

            var notas = await _context.NotasTrazabilidad
                .Where(n => n.TrazabilidadId == id)
                .ToListAsync();
            _context.NotasTrazabilidad.RemoveRange(notas);
            _context.TrazabilidadFacturas.Remove(t);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Eliminar", modulo: "Trazabilidad",
                detalle: $"Trazabilidad #{id} eliminada",
                registroId: id
            );

            return Ok();
        }

        // =====================================
        // GET NOTAS
        // =====================================
        [HttpGet("{id}/notas")]
        public async Task<IActionResult> GetNotas(int id)
        {
            var notas = await _context.NotasTrazabilidad
                .Where(n => n.TrazabilidadId == id)
                .OrderByDescending(n => n.Fecha)
                .ToListAsync();
            return Ok(notas);
        }

        // =====================================
        // POST NOTA
        // =====================================
        [HttpPost("{id}/notas")]
        public async Task<IActionResult> AgregarNota(int id, [FromBody] CrearNotaDto dto)
        {
            var trazabilidad = await _context.TrazabilidadFacturas.FindAsync(id);
            if (trazabilidad == null) return NotFound();

            var nota = new NotaTrazabilidad
            {
                TrazabilidadId = id,
                NumeroNota = dto.NumeroNota,
                Cliente = dto.Cliente,
                Conductor = dto.Conductor,
                FacturaEntregada = dto.FacturaEntregada,
                Observacion = dto.Observacion,
                Fecha = DateTime.Now
            };

            _context.NotasTrazabilidad.Add(nota);

            var registroNCE = new TrazabilidadFactura
            {
                FechaRegistro = DateTime.Now,
                AutorizacionId = trazabilidad.AutorizacionId,
                FacturaRemision = "NCE-" + dto.NumeroNota.Replace("NCE ", "").Replace("NCE-", "").Trim(),
                Cliente = dto.Cliente ?? trazabilidad.Cliente,
                Conductor = dto.Conductor ?? trazabilidad.Conductor,
                Transportadora = trazabilidad.Transportadora,
                Vehiculo = trazabilidad.Vehiculo,
                Estado = "Pendiente",
                AjusteRecibido = false,
                FacturaEntregada = dto.FacturaEntregada,
                Novedad = dto.Observacion
            };
            _context.TrazabilidadFacturas.Add(registroNCE);

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Crear", modulo: "Trazabilidad",
                detalle: $"Nota #{dto.NumeroNota} agregada a trazabilidad #{id}",
                registroId: id
            );

            return Ok(nota);
        }

        // =====================================
        // PUT NOTA
        // =====================================
        [HttpPut("notas/{notaId}")]
        public async Task<IActionResult> EditarNota(int notaId, [FromBody] CrearNotaDto dto)
        {
            var nota = await _context.NotasTrazabilidad.FindAsync(notaId);
            if (nota == null) return NotFound();

            nota.NumeroNota = dto.NumeroNota;
            nota.Cliente = dto.Cliente;
            nota.Conductor = dto.Conductor;
            nota.FacturaEntregada = dto.FacturaEntregada;
            nota.Observacion = dto.Observacion;

            await _context.SaveChangesAsync();
            return Ok(nota);
        }

        // =====================================
        // DELETE NOTA
        // =====================================
        [HttpDelete("notas/{notaId}")]
        public async Task<IActionResult> EliminarNota(int notaId)
        {
            var nota = await _context.NotasTrazabilidad.FindAsync(notaId);
            if (nota == null) return NotFound();

            _context.NotasTrazabilidad.Remove(nota);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // =====================================
        // GET AUTORIZACIONES PARA IMPORTAR
        // =====================================
        [HttpGet("autorizaciones-disponibles")]
        public async Task<IActionResult> GetAutorizacionesDisponibles()
        {
            var lista = await _context.Autorizaciones
                .Include(a => a.Conductor)
                .Include(a => a.Vehiculo)
                .Where(a => a.Estado == "Bodega" ||
                            a.Estado == "Porteria" ||
                            a.Estado == "Autorizado")
                .OrderByDescending(a => a.FechaCreacion)
                .Select(a => new
                {
                    a.Id,
                    a.FechaCreacion,
                    a.TipoVuelta,
                    a.DestinoCompleto,
                    a.NumeroGuia,
                    a.FacturasClientes,
                    Conductor = a.Conductor != null ? a.Conductor.Nombre : "-",
                    Vehiculo = a.Vehiculo != null ? a.Vehiculo.Placa : "-",
                })
                .ToListAsync();

            return Ok(lista);
        }
    }
}