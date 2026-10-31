
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using SistemaFlota.Models;
using SistemaFlota.Models.Calidad;
using SistemaFlota.Services.Calidad.Formatos.FormatosGenericos.SistemaFlota.Services.Calidad.Formatos.FormatosGenericos;
using SistemaFlota.Services.Pdf.Components;
using SistemaFlota.Services.Pdf.Styles;

using System.Globalization;
using System.Text.Json;

namespace SistemaFlota.Services.Calidad.Formatos.FormatosGenericos
{
    public class FormatoCalidadPdfService : IFormatoCalidadPdfService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        private static readonly CultureInfo Cultura =
            CultureInfo.GetCultureInfo("es-CO");

        public FormatoCalidadPdfService(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // MÉTODO PRINCIPAL

        public async Task<byte[]> GenerarPdfAsync(
            string codigo,
            DateTime? desde,
            DateTime? hasta,
            string? op)
        {
            var empresa = await _context.ConfiguracionEmpresa
                .FirstOrDefaultAsync();

            if (empresa == null)
                throw new InvalidOperationException(
                    "No existe la configuración de la empresa.");

            var tipo = await _context.TiposFormatoCalidad
                .FirstOrDefaultAsync(x => x.Codigo == codigo);

            if (tipo == null)
                throw new InvalidOperationException(
                    $"No existe el formato de calidad {codigo}.");

            var consulta = _context.RegistrosFormatoCalidad
                .AsNoTracking()
                .Where(x => x.TipoFormatoId == tipo.Id);

            if (desde.HasValue)
            {
                consulta = consulta.Where(x =>
                    x.Fecha >= desde.Value);
            }

            if (hasta.HasValue)
            {
                var fechaLimite = hasta.Value.Date.AddDays(1);

                consulta = consulta.Where(x =>
                    x.Fecha < fechaLimite);
            }

            if (!string.IsNullOrWhiteSpace(op))
            {
                consulta = consulta.Where(x =>
                    x.OrdenProduccion.Contains(op));
            }

            var registros = await consulta
                .OrderByDescending(x => x.Fecha)
                .ToListAsync();

            if (registros.Count == 0)
                throw new InvalidOperationException(
                    "No existen registros para generar el PDF.");

            var logo = ObtenerLogo(empresa.Logo);
            var marcaAgua = ObtenerMarcaAgua();

            var documento = new FormatoCalidadDocumento(
                empresa,
                logo,
                marcaAgua,
                tipo.Nombre,
                tipo.Codigo,
                codigo,
                registros);

            return Document.Create(documento.Compose)
                .GeneratePdf();
        }

        // RUTAS DE ARCHIVOS

        private string ObtenerLogo(string? logo)
        {
            var carpeta = Path.Combine(
                _environment.WebRootPath ??
                Path.Combine(_environment.ContentRootPath, "wwwroot"),
                "config");

            var ruta = Path.Combine(carpeta, "logo.png");

            if (!string.IsNullOrWhiteSpace(logo))
            {
                var nombre = Path.GetFileName(logo);
                var rutaPersonalizada = Path.Combine(carpeta, nombre);

                if (File.Exists(rutaPersonalizada))
                    return rutaPersonalizada;
            }

            return ruta;
        }

        private string ObtenerMarcaAgua()
        {
            return Path.Combine(
                _environment.WebRootPath ??
                Path.Combine(_environment.ContentRootPath, "wwwroot"),
                "config",
                "iguana3.png");
        }

        // LECTURA DE JSON

        private static JsonElement LeerJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            try
            {
                using var documento = JsonDocument.Parse(json);
                return documento.RootElement.Clone();
            }
            catch
            {
                return default;
            }
        }

