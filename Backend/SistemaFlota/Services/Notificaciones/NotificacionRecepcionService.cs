using Microsoft.EntityFrameworkCore;
using SistemaFlota.Services.ComprasNoFormalizadas.RecepcionMercancia;
using SistemaFlota.Services.Email;
using SistemaFlota.Services.Pdf.RecepcionMercancia;

namespace SistemaFlota.Services.Notificaciones;

public class NotificacionRecepcionService : INotificacionRecepcionService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly EmailTemplateService _template;
    private readonly IRecepcionMercanciaPdfService _pdfService;
    private readonly IRecepcionMercanciaNoFormalizadaPdfService _pdfNoFormalizadaService;


    public NotificacionRecepcionService(
        AppDbContext context,
        IEmailService emailService,
        EmailTemplateService template,
        IRecepcionMercanciaPdfService pdfService,
        IRecepcionMercanciaNoFormalizadaPdfService pdfNoFormalizadaService)
    {
        _context = context;
        _emailService = emailService;
        _template = template;
        _pdfService = pdfService;
        _pdfNoFormalizadaService = pdfNoFormalizadaService;
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

    public async Task EnviarRecepcionMercanciaNoFormalizadaAsync(
        int recepcionId,
        List<int> usuarios)
    {
        var recepcion =
            await _context.RecepcionesMercanciasNoFormalizadas
                .Include(x => x.OrdenCompraNoFormalizada)
                    .ThenInclude(x => x.ProveedorNoFormalizado)
                .FirstOrDefaultAsync(x => x.Id == recepcionId);

        if (recepcion == null)
            throw new Exception("Recepción no formalizada no encontrada");

        var destinatarios = await _context.Usuarios
            .Where(u => usuarios.Contains(u.Id) &&
                        u.Activo &&
                        !string.IsNullOrWhiteSpace(u.Email))
            .ToListAsync();

        var pdf = await _pdfNoFormalizadaService
            .GenerarPdfAsync(recepcionId);

        var html = _template.RecepcionMercancia(
            recepcion.NumeroRecepcion,
            recepcion.OrdenCompraNoFormalizada!.ProveedorNoFormalizado!.Nombre,
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