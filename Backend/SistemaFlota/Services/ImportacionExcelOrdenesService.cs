using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.Models;
namespace SistemaFlota
{
    public class ImportacionExcelOrdenesService : IProveedorOrdenesProduccion
    {
        private readonly AppDbContext _context;
        public ImportacionExcelOrdenesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrdenProduccionExterna?> BuscarPorNumero(string numeroOP)
        {
            return await _context.OrdenesProduccionExternas
                .FirstOrDefaultAsync(o => o.NumeroOP == numeroOP);
        }

        public async Task<List<OrdenProduccionExterna>> ImportarDesdeArchivo(Stream archivo, string nombreArchivo)
        {
            var resultado = new List<OrdenProduccionExterna>();
            using var workbook = new XLWorkbook(archivo);
            var hoja = workbook.Worksheet(1);
            var filaEncabezado = hoja.Row(1);
            // Busca la columna correcta según el texto del encabezado (sin importar el orden)
            int? colOP = null, colCliente = null, colCantidad = null, colReferencia = null;
            foreach (var celda in filaEncabezado.CellsUsed())
            {
                var texto = celda.GetString().Trim().ToLower();
                if (texto.Contains("op") || texto.Contains("orden")) colOP = celda.Address.ColumnNumber;
                else if (texto.Contains("cliente")) colCliente = celda.Address.ColumnNumber;
                else if (texto.Contains("cantidad")) colCantidad = celda.Address.ColumnNumber;
                else if (texto.Contains("referencia") || texto.Contains("producto")) colReferencia = celda.Address.ColumnNumber;
            }
            if (colOP == null)
                throw new InvalidOperationException("No se encontró una columna de 'Orden' o 'OP' en el archivo. Verifique los encabezados.");
            var filas = hoja.RowsUsed().Skip(1);
            foreach (var fila in filas)
            {
                var numeroOP = fila.Cell(colOP.Value).GetString().Trim();
                if (string.IsNullOrWhiteSpace(numeroOP)) continue;
                var cliente = colCliente.HasValue ? fila.Cell(colCliente.Value).GetString().Trim() : "";
                var referencia = colReferencia.HasValue ? fila.Cell(colReferencia.Value).GetString().Trim() : "";
                var cantidadTexto = colCantidad.HasValue ? fila.Cell(colCantidad.Value).GetString().Trim() : "0";
                if (!int.TryParse(cantidadTexto, out var cantidad)) cantidad = 0;
                var existente = await _context.OrdenesProduccionExternas
                    .FirstOrDefaultAsync(o => o.NumeroOP == numeroOP);
                if (existente != null)
                {
                    existente.Cliente = cliente;
                    existente.CantidadOP = cantidad;
                    existente.Referencia = referencia;
                    existente.FechaImportacion = DateTime.Now;
                }
                else
                {
                    existente = new OrdenProduccionExterna
                    {
                        NumeroOP = numeroOP,
                        Cliente = cliente,
                        CantidadOP = cantidad,
                        Referencia = referencia,
                        FechaImportacion = DateTime.Now
                    };
                    _context.OrdenesProduccionExternas.Add(existente);
                }
                resultado.Add(existente);
            }
            await _context.SaveChangesAsync();
            return resultado;
        }
    }
}