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


    public async Task EnviarRecepcionMercanciaAsync(int recepcionId)
    {
        var recepcion = await _context.RecepcionesMercancia
            .Include(x => x.OrdenCompra)
                .ThenInclude(x => x.Proveedor)
            .FirstOrDefaultAsync(x => x.Id == recepcionId);


        if (recepcion == null)
            throw new Exception("Recepción no encontrada");


        var correo = recepcion
            .OrdenCompra?
            .Proveedor?
            .CorreoElectronico;


        if (string.IsNullOrWhiteSpace(correo))
            return;


        var pdf = await _pdfService.GenerarPdfAsync(recepcionId);


        var html = _template.RecepcionMercancia(
            recepcion.NumeroRecepcion,
            recepcion.OrdenCompra.Proveedor.Nombre,
            recepcion.FechaRecepcion
        );


        await _emailService.EnviarAsync(
            correo,
            $"Recepción mercancía {recepcion.NumeroRecepcion}",
            html,
            pdf,
            $"Recepcion_{recepcion.NumeroRecepcion}.pdf"
        );
    }
}