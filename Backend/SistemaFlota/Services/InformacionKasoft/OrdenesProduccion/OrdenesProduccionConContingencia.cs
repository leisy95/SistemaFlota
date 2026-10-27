using Microsoft.Extensions.Logging;
using SistemaFlota;
using SistemaFlota.Models;

namespace SistemaFlota.Services.InformacionKasoft.OrdenesProduccion
{
    public class OrdenesProduccionConContingencia : IProveedorOrdenesProduccion
    {
        private readonly IOrdenProduccionService _proveedorEmpresa;
        private readonly ImportacionExcelOrdenesProduccionService _proveedorExcel;
        private readonly ILogger<OrdenesProduccionConContingencia> _logger;

        public OrdenesProduccionConContingencia(
            IOrdenProduccionService proveedorEmpresa,
            ImportacionExcelOrdenesProduccionService proveedorExcel,
            ILogger<OrdenesProduccionConContingencia> logger)
        {
            _proveedorEmpresa = proveedorEmpresa;
            _proveedorExcel = proveedorExcel;
            _logger = logger;
        }

        public async Task<OrdenProduccionExterna?> BuscarPorNumero(string numeroOP)
        {
            try
            {
                var resultado = await _proveedorEmpresa.BuscarPorNumeroAsync(numeroOP);

                if (resultado != null)
                    return resultado;

                _logger.LogInformation(
                    "OP {NumeroOP} no encontrada en la base de la empresa. Buscando en Excel local.",
                    numeroOP);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Falló la conexión a la base de la empresa al buscar OP {NumeroOP}. Usando Excel como respaldo.",
                    numeroOP);
            }

            return await _proveedorExcel.BuscarPorNumero(numeroOP);
        }

        public async Task<List<OrdenProduccionExterna>> ImportarDesdeArchivo(
            Stream archivo,
            string nombreArchivo)
        {
            return await _proveedorExcel.ImportarDesdeArchivo(archivo, nombreArchivo);
        }
    }
}