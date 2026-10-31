using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Services.Pdf.Components;
using SistemaFlota.Services.Pdf.Styles;
using FormatoFGC008Model = SistemaFlota.Models.Calidad.FormatoFGC008.FormatoFGC008;

namespace SistemaFlota.Services.Calidad.FormatoFGC008.FormatosFGC008Pdf;

public class FGC008Documento
{
    private readonly List<FormatoFGC008Model> _registros;
    private readonly ConfiguracionEmpresa _empresa;
    private readonly string _logo;
    private readonly string _marcaAgua;

    private const string Negro = "#000000";
    private const string Gris = "#E1E1E1";
    private const string Blanco = "#FFFFFF";

    public FGC008Documento(
        List<FormatoFGC008Model> registros,
        ConfiguracionEmpresa empresa,
        string logo,
        string marcaAgua)
    {
        _registros = registros;
        _empresa = empresa;
        _logo = logo;
        _marcaAgua = marcaAgua;
    }


    public void Compose(IDocumentContainer container)
    {
        var grupos = _registros
            .GroupBy(r => r.OrdenProduccion)
            .OrderBy(g => g.Key)
            .Select(g => g
                .OrderBy(r => r.Origen == "Bodega"
                    ? r.DiaEntrada ?? r.Fecha
                    : r.Fecha)
                .ToList())
            .ToList();

        container.Page(page =>
        {
            page.Size(PageSizes.Letter);
            page.Margin(10, Unit.Millimetre);
            page.DefaultTextStyle(x => x.FontSize(7));

            page.Background()
                .Element(c => MarcaAgua.Dibujar(c, _marcaAgua));

            // ÚNICO CONTENIDO DE LA PÁGINA
            page.Content()
                .PaddingTop(8)
                .Column(column =>
                {
                    // Encabezado corporativo
                    column.Item()
                        .Element(c => HeaderEmpresa.Dibujar(
                            c,
                            _logo,
                            _empresa,
                            "CONTROL BODEGA PRODUCTO TERMINADO",
                            "F-GC-008",
                            DateTime.Now.ToString("dd/MM/yyyy"),
                            $"{_registros.Count} REGISTROS"
                        ));

                    column.Item().PaddingTop(8);

                    // Formato oficial
                    ConstruirFormatoOficial(column, grupos);

                    // comienza en una página nueva mediante PageBreak()
                    ConstruirAnexo(column, grupos);
                });

            page.Footer()
                .Element(c => FooterEmpresa.Dibujar(
                    c,
                    "Layas Software - Control Bodega Producto Terminado"
                ));
        });
    }


    // FORMATO OFICIAL

    private void ConstruirFormatoOficial(
        ColumnDescriptor column,
        List<List<FormatoFGC008Model>> grupos)
    {
        column.Item()
            .PaddingBottom(5)
            .Text($"FECHA: {DateTime.Now:dd/MM/yyyy}")
            .Bold()
            .FontSize(8);

        column.Item()
            .Background(Gris)
            .Padding(5)
            .AlignCenter()
            .Text("REGISTRO VERIFICACION DE EMPAQUE")
            .Bold()
            .FontSize(8);

        column.Item()
            .PaddingVertical(5)
            .Text(
                "Nota: Las verificaciones consisten en comprobar la información " +
                "del empaque del producto final con los criterios de calidad y " +
                "la información especificada en la orden de producción."
            )
            .Italic()
            .FontSize(6);

        column.Item().Element(ConstruirTablaOficial);

        foreach (var filas in grupos)
        {
            ConstruirGrupoOficial(column, filas);
        }

        ConstruirAccionesYFirma(column);
    }

    private void ConstruirTablaOficial(IContainer container)
    {
        container.Table(table =>
        {
            DefinirColumnasOficiales(table);

            table.Header(header =>
            {
                string[] titulos =
                {
                    "ORDEN DE PRODUCCIÓN",
                    "FECHA ENTRADA",
                    "ETIQUETAS SI",
                    "ETIQUETAS NO",
                    "EMBALAJE SI",
                    "EMBALAJE NO",
                    "DEFECTOS SI",
                    "DEFECTOS NO",
                    "CANT. OP",
                    "CANT. REAL",
                    "CONCUERDA",
                    "LISTO SI",
                    "LISTO NO",
                    "DESPACHADO"
                };

                foreach (var titulo in titulos)
                {
                    header.Cell()
                        .Element(CeldaEncabezado)
                        .Text(titulo);
                }
            });
        });
    }

    private void DefinirColumnasOficiales(TableDescriptor table)
    {
        table.ColumnsDefinition(columns =>
        {
            columns.RelativeColumn(27);
            columns.RelativeColumn(21);

            for (int i = 0; i < 6; i++)
                columns.RelativeColumn(9);

            columns.RelativeColumn(18);
            columns.RelativeColumn(16);
            columns.RelativeColumn(14);
            columns.RelativeColumn(9);
            columns.RelativeColumn(9);
            columns.RelativeColumn(27);
        });
    }

