using System.ComponentModel.DataAnnotations;

namespace SistemaFlota
{
    public class Auditoria
    {
        [Key]
        public int Id { get; set; }

        // QUIÉN
        public string Usuario { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;

        // QUÉ
        // Login | Logout | Crear | Editar | Eliminar | Firmar | Revisar | Subir | Finalizar
        public string Accion { get; set; } = string.Empty;

        // DÓNDE
        // Inspecciones | Autorizaciones | Usuarios | Documentos | Mantenimiento | Incidentes | Configuracion
        public string Modulo { get; set; } = string.Empty;

        // DETALLE
        public string? Detalle { get; set; }

        // ID DEL REGISTRO AFECTADO
        public int? RegistroId { get; set; }

        // CUÁNDO
        public DateTime Fecha { get; set; } = DateTime.Now;

        // IP
        public string? IpAddress { get; set; }

        // RESULTADO
        // Exitoso | Fallido
        public string Resultado { get; set; } = "Exitoso";
    }
}