        private static List<CaracteristicaPdf> LeerCaracteristicas(
            JsonElement datos)
        {
            var resultado = new List<CaracteristicaPdf>();

            if (datos.ValueKind != JsonValueKind.Object ||
                !datos.TryGetProperty("caracteristicas", out var lista) ||
                lista.ValueKind != JsonValueKind.Array)
                return resultado;

            foreach (var item in lista.EnumerateArray())
            {
                resultado.Add(new CaracteristicaPdf
                {
                    Id = Texto(item, "id"),
                    Descripcion = Texto(item, "descripcion")
                });
            }

            return resultado;
        }

        private static Dictionary<string, string> LeerObservaciones(
            JsonElement datos)
        {
            var resultado = new Dictionary<string, string>();

            if (datos.ValueKind != JsonValueKind.Object ||
                !datos.TryGetProperty("observaciones", out var observaciones) ||
                observaciones.ValueKind != JsonValueKind.Object)
                return resultado;

            foreach (var propiedad in observaciones.EnumerateObject())
            {
                resultado[propiedad.Name] = Valor(propiedad.Value);
            }

            return resultado;
        }

        private static List<ParametroPdf> LeerParametros(string? json)
        {
            var resultado = new List<ParametroPdf>();

            if (string.IsNullOrWhiteSpace(json))
                return resultado;

            try
            {
                using var documento = JsonDocument.Parse(json);

                if (documento.RootElement.ValueKind != JsonValueKind.Object)
                    return resultado;

                RecorrerParametros(
                    documento.RootElement,
                    resultado,
                    "");

                return resultado;
            }
            catch
            {
                return resultado;
            }
        }

        private static void RecorrerParametros(
            JsonElement elemento,
            List<ParametroPdf> resultado,
            string prefijo)
        {
            if (elemento.ValueKind != JsonValueKind.Object)
                return;

            foreach (var propiedad in elemento.EnumerateObject())
            {
                if (propiedad.Name.Equals(
                    "motivoCambio",
                    StringComparison.OrdinalIgnoreCase))
                    continue;

                var nombre = EtiquetaParametro(propiedad.Name);

                var etiqueta = string.IsNullOrEmpty(prefijo)
                    ? nombre
                    : $"{prefijo} - {nombre}";

                if (propiedad.Value.ValueKind == JsonValueKind.Object)
                {
                    RecorrerParametros(
                        propiedad.Value,
                        resultado,
                        etiqueta);

                    continue;
                }

                if (propiedad.Value.ValueKind == JsonValueKind.Array)
                    continue;

                var valor = Valor(propiedad.Value);

                if (string.IsNullOrWhiteSpace(valor))
                    continue;

                resultado.Add(new ParametroPdf
                {
                    Etiqueta = etiqueta,
                    Valor = valor
                });
            }
        }

        private static string EtiquetaParametro(string nombre)
        {
            var etiquetas = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["corona"] = "Corona",
                ["molde"] = "Molde",
                ["aire"] = "Aire (H)",
                ["amperaje"] = "Amperaje (A)",
                ["alturaBurbuja"] = "Altura burbuja (cm)",
                ["vela"] = "Vela",
                ["maquina"] = "Máquina",
                ["maquina2"] = "Máquina 2",
                ["halador"] = "Halador",
                ["bobinador"] = "Bobinador",
                ["zona1"] = "Zona 1",
                ["zona2"] = "Zona 2",
                ["zona3"] = "Zona 3",
                ["zona4"] = "Zona 4",
                ["zona5"] = "Zona 5",
                ["zona6"] = "Zona 6",
                ["maquinaImpresion"] = "Máquina impresión",
                ["maquinaSellado"] = "Máquina sellado",
                ["maquinaPrecorte"] = "Máquina precorte",
                ["metrosPorMinuto"] = "Metros por minuto",
                ["velocidadMaquinaHz"] = "Velocidad máquina (Hz)",
                ["golpesPorMinuto"] = "Golpes por minuto",
                ["bolsasPorMinuto"] = "Bolsas por minuto",
                ["temperaturaSuperior"] = "Temperatura superior (°C)",
                ["temperaturaInferior"] = "Temperatura inferior (°C)",
                ["temperaturaPrecorte"] = "Temperatura precorte (°C)"
            };

