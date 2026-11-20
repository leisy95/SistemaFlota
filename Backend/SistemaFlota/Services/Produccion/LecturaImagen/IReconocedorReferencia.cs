namespace SistemaFlota.Services.Produccion.LecturaImagen
{
    public interface IReconocedorReferencia
    {
        Task<string> LeerTextoAsync(IFormFile imagen);
    }
}
