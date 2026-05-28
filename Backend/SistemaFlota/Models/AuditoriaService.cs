using Microsoft.EntityFrameworkCore;

namespace SistemaFlota
{
    public class AuditoriaService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditoriaService(
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task RegistrarAsync(
            string usuario,
            string rol,
            string accion,
            string modulo,
            string? detalle = null,
            int? registroId = null,
            string resultado = "Exitoso"
        )
        {
            try
            {
                var ip = _httpContextAccessor.HttpContext?
                    .Connection.RemoteIpAddress?.ToString() ?? "Desconocida";

                var auditoria = new Auditoria
                {
                    Usuario = usuario,
                    Rol = rol,
                    Accion = accion,
                    Modulo = modulo,
                    Detalle = detalle,
                    RegistroId = registroId,
                    IpAddress = ip,
                    Resultado = resultado,
                    Fecha = DateTime.Now
                };

                _context.Auditorias.Add(auditoria);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error auditoría: {ex.Message}");
            }
        }
    }
}