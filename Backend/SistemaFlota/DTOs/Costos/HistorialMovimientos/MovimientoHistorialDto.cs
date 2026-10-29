namespace SistemaFlota.DTOs.Costos.HistorialMovimientos
{
    public class MovimientoHistorialDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string? Detalle { get; set; }
        public int? RegistroId { get; set; }
        public string? IpAddress { get; set; }
        public string Resultado { get; set; } = string.Empty;

        public string Descripcion =>
            string.IsNullOrWhiteSpace(Detalle)
                ? $"{Accion} en {Modulo}"
                : Detalle;
    }
}