    private void ConstruirGrupoOficial(
        ColumnDescriptor column,
        List<FormatoFGC008Model> filas)
    {
        if (filas.Count == 0)
            return;

        var bodega = filas.Where(r => r.Origen == "Bodega").ToList();
        var baseRegistro = bodega.FirstOrDefault() ?? filas[0];

        decimal cantidadOP = baseRegistro.CantidadOP;

        string unidad = bodega.Count > 0
            ? ObtenerUnidad(bodega[0])
            : "";

        int decimales = unidad == "kg" ? 2 : 0;

        decimal totalReal = bodega.Count > 0
            ? bodega.Max(r => r.AcumuladoOP ?? r.CantidadReal)
            : filas.Sum(r => r.CantidadReal);

        column.Item().Table(table =>
        {
            DefinirColumnasOficiales(table);

            foreach (var r in filas)
            {
                decimal comparado = r.Origen == "Bodega"
                    ? r.AcumuladoOP ?? r.CantidadReal
                    : r.CantidadReal;

                bool concuerda = comparado >= cantidadOP;

                Celda(table, r.OrdenProduccion, true);
                Celda(table, FechaDia(r));
                Celda(table, Marca(r.EtiquetasSI));
                Celda(table, Marca(!r.EtiquetasSI));
                Celda(table, Marca(r.EmbalajeSI));
                Celda(table, Marca(!r.EmbalajeSI));
                Celda(table, Marca(r.DefectosSI));
                Celda(table, Marca(!r.DefectosSI));
                Celda(table, Numero(cantidadOP));
                Celda(table, Numero(r.CantidadReal, decimales) + " " + ObtenerUnidad(r));
                Celda(table, concuerda ? "SI" : "NO", true,
                    concuerda ? "#198754" : "#D32F2F");
                Celda(table, Marca(r.ListoBodega));
                Celda(table, Marca(!r.ListoBodega));
                Celda(table, r.Despachado ?? "-");
            }

            var diferencia = totalReal - cantidadOP;
            var porcentaje = cantidadOP > 0
                ? diferencia / cantidadOP * 100
                : 0;

            table.Cell()
                .ColumnSpan(9)
                .Element(CeldaTotal)
                .Text($"TOTAL {baseRegistro.OrdenProduccion} - {filas.Count} registros")
                .Bold();

            table.Cell()
                .Element(CeldaTotal)
                .Text(Numero(totalReal, decimales))
                .Bold();

            table.Cell()
                .ColumnSpan(4)
                .Element(CeldaTotal)
                .Text(
                    $"Diferencia: {(diferencia >= 0 ? "+" : "")}" +
                    $"{Numero(diferencia, decimales)} {unidad} " +
                    $"({Numero(porcentaje, 1)}%)"
                )
                .Bold();
        });

        column.Item().PaddingBottom(5);
    }

    // ANEXO

    private void ConstruirAnexo(
        ColumnDescriptor column,
        List<List<FormatoFGC008Model>> grupos)
    {
        column.Item()
            .PageBreak();

        column.Item()
            .Background(Gris)
            .Padding(6)
            .AlignCenter()
            .Text("ANEXO - DETALLE POR DÍA Y EVIDENCIA FOTOGRÁFICA")
            .Bold()
            .FontSize(9);

        foreach (var filas in grupos)
        {
            ConstruirDetalleOP(column, filas);
        }
    }

