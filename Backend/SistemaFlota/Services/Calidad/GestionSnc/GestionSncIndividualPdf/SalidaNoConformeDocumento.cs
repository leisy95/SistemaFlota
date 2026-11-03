
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Models.Calidad;
using SistemaFlota.Services.Pdf.Components;
using SistemaFlota.Services.Pdf.Styles;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SistemaFlota.Services.Calidad.GestionSnc.GestionSncIndividualPdf
{
    public class SalidaNoConformeDocumento : IDocument
    {
        private readonly SalidaNoConforme _registro;
        private readonly List<(string Paso, byte[] Contenido)> _fotos;
        private readonly ConfiguracionEmpresa _empresa;
        private readonly string _logo;

        private static readonly CultureInfo Cultura =
            new CultureInfo("es-CO");

        private const float MARGEN = 10;
        private const float ANCHO_ETIQUETA = 55;
        private const float ALTO_FOTO = 28;
        private const float ESPACIO_FOTO = 3;

        public SalidaNoConformeDocumento(
            SalidaNoConforme registro,
            List<(string Paso, byte[] Contenido)> fotos,
            ConfiguracionEmpresa empresa,
            string logo)
        {
            _registro = registro
                ?? throw new ArgumentNullException(nameof(registro));

            _fotos = fotos ?? new List<(string Paso, byte[] Contenido)>();
            _empresa = empresa
                ?? throw new ArgumentNullException(nameof(empresa));

            _logo = logo ?? "";
        }

        public DocumentMetadata GetMetadata()
        {
            return DocumentMetadata.Default;
        }

        // DOCUMENTO
        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(Mm(MARGEN));

                page.DefaultTextStyle(style =>
                    style.FontFamily("Helvetica").FontSize(8)
                );

                page.Content()
                    .PaddingTop(8)
                    .Column(column =>
                    {
                        column.Spacing(8);

                        // ENCABEZADO: SOLAMENTE PRIMERA PÁGINA
                        column.Item()
                            .Element(c => HeaderEmpresa.Dibujar(
                                c,
                                _logo,
                                _empresa,
                                "GESTIÓN S.N.C. - SALIDAS NO CONFORMES",
                                "F-GC-001 V2",
                                "10/09/2024",
                                $"OP: {Valor(_registro.OrdenProduccion)}"
                            ));

                        // ORDEN DE PRODUCCIÓN
                        column.Item()
                            .PaddingTop(3)
                            .Text($"Orden de Producción: {Valor(_registro.OrdenProduccion)}")
                            .Bold()
                            .FontSize(10);

                        // PASO 1: REPORTAR
                        DibujarTabla(
                            column,
                            "Paso 1 — Reportar",

                            ("Fecha", _registro.FechaReporte.ToString("g", Cultura)),
                            ("Cliente", _registro.Cliente),
                            ("Referencia", _registro.Referencia),
                            ("Línea", _registro.Linea),
                            ("Material", _registro.Material),
                            ("Proceso", _registro.Proceso),
                            ("Tipo de defecto", _registro.TipoDefecto),
                            ("Impacto", _registro.Impacto),
                            ("Descripción", _registro.DescripcionSalida),
                            ("Causa raíz", _registro.CausaRaiz),

                            ("Cantidad reportada",
                                $"{FormatoDecimal(_registro.CantidadReportadaKg)} {Valor(_registro.UnidadCantidadReportada, "")}".Trim()),

                            ("Reportado por",
                                _registro.NombreReporta ?? _registro.UsuarioReporta)
                        );

                        // FIRMA DE QUIEN REPORTA
                        DibujarFirma(
                            column,
                            _registro.FirmaReporta,
                            "Firma de quien reporta"
                        );

                        // EVIDENCIAS DEL PASO 1
                        DibujarFotos(column, "Reportar");

                        // PASO 2: TRATAMIENTO
                        bool tieneTratamiento =
                            !string.IsNullOrWhiteSpace(_registro.TratamientoAdoptado)
                            || !string.IsNullOrWhiteSpace(_registro.DescripcionTratamiento);

                        if (tieneTratamiento)
                        {
                            DibujarTabla(
                                column,
                                "Paso 2 — Tratamiento",

                                ("Tratamiento adoptado",
                                    _registro.TratamientoAdoptado),

                                ("Descripción",
                                    _registro.DescripcionTratamiento),

                                ("Fecha",
                                    _registro.FechaTratamiento?.ToString("d", Cultura)),

                                ("Registrado por",
                                    _registro.UsuarioTratamiento)
                            );

                            DibujarFirma(
                                column,
                                _registro.FirmaTratamiento,
                                "Firma de quien registra el tratamiento"
                            );

                            DibujarFotos(column, "Tratamiento");
                        }

                        // PASO 3: VERIFICACIÓN / CIERRE
                        if (string.Equals(
                            _registro.Estado,
                            "Cerrado",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            DibujarTabla(
                                column,
                                "Paso 3 — Verificación / Cierre",

                                ("Verificación de cumplimiento",
                                    _registro.VerificacionCumplimiento),

                                ("¿Requirió información al cliente?",
                                    _registro.RequiereInformacionCliente == true
                                        ? "SI"
                                        : "NO"),

                                ("Motivo",
                                    _registro.MotivoInformacionCliente),

                                ("Aceptación bajo concesión",
                                    _registro.AceptacionBajoConcesion == true
                                        ? "SI"
                                        : "NO"),

                                ("Detalle",
                                    _registro.DetalleAceptacionConcesion),

                                ("Revisado por",
                                    _registro.RevisadoPor),

                                ("Fecha de verificación",
                                    _registro.FechaVerificacion?.ToString("d", Cultura))
                            );

                            DibujarFirma(
                                column,
                                _registro.FirmaVerificacion,
                                "Firma de verificación"
                            );

                            DibujarFotos(column, "Verificacion");

                            column.Item()
                                .PaddingTop(10)
                                .AlignCenter()
                                .Text("— FIN DE DOCUMENTO —")
                                .Italic()
                                .FontSize(9);
                        }
                    });

                // FOOTER: TODAS LAS PÁGINAS
                page.Footer()
                    .Element(c => FooterEmpresa.Dibujar(
                        c,
                        "Layas Software - Gestión de Salidas No Conformes"
                    ));
            });
        }

        // TABLAS 
        private void DibujarTabla(
    ColumnDescriptor column,
    string titulo,
    params (string Etiqueta, string? Valor)[] filas)
        {
            column.Item()
                .PaddingTop(3)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(Mm(ANCHO_ETIQUETA));
                        columns.RelativeColumn();
                    });

                    // ENCABEZADO DEL PASO
                    table.Cell()
                        .ColumnSpan(2)
                        .Background("#E1E1E1")
                        .Border(0.5f)
                        .BorderColor(Colors.Black)
                        .PaddingVertical(6)
                        .PaddingHorizontal(7)
                        .Text(titulo)
                        .FontFamily("Helvetica")
                        .FontSize(8)
                        .Bold()
                        .FontColor(Colors.Black);

                    foreach (var fila in filas)
                    {
                        // ETIQUETA
                        table.Cell()
                            .Background("#F2F2F2")
                            .Border(0.5f)
                            .BorderColor(Colors.Black)
                            .PaddingVertical(5)
                            .PaddingHorizontal(7)
                            .AlignMiddle()
                            .Text(fila.Etiqueta)
                            .FontFamily("Helvetica")
                            .FontSize(8)
                            .Bold()
                            .FontColor(Colors.Black);

                        // VALOR
                        table.Cell()
                            .Background(Colors.White)
                            .Border(0.5f)
                            .BorderColor(Colors.Black)
                            .PaddingVertical(5)
                            .PaddingHorizontal(7)
                            .AlignMiddle()
                            .Text(Valor(fila.Valor))
                            .FontFamily("Helvetica")
                            .FontSize(8)
                            .FontColor(Colors.Black);
                    }
                });
        }

        // FIRMAS 
        private void DibujarFirma(
    ColumnDescriptor column,
    string? firma,
    string titulo)
        {
            if (string.IsNullOrWhiteSpace(firma))
                return;

            byte[]? imagen = ObtenerBytesFirma(firma);

            column.Item()
                .PaddingTop(5)
                .Width(Mm(75))
                .Border(0.7f)
                .BorderColor(Colors.Black)
                .Column(firmaColumn =>
                {
                    // TÍTULO
                    firmaColumn.Item()
                        .Background("#E1E1E1")
                        .PaddingVertical(5)
                        .PaddingHorizontal(7)
                        .Text(titulo)
                        .FontFamily("Helvetica")
                        .FontSize(8)
                        .Bold()
                        .FontColor(Colors.Black);

                    // ÁREA DE FIRMA
                    firmaColumn.Item()
                        .Height(Mm(22))
                        .Background(Colors.White)
                        .Padding(3)
                        .AlignCenter()
                        .AlignMiddle()
                        .Element(container =>
                        {
                            if (imagen != null)
                            {
                                container
                                    .Width(Mm(65))
                                    .Height(Mm(18))
                                    .Image(imagen)
                                    .FitArea();
                            }
                            else
                            {
                                container
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .Text(firma)
                                    .FontSize(12)
                                    .Italic()
                                    .FontColor(Colors.Black);
                            }
                        });
                });
        }

        private byte[]? ObtenerBytesFirma(string firma)
        {
            try
            {
                if (!firma.StartsWith(
                    "data:image",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                int separador = firma.IndexOf(',');

                if (separador < 0 || separador == firma.Length - 1)
                    return null;

                string base64 = firma.Substring(separador + 1);

                return Convert.FromBase64String(base64);
            }
            catch
            {
                return null;
            }
        }

        // EVIDENCIAS FOTOGRÁFICAS
        private void DibujarFotos(
            ColumnDescriptor column,
            string paso)
        {
            var fotos = _fotos
                .Where(f =>
                    string.Equals(
                        f.Paso,
                        paso,
                        StringComparison.OrdinalIgnoreCase)
                    && f.Contenido != null
                    && f.Contenido.Length > 0)
                .ToList();

            if (fotos.Count == 0)
                return;

            column.Item()
                .PaddingTop(3)
                .Column(fotosColumn =>
                {
                    fotosColumn.Spacing(4);

                    fotosColumn.Item()
                        .Text($"Evidencia — {paso}")
                        .Bold()
                        .FontSize(8);

                    for (int i = 0; i < fotos.Count; i += 3)
                    {
                        var grupo = fotos
                            .Skip(i)
                            .Take(3)
                            .ToList();

                        fotosColumn.Item()
                            .Row(row =>
                            {
                                foreach (var foto in grupo)
                                {
                                    row.RelativeItem()
                                        .PaddingRight(Mm(ESPACIO_FOTO))
                                        .Height(Mm(ALTO_FOTO))
                                        .Element(container =>
                                            container.Image(foto.Contenido).FitArea()
                                        );
                                }
                            });
                    }
                });
        }

        // UTILIDADES
        private static string Valor(
            string? valor,
            string defecto = "-")
        {
            return string.IsNullOrWhiteSpace(valor)
                ? defecto
                : valor.Trim();
        }

        private static string FormatoDecimal(decimal? valor)
        {
            return valor.HasValue
                ? valor.Value.ToString("N2", Cultura)
                : "-";
        }

        private static float Mm(float valor)
        {
            return valor * 72f / 25.4f;
        }
    }
}
