using Microsoft.EntityFrameworkCore;

namespace SistemaFlota.Services.Consecutivos
{
    public class ConsecutivoService : IConsecutivoService
    {
        private readonly AppDbContext _context;

        public ConsecutivoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerarAsync(string modulo)
        {
            var consecutivo = await _context.Consecutivos
                .FirstOrDefaultAsync(x => x.Modulo == modulo);

            if (consecutivo == null)
                throw new Exception(
                    $"No existe un consecutivo configurado para '{modulo}'.");

            consecutivo.UltimoNumero++;

            await _context.SaveChangesAsync();

            return $"{consecutivo.Prefijo}-{consecutivo.UltimoNumero
                .ToString()
                .PadLeft(consecutivo.Longitud, '0')}";
        }
    }
}