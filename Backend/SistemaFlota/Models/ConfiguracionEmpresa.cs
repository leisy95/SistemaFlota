using System.ComponentModel.DataAnnotations;

namespace SistemaFlota
{
    public class ConfiguracionEmpresa
    {
        [Key]
        public int Id { get; set; }

        public string NombreEmpresa { get; set; } = string.Empty;
        public string NIT { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public string ColorCorporativo { get; set; } = "#15803d";
        public string? SitioWeb { get; set; }
        public string? Descripcion { get; set; }
    }
}