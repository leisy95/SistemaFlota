namespace SistemaFlota.DTOs
{
    public record CrearVinculacionDto(
        int FlotaChatUsuarioId,
        string TipoEntidad,
        int EntidadId,
        string? Telefono
    );

    public record UsuarioFlotaChatDto(int Id, string Nombre, string? Celular, string Rol);
}
