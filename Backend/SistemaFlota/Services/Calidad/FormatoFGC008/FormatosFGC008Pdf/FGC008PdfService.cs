using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using FormatoFGC008Model = SistemaFlota.Models.Calidad.FormatoFGC008.FormatoFGC008;

namespace SistemaFlota.Services.Calidad.FormatoFGC008.FormatosFGC008Pdf;

public class FGC008PdfService : IFGC008PdfService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IFormatoFGC008Service _formatoService;

    public FGC008PdfService(
        AppDbContext context,
        IWebHostEnvironment environment,
        IFormatoFGC008Service formatoService)
    {
        _context = context;
        _environment = environment;
        _formatoService = formatoService;
    }

    public async Task<byte[]> GenerarPdfAsync(
        DateTime? desde,
        DateTime? hasta,
        string? op)
    {
        // Obtener todos los registros con los filtros solicitados.
        // Este método también carga las fotos y realiza los cálculos existentes.
        List<FormatoFGC008Model> registros =
            await _formatoService.ObtenerTodosAsync(desde, hasta, op);

        if (registros.Count == 0)
        {
            throw new InvalidOperationException(
                "No existen registros para generar el PDF.");
        }

        // Obtener configuración de la empresa.
        var empresa = await _context.ConfiguracionEmpresa
            .FirstOrDefaultAsync();

        if (empresa == null)
        {
            throw new InvalidOperationException(
                "No existe la configuración de la empresa.");
        }

        // Construir documento.
        var documento = new FGC008Documento(
            registros,
            empresa,
            ObtenerLogo(empresa.Logo),
            ObtenerMarcaAgua()
        );

        return Document
            .Create(documento.Compose)
            .GeneratePdf();
    }

    private string ObtenerMarcaAgua()
    {
        return Path.Combine(
            _environment.ContentRootPath,
            "wwwroot",
            "config",
            "iguana3.png"
        );
    }

    private string ObtenerLogo(string? logo)
    {
        return Path.Combine(
            _environment.ContentRootPath,
            "wwwroot",
            "config",
            "logo.png"
        );
    }
}