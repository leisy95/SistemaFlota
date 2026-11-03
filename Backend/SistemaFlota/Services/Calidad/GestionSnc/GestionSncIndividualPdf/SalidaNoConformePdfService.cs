
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SistemaFlota;
using SistemaFlota.Services.Calidad.GestionSnc;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaFlota.Services.Calidad.GestionSnc.GestionSncIndividualPdf
{
    public class SalidaNoConformePdfService : ISalidaNoConformePdfService
    {
        private readonly ISalidaNoConformeService _sncService;
        private readonly IWebHostEnvironment _environment;
        private readonly AppDbContext _context;

        public SalidaNoConformePdfService(
            ISalidaNoConformeService sncService,
            IWebHostEnvironment environment,
            AppDbContext context)
        {
            _sncService = sncService;
            _environment = environment;
            _context = context;
        }

        // GENERAR PDF
        public async Task<byte[]> GenerarPdfAsync(int registroId)
        {
            // OBTENER REGISTRO
            var registro = await _sncService.ObtenerAsync(registroId);

            if (registro == null)
            {
                throw new KeyNotFoundException(
                    $"No se encontró la salida no conforme con ID {registroId}"
                );
            }

            // OBTENER EVIDENCIAS
            var evidencias = await _sncService.ObtenerEvidenciasAsync(registroId);

            var fotos = new List<(string Paso, byte[] Contenido)>();

            foreach (var evidencia in evidencias)
            {
                // SOLAMENTE CARGAR IMÁGENES
                if (!string.Equals(
                    evidencia.TipoArchivo,
                    "Imagen",
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // OBTENER RUTA SEGURA
                var ruta = ObtenerRutaArchivo(evidencia.NombreArchivo);

                if (!File.Exists(ruta))
                    continue;

                try
                {
                    var contenido = await File.ReadAllBytesAsync(ruta);

                    if (contenido.Length == 0)
                        continue;

                    fotos.Add((
                        evidencia.Paso,
                        contenido
                    ));
                }
                catch (IOException)
                {
                    // Si una evidencia no se puede leer,
                    // se continúa con las demás.
                    continue;
                }
            }

            // OBTENER CONFIGURACIÓN DE EMPRESA
            var empresa = await _context.ConfiguracionEmpresa
                .FirstOrDefaultAsync();

            if (empresa == null)
            {
                throw new InvalidOperationException(
                    "No se encontró la configuración de la empresa."
                );
            }

            // OBTENER LOGO
            var carpetaWeb = _environment.WebRootPath
                ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

            var nombreLogo = string.IsNullOrWhiteSpace(empresa.Logo)
                ? "logo.png"
                : Path.GetFileName(empresa.Logo);

            var logo = Path.Combine(
                carpetaWeb,
                "config",
                nombreLogo
            );

            if (!File.Exists(logo))
                logo = "";

            // CREAR DOCUMENTO
            var documento = new SalidaNoConformeDocumento(
                registro,
                fotos,
                empresa,
                logo
            );

            // GENERAR PDF
            return documento.GeneratePdf();
        }

        // OBTENER RUTA DE EVIDENCIA
        private string ObtenerRutaArchivo(string nombreArchivo)
        {
            var nombre = Path.GetFileName(nombreArchivo);

            var carpeta = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                "snc"
            );

            return Path.Combine(carpeta, nombre);
        }
    }
}
