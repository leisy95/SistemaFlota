using SistemaFlota.Models;

namespace SistemaFlota
{
    public class ProveedorOrdenesConContingencia : IProveedorOrdenesProduccion
    {
        private readonly EmpresaOrdenesService _proveedorEmpresa;
        private readonly ImportacionExcelOrdenesService _proveedorExcel;
        private readonly ILogger<ProveedorOrdenesConContingencia> _logger;

        public ProveedorOrdenesConContingencia(
            EmpresaOrdenesService proveedorEmpresa,
            ImportacionExcelOrdenesService proveedorExcel,
            ILogger<ProveedorOrdenesConContingencia> logger)
        {
            _proveedorEmpresa = proveedorEmpresa;
            _proveedorExcel = proveedorExcel;
            _logger = logger;
        }

        public async Task<OrdenProduccionExterna?> BuscarPorNumero(string numeroOP)
        {
            try
            {
                var resultado = await _proveedorEmpresa.BuscarPorNumero(numeroOP);
                if (resultado != null)
                    return resultado;

                _logger.LogInformation(
                    "OP {NumeroOP} no encontrada en base de la empresa, buscando en Excel local.",
                    numeroOP);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Falló la conexión a la base de la empresa al buscar OP {NumeroOP}. Usando Excel como respaldo.",
                    numeroOP);
            }

            // Contingencia: si falla o no encuentra, usa el Excel local
            return await _proveedorExcel.BuscarPorNumero(numeroOP);
        }

        public async Task<List<OrdenProduccionExterna>> ImportarDesdeArchivo(Stream archivo, string nombreArchivo)
        {
            // La importación de Excel sigue funcionando igual, para seguir alimentando el respaldo
            return await _proveedorExcel.ImportarDesdeArchivo(archivo, nombreArchivo);
        }
    }
}