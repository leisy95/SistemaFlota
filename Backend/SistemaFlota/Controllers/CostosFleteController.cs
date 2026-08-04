using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SistemaFlota.DTOs;

namespace SistemaFlota.Controllers
{
    [ApiController]
    [Route("api/CostosFletes")]
    [Authorize]
    public class CostosFleteController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;

        public CostosFleteController(AppDbContext context, AuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        private string GetUsuario() => User.FindFirstValue(ClaimTypes.Name) ?? "Sistema";
        private string GetRol() => User.FindFirstValue(ClaimTypes.Role) ?? "";

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? desde = null,
            [FromQuery] string? hasta = null,
            [FromQuery] string? conductor = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? ciudad = null)
        {
            var query = _context.CostosFletes
                .Include(c => c.Autorizacion)
                    .ThenInclude(a => a!.Conductor)
                .Include(c => c.Autorizacion)
                    .ThenInclude(a => a!.Vehiculo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(desde))
                query = query.Where(c => c.FechaRegistro >= DateTime.Parse(desde));
            if (!string.IsNullOrEmpty(hasta))
                query = query.Where(c => c.FechaRegistro <= DateTime.Parse(hasta).AddDays(1));
            if (!string.IsNullOrEmpty(conductor))
                query = query.Where(c => c.Autorizacion.Conductor.Nombre.Contains(conductor));
            if (!string.IsNullOrEmpty(estado))
                query = query.Where(c => c.Estado == estado);
            if (!string.IsNullOrEmpty(ciudad))
                query = query.Where(c => c.Autorizacion.DestinoCompleto.Contains(ciudad));

            var lista = await query
                .OrderByDescending(c => c.FechaRegistro)
                .ThenByDescending(c => c.Id)
                .ToListAsync();

            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var registro = await _context.CostosFletes
                .Include(c => c.Autorizacion)
                    .ThenInclude(a => a!.Conductor)
                .Include(c => c.Autorizacion)
                    .ThenInclude(a => a!.Vehiculo)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (registro == null) return NotFound();
            return Ok(registro);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CostoFleteDto dto)
        {
            var registro = new CosteFlete
            {
                AutorizacionId = dto.AutorizacionId,
                FechaRegistro = DateTime.Now,
                Peajes = dto.Peajes,
                Combustible = dto.Combustible,
                Parqueos = dto.Parqueos,
                DescarguesMcia = dto.DescarguesMcia,
                CargueMateriales = dto.CargueMateriales,
                Alimentacion = dto.Alimentacion,
                Hospedaje = dto.Hospedaje,
                Varios = dto.Varios,
                Total = dto.Total,
                Observaciones = dto.Observaciones,
                Estado = "Pendiente"
            };
            _context.CostosFletes.Add(registro);
            await _context.SaveChangesAsync();
            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Crear", modulo: "CostosFletes",
                detalle: $"Costo flete creado para autorización #{dto.AutorizacionId}",
                registroId: registro.Id
            );
            return Ok(registro);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] CostoFleteDto dto)
        {
            var registro = await _context.CostosFletes.FindAsync(id);
            if (registro == null) return NotFound();
            registro.Peajes = dto.Peajes;
            registro.Combustible = dto.Combustible;
            registro.Parqueos = dto.Parqueos;
            registro.DescarguesMcia = dto.DescarguesMcia;
            registro.CargueMateriales = dto.CargueMateriales;
            registro.Alimentacion = dto.Alimentacion;
            registro.Hospedaje = dto.Hospedaje;
            registro.Varios = dto.Varios;
            registro.Total = dto.Total;
            registro.Observaciones = dto.Observaciones;
            await _context.SaveChangesAsync();
            return Ok(registro);
        }

        [HttpPut("{id}/verificar")]
        public async Task<IActionResult> Verificar(int id, [FromBody] VerificarDto dto)
        {
            var registro = await _context.CostosFletes.FindAsync(id);
            if (registro == null) return NotFound();
            registro.Estado = "Verificado";
            registro.VerificadoPor = dto.VerificadoPor;
            registro.FirmaVerificacion = dto.FirmaVerificacion;
            registro.FechaVerificacion = DateTime.Now;
            await _context.SaveChangesAsync();
            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Verificar", modulo: "CostosFletes",
                detalle: $"Flete #{id} verificado por {dto.VerificadoPor}",
                registroId: id
            );
            return Ok(registro);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var registro = await _context.CostosFletes.FindAsync(id);
            if (registro == null) return NotFound();
            _context.CostosFletes.Remove(registro);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}