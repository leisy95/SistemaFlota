using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace SistemaFlota.Services.ImpresionEtiquetas.Componentes;

public static class QrComponent
{
    public static void Dibujar(
        IContainer container,
        string contenidoQr)
    {
        container
            .Border(1)
            .Padding(5)
            .AlignCenter()
            .AlignMiddle()
            .Height(120)
            .Text("QR");
    }
}