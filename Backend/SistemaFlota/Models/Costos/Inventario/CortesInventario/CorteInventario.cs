namespace SistemaFlota.Models.Costos.Inventario.CortesInventario
{
    public class CorteInventario
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public string Estado { get; set; } = "Pendiente";

        public int UsuarioId { get; set; }

        public ICollection<DetalleCorteInventario> Detalles { get; set; }
            = new List<DetalleCorteInventario>();
    }
}
