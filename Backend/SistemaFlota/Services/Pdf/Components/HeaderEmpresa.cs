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
            string codigoFormato,
            string tituloDocumento,
            string numeroDocumento)
        {
            container
                .Background(PdfColors.VerdePrincipal)
                .Padding(15)
                .Row(row =>
                {
                    // LOGO
                    row.ConstantItem(110)
                        .Background(Colors.White)
                        .CornerRadius(7)
                        .Padding(9)
                        .AlignMiddle()
                        .Element(x =>
                        {
                            if (File.Exists(logo))
                            {
                                x.Image(logo).FitArea();
                            }
                            else
                            {
                                x.AlignCenter()
                                    .AlignMiddle()
                                    .Text("LOGO")
                                    .Bold()
                                    .FontSize(12)
                                    .FontColor(PdfColors.AzulOscuro);
                            }
                        });

                    // SEPARACIÓN
                    row.ConstantItem(18);

                    // INFORMACIÓN EMPRESA
                    row.RelativeItem()
                        .AlignMiddle()
                        .Column(col =>
                        {
                            col.Spacing(3);

                            col.Item()
                                .Text(empresa.NombreEmpresa.ToUpper())
                                .FontSize(15)
                                .Bold()
                                .FontColor(Colors.White);

                            col.Item()
                                .Text($"NIT: {empresa.NIT}")
                                .FontSize(9)
                                .FontColor(Colors.White);

                            col.Item()
                                .Text(empresa.Direccion)
                                .FontSize(9)
                                .FontColor(Colors.White);

                            col.Item()
                                .Text($"Tel: {empresa.Telefono}")
                                .FontSize(9)
                                .FontColor(Colors.White);

                            col.Item()
                                .Text(empresa.Email)
                                .FontSize(9)
                                .FontColor(Colors.White);

                            if (!string.IsNullOrWhiteSpace(empresa.SitioWeb))
                            {
                                col.Item()
                                    .Text(empresa.SitioWeb)
                                    .FontSize(9)
                                    .FontColor(Colors.White);
                            }

                            col.Item()
                                .Text("Dosquebradas - Risaralda")
                                .FontSize(9)
                                .FontColor(Colors.White);
                        });

                    // SEPARACIÓN
                    row.ConstantItem(18);

                    // INFORMACIÓN DEL DOCUMENTO
                    row.ConstantItem(115)
                        .Background(Colors.White)
                        .CornerRadius(7)
                        .Padding(9)
                        .AlignMiddle()
                        .Column(col =>
                        {
                            col.Spacing(6);

                            // CÓDIGO DEL FORMATO
                            col.Item()
                                .AlignCenter()
                                .Text(codigoFormato)
                                .FontSize(8)
                                .Bold()
                                .FontColor(PdfColors.AzulOscuro);

                            col.Item()
                                .AlignCenter()
                                .Text(tituloDocumento.ToUpper())
                                .FontSize(11)
                                .Bold()
                                .FontColor(PdfColors.VerdePrincipal);

                            col.Item()
                                .LineHorizontal(1)
                                .LineColor(PdfColors.GrisClaro);

                            col.Item()
                                .AlignCenter()
                                .Text(numeroDocumento)
                                .FontSize(12)
                                .Bold()
                                .FontColor(PdfColors.AzulOscuro);
                        });
                });
        }
    }
}