using Microsoft.EntityFrameworkCore;

namespace SistemaFlota.Models.VistasInventarios
{
    [Keyless]
    public class VInvOrdenProduccion
    {
        public int Numop { get; set; }

        public DateTime Fechaop { get; set; }

        public TimeSpan Horaop { get; set; }

        public double Cliente { get; set; }

        public string Nombrecliente { get; set; } = string.Empty;

        public string Refer { get; set; } = string.Empty;

        public string? Descrip { get; set; }

        public double Canprog { get; set; }

        public string Um { get; set; } = string.Empty;
    }
}
