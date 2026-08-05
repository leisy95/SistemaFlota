using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CambioRutaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;
        private readonly IMensajeriaService _mensajeria;

        public CambioRutaController(
            AppDbContext context,
            AuditoriaService auditoria,
            IMensajeriaService twilio)
        {
            _context = context;
            _auditoria = auditoria;
            _mensajeria = twilio;
        }

        private string GetUsuario() =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";
        private string GetRol() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lista = await _context.CambiosRuta
                .Include(c => c.Conductor)
                .Include(c => c.Vehiculo)
                .Include(c => c.Autorizacion)
                .OrderByDescending(c => c.FechaSolicitud)
                .Select(c => new
                {
                    c.Id,
                    c.FechaSolicitud,
                    c.RutaOriginal,
                    c.NuevaRuta,
                    c.MotivoCambio,
                    c.Estado,
                    c.AutorizadoPor,
                    c.ObservacionAut,
                    c.FechaAutorizacion,
                    c.AutorizacionId,
                    Conductor = new { c.Conductor!.Id, c.Conductor.Nombre, c.Conductor.Telefono },
                    Vehiculo = new { c.Vehiculo!.Id, c.Vehiculo.Placa }
                })
                .ToListAsync();
            return Ok(lista);
        }

        [HttpGet("pendientes")]
        public async Task<IActionResult> GetPendientes()
        {
            var lista = await _context.CambiosRuta
                .Include(c => c.Conductor)
                .Include(c => c.Vehiculo)
                .Where(c => c.Estado == "Pendiente")
                .OrderByDescending(c => c.FechaSolicitud)
                .ToListAsync();
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cambio = await _context.CambiosRuta
                .Include(c => c.Conductor)
                .Include(c => c.Vehiculo)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cambio == null) return NotFound();
            return Ok(cambio);
        }

        // =====================================
        // POST — SOLICITAR CAMBIO
        // =====================================
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CrearCambioRutaDto dto)
        {
            string rutaOriginal = dto.RutaOriginal;
            string? numeroGuia = null;
            string? destino = null;

            // -- Tomar datos de la autorización de salida --
            if (dto.AutorizacionId.HasValue)
            {
                var aut = await _context.Autorizaciones
                    .FirstOrDefaultAsync(a => a.Id == dto.AutorizacionId.Value);
                if (aut != null)
                {
                    if (!string.IsNullOrWhiteSpace(aut.DestinoCompleto))
                        rutaOriginal = aut.DestinoCompleto;
                    numeroGuia = aut.NumeroGuia;
                    destino = aut.DestinoCompleto;
                }
            }

            var cambio = new CambioRuta
            {
                AutorizacionId = dto.AutorizacionId,
                ConductorId = dto.ConductorId,
                VehiculoId = dto.VehiculoId,
                RutaOriginal = rutaOriginal,
                NuevaRuta = dto.NuevaRuta,
                MotivoCambio = dto.MotivoCambio,
                Estado = "Pendiente",
                FechaSolicitud = DateTime.Now
            };

            _context.CambiosRuta.Add(cambio);
            await _context.SaveChangesAsync();

            var conductor = await _context.Conductores.FindAsync(dto.ConductorId);
            var vehiculo = await _context.Vehiculos.FindAsync(dto.VehiculoId);

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Crear", modulo: "CambioRuta",
                detalle: $"Cambio de ruta solicitado — Conductor: {conductor?.Nombre ?? "-"}, Nueva ruta: {dto.NuevaRuta}",
                registroId: cambio.Id
            );

            // -- TWILIO: notificar al grupo --
            var hora = DateTime.Now.ToString("hh:mm tt");
            var fecha = DateTime.Now.ToString("dd/MM/yyyy");

            var mensajeGrupo =
                $"?? *SOLICITUD CAMBIO DE RUTA*\n" +
                $"?? Conductor: {conductor?.Nombre ?? "-"}\n" +
                $"?? Vehículo: {vehiculo?.Placa ?? "-"}\n" +
                (dto.AutorizacionId.HasValue ? $"?? Auth #{dto.AutorizacionId}\n" : "") +
                (numeroGuia != null ? $"?? Guía: {numeroGuia}\n" : "") +
                $"?? Ruta autorizada: {rutaOriginal}\n" +
                $"?? Nueva ruta solicitada: {dto.NuevaRuta}\n" +
                $"?? Motivo: {dto.MotivoCambio}\n" +
                $"?? Hora: {hora} — {fecha}\n" +
                $"?? Requiere autorización";

            var numerosGrupo = await _context.ContactosNotificacion
                .Where(c => c.Activo && c.RecibeIncidentes)
                .Select(c => c.NumeroWhatsApp)
                .ToListAsync();

            Console.WriteLine($"?? Contactos grupo: {numerosGrupo.Count}");
            if (numerosGrupo.Any())
                await _mensajeria.EnviarAMultiplesAsync(numerosGrupo, mensajeGrupo, "Incidentes");

            return Ok(cambio);
        }

        // =====================================
        // PUT — AUTORIZAR
        // =====================================
        [HttpPut("{id}/autorizar")]
        public async Task<IActionResult> Autorizar(int id, [FromBody] AutorizarCambioDto dto)
        {
            var cambio = await _context.CambiosRuta
                .Include(c => c.Conductor)
                .Include(c => c.Vehiculo)
                .Include(c => c.Autorizacion)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cambio == null) return NotFound();

            cambio.Estado = "Autorizado";
            cambio.AutorizadoPor = dto.AutorizadoPor;
            cambio.ObservacionAut = dto.Observacion;
            cambio.FechaAutorizacion = DateTime.Now;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Autorizar", modulo: "CambioRuta",
                detalle: $"Cambio de ruta #{id} AUTORIZADO por: {dto.AutorizadoPor}",
                registroId: id
            );

            // -- TWILIO: notificar al conductor --
            if (!string.IsNullOrWhiteSpace(cambio.Conductor?.Telefono))
            {
                var mensaje =
                    $"? *CAMBIO DE RUTA AUTORIZADO*\n" +
                    $"Hola {cambio.Conductor.Nombre.Split(' ')[0]},\n" +
                    $"Tu cambio de ruta fue autorizado.\n" +
                    (cambio.AutorizacionId.HasValue ? $"?? Auth #{cambio.AutorizacionId}\n" : "") +
                    $"?? Ruta autorizada: {cambio.RutaOriginal}\n" +
                    $"?? Nueva ruta confirmada: {cambio.NuevaRuta}\n" +
                    $"?? Autorizado por: {dto.AutorizadoPor}\n" +
                    $"?? Por favor confirma que recibiste este mensaje respondiendo CONFIRMO";

                await _mensajeria.EnviarMensajeAsync(cambio.Conductor.Telefono, mensaje, "Conductores");
            }

            return Ok(cambio);
        }

        // =====================================
        // PUT — CONFIRMAR (CONDUCTOR)
        // =====================================
        [HttpPut("{id}/confirmar")]
        public async Task<IActionResult> Confirmar(int id)
        {
            var cambio = await _context.CambiosRuta
                .Include(c => c.Conductor)
                .Include(c => c.Vehiculo)
                .Include(c => c.Autorizacion)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cambio == null) return NotFound();
            if (cambio.Estado != "Autorizado")
                return BadRequest("Solo se puede confirmar un cambio autorizado");

            cambio.Estado = "Confirmado";
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Confirmar", modulo: "CambioRuta",
                detalle: $"Cambio de ruta #{id} CONFIRMADO por conductor: {cambio.Conductor?.Nombre ?? "-"}",
                registroId: id
            );

            // -- TWILIO: notificar al grupo que conductor confirmó --
            var hora = DateTime.Now.ToString("hh:mm tt");
            var fecha = DateTime.Now.ToString("dd/MM/yyyy");

            var mensajeGrupo =
                $"? *CAMBIO DE RUTA CONFIRMADO*\n" +
                $"?? Conductor: {cambio.Conductor?.Nombre ?? "-"}\n" +
                $"?? Vehículo: {cambio.Vehiculo?.Placa ?? "-"}\n" +
                (cambio.AutorizacionId.HasValue ? $"?? Auth #{cambio.AutorizacionId}\n" : "") +
                $"?? Ruta autorizada: {cambio.RutaOriginal}\n" +
                $"?? Nueva ruta confirmada: {cambio.NuevaRuta}\n" +
                $"? El conductor confirmó el cambio de ruta\n" +
                $"?? Hora: {hora} — {fecha}";

            var numerosGrupo = await _context.ContactosNotificacion
                .Where(c => c.Activo && c.RecibeIncidentes)
                .Select(c => c.NumeroWhatsApp)
                .ToListAsync();

            if (numerosGrupo.Any())
                await _mensajeria.EnviarAMultiplesAsync(numerosGrupo, mensajeGrupo, "Incidentes");

            return Ok(cambio);
        }

        // =====================================
        // PUT — RECHAZAR
        // =====================================
        [HttpPut("{id}/rechazar")]
        public async Task<IActionResult> Rechazar(int id, [FromBody] AutorizarCambioDto dto)
        {
            var cambio = await _context.CambiosRuta
                .Include(c => c.Conductor)
                .Include(c => c.Vehiculo)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cambio == null) return NotFound();

            cambio.Estado = "Rechazado";
            cambio.AutorizadoPor = dto.AutorizadoPor;
            cambio.ObservacionAut = dto.Observacion;
            cambio.FechaAutorizacion = DateTime.Now;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Rechazar", modulo: "CambioRuta",
                detalle: $"Cambio de ruta #{id} RECHAZADO por: {dto.AutorizadoPor}",
                registroId: id
            );

            // -- TWILIO: notificar al conductor --
            if (!string.IsNullOrWhiteSpace(cambio.Conductor?.Telefono))
            {
                var mensaje =
                    $"? *CAMBIO DE RUTA RECHAZADO*\n" +
                    $"Hola {cambio.Conductor.Nombre.Split(' ')[0]},\n" +
                    $"Tu solicitud fue rechazada.\n" +
                    $"?? Motivo: {dto.Observacion ?? "Sin observación"}\n" +
                    $"?? Continúa con la ruta autorizada: {cambio.RutaOriginal}";
                await _mensajeria.EnviarMensajeAsync(cambio.Conductor.Telefono, mensaje, "Conductores");
            }

            return Ok(cambio);
        }

        // =====================================
        // DELETE
        // =====================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var cambio = await _context.CambiosRuta.FindAsync(id);
            if (cambio == null) return NotFound();
            _context.CambiosRuta.Remove(cambio);
            await _context.SaveChangesAsync();
            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Eliminar", modulo: "CambioRuta",
                detalle: $"Cambio de ruta #{id} eliminado",
                registroId: id
            );
            return Ok();
        }
    }

    public class CrearCambioRutaDto
    {
        public int? AutorizacionId { get; set; }
        public int ConductorId { get; set; }
        public int VehiculoId { get; set; }
        public string RutaOriginal { get; set; } = string.Empty;
        public string NuevaRuta { get; set; } = string.Empty;
        public string MotivoCambio { get; set; } = string.Empty;
    }

    public class AutorizarCambioDto
    {
        public string AutorizadoPor { get; set; } = string.Empty;
        public string? Observacion { get; set; }
    }
}




