using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Services.Pdf.Styles;

namespace SistemaFlota.Services.Pdf.Components
{
    public static class HeaderEmpresa
    {
        public static void Dibujar(
            IContainer container,
            string logo,
            ConfiguracionEmpresa empresa,
            string tituloDocumento,
            string numeroDocumento)
        {
            container
                .Background(PdfColors.VerdePrincipal)
                .Padding(18)
                .Row(row =>
                {
                   // LOGO
                   row.ConstantItem(120)
                        .Background(Colors.White)
                        .CornerRadius(10)
                        .Padding(10)
                        .AlignMiddle()
                        .Element(x =>
                        {
                            if (File.Exists(logo))
                            {
                                x.Image(logo)
                                 .FitArea();
                            }
                            else
                            {
                                x.Text("LOGO")
                                 .Bold()
                                 .FontColor(PdfColors.AzulOscuro);
                            }
                        });


                    // separación
                    row.ConstantItem(25);


                    // INFORMACION EMPRESA
                    row.RelativeItem()
                        .AlignMiddle()
                        .Column(col =>
                        {
                            col.Spacing(5);


                            col.Item()
                                .Text(empresa.NombreEmpresa.ToUpper())
                                .FontSize(16)
                                .Bold()
                                .FontColor(Colors.White);


                            col.Item()
                                .Text($"NIT: {empresa.NIT}")
                                .FontSize(12)
                                .FontColor(Colors.White);


                            col.Item()
                                .Text(empresa.Direccion)
                                .FontSize(12)
                                .FontColor(Colors.White);


                            col.Item()
                                .Text($"Tel: {empresa.Telefono}")
                                .FontSize(12)
                                .FontColor(Colors.White);


                            col.Item()
                                .Text(empresa.Email)
                                .FontSize(12)
                                .FontColor(Colors.White);


                            if (!string.IsNullOrWhiteSpace(empresa.SitioWeb))
                            {
                                col.Item()
                                    .Text(empresa.SitioWeb)
                                    .FontSize(12)
                                    .FontColor(Colors.White);
                            }


                            col.Item()
                                .Text("Dosquebradas - Risaralda")
                                .FontSize(12)
                                .FontColor(Colors.White);
                        });

                    // separación
                    row.ConstantItem(25);

                    // INFORMACION DOCUMENTO
                    row.ConstantItem(120)
                        .Background(Colors.White)
                        .CornerRadius(10)
                        .Padding(10)
                        .AlignMiddle()
                        .Column(col =>
                        {
                            col.Spacing(8);

                            col.Item()
                                .AlignCenter()
                                .Text(tituloDocumento)
                                .FontSize(16)
                                .Bold()
                                .FontColor("#15803D");


                            col.Item()
                                .LineHorizontal(1)
                                .LineColor("#E5E7EB");


                            col.Item()
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.Span(numeroDocumento)
                                        .FontSize(14)
                                        .Bold();
                                });

                        });
                });
        }
    }
}