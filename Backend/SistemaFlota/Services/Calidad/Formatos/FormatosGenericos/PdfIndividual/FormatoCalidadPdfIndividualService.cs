
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota;
using SistemaFlota.Models;
using SistemaFlota.Services.Pdf.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SistemaFlota.Services.Calidad.Formatos.FormatosGenericos.PdfIndividual
{
    public class FormatoCalidadPdfIndividualService
        : IFormatoCalidadPdfIndividualService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly MezclaOrdenService _mezclaOrdenService;

        private static readonly CultureInfo Cultura =
            CultureInfo.GetCultureInfo("es-CO");

        private const string CarpetaFotos = "formatos-calidad";

        private const string Negro = "#000000";
        private const string Gris = "#E1E1E1";
        private const string Blanco = "#FFFFFF";

        public FormatoCalidadPdfIndividualService(
            AppDbContext context,
            IWebHostEnvironment environment,
            MezclaOrdenService mezclaOrdenService)
        {
            _context = context;
            _environment = environment;
            _mezclaOrdenService = mezclaOrdenService;
        }

        // GENERACIÓN DEL PDF

        public async Task<byte[]> GenerarPdfRegistroAsync(int registroId)
        {
            // 1. Consultar registro
            var registro = await _context.RegistrosFormatoCalidad
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == registroId);

            if (registro == null)
                throw new InvalidOperationException(
                    "No existe el registro de calidad solicitado.");

            // 2. Consultar empresa
            var empresa = await _context.ConfiguracionEmpresa
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (empresa == null)
                throw new InvalidOperationException(
                    "No existe la configuración de la empresa.");

            // 3. Consultar tipo de formato
            var tipo = await _context.TiposFormatoCalidad
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == registro.TipoFormatoId);

            if (tipo == null)
                throw new InvalidOperationException(
                    "No existe el tipo de formato asociado al registro.");

            // 4. Consultar parámetros guardados por operario
            var parametrosOperarios = await _context.RegistrosParametrosOperario
                .AsNoTracking()
                .Where(x => x.RegistroFormatoCalidadId == registroId)
                .OrderByDescending(x => x.FechaGuardado)
                .Select(x => new ParametroOperarioPdf
                {
                    OperarioNombre = x.OperarioNombre,
                    VariablesCriticasJson = x.VariablesCriticasJson,
                    FechaGuardado = x.FechaGuardado,
                    MotivoCambio = x.MotivoCambio
                })
                .ToListAsync();

            // 5. Consultar fotografías de la tabla
            var fotosTabla = await _context.FormatosCalidadFotos
                .AsNoTracking()
                .Where(x => x.RegistroFormatoCalidadId == registroId)
                .ToListAsync();

            // 6. Consultar mezcla de la orden
            var mezcla = new List<MezclaOrdenItem>();

            try
            {
                mezcla = await _mezclaOrdenService.ObtenerAsync(
                    registro.OrdenProduccion);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error consultando mezcla de la OP {registro.OrdenProduccion}: {ex.Message}");
            }

            // 7. Leer información JSON
            var datos = LeerJson(registro.ResultadosJson);

            var rondas = LeerRondas(datos);
            var caracteristicas = LeerCaracteristicas(datos);
            var observaciones = LeerObservaciones(datos);

            var variablesCriticas =
                LeerParametros(registro.VariablesCriticasJson);

            // 8. Combinar fotografías del JSON y de la tabla
            var fotosPorRonda = new Dictionary<string, List<string>>();

            foreach (var ronda in rondas)
            {
                var archivos = new List<string>();

                if (ronda.TryGetProperty("fotos", out var fotosJson) &&
                    fotosJson.ValueKind == JsonValueKind.Array)
                {
                    foreach (var foto in fotosJson.EnumerateArray())
                    {
                        var archivo = Valor(foto);

                        if (!string.IsNullOrWhiteSpace(archivo))
                            archivos.Add(archivo);
                    }
                }

                var rondaId = Texto(ronda, "id");

                archivos.AddRange(
                    fotosTabla
                        .Where(f =>
                            string.Equals(
                                f.RondaId?.ToString(),
                                rondaId,
                                StringComparison.OrdinalIgnoreCase))
                        .Select(f => f.Archivo)
                        .Where(f => !string.IsNullOrWhiteSpace(f))
                        .Select(f => f!));

                fotosPorRonda[rondaId] = archivos
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            // 9. Cargar imágenes
            var imagenes = new Dictionary<string, byte[]?>(
                StringComparer.OrdinalIgnoreCase);

            var archivosUnicos = fotosPorRonda.Values
                .SelectMany(x => x)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var archivo in archivosUnicos)
                imagenes[archivo] = ObtenerImagen(archivo);

            var logo = ObtenerLogo(empresa.Logo);
            var firma = ObtenerImagenBase64(registro.FirmaDigital);

            // 10. Construir documento
            var documento = new DocumentoIndividual(
                empresa,
                logo,
                tipo.Nombre,
                tipo.Codigo,
                registro,
                mezcla,
                parametrosOperarios,
                variablesCriticas,
                rondas,
                caracteristicas,
                observaciones,
                fotosPorRonda,
                imagenes,
                firma);

            return Document.Create(documento.Compose).GeneratePdf();
        }

        // ARCHIVOS E IMÁGENES

        private string ObtenerLogo(string? logo)
        {
            var carpeta = ObtenerCarpetaConfig();

            if (!string.IsNullOrWhiteSpace(logo))
            {
                var nombre = Path.GetFileName(logo);
                var personalizado = Path.Combine(carpeta, nombre);

                if (File.Exists(personalizado))
                    return personalizado;
            }

            return Path.Combine(carpeta, "logo.png");
        }

        private string ObtenerCarpetaConfig()
        {
            var raiz = _environment.WebRootPath ??
                Path.Combine(_environment.ContentRootPath, "wwwroot");

            return Path.Combine(raiz, "config");
        }

        private byte[]? ObtenerImagen(string archivo)
        {
            if (string.IsNullOrWhiteSpace(archivo))
                return null;

            var raiz = _environment.WebRootPath ??
                Path.Combine(_environment.ContentRootPath, "wwwroot");

            var carpeta = Path.GetFullPath(
                Path.Combine(raiz, CarpetaFotos));

            var nombre = Path.GetFileName(archivo);

            var ruta = Path.GetFullPath(
                Path.Combine(carpeta, nombre));

            if (!ruta.StartsWith(
                carpeta + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
                return null;

            return File.Exists(ruta)
                ? File.ReadAllBytes(ruta)
                : null;
        }

        private static byte[]? ObtenerImagenBase64(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            try
            {
                var base64 = valor;

                if (base64.Contains(','))
                    base64 = base64[(base64.IndexOf(',') + 1)..];

                return Convert.FromBase64String(base64);
            }
            catch
            {
                return null;
            }
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

        private static List<JsonElement> LeerRondas(JsonElement datos)
        {
            if (datos.ValueKind == JsonValueKind.Object &&
                datos.TryGetProperty("rondas", out var rondas) &&
                rondas.ValueKind == JsonValueKind.Array)
            {
                return rondas.EnumerateArray()
                    .Select(x => x.Clone())
                    .ToList();
            }

            return new List<JsonElement>();
        }

        private static List<JsonElement> LeerCaracteristicas(JsonElement datos)
        {
            if (datos.ValueKind == JsonValueKind.Object &&
                datos.TryGetProperty("caracteristicas", out var lista) &&
                lista.ValueKind == JsonValueKind.Array)
            {
                return lista.EnumerateArray()
                    .Select(x => x.Clone())
                    .ToList();
            }

            return new List<JsonElement>();
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
                resultado[propiedad.Name] = Valor(propiedad.Value);

            return resultado;
        }

        private static string Texto(JsonElement elemento, string propiedad)
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

        private static string Mostrar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? "-" : valor;
        }

        private static string Fecha(DateTime? fecha)
        {
            return fecha.HasValue
                ? fecha.Value.ToString("dd/MM/yyyy HH:mm", Cultura)
                : "-";
        }

        // PARÁMETROS

        private class ParametroPdf
        {
            public string Etiqueta { get; set; } = "";
            public string Valor { get; set; } = "";
        }

        private class ParametroOperarioPdf
        {
            public string? OperarioNombre { get; set; }
            public string? VariablesCriticasJson { get; set; }
            public DateTime? FechaGuardado { get; set; }
            public string? MotivoCambio { get; set; }
        }

        private static List<ParametroPdf> LeerParametros(string? json)
        {
            var resultado = new List<ParametroPdf>();
            var datos = LeerJson(json);

            if (datos.ValueKind != JsonValueKind.Object)
                return resultado;

            RecorrerParametros(datos, resultado, "");

            return resultado;
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

                if (propiedad.Value.ValueKind == JsonValueKind.Object)
                {
                    RecorrerParametros(
                        propiedad.Value,
                        resultado,
                        prefijo);

                    continue;
                }

                if (propiedad.Value.ValueKind == JsonValueKind.Array)
                    continue;

                var etiqueta = EtiquetaParametro(propiedad.Name);

                etiqueta = string.IsNullOrEmpty(prefijo)
                    ? etiqueta
                    : $"{prefijo} - {etiqueta}";

                var valor = Valor(propiedad.Value);

                if (!string.IsNullOrWhiteSpace(valor))
                {
                    resultado.Add(new ParametroPdf
                    {
                        Etiqueta = etiqueta,
                        Valor = valor
                    });
                }
            }
        }

        private static string EtiquetaParametro(string nombre)
        {
            var etiquetas = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["corona"] = "Molde",
                ["molde"] = "Medida del molde",
                ["aire"] = "Aire (H)",
                ["amperaje"] = "Amperaje (A)",
                ["alturaBurbuja"] = "Altura burbuja (cm)",
                ["vela"] = "Vela",
                ["maquina"] = "Motor / Motor 1 (Hz)",
                ["maquina2"] = "Motor 2 (Hz)",
                ["halador"] = "Halador (Hz)",
                ["bobinador"] = "Bobinador (Hz)",
                ["zona1"] = "Zona 1 (°C)",
                ["zona2"] = "Zona 2 (°C)",
                ["zona3"] = "Zona 3 (°C)",
                ["zona4"] = "Zona 4 (°C)",
                ["zona5"] = "Zona 5 (°C)",
                ["zona6"] = "Zona 6 (°C)",
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

            return Regex.Replace(
                nombre,
                "([a-z])([A-Z])",
                "$1 $2");
        }

        // DOCUMENTO PDF

        private class DocumentoIndividual
        {
            private readonly ConfiguracionEmpresa _empresa;
            private readonly string _logo;
            private readonly string _nombreFormato;
            private readonly string _codigoFormato;
            private readonly RegistroFormatoCalidad _registro;
            private readonly List<MezclaOrdenItem> _mezcla;
            private readonly List<ParametroOperarioPdf> _operarios;
            private readonly List<ParametroPdf> _parametros;
            private readonly List<JsonElement> _rondas;
            private readonly List<JsonElement> _caracteristicas;
            private readonly Dictionary<string, string> _observaciones;
            private readonly Dictionary<string, List<string>> _fotosPorRonda;
            private readonly Dictionary<string, byte[]?> _imagenes;
            private readonly byte[]? _firma;

            public DocumentoIndividual(
                ConfiguracionEmpresa empresa,
                string logo,
                string nombreFormato,
                string codigoFormato,
                RegistroFormatoCalidad registro,
                List<MezclaOrdenItem> mezcla,
                List<ParametroOperarioPdf> operarios,
                List<ParametroPdf> parametros,
                List<JsonElement> rondas,
                List<JsonElement> caracteristicas,
                Dictionary<string, string> observaciones,
                Dictionary<string, List<string>> fotosPorRonda,
                Dictionary<string, byte[]?> imagenes,
                byte[]? firma)
            {
                _empresa = empresa;
                _logo = logo;
                _nombreFormato = nombreFormato;
                _codigoFormato = codigoFormato;
                _registro = registro;
                _mezcla = mezcla;
                _operarios = operarios;
                _parametros = parametros;
                _rondas = rondas;
                _caracteristicas = caracteristicas;
                _observaciones = observaciones;
                _fotosPorRonda = fotosPorRonda;
                _imagenes = imagenes;
                _firma = firma;
            }

            // COMPOSICIÓN PRINCIPAL

            public void Compose(IDocumentContainer container)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(10, Unit.Millimetre);

                    page.DefaultTextStyle(x => x
                        .FontSize(7)
                        .FontFamily("Arial")
                        .FontColor(Negro));

                    // Encabezado empresarial
                    page.Header()
                    .ShowOnce()
                    .Element(c =>
                        HeaderEmpresa.Dibujar(
                            c,
                            _logo,
                            _empresa,
                            _nombreFormato,
                            $"{_codigoFormato} V2",
                            Fecha(_registro.Fecha),
                            Mostrar(_registro.OrdenProduccion)));

                    // CONTENIDO DEL DOCUMENTO
                    page.Content().PaddingTop(8).Column(column =>
                    {
                        column.Spacing(6);

                        DatosGenerales(column);
                        SeccionMezcla(column);
                        SeccionParametros(column);
                        SeccionParametrosOperarios(column);
                        SeccionVerificacion(column);
                        SeccionFotografias(column);
                        SeccionLiberacion(column);
                    });

                    // Pie empresarial
                    page.Footer().Element(c =>
                        FooterEmpresa.Dibujar(
                            c,
                            "Layas Software - Formato de Calidad"));
                });
            }

            private static void CardSeccion(
                ColumnDescriptor column,
                string titulo,
                Action<ColumnDescriptor> contenido)
            {
                column.Item()
                    .PaddingBottom(5)
                    .Border(1)
                    .BorderColor(Negro)
                    .Column(card =>
                    {
                        card.Item()
                            .Background(Gris)
                            .Padding(5)
                            .Text(titulo)
                            .Bold()
                            .FontSize(8)
                            .FontColor(Negro);

                        card.Item()
                            .Padding(6)
                            .Column(contenido);
                    });
            }

            // DATOS GENERALES

            private void DatosGenerales(ColumnDescriptor column)
            {
                CardSeccion(
                    column,
                    "1. DATOS GENERALES",
                    contenido =>
                    {
                        TablaDatos(contenido, new[]
                        {
                            ("Orden de producción",
                                Mostrar(_registro.OrdenProduccion)),

                            ("Fecha",
                                Fecha(_registro.Fecha)),

                            ("Cliente",
                                Mostrar(_registro.Cliente)),

                            ("Referencia",
                                Mostrar(_registro.Referencia)),

                            ("Operarios",
                                Mostrar(_registro.Operarios)),

                            ("Máquina",
                                Mostrar(_registro.Maquina)),

                            ("Hora",
                                Mostrar(_registro.Hora)),

                            ("Estado",
                                Mostrar(_registro.Estado))
                        });
                    });
            }

            // MEZCLA DE LA ORDEN

            private void SeccionMezcla(ColumnDescriptor column)
            {
                CardSeccion(
                    column,
                    "2. MEZCLA DE LA ORDEN",
                    contenido =>
                    {
                        if (_mezcla == null || _mezcla.Count == 0)
                        {
                            contenido.Item()
                                .Text("No hay información de mezcla disponible.")
                                .FontSize(7);

                            return;
                        }

                        contenido.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn(2);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            string[] encabezados =
                            {
                                "Proceso",
                                "Referencia",
                                "Descripción",
                                "Cantidad",
                                "Porcentaje"
                            };

                            table.Header(header =>
                            {
                                foreach (var encabezado in encabezados)
                                {
                                    header.Cell()
                                        .Element(CeldaEncabezado)
                                        .Text(encabezado)
                                        .Bold()
                                        .FontSize(6);
                                }
                            });

                            foreach (var item in _mezcla)
                            {
                                table.Cell().Element(Celda)
                                    .Text(Mostrar(item.Proceso));

                                table.Cell().Element(Celda)
                                    .Text(Mostrar(item.Referencia));

                                table.Cell().Element(Celda)
                                    .Text(Mostrar(item.Descripcion));

                                table.Cell().Element(Celda)
                                    .Text($"{item.Cantidad:N2} {item.Unidad}");

                                table.Cell().Element(Celda)
                                    .Text($"{item.Porcentaje:N2} %");
                            }
                        });
                    });
            }

            // PARÁMETROS DEL REGISTRO

            private void SeccionParametros(ColumnDescriptor column)
            {
                CardSeccion(
                    column,
                    "3. PARÁMETROS DEL REGISTRO",
                    contenido =>
                    {
                        if (_parametros.Count == 0)
                        {
                            contenido.Item()
                                .Text("No hay parámetros registrados.");

                            return;
                        }

                        TablaParametros(contenido, _parametros);
                    });
            }

            // PARÁMETROS POR OPERARIO

            private void SeccionParametrosOperarios(
                ColumnDescriptor column)
            {
                CardSeccion(
                    column,
                    "4. PARÁMETROS POR OPERARIO",
                    contenido =>
                    {
                        if (_operarios.Count == 0)
                        {
                            contenido.Item()
                                .Text("No hay parámetros guardados por operario.");

                            return;
                        }

                        foreach (var operario in _operarios)
                        {
                            var parametros = LeerParametros(
                                operario.VariablesCriticasJson);

                            contenido.Item()
                                .PaddingBottom(6)
                                .Border(0.5f)
                                .BorderColor(Negro)
                                .Column(col =>
                                {
                                    col.Item()
                                        .Background(Gris)
                                        .Padding(4)
                                        .Text(
                                            $"OPERARIO: {Mostrar(operario.OperarioNombre)}")
                                        .Bold()
                                        .FontSize(7);

                                    col.Item()
                                        .Padding(5)
                                        .Column(detalle =>
                                        {
                                            detalle.Item()
                                                .Text(
                                                    $"Fecha de guardado: {Fecha(operario.FechaGuardado)}")
                                                .FontSize(7);

                                            if (parametros.Count > 0)
                                            {
                                                TablaParametros(
                                                    detalle,
                                                    parametros);
                                            }
                                            else
                                            {
                                                detalle.Item()
                                                    .Text("No hay parámetros registrados para este operario.")
                                                    .FontSize(7);
                                            }

                                            if (!string.IsNullOrWhiteSpace(
                                                operario.MotivoCambio))
                                            {
                                                detalle.Item()
                                                    .PaddingTop(4)
                                                    .Text(
                                                        $"Motivo del cambio: {operario.MotivoCambio}")
                                                    .FontSize(7);
                                            }
                                        });
                                });
                        }
                    });
            }

            // OBTENER VALOR DE UNA RONDA

            private static string ObtenerValorRonda(
                JsonElement ronda,
                string caracteristicaId)
            {
                if (ronda.ValueKind != JsonValueKind.Object ||
                    !ronda.TryGetProperty("valores", out var valores) ||
                    valores.ValueKind != JsonValueKind.Array)
                {
                    return "";
                }

                foreach (var item in valores.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object &&
                        item.TryGetProperty("caracteristicaId", out var idProp) &&
                        idProp.ValueKind == JsonValueKind.String &&
                        idProp.GetString()!.Equals(
                            caracteristicaId,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        if (item.TryGetProperty("valor", out var valProp))
                            return Valor(valProp);
                    }
                }

                return "";
            }

            // VERIFICACIÓN POR HORA

            private void SeccionVerificacion(ColumnDescriptor column)
            {
                CardSeccion(
                    column,
                    "5. VERIFICACIÓN POR HORA",
                    contenido =>
                    {
                        if (_caracteristicas.Count == 0 || _rondas.Count == 0)
                        {
                            contenido.Item()
                                .Text("No hay información de verificación por hora.");

                            return;
                        }

                        contenido.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);

                                foreach (var _ in _rondas)
                                    columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell()
                                    .Element(CeldaEncabezado)
                                    .Text("Característica")
                                    .Bold()
                                    .FontSize(6);

                                for (int i = 0; i < _rondas.Count; i++)
                                {
                                    header.Cell()
                                        .Element(CeldaEncabezado)
                                        .Text($"Ronda {i + 1}")
                                        .Bold()
                                        .FontSize(6);
                                }
                            });

                            foreach (var caracteristica in _caracteristicas)
                            {
                                var id = Texto(caracteristica, "id");
                                var descripcion =
                                    Texto(caracteristica, "descripcion");

                                table.Cell()
                                    .Element(Celda)
                                    .Text(Mostrar(descripcion));

                                foreach (var ronda in _rondas)
                                {
                                    var valor = ObtenerValorRonda(ronda, id);

                                    table.Cell()
                                        .Element(Celda)
                                        .Text(Mostrar(valor));
                                }
                            }
                        });

                        foreach (var ronda in _rondas)
                        {
                            var hora = Texto(ronda, "hora");
                            var operario = Texto(ronda, "operario");
                            var desperdicio = Texto(ronda, "kilosDesperdicio");
                            var final = Texto(ronda, "final");

                            contenido.Item()
                                .PaddingTop(5)
                                .Text(
                                    $"Hora: {Mostrar(hora)} | " +
                                    $"Operario: {Mostrar(operario)} | " +
                                    $"Desperdicio: {Mostrar(desperdicio)} kg | " +
                                    $"Final: {Mostrar(final)}")
                                .FontSize(7);

                            var rondaId = Texto(ronda, "id");

                            if (_observaciones.TryGetValue(
                                rondaId,
                                out var observacion) &&
                                !string.IsNullOrWhiteSpace(observacion))
                            {
                                contenido.Item()
                                    .Text($"Observación: {observacion}")
                                    .FontSize(7);
                            }
                        }
                    });
            }

            // FOTOGRAFÍAS DE EVIDENCIA

            private void SeccionFotografias(ColumnDescriptor column)
            {
                CardSeccion(
                    column,
                    "6. FOTOGRAFÍAS DE EVIDENCIA",
                    contenido =>
                    {
                        if (_fotosPorRonda.Count == 0)
                        {
                            contenido.Item()
                                .Text("No hay fotografías registradas.");

                            return;
                        }

                        bool hayFotografias = false;

                        foreach (var ronda in _rondas)
                        {
                            var rondaId = Texto(ronda, "id");

                            if (!_fotosPorRonda.TryGetValue(
                                rondaId,
                                out var archivos) ||
                                archivos.Count == 0)
                                continue;

                            var archivosDisponibles = archivos
                                .Where(archivo =>
                                    _imagenes.TryGetValue(
                                        archivo,
                                        out var bytes) &&
                                    bytes != null)
                                .ToList();

                            if (archivosDisponibles.Count == 0)
                                continue;

                            hayFotografias = true;

                            contenido.Item()
                                .PaddingTop(5)
                                .Background(Gris)
                                .Padding(4)
                                .Text(
                                    $"RONDA: {Mostrar(rondaId)} | " +
                                    $"Hora: {Mostrar(Texto(ronda, "hora"))} | " +
                                    $"Operario: {Mostrar(Texto(ronda, "operario"))}")
                                .Bold()
                                .FontSize(7);

                            contenido.Item().Grid(grid =>
                            {
                                grid.Columns(4);
                                grid.Spacing(5);

                                foreach (var archivo in archivosDisponibles)
                                {
                                    var bytes = _imagenes[archivo];

                                    if (bytes == null)
                                        continue;

                                    grid.Item()
                                        .Height(90)
                                        .Border(1)
                                        .BorderColor(Negro)
                                        .Padding(3)
                                        .Image(bytes)
                                        .FitArea();
                                }
                            });
                        }

                        if (!hayFotografias)
                        {
                            contenido.Item()
                                .Text("No hay fotografías disponibles para mostrar.");
                        }
                    });
            }

            // LIBERACIÓN

            private void SeccionLiberacion(ColumnDescriptor column)
            {
                CardSeccion(
                    column,
                    "7. LIBERACIÓN",
                    contenido =>
                    {
                        TablaDatos(contenido, new[]
                        {
                            ("¿Puede liberarse?",
                                Mostrar(_registro.PuedeLiberarse == true
                                    ? "Sí"
                                    : "No")),

                            ("Producción kg/hora",
                                Convert.ToString(
                                    _registro.ProduccionKgHora,
                                    Cultura) ?? "-"),

                            ("Desperdicio total kg",
                                Convert.ToString(
                                    _registro.DesperdicioTotalKg,
                                    Cultura) ?? "-"),

                            ("Cargo",
                                Mostrar(_registro.CargoFirma)),

                            ("Revisado por",
                                Mostrar(_registro.RevisadoPor)),

                            ("Fecha de revisión",
                                Fecha(_registro.FechaRevision)),

                            ("Explicación",
                                Mostrar(_registro.ExplicacionNoLiberado))
                        });

                        if (_firma != null)
                        {
                            contenido.Item()
                                .PaddingTop(8)
                                .Text("FIRMA DIGITAL")
                                .Bold()
                                .FontSize(7);

                            contenido.Item()
                                .PaddingTop(4)
                                .Height(60)
                                .Width(150)
                                .Image(_firma)
                                .FitArea();
                        }
                    });
            }

            // TABLA DE DATOS GENERALES

            private static void TablaDatos(
                ColumnDescriptor column,
                (string Etiqueta, string Valor)[] datos)
            {
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn(2);
                    });

                    for (int i = 0; i < datos.Length; i += 2)
                    {
                        var item1 = datos[i];

                        table.Cell()
                            .Element(CeldaEtiqueta)
                            .Text(item1.Etiqueta)
                            .Bold();

                        table.Cell()
                            .Element(Celda)
                            .Text(item1.Valor);

                        if (i + 1 < datos.Length)
                        {
                            var item2 = datos[i + 1];

                            table.Cell()
                                .Element(CeldaEtiqueta)
                                .Text(item2.Etiqueta)
                                .Bold();

                            table.Cell()
                                .Element(Celda)
                                .Text(item2.Valor);
                        }
                        else
                        {
                            table.Cell().Element(Celda);
                            table.Cell().Element(Celda);
                        }
                    }
                });
            }

            // TABLA DE PARÁMETROS

            private static void TablaParametros(
                ColumnDescriptor column,
                List<ParametroPdf> parametros)
            {
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    for (int i = 0; i < parametros.Count; i += 2)
                    {
                        var p1 = parametros[i];

                        table.Cell()
                            .Element(CeldaEtiqueta)
                            .Text(p1.Etiqueta)
                            .Bold();

                        table.Cell()
                            .Element(Celda)
                            .Text(p1.Valor);

                        if (i + 1 < parametros.Count)
                        {
                            var p2 = parametros[i + 1];

                            table.Cell()
                                .Element(CeldaEtiqueta)
                                .Text(p2.Etiqueta)
                                .Bold();

                            table.Cell()
                                .Element(Celda)
                                .Text(p2.Valor);
                        }
                        else
                        {
                            table.Cell().Element(Celda);
                            table.Cell().Element(Celda);
                        }
                    }
                });
            }

            // ESTILOS DE CELDAS

            private static IContainer Celda(IContainer container)
            {
                return container
                    .Border(0.5f)
                    .BorderColor(Negro)
                    .Padding(3)
                    .AlignMiddle()
                    .DefaultTextStyle(x => x
                        .FontSize(7)
                        .FontColor(Negro));
            }

            private static IContainer CeldaEtiqueta(IContainer container)
            {
                return container
                    .Background(Gris)
                    .Border(0.5f)
                    .BorderColor(Negro)
                    .Padding(3)
                    .AlignMiddle()
                    .DefaultTextStyle(x => x
                        .FontSize(7)
                        .FontColor(Negro));
            }

            private static IContainer CeldaEncabezado(IContainer container)
            {
                return container
                    .Background(Gris)
                    .Border(0.5f)
                    .BorderColor(Negro)
                    .Padding(3)
                    .AlignCenter()
                    .AlignMiddle()
                    .DefaultTextStyle(x => x
                        .FontSize(6)
                        .FontColor(Negro));
            }
        }
    }
}
