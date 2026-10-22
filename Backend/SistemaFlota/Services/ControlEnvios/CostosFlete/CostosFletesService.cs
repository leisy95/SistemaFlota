using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.ControlEnvios.CostosFletes;
using System.Globalization;

namespace SistemaFlota.Services.ControlEnvios.CostosFlete
{
    public class CostosFletesService : ICostosFletesService
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;

        public CostosFletesService(
            AppDbContext context,
            AuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }


        public async Task<List<CosteFlete>> ObtenerAsync(
            string? desde,
            string? hasta,
            string? conductor,
            string? estado,
            string? ciudad,
            string? placa)
        {
            var query = _context.CostosFletes
                .AsNoTracking()
                .Include(c => c.Autorizacion)
                    .ThenInclude(a => a!.Conductor)
                .Include(c => c.Autorizacion)
                    .ThenInclude(a => a!.Vehiculo)
                .Include(c => c.Trazabilidad)
                .AsQueryable();

            // Fecha inicial: incluye todo desde las 00:00.
            if (!string.IsNullOrWhiteSpace(desde))
            {
                if (!DateTime.TryParseExact(
                        desde,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var fechaDesde))
                {
                    throw new ArgumentException(
                        "La fecha inicial debe tener formato yyyy-MM-dd.");
                }

                fechaDesde = DateTime.SpecifyKind(
                    fechaDesde.Date, DateTimeKind.Unspecified);

                query = query.Where(
                    c => c.FechaRegistro >= fechaDesde);
            }

            // Fecha final: excluye la medianoche del día siguiente.
            if (!string.IsNullOrWhiteSpace(hasta))
            {
                if (!DateTime.TryParseExact(
                        hasta,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var fechaHasta))
                {
                    throw new ArgumentException(
                        "La fecha final debe tener formato yyyy-MM-dd.");
                }

                var limiteExclusivo = DateTime.SpecifyKind(
                    fechaHasta.Date.AddDays(1),
                    DateTimeKind.Unspecified);

                query = query.Where(
                    c => c.FechaRegistro < limiteExclusivo);
            }

            if (!string.IsNullOrWhiteSpace(conductor))
            {
                conductor = conductor.Trim();

                query = query.Where(c =>
                    (c.Autorizacion != null &&
                     c.Autorizacion.Conductor != null &&
                     c.Autorizacion.Conductor.Nombre.Contains(conductor))
                    ||
                    (c.Trazabilidad != null &&
                     c.Trazabilidad.Conductor.Contains(conductor)));
            }

            if (!string.IsNullOrWhiteSpace(placa))
            {
                placa = placa.Trim();

                query = query.Where(c =>
                    (c.Autorizacion != null &&
                     c.Autorizacion.Vehiculo != null &&
                     c.Autorizacion.Vehiculo.Placa.Contains(placa))
                    ||
                    (c.Trazabilidad != null &&
                     c.Trazabilidad.Vehiculo != null &&
                     c.Trazabilidad.Vehiculo.Contains(placa)));
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                estado = estado.Trim();
                query = query.Where(c => c.Estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(ciudad))
            {
                ciudad = ciudad.Trim();

                query = query.Where(c =>
                    c.Autorizacion != null &&
                    c.Autorizacion.DestinoCompleto.Contains(ciudad));
            }

            return await query
                .OrderByDescending(c => c.FechaRegistro)
                .ThenByDescending(c => c.Id)
                .ToListAsync();
        }


        public async Task<CosteFlete> CrearAsync(
            CostoFleteDto dto,
            string usuario,
            string rol)
        {
            var registro = new CosteFlete
            {
                AutorizacionId = dto.AutorizacionId,
                FechaRegistro = DateTime.Now,
                Peajes = dto.Peajes,
                Combustible = dto.Combustible,
                Parqueos = dto.Parqueos,
                DescarguesMcia = dto.DescarguesMcia,
                CargueMateriales = dto.CargueMateriales,
                Alimentacion = dto.Alimentacion,
                Hospedaje = dto.Hospedaje,
                Varios = dto.Varios,
                Total = dto.Total,
                Observaciones = dto.Observaciones,
                VariosDetalle = dto.VariosDetalle,
                Estado = "Pendiente"
            };

            _context.CostosFletes.Add(registro);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: usuario,
                rol: rol,
                accion: "Crear",
                modulo: "CostosFletes",
                detalle:
                    $"Costo flete creado para autorización #{dto.AutorizacionId}",
                registroId: registro.Id
            );

            return registro;
        }

        public async Task<CosteFlete?> EditarAsync(
            int id,
            CostoFleteDto dto)
        {
            var registro = await _context.CostosFletes.FindAsync(id);

            if (registro == null)
                return null;

            registro.Peajes = dto.Peajes;
            registro.Combustible = dto.Combustible;
            registro.Parqueos = dto.Parqueos;
            registro.DescarguesMcia = dto.DescarguesMcia;
            registro.CargueMateriales = dto.CargueMateriales;
            registro.Alimentacion = dto.Alimentacion;
            registro.Hospedaje = dto.Hospedaje;
            registro.Varios = dto.Varios;
            registro.Total = dto.Total;
            registro.VariosDetalle = dto.VariosDetalle;
            registro.Observaciones = dto.Observaciones;

            await _context.SaveChangesAsync();

            return registro;
        }

        public async Task<CosteFlete?> VerificarAsync(
            int id,
            VerificarDto dto,
            string usuario,
            string rol)
        {
            var registro = await _context.CostosFletes.FindAsync(id);

            if (registro == null)
                return null;

            registro.Estado = "Verificado";
            registro.VerificadoPor = dto.VerificadoPor;
            registro.FirmaVerificacion = dto.FirmaVerificacion;
            registro.FechaVerificacion = DateTime.Now;

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: usuario,
                rol: rol,
                accion: "Verificar",
                modulo: "CostosFletes",
                detalle: $"Flete #{id} verificado por {dto.VerificadoPor}",
                registroId: id
            );

            return registro;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var registro = await _context.CostosFletes.FindAsync(id);

            if (registro == null)
                return false;

            _context.CostosFletes.Remove(registro);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
