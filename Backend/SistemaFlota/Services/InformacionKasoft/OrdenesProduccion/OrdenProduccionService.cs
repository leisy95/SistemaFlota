
using SistemaFlota.DTOs.InformacionKasoft;
using SistemaFlota.Models;
using SistemaFlota.Services.InformacionKasoft;

namespace SistemaFlota.Services.InformacionKasoft.OrdenesProduccion
{
    public class OrdenProduccionService : IOrdenProduccionService
    {
        private readonly IInformacionKasoftService _informacion;

        public OrdenProduccionService(
            IInformacionKasoftService informacion)
        {
            _informacion = informacion;
        }

        public async Task<OrdenProduccionExterna?> BuscarPorNumeroAsync(
            string numeroOP)
        {
            if (!int.TryParse(numeroOP?.Trim(), out int numero))
                return null;

            var ordenes =
                await _informacion.ObtenerOrdenesProduccionAsync();

            var orden = ordenes.FirstOrDefault(o => o.Numop == numero);

            if (orden == null)
                return null;

            return new OrdenProduccionExterna
            {
                NumeroOP = orden.Numop.ToString(),
                Cliente = orden.Nombrecliente,
                Referencia = orden.Refer,
                Descripcion = orden.Descrip,
                CantidadOP = Convert.ToInt32(orden.Canprog),
                Unidad = orden.Um,
                FechaImportacion = DateTime.Now
            };
        }

        public async Task<decimal> ObtenerCantidadRealAsync(
            string numeroOP)
        {
            if (!int.TryParse(numeroOP?.Trim(), out int numero))
                return 0;

            var registros =
                await _informacion.ObtenerRegistroProduccionAsync();

            return Convert.ToDecimal(
                registros
                    .Where(r => r.Numop == numero)
                    .Sum(r => r.Kilos ?? 0));
        }

        public async Task<List<string>> BuscarReferenciasPorDescripcionAsync(
            string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return new List<string>();

            var referencias =
                await _informacion.ObtenerReferenciasAsync();

            return referencias
                .Where(r => !string.IsNullOrWhiteSpace(r.Descrip) &&
                    r.Descrip.Contains(
                        texto.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Codigo)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();
        }

        public Task<List<OrdenProduccionExterna>> ImportarDesdeArchivo(
            Stream archivo,
            string nombreArchivo)
        {
            throw new NotSupportedException(
                "La importación desde archivo no aplica para la fuente de base de datos de la empresa.");
        }

        public async Task<Dictionary<string, string>> ObtenerDescripcionesReferenciasAsync(
             IEnumerable<string> referencias)
        {
            var codigos = referencias
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var referenciasDisponibles =
                await _informacion.ObtenerReferenciasAsync();

            return referenciasDisponibles
                .Where(r =>
                    !string.IsNullOrWhiteSpace(r.Codigo) &&
                    !string.IsNullOrWhiteSpace(r.Descrip) &&
                    codigos.Contains(r.Codigo))
                .GroupBy(r => r.Codigo!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().Descrip!,
                    StringComparer.OrdinalIgnoreCase);
        }

    }
}
