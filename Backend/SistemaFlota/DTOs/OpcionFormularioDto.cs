namespace SistemaFlota.DTOs
{
    public class OpcionFormularioDto
    {
        public string Categoria { get; set; } = string.Empty;
        public int? TipoFormatoId { get; set; }
        public string Valor { get; set; } = string.Empty;
        public int Orden { get; set; } = 0;
    }
}