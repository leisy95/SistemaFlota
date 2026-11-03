using SistemaFlota.DTOs;

namespace SistemaFlota
{
    public interface IFlotaChatDirectorio
    {    
        Task<List<UsuarioFlotaChatDto>?> ObtenerUsuariosAsync();
    }
}