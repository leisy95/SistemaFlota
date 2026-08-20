using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace SistemaFlota.Services.Pdf.Components;

public static class MarcaAgua
{
    public static void Dibujar(IContainer container, string imagen)
    {
        if (!File.Exists(imagen))
            return;

        container
           .PaddingLeft(-80)      
           .PaddingTop(40)        
           .PaddingRight(-30)     
           .PaddingBottom(-20)    
           .AlignCenter()
           .AlignMiddle()
           .Scale(4.9f)          
           .Image(imagen)
           .FitArea();
    }
}