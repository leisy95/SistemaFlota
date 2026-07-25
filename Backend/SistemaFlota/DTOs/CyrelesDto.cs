namespace SistemaFlota.DTOs
{
    public class CajonDto
    {
        public int Numero { get; set; }
        public string? Descripcion { get; set; }
    }

    public class RegistroDto
    {
        public int CajonId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public List<IFormFile>? Fotos { get; set; }
    }
}