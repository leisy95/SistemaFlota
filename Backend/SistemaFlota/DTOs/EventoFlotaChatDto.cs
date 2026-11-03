namespace SistemaFlota.DTOs
{
    
    public record EventoFlotaChatDto(
        string IdEvento,
        string Tipo,              
        DateTime Fecha,
        int MensajeId,
        int GrupoId,
        int UsuarioId,
        string NombreConductor,   
        string? Rol,
        string TipoMensaje,      
        string Contenido,
        double? Latitud,
        double? Longitud
    );
}