using Microsoft.EntityFrameworkCore;
using SistemaFlota.Models;

namespace SistemaFlota
{
    public class EmpresaOrdenesService : IProveedorOrdenesProduccion
    {
        private readonly AppDbContext _context;

        public EmpresaOrdenesService(AppDbContext context)
        {
            _context = context;
        }

        // BUSCAR ORDEN DE PRODUCCIÓN
        public async Task<OrdenProduccionExterna?> BuscarPorNumero(string numeroOP)
        {
            if (!int.TryParse(numeroOP, out int numero))
                return null;

            try
            {
                var data = await _context.VInvOrdenesProduccion
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Numop == numero);

                if (data == null)
                    return null;

                return new OrdenProduccionExterna
                {
                    NumeroOP = data.Numop.ToString(),
                    Cliente = data.Nombrecliente,
                    Referencia = data.Refer,
                    Descripcion = data.Descrip,
                    CantidadOP = Convert.ToInt32(data.Canprog),
                    Unidad = data.Um,
                    FechaImportacion = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("======================================");
                Console.WriteLine("ERROR CONSULTANDO OP");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("======================================");

                throw;
            }
        }

        // OBTENER CANTIDAD REAL PRODUCIDA
        public async Task<decimal> ObtenerCantidadRealAsync(string numeroOP)
        {
            if (!int.TryParse(numeroOP, out int numero))
                return 0;

            var cantidad = await _context.VInvRegistroProduccion
                .AsNoTracking()
                .Where(x => x.Numop == numero)
                .SumAsync(x => (decimal?)x.Kilos);

            return cantidad ?? 0;
        }

        // BUSCAR REFERENCIAS POR DESCRIPCIÓN
        public async Task<List<string>> BuscarReferenciasPorDescripcion(string texto)
        {
            return await _context.VInvReferencias
                .AsNoTracking()
                .Where(x => x.Descrip.Contains(texto))
                .Select(x => x.Codigo)
                .ToListAsync();
        }

        // IMPORTAR DESDE ARCHIVO
        public Task<List<OrdenProduccionExterna>> ImportarDesdeArchivo(
            Stream archivo,
            string nombreArchivo)
        {
            throw new NotSupportedException(
                "La importación desde archivo no aplica para la fuente de base de datos de la empresa.");
        }
    }
}