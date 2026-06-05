using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PedidosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;
        private readonly ITwilioService _twilio;

        public PedidosController(
            AppDbContext context,
            AuditoriaService auditoria,
            ITwilioService twilio)
        {
            _context = context;
            _auditoria = auditoria;
            _twilio = twilio;
        }

        private string GetUsuario() =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";
        private string GetRol() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

        // =====================================
        // GET — TODOS
        // =====================================
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lista = await _context.Pedidos
                .OrderByDescending(p => p.FechaRegistro)
                .ToListAsync();
            return Ok(lista);
        }

        // =====================================
        // GET — POR ID
        // =====================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();
            return Ok(pedido);
        }

        // =====================================
        // POST — CREAR PEDIDO
        // =====================================
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CrearPedidoDto dto)
        {
            var pedido = new Pedido
            {
                VendedorNombre = dto.VendedorNombre,
                Cliente = dto.Cliente,
                Referencia = dto.Referencia,
                Destino = dto.Destino,
                CantidadKg = dto.CantidadKg,
                CantidadUnidades = dto.CantidadUnidades,
                Prioridad = dto.Prioridad,
                Observaciones = dto.Observaciones,
                Estado = "Pendiente",
                FechaRegistro = FechaHelper.Ahora()
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Crear", modulo: "Pedidos",
                detalle: $"Pedido #{pedido.Id} — Cliente: {dto.Cliente}, Destino: {dto.Destino}, Prioridad: {dto.Prioridad}",
                registroId: pedido.Id
            );

            // ── TWILIO: solo si es SOS o Urgente → solo a contactos con RecibePedidos ──
            if (dto.Prioridad == "SOS" || dto.Prioridad == "Urgente")
            {
                var ahora = FechaHelper.Ahora();
                var hora = ahora.ToString("hh:mm tt");
                var fecha = ahora.ToString("dd/MM/yyyy");

                var emoji = dto.Prioridad == "SOS" ? "🔴" : "🟡";

                var cantidad = "";
                if (dto.CantidadKg.HasValue) cantidad += $"{dto.CantidadKg} kg";
                if (dto.CantidadUnidades.HasValue) cantidad += (cantidad.Length > 0 ? " / " : "") + $"{dto.CantidadUnidades} uds";

                var mensaje =
                    $"{emoji} *PEDIDO {dto.Prioridad.ToUpper()}*\n" +
                    $"━━━━━━━━━━━━━━━━━━\n" +
                    $"👤 Vendedor: {dto.VendedorNombre}\n" +
                    $"🏪 Cliente: {dto.Cliente}\n" +
                    $"📦 Referencia: {dto.Referencia}\n" +
                    $"📍 Destino: {dto.Destino}\n" +
                    $"⚖️ Cantidad: {cantidad}\n" +
                    (dto.Observaciones != null ? $"📋 Obs: {dto.Observaciones}\n" : "") +
                    $"🕐 Hora: {hora} — {fecha}\n" +
                    $"━━━━━━━━━━━━━━━━━━\n" +
                    $"⚠️ Requiere atención inmediata";

                var numerosBodyga = await _context.ContactosNotificacion
                    .Where(c => c.Activo && c.RecibePedidos)
                    .Select(c => c.NumeroWhatsApp)
                    .ToListAsync();

                Console.WriteLine($"📦 Contactos bodega: {numerosBodyga.Count}");
                if (numerosBodyga.Any())
                    await _twilio.EnviarAMultiplesAsync(numerosBodyga, mensaje);
            }

            return Ok(pedido);
        }

        // =====================================
        // PUT — CAMBIAR ESTADO
        // =====================================
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoPedidoDto dto)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            var estadoAnterior = pedido.Estado;
            pedido.Estado = dto.Estado;
            pedido.GestionadoPor = dto.GestionadoPor;

            if (dto.Estado == "Despachado") pedido.FechaDespacho = FechaHelper.Ahora();
            if (dto.Estado == "Entregado") pedido.FechaEntrega = FechaHelper.Ahora();

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Editar", modulo: "Pedidos",
                detalle: $"Pedido #{id} — Estado: {estadoAnterior} → {dto.Estado}, por: {dto.GestionadoPor}",
                registroId: id
            );

            return Ok(pedido);
        }

        // =====================================
        // PUT — EDITAR PEDIDO
        // =====================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] CrearPedidoDto dto)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            pedido.VendedorNombre = dto.VendedorNombre;
            pedido.Cliente = dto.Cliente;
            pedido.Referencia = dto.Referencia;
            pedido.Destino = dto.Destino;
            pedido.CantidadKg = dto.CantidadKg;
            pedido.CantidadUnidades = dto.CantidadUnidades;
            pedido.Prioridad = dto.Prioridad;
            pedido.Observaciones = dto.Observaciones;

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Editar", modulo: "Pedidos",
                detalle: $"Pedido #{id} editado — Cliente: {dto.Cliente}, Prioridad: {dto.Prioridad}",
                registroId: id
            );

            return Ok(pedido);
        }

        // =====================================
        // DELETE
        // =====================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();
            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();
            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Eliminar", modulo: "Pedidos",
                detalle: $"Pedido #{id} eliminado",
                registroId: id
            );
            return Ok();
        }
    }

    public class CrearPedidoDto
    {
        public string VendedorNombre { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public decimal? CantidadKg { get; set; }
        public decimal? CantidadUnidades { get; set; }
        public string Prioridad { get; set; } = "Normal";
        public string? Observaciones { get; set; }
    }

    public class CambiarEstadoPedidoDto
    {
        public string Estado { get; set; } = string.Empty;
        public string? GestionadoPor { get; set; }
    }
}