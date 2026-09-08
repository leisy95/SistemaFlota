using Microsoft.EntityFrameworkCore;

namespace SistemaFlota.Models.VistasInventarios
{
    [Keyless]
    public class VInvReferencia
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descrip { get; set; } = string.Empty;
        public string Linea { get; set; } = string.Empty;
        public string Deslinea { get; set; } = string.Empty;
    }
}
