namespace SistemaFlota.DTOs
{
    public record CostoFleteDto(
        int AutorizacionId,
        decimal Peajes, decimal Combustible, decimal Parqueos,
        decimal DescarguesMcia, decimal CargueMateriales,
        decimal Alimentacion, decimal Hospedaje, decimal Varios,
        decimal Total, string? Observaciones
    );

    public record VerificarDto(string VerificadoPor, string FirmaVerificacion);
}