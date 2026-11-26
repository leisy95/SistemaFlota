
using SistemaFlota.DTOs.Produccion.LecturaImagen;

namespace SistemaFlota.Services.Produccion.LecturaImagen
{
    public class LectorReferenciaService : ILectorReferenciaService
    {
        private readonly IReconocedorReferencia _reconocedor;

        public LectorReferenciaService(
            IReconocedorReferencia reconocedor)
        {
            _reconocedor = reconocedor;
        }

        public async Task<LectorReferenciaDto> LeerReferenciaAsync(
            IFormFile imagen)
        {
            if (imagen == null || imagen.Length == 0)
            {
                return new LectorReferenciaDto
                {
                    LeidaCorrectamente = false,
                    Mensaje = "Debes seleccionar una imagen."
                };
            }

            var extensionesPermitidas = new[]
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            var extension = Path.GetExtension(imagen.FileName)
                .ToLowerInvariant();

            if (!extensionesPermitidas.Contains(extension))
            {
                return new LectorReferenciaDto
                {
                    LeidaCorrectamente = false,
                    Mensaje = "El formato de imagen no está permitido."
                };
            }

            if (imagen.Length > 10 * 1024 * 1024)
            {
                return new LectorReferenciaDto
                {
                    LeidaCorrectamente = false,
                    Mensaje = "La imagen no puede superar los 10 MB."
                };
            }

            // Ejecutar reconocimiento OCR
            var texto = await _reconocedor.LeerTextoAsync(imagen);

            texto = texto?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(texto))
            {
                return new LectorReferenciaDto
                {
                    LeidaCorrectamente = false,
                    Mensaje = "No se pudo reconocer texto en la imagen."
                };
            }

            // Separar el texto reconocido por líneas
            var lineas = texto
                .Split(
                    new[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries
                )
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            return new LectorReferenciaDto
            {
                Texto = texto,

                NumeroOrden = lineas.Length > 0 ? lineas[0] : "",
                Codigo = lineas.Length > 1 ? lineas[1] : "",
                NombreOperario = lineas.Length > 2 ? lineas[2] : "",
                Turno = lineas.Length > 3 ? lineas[3] : "",

                LeidaCorrectamente = true,
                Mensaje = "Datos reconocidos correctamente. Verifica la información."
            };
        }
    }
}
