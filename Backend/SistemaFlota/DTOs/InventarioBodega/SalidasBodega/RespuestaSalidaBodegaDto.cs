namespace SistemaFlota.DTOs.InventarioBodega.SalidasBodega
{
    public class RespuestaSalidaBodegaDto
    {
        public bool Exito { get; set; }

        public string Mensaje { get; set; } = string.Empty;

        public int? Id { get; set; }
    }
}
