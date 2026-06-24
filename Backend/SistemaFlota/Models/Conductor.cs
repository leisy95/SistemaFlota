using System.ComponentModel.DataAnnotations;
namespace SistemaFlota
{
    public class Conductor
    {
        [Key]
        public int Id { get; set; }

        // ========================
        // DATOS PERSONALES
        // ========================
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Foto { get; set; }
        public string? Cedula { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Direccion { get; set; }
        public string? TipoSangre { get; set; }
        public string? Eps { get; set; }
        public string? Arl { get; set; }
        public string? FondoPension { get; set; }
        public string? ContactoEmergencia { get; set; }
        public string? TelefonoEmergencia { get; set; }

        // ========================
        // LICENCIA
        // ========================
        public string Licencia { get; set; } = string.Empty;
        public string? CategoriaLicencia { get; set; } // A1, A2, B1, B2, B3, C1, C2, C3
        public DateTime? FechaVencimientoLicencia { get; set; }
        public DateTime? FechaExpedicionLicencia { get; set; }

        // ========================
        // ESTADO
        // ========================
        public string Estado { get; set; } = "Activo";
        public string? ClasificacionVehiculo { get; set; }
        public string? TipoVehiculo { get; set; } // Activo | Inactivo | Suspendido | Vacaciones

        // ========================
        // RELACIONES
        // ========================
        public List<ExamenMedico> ExamenesMedicos { get; set; } = new();
        public List<Capacitacion> Capacitaciones { get; set; } = new();
        public List<InfraccionConductor> Infracciones { get; set; } = new();
    }
}