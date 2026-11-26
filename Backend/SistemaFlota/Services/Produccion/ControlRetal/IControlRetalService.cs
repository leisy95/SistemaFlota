using SistemaFlota.DTOs.Produccion.ControlRetal;

namespace SistemaFlota.Services.Produccion.ControlRetal
{
    public interface IControlRetalService
    {
        Task<ConsultarOrdenControlRetalDto?> ObtenerReferenciaPorOrdenAsync(int numop);
    }
}
