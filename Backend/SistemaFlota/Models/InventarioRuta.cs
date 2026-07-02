namespace SistemaFlota
{
    public class InventarioRuta
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Archivo { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public string? CreadoPor { get; set; }
    }
}
