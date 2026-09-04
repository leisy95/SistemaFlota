namespace SistemaFlota.Models.Idempotencia;

public class IdempotencyLog
{
    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Method { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string? UsuarioId { get; set; }

    public int StatusCode { get; set; }

    public string? ContentType { get; set; }

    public string? ResponseBody { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}