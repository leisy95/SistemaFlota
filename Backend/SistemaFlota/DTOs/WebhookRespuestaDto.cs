namespace SistemaFlota.DTOs
{
    public record WebhookRespuestaDto(
        int GrupoId,
        int UsuarioId,
        string NombreConductor,
        string Contenido,
        DateTime Fecha
    );
}