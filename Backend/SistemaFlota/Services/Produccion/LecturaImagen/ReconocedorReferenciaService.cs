
using System.Diagnostics;

namespace SistemaFlota.Services.Produccion.LecturaImagen
{
    public class ReconocedorReferenciaService : IReconocedorReferencia
    {
        private const string RutaTesseractWindows =
            @"C:\Program Files\Tesseract-OCR\tesseract.exe";

        private const string CarpetaPruebasOCR =
            @"C:\Temp\PruebasOCR";

        public async Task<string> LeerTextoAsync(IFormFile imagen)
        {
            if (imagen == null || imagen.Length == 0)
                throw new ArgumentException("La imagen está vacía.");

            // Crear carpeta para conservar las fotografías recibidas desde Angular.
            Directory.CreateDirectory(CarpetaPruebasOCR);

            string archivoTemporal = Path.Combine(
                CarpetaPruebasOCR,
                $"{Guid.NewGuid()}.png"
            );

            try
            {
                Console.WriteLine("OCR: Iniciando reconocimiento.");

                // Guardar la fotografía capturada desde el frontend.
                await using (var stream = new FileStream(
                    archivoTemporal,
                    FileMode.CreateNew,
                    FileAccess.Write))
                {
                    await imagen.CopyToAsync(stream);
                }

                Console.WriteLine(
                    $"OCR: Imagen guardada: {archivoTemporal}"
                );

                string ejecutable = OperatingSystem.IsWindows()
                    ? RutaTesseractWindows
                    : "tesseract";

                if (OperatingSystem.IsWindows() &&
                    !File.Exists(ejecutable))
                {
                    throw new FileNotFoundException(
                        "No se encontró Tesseract en la ruta configurada.",
                        ejecutable
                    );
                }

                var inicio = new ProcessStartInfo
                {
                    FileName = ejecutable,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                inicio.ArgumentList.Add(archivoTemporal);
                inicio.ArgumentList.Add("stdout");
                inicio.ArgumentList.Add("-l");
                inicio.ArgumentList.Add("eng");
                inicio.ArgumentList.Add("--psm");
                inicio.ArgumentList.Add("11");
                inicio.ArgumentList.Add("-c");
                inicio.ArgumentList.Add(
                    "tessedit_char_whitelist=ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"
                );

                using var proceso = new Process
                {
                    StartInfo = inicio
                };

                Console.WriteLine("OCR: Ejecutando Tesseract.");

                proceso.Start();

                Console.WriteLine("OCR: Tesseract iniciado.");

                var lecturaSalida =
                    proceso.StandardOutput.ReadToEndAsync();

                var lecturaError =
                    proceso.StandardError.ReadToEndAsync();

                await proceso.WaitForExitAsync();

                var texto = await lecturaSalida;
                var error = await lecturaError;

                Console.WriteLine(
                    $"OCR: Proceso terminado. Código: {proceso.ExitCode}"
                );

                Console.WriteLine($"OCR: Texto reconocido: {texto}");
                Console.WriteLine($"OCR: Error: {error}");

                if (proceso.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        $"Error ejecutando Tesseract: {error}"
                    );
                }

                return texto.Trim();
            }
            finally
            {
                // No eliminar la fotografía durante las pruebas.
                Console.WriteLine(
                    $"OCR: Imagen conservada en: {archivoTemporal}"
                );
            }
        }
    }
}
