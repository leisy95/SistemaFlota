using Microsoft.EntityFrameworkCore;

namespace SistemaFlota.Models.VistasInventarios
{
    [Keyless]
    public class VInvRegistroProduccion
    {
        public int Numop { get; set; }

        public DateTime Fhregistro { get; set; }

        public string Proceso { get; set; } = string.Empty;

        public string Operario { get; set; } = string.Empty;

        public string? Maquina { get; set; }

        public double? Cant { get; set; }

        public string? Unm { get; set; }

        public double? Kilos { get; set; }

        public string? Idregistro { get; set; }
    }
}
