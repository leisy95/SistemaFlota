namespace SistemaFlota.DTOs.Usuarios
{
    public class PermisoDto
    {
        public string Modulo { get; set; } = string.Empty;

        public bool PuedeVer { get; set; } = true;

        public bool PuedeCrear { get; set; } = false;

        public bool PuedeEditar { get; set; } = false;

        public bool PuedeEliminar { get; set; } = false;

        public bool PuedeEnviarCorreo { get; set; } = false;

        public bool PuedeVerDatosNumericos { get; set; } = false;
    }
}
