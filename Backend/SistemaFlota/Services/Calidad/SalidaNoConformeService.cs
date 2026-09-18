using SistemaFlota.DTOs;
using SistemaFlota.Models.Calidad;
using SistemaFlota.Repositories.Calidad;

namespace SistemaFlota.Services.Calidad
{
    public class SalidaNoConformeService : ISalidaNoConformeService
    {
        private readonly ISalidaNoConformeRepository _repository;

        public SalidaNoConformeService(ISalidaNoConformeRepository repository)
        {
            _repository = repository;
        }

        public Task<List<SalidaNoConforme>> ListarAsync(DateTime? desde, DateTime? hasta, string? referencia, string? material, string? ordenProduccion, string? tipoDefecto, string? proceso)
            => _repository.ObtenerTodosAsync(desde, hasta, referencia, material, ordenProduccion, tipoDefecto, proceso);

        public Task<SalidaNoConforme?> ObtenerAsync(int id)
            => _repository.ObtenerPorIdAsync(id);

        public async Task<SalidaNoConforme> CrearAsync(CrearSalidaNoConformeDto dto, string usuario, IFormFile? evidenciaPdf)
        {
            string? nombrePdf = await GuardarArchivo(evidenciaPdf, "wwwroot/snc");

            var entidad = new SalidaNoConforme
            {
                HoraReporte = DateTime.Now.ToString("HH:mm"),
                OrdenProduccion = dto.OrdenProduccion,
                Referencia = dto.Referencia,
                CantidadKg = dto.CantidadKg,
                Cliente = dto.Cliente,
                Linea = dto.Linea,
                Material = dto.Material,
                Proceso = dto.Proceso,
                DescripcionSalida = dto.DescripcionSalida,
                TipoDefecto = dto.TipoDefecto,
                Impacto = dto.Impacto,
                CausaRaiz = dto.CausaRaiz,
                CantidadReportadaKg = dto.CantidadReportadaKg,
                FirmaReporta = dto.FirmaReporta,
                UsuarioReporta = usuario,
                EvidenciaPdf = nombrePdf,
                NombreReporta = dto.NombreReporta,
                Estado = "Reportado"
            };

            await _repository.AgregarAsync(entidad);
            await _repository.GuardarCambiosAsync();
            return entidad;
        }

        public async Task<SalidaNoConforme?> RegistrarTratamientoAsync(int id, TratamientoSncDto dto, string usuario)
        {
            var entidad = await _repository.ObtenerPorIdAsync(id);
            if (entidad == null) return null;

            entidad.TratamientoAdoptado = dto.TratamientoAdoptado;
            entidad.DescripcionTratamiento = dto.DescripcionTratamiento;
            entidad.FechaTratamiento = dto.FechaTratamiento ?? DateTime.Now;
            entidad.FirmaTratamiento = dto.FirmaTratamiento;
            entidad.UsuarioTratamiento = usuario;
            entidad.Estado = "Tratamiento";

            await _repository.GuardarCambiosAsync();
            return entidad;
        }

        public async Task<SalidaNoConforme?> CerrarAsync(int id, VerificacionSncDto dto)
        {
            var entidad = await _repository.ObtenerPorIdAsync(id);
            if (entidad == null) return null;

            entidad.VerificacionCumplimiento = dto.VerificacionCumplimiento;
            entidad.FechaVerificacion = DateTime.Now;
            entidad.RequiereInformacionCliente = dto.RequiereInformacionCliente;
            entidad.MotivoInformacionCliente = dto.MotivoInformacionCliente;
            entidad.AceptacionBajoConcesion = dto.AceptacionBajoConcesion;
            entidad.DetalleAceptacionConcesion = dto.DetalleAceptacionConcesion;
            entidad.FirmaVerificacion = dto.FirmaVerificacion;
            entidad.RevisadoPor = dto.RevisadoPor;
            entidad.Estado = "Cerrado";

            await _repository.GuardarCambiosAsync();
            return entidad;
        }

        public async Task<bool> EliminarAsync(int id, bool esAdmin)
        {
            var entidad = await _repository.ObtenerPorIdAsync(id);
            if (entidad == null) return false;

            // Regla de negocio: una vez reportado, solo Admin puede eliminar/modificar
            if (entidad.Estado != "Reportado" && !esAdmin) return false;

            _repository.Eliminar(entidad);
            await _repository.GuardarCambiosAsync();
            return true;
        }

        public decimal SumarKgDelMes(List<SalidaNoConforme> registros, int mes, int anio)
        {
            return registros
                .Where(r => r.FechaReporte.Month == mes && r.FechaReporte.Year == anio)
                .Sum(r => r.CantidadReportadaKg ?? 0);
        }

        public async Task<List<SalidaNoConformeEvidencia>> SubirEvidenciasAsync(int salidaNoConformeId, string paso, List<IFormFile> fotos)
        {
            var evidencias = new List<SalidaNoConformeEvidencia>();

            foreach (var foto in fotos.Take(5))
            {
                var nombre = await GuardarArchivo(foto, "wwwroot/snc");
                if (nombre != null)
                {
                    evidencias.Add(new SalidaNoConformeEvidencia
                    {
                        SalidaNoConformeId = salidaNoConformeId,
                        Paso = paso,
                        NombreArchivo = nombre,
                        TipoArchivo = "Imagen"
                    });
                }
            }

            await _repository.AgregarEvidenciasAsync(evidencias);
            return evidencias;
        }

        public Task<List<SalidaNoConformeEvidencia>> ObtenerEvidenciasAsync(int salidaNoConformeId)
            => _repository.ObtenerEvidenciasAsync(salidaNoConformeId);

        private async Task<string?> GuardarArchivo(IFormFile? archivo, string carpetaRelativa)
        {
            if (archivo == null) return null;

            var carpeta = Path.Combine(Directory.GetCurrentDirectory(), carpetaRelativa);
            if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

            var nombre = Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName);
            using var stream = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create);
            await archivo.CopyToAsync(stream);

            return nombre;
        }
    }
}