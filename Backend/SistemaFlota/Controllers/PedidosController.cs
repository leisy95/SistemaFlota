using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SistemaFlota.DTOs;

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
        public async Task<IActionResult> Get(
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 20,
            [FromQuery] string? buscar = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? prioridad = null)
        {
            var query = _context.Pedidos
                .Include(p => p.Referencias)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var q = buscar.ToLower();
                query = query.Where(p =>
                    p.Cliente.ToLower().Contains(q) ||
                    p.VendedorNombre.ToLower().Contains(q) ||
                    p.Destino.ToLower().Contains(q));
            }

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(p => p.Estado == estado);

            if (!string.IsNullOrWhiteSpace(prioridad))
                query = query.Where(p => p.Prioridad == prioridad);

            var total = await query.CountAsync();

            var lista = await query
                .OrderByDescending(p => p.FechaRegistro)
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

        // =====================================
        // GET — POR ID
        // =====================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Referencias)
                .FirstOrDefaultAsync(p => p.Id == id);
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
                Destino = dto.Destino,
                Prioridad = dto.Prioridad,
                Observaciones = dto.Observaciones,
                Estado = "Pendiente",
                FechaRegistro = FechaHelper.Ahora(),
                Referencias = dto.Referencias.Select(r => new PedidoReferencia
                {
                    Referencia = r.Referencia,
                    CantidadKg = r.CantidadKg,
                    CantidadUnidades = r.CantidadUnidades
                }).ToList()
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Crear", modulo: "Pedidos",
                detalle: $"Pedido #{pedido.Id} — Cliente: {dto.Cliente}, Destino: {dto.Destino}, Prioridad: {dto.Prioridad}, Referencias: {dto.Referencias.Count}",
                registroId: pedido.Id
            );

            // ── Notificar SIEMPRE, sin importar prioridad ──
            {
                var ahora = FechaHelper.Ahora();
                var hora = ahora.ToString("hh:mm tt");
                var fecha = ahora.ToString("dd/MM/yyyy");
                var emoji = dto.Prioridad == "SOS" ? "🔴" : dto.Prioridad == "Urgente" ? "🟡" : "🟢";

                // ── Construir lista de referencias ──
                var refsTexto = string.Join("\n", dto.Referencias.Select(r =>
                {
                    var cant = "";
                    if (r.CantidadKg.HasValue) cant += $"{r.CantidadKg} kg";
                    if (r.CantidadUnidades.HasValue) cant += (cant.Length > 0 ? " / " : "") + $"{r.CantidadUnidades} uds";
                    return $"  • {r.Referencia} — {cant}";
                }));

                var mensaje =
                    $"{emoji} *PEDIDO {dto.Prioridad.ToUpper()}*\n" +
                    $"━━━━━━━━━━━━━━━━━━\n" +
                    $"👤 Vendedor: {dto.VendedorNombre}\n" +
                    $"🏪 Cliente: {dto.Cliente}\n" +
                    $"📍 Destino: {dto.Destino}\n" +
                    $"📦 Referencias:\n{refsTexto}\n" +
                    (dto.Observaciones != null ? $"📋 Obs: {dto.Observaciones}\n" : "") +
                    $"🕐 Hora: {hora} — {fecha}\n" +
                    $"━━━━━━━━━━━━━━━━━━" +
                    (dto.Prioridad == "SOS" || dto.Prioridad == "Urgente" ? "\n⚠️ Requiere atención inmediata" : "");

                var numerosBodega = await _context.ContactosNotificacion
                    .Where(c => c.Activo && c.RecibePedidos)
                    .Select(c => c.NumeroWhatsApp)
                    .ToListAsync();

                Console.WriteLine($"📦 Contactos bodega: {numerosBodega.Count}");
                if (numerosBodega.Any())
                    await _twilio.EnviarAMultiplesAsync(numerosBodega, mensaje);
            }

            return Ok(pedido);
        }

        // =====================================
        // PUT — EDITAR PEDIDO
        // =====================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] CrearPedidoDto dto)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Referencias)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null) return NotFound();

            pedido.VendedorNombre = dto.VendedorNombre;
            pedido.Cliente = dto.Cliente;
            pedido.Destino = dto.Destino;
            pedido.Prioridad = dto.Prioridad;
            pedido.Observaciones = dto.Observaciones;

            // ── Actualizar referencias ──
            _context.PedidoReferencias.RemoveRange(pedido.Referencias);
            pedido.Referencias = dto.Referencias.Select(r => new PedidoReferencia
            {
                PedidoId = id,
                Referencia = r.Referencia,
                CantidadKg = r.CantidadKg,
                CantidadUnidades = r.CantidadUnidades
            }).ToList();

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
        // DELETE
        // =====================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Referencias)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null) return NotFound();

            _context.PedidoReferencias.RemoveRange(pedido.Referencias);
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
}