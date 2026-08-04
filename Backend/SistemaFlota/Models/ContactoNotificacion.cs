using System.ComponentModel.DataAnnotations;

namespace SistemaFlota
{
    public class ContactoNotificacion
    {
        [Key]
        public int Id { get; set; }
        // NOMBRE DEL ÁREA O PERSONA
        public string Nombre { get; set; } = string.Empty;
        // ÁREA
        // Operaciones | RRHH | Gerencia | Mantenimiento | Otro
        public string Area { get; set; } = string.Empty;
        // NÚMERO WHATSAPP (con código de país, ej: 573001234567)
        public string NumeroWhatsApp { get; set; } = string.Empty;
        // ACTIVO
        public bool Activo { get; set; } = true;
        // RECIBE NOTIFICACIONES DE INCIDENTES
        public bool RecibeIncidentes { get; set; } = true;
        // RECIBE NOTIFICACIONES DE PEDIDOS
        public bool RecibePedidos { get; set; } = false;
        // RECIBE NOTIFICACIONES DE INSPECCIONES
        public bool RecibeInspecciones { get; set; } = false;
        public bool RecibeLiberaciones { get; set; } = false;
    }
}