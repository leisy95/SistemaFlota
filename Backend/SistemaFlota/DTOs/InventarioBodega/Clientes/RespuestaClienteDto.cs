namespace SistemaFlota.DTOs.InventarioBodega.Clientes
{
    public class RespuestaClienteDto
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = "";
        public int? Id { get; set; }
    }
}
