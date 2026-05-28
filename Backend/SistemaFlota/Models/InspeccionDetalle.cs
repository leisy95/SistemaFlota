using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota
{
    public class InspeccionDetalle
    {
        [Key]
        public int Id
        { get; set; }

        // INSPECCIÓN

        public int InspeccionId
        { get; set; }

        [ForeignKey("InspeccionId")]
        public Inspeccion?
            Inspeccion
        { get; set; }

        // CHECKLIST ITEM

        public int ChecklistItemId
        { get; set; }

        [ForeignKey("ChecklistItemId")]
        public ChecklistItem?
            ChecklistItem
        { get; set; }

        // RESULTADO

        public string Estado
        { get; set; }
            = string.Empty;

        // OBSERVACIÓN

        public string? Observacion
        { get; set; }

        // FOTO EVIDENCIA

        public string? FotoEvidencia
        { get; set; }
    }
}