    private void ConstruirDetalleOP(
        ColumnDescriptor column,
        List<FormatoFGC008Model> filas)
    {
        if (filas.Count == 0)
            return;

        var baseRegistro = filas.FirstOrDefault(r => r.Origen == "Bodega")
            ?? filas[0];

        column.Item()
            .PaddingTop(8)
            .Background(Gris)
            .Padding(5)
            .Text(
                $"OP {baseRegistro.OrdenProduccion} | " +
                $"{baseRegistro.Cliente ?? "-"} | " +
                $"{baseRegistro.Referencia ?? "-"}"
            )
            .Bold();

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1.1f);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(0.6f);
                columns.RelativeColumn(0.7f);
                columns.RelativeColumn(0.8f);
                columns.RelativeColumn(0.8f);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1.3f);
                columns.RelativeColumn(1.6f);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                string[] titulos =
                {
                    "Día", "Sección", "Operarios", "Bultos",
                    "Troq.", "Und", "Kg", "Acumulado",
                    "Falta / Dif.", "Revisión", "Despachado"
                };

                foreach (var titulo in titulos)
                {
                    header.Cell()
                        .Element(CeldaEncabezado)
                        .Text(titulo);
                }
            });

            foreach (var r in filas)
            {
                bool esBodega = r.Origen == "Bodega";
                string unidad = ObtenerUnidad(r);
                int decimales = unidad == "kg" ? 2 : 0;

                Celda(table, FechaDia(r));
                Celda(table, NombreSeccion(r));
                Celda(table, esBodega ? r.OperariosBodega ?? "-" : "-");
                Celda(table, esBodega ? r.Bultos.ToString() : "-");
                Celda(table, esBodega ? r.Troquel ?? "-" : "-");
                Celda(table, esBodega ? Numero(r.UnidadesEntrada) : "-");
                Celda(table, esBodega ? Numero(r.KilosEntrada, 2) : "-");

                string acumulado = esBodega
                    ? Numero(r.AcumuladoOP ?? r.CantidadReal, decimales) + " " + unidad
                    : Numero(r.CantidadReal, decimales);

                Celda(table, acumulado);
                Celda(table, ObtenerDiferencia(r));
                Celda(table, ObtenerEstadoRevision(r));
                Celda(table, r.Despachado ?? "-");
            }
        });

        var acciones = filas
            .Where(r => !string.IsNullOrWhiteSpace(r.AccionesTomadas))
            .Select(r => $"{FechaDia(r)}: {r.AccionesTomadas}")
            .ToList();

        if (acciones.Count > 0)
        {
            column.Item()
                .PaddingTop(4)
                .Text("ACCIONES TOMADAS")
                .Bold();

            foreach (var accion in acciones)
            {
                column.Item()
                    .Text(accion)
                    .FontSize(7);
            }
        }

        ConstruirFotos(column, filas);
    }

    // ACCIONES, FIRMA Y FOTOS

    private void ConstruirAccionesYFirma(ColumnDescriptor column)
    {
        column.Item()
            .PaddingTop(8)
            .Border(1)
            .BorderColor(Negro)
            .Padding(6)
            .Column(c =>
            {
                c.Item()
                    .Text("ACCIONES TOMADAS PARA LAS OP QUE INCUMPLIERON")
                    .Bold();

                foreach (var r in _registros.Where(r =>
                    !string.IsNullOrWhiteSpace(r.AccionesTomadas)))
                {
                    c.Item().Text(
                        $"{r.OrdenProduccion} ({FechaDia(r)}): {r.AccionesTomadas}"
                    );
                }
            });

        var ultimo = _registros.FirstOrDefault(r =>
            !string.IsNullOrWhiteSpace(r.FirmaDigital))
            ?? _registros.FirstOrDefault();

        column.Item()
            .PaddingTop(8)
            .Row(row =>
            {
                row.RelativeItem(3)
                    .Border(1)
                    .BorderColor(Negro)
                    .Padding(6)
                    .Column(c =>
                    {
                        c.Item()
                            .Text("Firma de quien hace la entrada a Bodega")
                            .Bold();

                        if (ultimo != null)
                            AgregarFirma(c, ultimo.FirmaDigital);

                        c.Item().Text(
                            $"Realizado por: {ultimo?.RevisadoPor ?? "-"}"
                        );
                    });

                row.RelativeItem(2)
                    .Border(1)
                    .BorderColor(Negro)
                    .Padding(6)
                    .Column(c =>
                    {
                        c.Item().Text("Cargo").Bold();
                        c.Item().Text("Bodega");
                    });
            });

        column.Item()
            .PaddingTop(6)
            .Text(
                "Nota: Los métodos de verificación de las características y " +
                "los criterios de cumplimiento a tener en cuenta están definidos " +
                "en el Procedimiento de Pruebas y Ensayos."
            )
            .Italic()
            .FontSize(6);
    }

    private void ConstruirFotos(
        ColumnDescriptor column,
        List<FormatoFGC008Model> filas)
    {
        var fotos = filas
            .SelectMany(r => ObtenerFotos(r)
                .Select(f => new
                {
                    Archivo = f,
                    Fecha = FechaDia(r),
                    Seccion = NombreSeccion(r)
                }))
            .ToList();

        if (fotos.Count == 0)
            return;

        column.Item()
            .PaddingTop(6)
            .Text($"FOTOS DE EVIDENCIA ({fotos.Count})")
            .Bold();

        column.Item().Grid(grid =>
        {
            grid.Columns(4);
            grid.Spacing(5);

            foreach (var foto in fotos)
            {
                grid.Item()
                    .Column(c =>
                    {
                        var ruta = ObtenerRutaFoto(foto.Archivo);

                        if (File.Exists(ruta))
                        {
                            c.Item()
                                .Height(85)
                                .Border(1)
                                .BorderColor(PdfColors.GrisClaro)
                                .Padding(2)
                                .Image(File.ReadAllBytes(ruta))
                                .FitArea();
                        }
                        else
                        {
                            c.Item()
                                .Height(85)
                                .Border(1)
                                .BorderColor(PdfColors.GrisClaro)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text("No disponible")
                                .FontSize(6);
                        }

                        c.Item()
                            .Text($"{foto.Fecha} - {foto.Seccion}")
                            .FontSize(6);
                    });
            }
        });
    }

    private void AgregarFirma(
        ColumnDescriptor column,
        string? firma)
    {
        if (string.IsNullOrWhiteSpace(firma))
            return;

        try
        {
            var base64 = firma.Contains(',')
                ? firma.Split(',')[1]
                : firma;

            var bytes = Convert.FromBase64String(base64);

            column.Item()
                .Height(35)
                .Image(bytes)
                .FitArea();
        }
        catch
        {
            column.Item().Text("Firma no disponible").FontSize(6);
        }
    }

    // CELDAS Y UTILIDADES

    private static IContainer CeldaEncabezado(IContainer container)
    {
        return container
            .Background(Gris)
            .Border(0.5f)
            .BorderColor(Negro)
            .Padding(2)
            .AlignCenter()
            .AlignMiddle();
    }

    private static IContainer CeldaTotal(IContainer container)
    {
        return container
            .Background(Gris)
            .Border(0.5f)
            .BorderColor(Negro)
            .Padding(3)
            .AlignMiddle();
    }

    private static void Celda(
    TableDescriptor table,
    string? valor,
    bool negrita = false,
    string? color = null)
    {
        var texto = table.Cell()
            .Border(0.5f)
            .BorderColor(Negro)
            .Padding(2)
            .AlignMiddle()
            .Text(valor ?? "-")
            .FontSize(6)
            .FontColor(color ?? Negro);

        if (negrita)
            texto.Bold();
    }

    private static string Marca(bool valor) => valor ? "✓" : "";

    private static string Numero(decimal valor, int decimales = 0)
    {
        return valor.ToString(
            decimales == 0 ? "N0" : $"N{decimales}",
            System.Globalization.CultureInfo.GetCultureInfo("es-CO")
        );
    }

    private static string FechaDia(FormatoFGC008Model r)
    {
        var fecha = r.Origen == "Bodega"
            ? r.DiaEntrada ?? r.Fecha
            : r.Fecha;

        return fecha.ToString("dd/MM/yyyy");
    }

    private static string ObtenerUnidad(FormatoFGC008Model r)
    {
        if (r.Origen != "Bodega")
            return "";

        return string.Equals(
            r.UnidadReal,
            "UN",
            StringComparison.OrdinalIgnoreCase)
            ? "und"
            : "kg";
    }

    private static string NombreSeccion(FormatoFGC008Model r)
    {
        return r.Origen switch
        {
            "Bodega" => r.Seccion ?? "-",
            "Anterior" => "Registro anterior",
            _ => "Manual"
        };
    }

    private static string ObtenerEstadoRevision(FormatoFGC008Model r)
    {
        if (r.EnProceso)
            return "En proceso";

        if (r.CambioDespuesDeRevisar)
            return "Cambió después de revisar";

        if (!string.IsNullOrWhiteSpace(r.RevisadoPor) &&
            !r.RevisadoPor.StartsWith("Sistema", StringComparison.OrdinalIgnoreCase))
        {
            return "Revisada";
        }

        return "Sin revisar";
    }

    private static string ObtenerDiferencia(FormatoFGC008Model r)
    {
        if (r.Origen == "Bodega")
        {
            if (r.PorHacer > 0)
            {
                return $"Faltan {Numero(r.PorHacer.Value)}";
            }

            if (r.DiferenciaPct.HasValue)
            {
                return $"{Numero(r.DiferenciaPct.Value, 1)}%";
            }

            return "-";
        }

        if (r.CantidadOP <= 0)
            return "-";

        var porcentaje =
            ((r.CantidadReal - r.CantidadOP) / r.CantidadOP) * 100;

        return $"{Numero(porcentaje, 1)}%";
    }

    private static IEnumerable<string> ObtenerFotos(FormatoFGC008Model r)
    {
        if (!string.IsNullOrWhiteSpace(r.FotoEvidencia))
            yield return r.FotoEvidencia;

        foreach (var foto in r.FotosEvidencia ?? [])
        {
            if (!string.IsNullOrWhiteSpace(foto.Archivo))
                yield return foto.Archivo;
        }
    }

    private string ObtenerRutaFoto(string archivo)
    {
        var nombre = Path.GetFileName(archivo);

        var wwwroot = Directory.GetParent(
            Path.GetDirectoryName(_marcaAgua) ?? ""
        )?.FullName;

        if (string.IsNullOrWhiteSpace(wwwroot))
            return "";

        return Path.Combine(wwwroot, "formatos", nombre);
    }
}