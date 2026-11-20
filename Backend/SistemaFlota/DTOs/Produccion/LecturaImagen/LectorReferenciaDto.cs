namespace SistemaFlota.DTOs.Produccion.LecturaImagen
{
    public class LectorReferenciaDto
    {
        // Texto completo reconocido por Tesseract
        public string Texto { get; set; } = string.Empty;

        // Datos separados para mostrar en Angular
        public string NumeroOrden { get; set; } = string.Empty;

        public string Codigo { get; set; } = string.Empty;

        public string NombreOperario { get; set; } = string.Empty;

        public string Turno { get; set; } = string.Empty;

        public bool LeidaCorrectamente { get; set; }

        public string Mensaje { get; set; } = string.Empty;
    }
}