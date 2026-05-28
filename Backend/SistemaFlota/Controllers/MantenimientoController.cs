using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MantenimientoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;

        public MantenimientoController(
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
        // GET TODOS
        // =====================================

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lista = await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                .OrderByDescending(m => m.FechaEntrada)
                .Select(m => new
                {
                    m.Id,
                    m.TipoMantenimiento,
                    m.FechaEntrada,
                    m.FechaSalida,
                    m.KilometrajeEntrada,
                    m.KilometrajeSiguiente,
                    m.FechaSiguiente,
                    m.NombreTaller,
                    m.TecnicoResponsable,
                    m.TelefonoTaller,
                    m.TrabajosRealizados,
                    m.RepuestosUtilizados,
                    m.CostoManoObra,
                    m.CostoRepuestos,
                    m.CostoTotal,
                    m.Estado,
                    m.Observaciones,
                    m.Fotos,
                    Vehiculo = new
                    {
                        m.Vehiculo!.Id,
                        m.Vehiculo.Placa,
                        m.Vehiculo.Marca,
                        m.Vehiculo.Modelo
                    }
                })
                .ToListAsync();

            return Ok(lista);
        }

        // =====================================
        // GET POR VEHÍCULO
        // =====================================

        [HttpGet("vehiculo/{vehiculoId}")]
        public async Task<IActionResult> GetPorVehiculo(int vehiculoId)
        {
            var lista = await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                .Where(m => m.VehiculoId == vehiculoId)
                .OrderByDescending(m => m.FechaEntrada)
                .ToListAsync();

            return Ok(lista);
        }

        // =====================================
        // GET POR ID
        // =====================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var mant = await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mant == null) return NotFound();
            return Ok(mant);
        }

        // =====================================
        // POST — CREAR
        // =====================================

        [HttpPost]
        public async Task<IActionResult> Post(
            [FromForm] int VehiculoId,
            [FromForm] string TipoMantenimiento,
            [FromForm] DateTime FechaEntrada,
            [FromForm] int KilometrajeEntrada,
            [FromForm] string NombreTaller,
            [FromForm] string TrabajosRealizados,
            [FromForm] decimal CostoManoObra,
            [FromForm] decimal CostoRepuestos,
            [FromForm] string? TecnicoResponsable,
            [FromForm] string? TelefonoTaller,
            [FromForm] string? RepuestosUtilizados,
            [FromForm] string? Observaciones,
            [FromForm] int? KilometrajeSiguiente,
            [FromForm] DateTime? FechaSiguiente,
            [FromForm] List<IFormFile> Fotos
        )
        {
            try
            {
                var carpeta = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/mantenimiento"
                );

                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                var nombresfotos = new List<string>();
                foreach (var foto in Fotos.Take(5))
                {
                    var nombre = Guid.NewGuid().ToString()
                        + Path.GetExtension(foto.FileName);
                    using var stream = new FileStream(
                        Path.Combine(carpeta, nombre), FileMode.Create);
                    await foto.CopyToAsync(stream);
                    nombresfotos.Add(nombre);
                }

                var mant = new Mantenimiento
                {
                    VehiculoId = VehiculoId,
                    TipoMantenimiento = TipoMantenimiento,
                    FechaEntrada = FechaEntrada,
                    KilometrajeEntrada = KilometrajeEntrada,
                    NombreTaller = NombreTaller,
                    TrabajosRealizados = TrabajosRealizados,
                    CostoManoObra = CostoManoObra,
                    CostoRepuestos = CostoRepuestos,
                    CostoTotal = CostoManoObra + CostoRepuestos,
                    TecnicoResponsable = TecnicoResponsable,
                    TelefonoTaller = TelefonoTaller,
                    RepuestosUtilizados = RepuestosUtilizados,
                    Observaciones = Observaciones,
                    KilometrajeSiguiente = KilometrajeSiguiente,
                    FechaSiguiente = FechaSiguiente,
                    Estado = "EnTaller",
                    Fotos = string.Join(",", nombresfotos)
                };

                _context.Mantenimientos.Add(mant);
                await _context.SaveChangesAsync();

                var vehiculo = await _context.Vehiculos.FindAsync(VehiculoId);

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(),
                    rol: GetRol(),
                    accion: "Crear",
                    modulo: "Mantenimiento",
                    detalle: $"Mantenimiento registrado — Vehículo: {vehiculo?.Placa ?? "-"}, Tipo: {TipoMantenimiento}, Taller: {NombreTaller}",
                    registroId: mant.Id
                );

                return Ok(mant);
            }
            catch (Exception ex)
            {
                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(),
                    rol: GetRol(),
                    accion: "Crear",
                    modulo: "Mantenimiento",
                    detalle: $"Error: {ex.Message}",
                    resultado: "Fallido"
                );
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // =====================================
        // PUT — FINALIZAR
        // =====================================

        [HttpPut("{id}/finalizar")]
        public async Task<IActionResult> Finalizar(
            int id,
            [FromBody] FinalizarMantenimientoDto dto)
        {
            var mant = await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mant == null) return NotFound();

            mant.Estado = "Finalizado";
            mant.FechaSalida = dto.FechaSalida ?? DateTime.Now;

            if (dto.ObservacionesFinal != null)
                mant.Observaciones = dto.ObservacionesFinal;

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(),
                rol: GetRol(),
                accion: "Finalizar",
                modulo: "Mantenimiento",
                detalle: $"Mantenimiento #{id} finalizado — Vehículo: {mant.Vehiculo?.Placa ?? "-"}",
                registroId: id
            );

            return Ok(mant);
        }

        // =====================================
        // PUT — EDITAR
        // =====================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(
            int id,
            [FromForm] string TipoMantenimiento,
            [FromForm] string NombreTaller,
            [FromForm] string TrabajosRealizados,
            [FromForm] decimal CostoManoObra,
            [FromForm] decimal CostoRepuestos,
            [FromForm] string? TecnicoResponsable,
            [FromForm] string? TelefonoTaller,
            [FromForm] string? RepuestosUtilizados,
            [FromForm] string? Observaciones,
            [FromForm] int? KilometrajeSiguiente,
            [FromForm] DateTime? FechaSiguiente,
            [FromForm] string Estado
        )
        {
            var mant = await _context.Mantenimientos.FindAsync(id);
            if (mant == null) return NotFound();

            mant.TipoMantenimiento = TipoMantenimiento;
            mant.NombreTaller = NombreTaller;
            mant.TrabajosRealizados = TrabajosRealizados;
            mant.CostoManoObra = CostoManoObra;
            mant.CostoRepuestos = CostoRepuestos;
            mant.CostoTotal = CostoManoObra + CostoRepuestos;
            mant.TecnicoResponsable = TecnicoResponsable;
            mant.TelefonoTaller = TelefonoTaller;
            mant.RepuestosUtilizados = RepuestosUtilizados;
            mant.Observaciones = Observaciones;
            mant.KilometrajeSiguiente = KilometrajeSiguiente;
            mant.FechaSiguiente = FechaSiguiente;
            mant.Estado = Estado;

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(),
                rol: GetRol(),
                accion: "Editar",
                modulo: "Mantenimiento",
                detalle: $"Mantenimiento #{id} editado — Tipo: {TipoMantenimiento}",
                registroId: id
            );

            return Ok(mant);
        }

        // =====================================
        // DELETE
        // =====================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var mant = await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mant == null) return NotFound();

            var placa = mant.Vehiculo?.Placa ?? "-";

            _context.Mantenimientos.Remove(mant);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(),
                rol: GetRol(),
                accion: "Eliminar",
                modulo: "Mantenimiento",
                detalle: $"Mantenimiento #{id} eliminado — Vehículo: {placa}",
                registroId: id
            );

            return Ok();
        }

        // =====================================
        // GET PRÓXIMOS
        // =====================================

        [HttpGet("proximos")]
        public async Task<IActionResult> GetProximos()
        {
            var fecha = DateTime.Now.AddDays(30);

            var lista = await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                .Where(m =>
                    m.Estado == "Finalizado" &&
                    m.FechaSiguiente != null &&
                    m.FechaSiguiente <= fecha)
                .OrderBy(m => m.FechaSiguiente)
                .Select(m => new
                {
                    m.Id,
                    m.TipoMantenimiento,
                    m.FechaSiguiente,
                    m.KilometrajeSiguiente,
                    Vehiculo = new
                    {
                        m.Vehiculo!.Id,
                        m.Vehiculo.Placa,
                        m.Vehiculo.Marca,
                        m.Vehiculo.Modelo
                    }
                })
                .ToListAsync();

            return Ok(lista);
        }
    }

    public class FinalizarMantenimientoDto
    {
        public DateTime? FechaSalida { get; set; }
        public string? ObservacionesFinal { get; set; }
    }
}