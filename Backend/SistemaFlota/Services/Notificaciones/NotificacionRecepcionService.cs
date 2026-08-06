using Microsoft.EntityFrameworkCore;
using SistemaFlota.Services.Email;
using SistemaFlota.Services.Pdf.RecepcionMercancia;

namespace SistemaFlota.Services.Notificaciones;

public class NotificacionRecepcionService : INotificacionRecepcionService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly EmailTemplateService _template;
    private readonly IRecepcionMercanciaPdfService _pdfService;


    public NotificacionRecepcionService(
        AppDbContext context,
        IEmailService emailService,
        EmailTemplateService template,
        IRecepcionMercanciaPdfService pdfService)
    {
        _context = context;
        _emailService = emailService;
        _template = template;
        _pdfService = pdfService;
    }


    public async Task EnviarRecepcionMercanciaAsync(int recepcionId, List<int> usuarios)
    {
        var recepcion = await _context.RecepcionesMercancias
            .Include(x => x.OrdenCompra)
                .ThenInclude(x => x.Proveedor)
            .FirstOrDefaultAsync(x => x.Id == recepcionId);

        if (recepcion == null)
            throw new Exception("Recepción no encontrada");

        var destinatarios = await _context.Usuarios
            .Where(u => usuarios.Contains(u.Id) &&
                        u.Activo &&
                        !string.IsNullOrWhiteSpace(u.Email))
            .ToListAsync();

        var pdf = await _pdfService.GenerarPdfAsync(recepcionId);

        var html = _template.RecepcionMercancia(
            recepcion.NumeroRecepcion,
            recepcion.OrdenCompra!.Proveedor.Nombre,
            recepcion.FechaRecepcion
        );

        foreach (var usuario in destinatarios)
        {
            await _emailService.EnviarAsync(
                usuario.Email!,
                $"Recepción mercancía {recepcion.NumeroRecepcion}",
                html,
                pdf,
                $"Recepcion_{recepcion.NumeroRecepcion}.pdf"
            );
        }
    }
}