using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs;

namespace SistemaFlota
{
   
    public class ConfirmacionPedidosService
    {
        private static readonly Regex Numero = new(@"\b(\d{1,9})\b", RegexOptions.Compiled);

        private readonly AppDbContext _context;
        private readonly IFlotaChatAvisos _avisos;
        private readonly AuditoriaService _auditoria;
        private readonly ILogger<ConfirmacionPedidosService> _logger;

        public ConfirmacionPedidosService(AppDbContext context, IFlotaChatAvisos avisos, AuditoriaService auditoria,
                                          ILogger<ConfirmacionPedidosService> logger)
        {
            _context = context;
            _avisos = avisos;
            _auditoria = auditoria;
            _logger = logger;
        }

        public async Task ProcesarAsync(EventoFlotaChatDto evento)
        {
            if (evento.TipoMensaje == "ubicacion") return;

            var t = InterpreteConductor.Normalizar(evento.Contenido);
            if (!t.Split(' ').Any(p => p.StartsWith("confirm"))) return;   

            // ¿Está autorizado? Contacto de pedidos, activo y vinculado.
            var contacto = await (from v in _context.VinculacionesFlotaChat
                                  join c in _context.ContactosNotificacion on v.EntidadId equals c.Id
                                  where v.FlotaChatUsuarioId == evento.UsuarioId && v.TipoEntidad == "ContactoNotificacion"
                                        && c.Activo && c.RecibePedidos
                                  select c).FirstOrDefaultAsync();

            if (contacto == null)
            {
                await _avisos.EnviarAUsuarioAsync(evento.UsuarioId,
                    "⚠️ No estás autorizado para confirmar pedidos. Si deberías poder hacerlo, avisa al administrador.", "Pedidos");
                return;
            }

            // ¿Qué pedido? El número escrito, o el único pendiente.
            Pedido? pedido;
            var coincidencia = Numero.Match(t);
            if (coincidencia.Success)
            {
                var id = int.Parse(coincidencia.Groups[1].Value);
                pedido = await _context.Pedidos.FindAsync(id);
                if (pedido == null)
                {
                    await _avisos.PublicarEnGrupoAsync("Pedidos", $"❌ No encontré el pedido #{id}.");
                    return;
                }
            }
            else
            {
                var pendientes = await _context.Pedidos
                    .Where(p => p.Estado == "Pendiente")
                    .OrderBy(p => p.FechaRegistro)
                    .Select(p => p.Id)
                    .Take(11)
                    .ToListAsync();

                if (pendientes.Count == 0)
                {
                    await _avisos.PublicarEnGrupoAsync("Pedidos", "No hay pedidos pendientes por confirmar.");
                    return;
                }
                if (pendientes.Count > 1)
                {
                    var lista = string.Join(", ", pendientes.Take(10).Select(p => $"#{p}")) + (pendientes.Count > 10 ? "…" : "");
                    await _avisos.PublicarEnGrupoAsync("Pedidos",
                        $"Hay varios pedidos pendientes: {lista}.\nEscribe *CONFIRMAR* y el número, por ejemplo: *CONFIRMAR {pendientes[0]}*");
                    return;
                }
                pedido = await _context.Pedidos.FindAsync(pendientes[0]);
                if (pedido == null) return;
            }

            if (pedido.Estado != "Pendiente")
            {
                var quien = string.IsNullOrWhiteSpace(pedido.GestionadoPor) ? "" : $" (lo tomó {pedido.GestionadoPor})";
                await _avisos.PublicarEnGrupoAsync("Pedidos", $"El pedido #{pedido.Id} ya está *{pedido.Estado}*{quien}.");
                return;
            }

            // Solo cambia si sigue "Pendiente": si dos personas confirman a la vez, gana una.
            var ahora = FechaHelper.Ahora();
            var nombre = string.IsNullOrWhiteSpace(contacto.Nombre) ? evento.NombreConductor : contacto.Nombre;
            var cambiados = await _context.Pedidos
                .Where(p => p.Id == pedido.Id && p.Estado == "Pendiente")
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Estado, "En proceso")
                    .SetProperty(p => p.GestionadoPor, nombre)
                    .SetProperty(p => p.FechaConfirmacion, (DateTime?)ahora)
                    .SetProperty(p => p.ConfirmadoPorFlotaChatUsuarioId, (int?)evento.UsuarioId));

            if (cambiados == 0)
            {
                await _context.Entry(pedido).ReloadAsync();
                await _avisos.PublicarEnGrupoAsync("Pedidos",
                    $"El pedido #{pedido.Id} ya está *{pedido.Estado}* (lo tomó {pedido.GestionadoPor ?? "otra persona"}).");
                return;
            }

            await _auditoria.RegistrarAsync(
                usuario: nombre, rol: "FlotaChat",
                accion: "Editar", modulo: "Pedidos",
                detalle: $"Pedido #{pedido.Id} — Estado: Pendiente → En proceso, confirmado por chat ({nombre})",
                registroId: pedido.Id);

            await _avisos.PublicarEnGrupoAsync("Pedidos", $"✅ Pedido #{pedido.Id} en proceso, confirmado por {nombre}.");

            // Directo a quien creó el pedido, si está vinculado.
            var vendedor = pedido.CreadoPorUsuarioId == null ? null : await _context.VinculacionesFlotaChat
                .Where(v => v.TipoEntidad == "Usuario" && v.EntidadId == pedido.CreadoPorUsuarioId)
                .Select(v => (int?)v.FlotaChatUsuarioId)
                .FirstOrDefaultAsync();

            if (vendedor.HasValue)
                await _avisos.EnviarAUsuarioAsync(vendedor.Value,
                    $"📦 Pedido #{pedido.Id} recibido, su pedido está en proceso.\n🏪 Cliente: {pedido.Cliente}", "Pedidos");
            else
                _logger.LogWarning("Pedido #{Pedido}: quien lo creó (usuario {Usuario}) no está vinculado a FlotaChat; no se le avisó",
                    pedido.Id, pedido.CreadoPorUsuarioId);
        }
    }
}