            if (etiquetas.TryGetValue(nombre, out var etiqueta))
                return etiqueta;

            return System.Text.RegularExpressions.Regex
                .Replace(nombre, "([a-z])([A-Z])", "$1 $2");
        }

        private static string Texto(
            JsonElement elemento,
            string propiedad)
        {
            if (elemento.ValueKind != JsonValueKind.Object ||
                !elemento.TryGetProperty(propiedad, out var valor))
                return "";

            return Valor(valor);
        }

        private static string Valor(JsonElement elemento)
        {
            return elemento.ValueKind switch
            {
                JsonValueKind.String => elemento.GetString() ?? "",
                JsonValueKind.Number => elemento.ToString(),
                JsonValueKind.True => "Sí",
                JsonValueKind.False => "No",
                JsonValueKind.Null => "",
                JsonValueKind.Undefined => "",
                _ => elemento.ToString()
            };
        }

        private static string Fecha(DateTime? fecha)
        {
            return fecha.HasValue
                ? fecha.Value.ToString("dd/MM/yyyy HH:mm", Cultura)
                : "-";
        }

        private static string Mostrar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? "-"
                : valor;
        }

        // MODELOS AUXILIARES DEL PDF

        private class ParametroPdf
        {
            public string Etiqueta { get; set; } = "";
            public string Valor { get; set; } = "";
        }

        private class CaracteristicaPdf
        {
            public string Id { get; set; } = "";
            public string Descripcion { get; set; } = "";
        }

        // DOCUMENTO QUESTPDF

        private class FormatoCalidadDocumento
        {
            private readonly ConfiguracionEmpresa _empresa;
            private readonly string _logo;
            private readonly string _marcaAgua;
            private readonly string _nombreFormato;
            private readonly string _codigoFormato;
            private readonly string _numeroDocumento;
            private readonly List<RegistroFormatoCalidad> _registros;

            public FormatoCalidadDocumento(
                ConfiguracionEmpresa empresa,
                string logo,
                string marcaAgua,
                string nombreFormato,
                string codigoFormato,
                string numeroDocumento,
                List<RegistroFormatoCalidad> registros)
            {
                _empresa = empresa;
                _logo = logo;
                _marcaAgua = marcaAgua;
                _nombreFormato = nombreFormato;
                _codigoFormato = codigoFormato;
                _numeroDocumento = numeroDocumento;
                _registros = registros;
            }

            public void Compose(IDocumentContainer container)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(25);

                    page.DefaultTextStyle(x =>
                        x.FontSize(8)
                            .FontColor(PdfColors.AzulOscuro));

                    page.Content().Column(columna =>
                    {
                        columna.Spacing(8);

                        columna.Item().Element(c =>
                            HeaderEmpresa.Dibujar(
                                c,
                                _logo,
                                _empresa,
                                $"CONTROL Y LIBERACIÓN DE PRODUCTO - {_nombreFormato}",
                                $"{_codigoFormato} V2",
                                DateTime.Now.ToString("dd/MM/yyyy"),
                                _numeroDocumento
                            ));

                        ComponerRegistrosGenerales(columna);
                    });

                    page.Footer().Element(c =>
                        FooterEmpresa.Dibujar(
                            c,
                            "SistemaFlota - Formatos de Calidad"));
                });
            }

            private static void Seccion(
                ColumnDescriptor columna,
                string titulo,
                Action<ColumnDescriptor> contenido)
            {
                columna.Item().Column(col =>
                {
                    col.Item()
                        .Background(PdfColors.GrisClaro)
                        .Border(0.5f)
                        .BorderColor(PdfColors.GrisMedio)
                        .Padding(3)
                        .Text(titulo)
                        .Bold()
                        .FontSize(9);

                    col.Item()
                        .Border(0.5f)
                        .BorderColor(PdfColors.GrisMedio)
                        .Padding(4)
                        .Column(contenido);
                });
            }

            // TABLA DE DATOS GENERALES

            private static void TablaDatos(
                ColumnDescriptor contenedor,
                (string Clave, string Valor)[] datos)
            {
                contenedor.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                    });

                    for (int i = 0; i < datos.Length; i += 2)
                    {
                        var d1 = datos[i];

                        table.Cell()
                            .Border(0.5f)
                            .BorderColor(PdfColors.GrisClaro)
                            .Padding(3)
                            .Text(d1.Clave)
                            .Bold();

                        table.Cell()
                            .Border(0.5f)
                            .BorderColor(PdfColors.GrisClaro)
                            .Padding(3)
                            .Text(d1.Valor);

                        if (i + 1 < datos.Length)
                        {
                            var d2 = datos[i + 1];

                            table.Cell()
                                .Border(0.5f)
                                .BorderColor(PdfColors.GrisClaro)
                                .Padding(3)
                                .Text(d2.Clave)
                                .Bold();

                            table.Cell()
                                .Border(0.5f)
                                .BorderColor(PdfColors.GrisClaro)
                                .Padding(3)
                                .Text(d2.Valor);
                        }
                        else
                        {
                            table.Cell()
                                .ColumnSpan(2)
                                .Border(0.5f)
                                .BorderColor(PdfColors.GrisClaro)
                                .Padding(3)
                                .Text("");
                        }
                    }
                });
            }

            // TABLA DINÁMICA DE PARÁMETROS

            private static void TablaParametros(ColumnDescriptor contenedor, List<ParametroPdf> parametros)
            {
                // Función de búsqueda flexible para encontrar el valor sin importar el prefijo del JSON
                string GetVal(params string[] claves)
                {
                    foreach (var clave in claves)
                    {
                        var match = parametros.FirstOrDefault(p =>
                            p.Etiqueta.Equals(clave, StringComparison.OrdinalIgnoreCase) ||
                            p.Etiqueta.EndsWith($" - {clave}", StringComparison.OrdinalIgnoreCase) ||
                            p.Etiqueta.Contains(clave, StringComparison.OrdinalIgnoreCase)
                        );

                        if (match != null && !string.IsNullOrWhiteSpace(match.Valor) && match.Valor != "-")
                            return match.Valor;
                    }
                    return "-";
                }

                contenedor.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // Columna Izquierda: Etiqueta
                        columns.RelativeColumn(1); // Columna Izquierda: Valor
                        columns.RelativeColumn(2); // Columna Derecha: Etiqueta
                        columns.RelativeColumn(1); // Columna Derecha: Valor
                    });

                    // 1. Fila superior: Corona y Molde
                    table.Cell().Border(0.5f).BorderColor(PdfColors.GrisClaro).Padding(3).Text("Corona").Bold();
                    table.Cell().Border(0.5f).BorderColor(PdfColors.GrisClaro).Padding(3).Text(GetVal("Corona"));
                    table.Cell().Border(0.5f).BorderColor(PdfColors.GrisClaro).Padding(3).Text("Molde").Bold();
                    table.Cell().Border(0.5f).BorderColor(PdfColors.GrisClaro).Padding(3).Text(GetVal("Molde", "molde"));

                    // 2. Cabeceras principales divididas en dos columnas
                    table.Cell().Background(PdfColors.GrisClaro).Border(0.5f).BorderColor(PdfColors.GrisMedio).Padding(3).Text("Temperaturas Máquina (°C)").Bold();
                    table.Cell().Background(PdfColors.GrisClaro).Border(0.5f).BorderColor(PdfColors.GrisMedio).Padding(3).Text("");
                    table.Cell().Background(PdfColors.GrisClaro).Border(0.5f).BorderColor(PdfColors.GrisMedio).Padding(3).Text("Velocidades Máquina (Hz)").Bold();
                    table.Cell().Background(PdfColors.GrisClaro).Border(0.5f).BorderColor(PdfColors.GrisMedio).Padding(3).Text("");

                    // Zonas de temperatura y velocidades en paralelo
                    string[] zonas = { "Zona 1", "Zona 2", "Zona 3", "Zona 4", "Zona 5", "Zona 6" };
                    string[] velocidades = { "Máquina", "Halador", "Bobinador", "Máquina 2", "", "" };

                    for (int i = 0; i < 6; i++)
                    {
                        // Izquierda: Temperatura Zona i
                        table.Cell().Border(0.5f).BorderColor(PdfColors.GrisClaro).Padding(3).Text(zonas[i]);
                        table.Cell().Border(0.5f).BorderColor(PdfColors.GrisClaro).Padding(3).Text(GetVal(zonas[i]));

                        // Derecha: Velocidad correspondiente
                        table.Cell().Border(0.5f).BorderColor(PdfColors.GrisClaro).Padding(3).Text(velocidades[i]);
                        table.Cell().Border(0.5f).BorderColor(PdfColors.GrisClaro).Padding(3).Text(string.IsNullOrEmpty(velocidades[i]) ? "" : GetVal(velocidades[i]));
                    }

                    // 3. Fila inferior: Aire, Amperaje, Altura de burbuja y Vela
                    table.Cell().Border(0.5f).BorderColor(PdfColors.GrisClaro).Padding(3).Text("Aire (H)").Bold();
                    table.Cell().Border(0.5f).BorderColor(PdfColors.GrisClaro).Padding(3).Text(GetVal("Aire (H)", "aire"));
                    table.Cell().Border(0.5f).BorderColor(PdfColors.GrisClaro).Padding(3).Text("Amperaje (A)").Bold();
                    table.Cell().Border(0.5f).BorderColor(PdfColors.GrisClaro).Padding(3).Text(GetVal("Amperaje (A)", "amperaje"));

                    table.Cell().Border(0.5f).BorderColor(PdfColors.GrisClaro).Padding(3).Text("Altura burbuja (cm)").Bold();
                    table.Cell().Border(0.5f).BorderColor(PdfColors.GrisClaro).Padding(3).Text(GetVal("Altura burbuja (cm)", "alturaBurbuja"));
                    table.Cell().Border(0.5f).BorderColor(PdfColors.GrisClaro).Padding(3).Text("Vela").Bold();
                    table.Cell().Border(0.5f).BorderColor(PdfColors.GrisClaro).Padding(3).Text(GetVal("Vela", "vela"));
                });
            }

            // TABLA GENÉRICA

            private static void Tabla(
                ColumnDescriptor contenedor,
                string[] cabeceras,
                List<string[]> filas)
            {
                contenedor.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        foreach (var _ in cabeceras)
                            columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        foreach (var titulo in cabeceras)
                        {
                            header.Cell()
                                .Background(PdfColors.GrisClaro)
                                .Border(0.5f)
                                .BorderColor(PdfColors.GrisMedio)
                                .Padding(3)
                                .Text(titulo)
                                .Bold();
                        }
                    });

                    foreach (var fila in filas)
                    {
                        foreach (var celda in fila)
                        {
                            table.Cell()
                                .Border(0.5f)
                                .BorderColor(PdfColors.GrisClaro)
                                .Padding(3)
                                .Text(celda);
                        }
                    }
                });
            }

            // REGISTROS GENERALES

            private void ComponerRegistrosGenerales(
    ColumnDescriptor columna)
            {
                columna.Item()
                    .Border(1)
                    .BorderColor(PdfColors.GrisClaro)
                    .Padding(7)
                    .Column(nota =>
                    {
                        nota.Spacing(4);

                        nota.Item().Text(
                            "Nota: Las verificaciones consisten en comparar " +
                            "los datos obtenidos con los instrumentos de " +
                            "medición e inspecciones, teniendo en cuenta " +
                            "los criterios de calidad y las especificaciones " +
                            "de la orden de producción.");

                        nota.Item().Text(
                            "Nota: Las verificaciones se registran de acuerdo " +
                            "con los controles establecidos para cada formato.");
                    });

                foreach (var registro in _registros)
                {
                    columna.Item()
                        .PaddingTop(10)
                        .Column(contenido =>
                        {
                            contenido.Spacing(5);

                            contenido.Item().Text(
                                $"ORDEN DE PRODUCCIÓN: " +
                                $"{Mostrar(registro.OrdenProduccion)}")
                                .FontSize(10)
                                .Bold();

                            Seccion(
                                contenido,
                                "DATOS GENERALES",
                                c =>
                                {
                                    TablaDatos(c, new[]
                                    {
                            ("Orden de producción",
                                Mostrar(registro.OrdenProduccion)),

                            ("Fecha",
                                Fecha(registro.Fecha)),

                            ("Cliente",
                                Mostrar(registro.Cliente)),

                            ("Referencia",
                                Mostrar(registro.Referencia)),

                            ("Operario(s)",
                                Mostrar(registro.Operarios)),

                            ("Máquina",
                                Mostrar(registro.Maquina)),

                            ("Hora",
                                Mostrar(registro.Hora)),

                            ("Estado",
                                registro.Estado == "Liberado"
                                    ? "Liberado"
                                    : "Pendiente de liberación")
                                    });
                                });

                            // Variables críticas
                            var parametros = LeerParametros(
                                registro.VariablesCriticasJson);

                            if (parametros.Count > 0)
                            {
                                Seccion(
                                    contenido,
                                    "REGISTRO DE VARIABLES CRÍTICAS",
                                    c =>
                                    {
                                        TablaParametros(c, parametros);
                                    });
                            }

                            // Resultados y características
                            var datos = LeerJson(registro.ResultadosJson);

                            var caracteristicas =
                                LeerCaracteristicas(datos);

                            var observaciones =
                                LeerObservaciones(datos);

                            if (caracteristicas.Count > 0)
                            {
                                Seccion(
                                    contenido,
                                    "VERIFICACIÓN DE CARACTERÍSTICAS",
                                    c =>
                                    {
                                        Tabla(
                                            c,
                                            new[]
                                            {
                                    "Característica",
                                    "Observación"
                                            },
                                            caracteristicas.Select(carac =>
                                                new[]
                                                {
                                        Mostrar(carac.Descripcion),

                                        observaciones.TryGetValue(
                                            carac.Id,
                                            out var obs)
                                            ? Mostrar(obs)
                                            : "-"
                                                }).ToList());
                                    });
                            }

                            // Liberación
                            Seccion(
                                contenido,
                                "LIBERACIÓN DEL PRODUCTO",
                                c =>
                                {
                                    TablaDatos(c, new[]
                                    {
                            ("¿Puede ser liberado?",
                                registro.PuedeLiberarse == true
                                    ? "SI"
                                    : registro.PuedeLiberarse == false
                                        ? "NO"
                                        : "-"),

                            ("Producción (Kg/h)",
                                Mostrar(registro.ProduccionKgHora)),

                            ("Desperdicio total (Kg)",
                                registro.DesperdicioTotalKg?
                                    .ToString("N2", Cultura) ?? "-"),

                            ("Revisado por",
                                Mostrar(registro.RevisadoPor)),

                            ("Cargo",
                                Mostrar(registro.CargoFirma)),

                            ("Fecha de revisión",
                                Fecha(registro.FechaRevision))
                                    });

                                    if (!string.IsNullOrWhiteSpace(
                                        registro.ExplicacionNoLiberado))
                                    {
                                        c.Item().Text(
                                            $"Motivo de no liberación: " +
                                            $"{registro.ExplicacionNoLiberado}");
                                    }
                                });

                            contenido.Item()
                                .PaddingBottom(5)
                                .BorderBottom(0.5f)
                                .BorderColor(PdfColors.GrisMedio);
                        });
                }

                columna.Item()
                    .PaddingTop(10)
                    .AlignCenter()
                    .Text("FIN DEL DOCUMENTO")
                    .FontSize(11)
                    .Bold();
            }
        }
    }
}