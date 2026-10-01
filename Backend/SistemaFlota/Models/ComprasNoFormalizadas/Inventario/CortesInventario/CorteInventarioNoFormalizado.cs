namespace SistemaFlota.Models.ComprasNoFormalizadas.Inventario.CortesInventario
{
    public class CorteInventarioNoFormalizado
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public string Estado { get; set; } = "Pendiente";

        public int UsuarioId { get; set; }

        public ICollection<DetalleCorteInventarioNoFormalizado> Detalles { get; set; }
            = new List<DetalleCorteInventarioNoFormalizado>();
    }
}
