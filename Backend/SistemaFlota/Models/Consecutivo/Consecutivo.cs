using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.Models.Consecutivo
{
    public class Consecutivo
    {
        public int Id { get; set; }

        public string Modulo { get; set; } = string.Empty;

        public string Prefijo { get; set; } = string.Empty;

        public int UltimoNumero { get; set; }

        public int Longitud { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = default!;
    }
}
