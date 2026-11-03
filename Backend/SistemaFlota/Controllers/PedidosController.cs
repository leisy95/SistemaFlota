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
        private readonly IFlotaChatAvisos _avisos;

        public PedidosController(AppDbContext context, AuditoriaService auditoria, IFlotaChatAvisos avisos)
        {
            _context = context;
            _auditoria = auditoria;
            _avisos = avisos;
        }

        private string GetUsuario() =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";
        private string GetRol() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Referencias)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null) return NotFound();
            return Ok(pedido);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CrearPedidoDto dto)
        {
            var username = GetUsuario();
            var creadorId = await _context.Usuarios
                .Where(u => u.Username == username)
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync();

            var pedido = new Pedido
            {
                VendedorNombre = dto.VendedorNombre,
                Cliente = dto.Cliente,
                Destino = dto.Destino,
                Prioridad = dto.Prioridad,
                Observaciones = dto.Observaciones,
                Estado = "Pendiente",
                FechaRegistro = FechaHelper.Ahora(),
                CreadoPorUsuarioId = creadorId,
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
                usuario: username, rol: GetRol(),
                accion: "Crear", modulo: "Pedidos",
                detalle: $"Pedido #{pedido.Id} — Cliente: {dto.Cliente}, Destino: {dto.Destino}, Prioridad: {dto.Prioridad}, Referencias: {dto.Referencias.Count}",
                registroId: pedido.Id
            );

            // Un solo aviso en el grupo Pedidos: ahí mismo se confirma.
            try
            {
                var ahora = FechaHelper.Ahora();
                var emoji = dto.Prioridad == "SOS" ? "🔴" : dto.Prioridad == "Urgente" ? "🟡" : "🟢";

                var refsTexto = string.Join("\n", dto.Referencias.Select(r =>
                {
                    var cant = "";
                    if (r.CantidadKg.HasValue) cant += $"{r.CantidadKg} kg";
                    if (r.CantidadUnidades.HasValue) cant += (cant.Length > 0 ? " / " : "") + $"{r.CantidadUnidades} uds";
                    return $"  • {r.Referencia} — {cant}";
                }));

                var mensaje =
                    $"{emoji} *PEDIDO #{pedido.Id} {dto.Prioridad.ToUpper()}*\n" +
                    $"━━━━━━━━━━━━━━━━━━\n" +
                    $"👤 Vendedor: {dto.VendedorNombre}\n" +
                    $"🏪 Cliente: {dto.Cliente}\n" +
                    $"📍 Destino: {dto.Destino}\n" +
                    $"📦 Referencias:\n{refsTexto}\n" +
                    (dto.Observaciones != null ? $"📋 Obs: {dto.Observaciones}\n" : "") +
                    $"🕐 Hora: {ahora:hh:mm tt} — {ahora:dd/MM/yyyy}\n" +
                    "━━━━━━━━━━━━━━━━━━" +
                    (dto.Prioridad == "SOS" || dto.Prioridad == "Urgente" ? "\n⚠️ Requiere atención inmediata" : "") +
                    $"\n\n👉 Responde *CONFIRMAR {pedido.Id}*";

                pedido.AvisoMensajeId = await _avisos.PublicarEnGrupoAsync("Pedidos", mensaje);
                await _context.SaveChangesAsync();

                Console.WriteLine($"[Pedidos] Pedido #{pedido.Id} publicado en FlotaChat (mensaje {pedido.AvisoMensajeId?.ToString() ?? "no enviado"}).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Pedidos] Error enviando el aviso del pedido #{pedido.Id}: {ex}");
            }

            return Ok(pedido);
        }

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