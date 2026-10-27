namespace SistemaFlota.DTOs.InformacionKasoft
{
    public class MezclaPorOrdenDto
    {
        public int Numop { get; set; }

        public string Proceso { get; set; } = string.Empty;

        public string Refer { get; set; } = string.Empty;

        public string? Descrip { get; set; }

        public double? Cant { get; set; }

        public string? Unm { get; set; }

        public double? Porc { get; set; }

        public string Observ { get; set; } = string.Empty;
    }
}
