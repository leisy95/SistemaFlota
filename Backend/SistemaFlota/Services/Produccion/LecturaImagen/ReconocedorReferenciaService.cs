
using Google.Cloud.Vision.V1;
using System.Diagnostics;

namespace SistemaFlota.Services.Produccion.LecturaImagen
{
    public class ReconocedorReferenciaService : IReconocedorReferencia
    {
        private readonly ImageAnnotatorClient _visionClient;

        public ReconocedorReferenciaService(
            ImageAnnotatorClient visionClient)
        {
            _visionClient = visionClient;
        }

        public async Task<string> LeerTextoAsync(IFormFile imagen)
        {
            if (imagen == null || imagen.Length == 0)
                throw new ArgumentException("La imagen está vacía.");

            await using var stream = imagen.OpenReadStream();

            var imagenGoogle = await Image.FromStreamAsync(stream);

            var respuesta = await _visionClient.DetectDocumentTextAsync(
                imagenGoogle
            );

            return respuesta?.Text?.Trim() ?? string.Empty;
        }
    }
}