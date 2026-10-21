
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.ControlEnvios.Trazabilidad;

namespace SistemaFlota.Services.ControlEnvios.Trazabilidad
{
    public class TrazabilidadService : ITrazabilidadService
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;

        public TrazabilidadService(
            AppDbContext context,
            AuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        // FILTROS COMPARTIDOS POR LISTADO Y RESUMEN

        private IQueryable<TrazabilidadFactura> AplicarFiltros(
            IQueryable<TrazabilidadFactura> query,
            string? buscar,
            string? estado,
            string? entregada,
            string? tipo,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var q = buscar.ToLower();

                query = query.Where(t =>
                    t.FacturaRemision.ToLower().Contains(q) ||
                    t.Cliente.ToLower().Contains(q) ||
                    t.Conductor.ToLower().Contains(q) ||
                    (t.Vehiculo != null && t.Vehiculo.ToLower().Contains(q)) ||
                    (t.Guia != null && t.Guia.ToLower().Contains(q)));
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(t => t.Estado == estado);
            }

            if (entregada == "si")
            {
                query = query.Where(t => t.FacturaEntregada);
            }
            else if (entregada == "no")
            {
                query = query.Where(t => !t.FacturaEntregada);
            }

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                var tipoUpper = tipo.ToUpper();

                var alternativo =
                    tipoUpper == "CT" ? "COT" :
                    tipoUpper == "RM" ? "RE" :
                    tipoUpper == "NCE" ? "NC" :
                    tipoUpper == "COT" ? "CT" :
                    tipoUpper == "RE" ? "RM" :
                    tipoUpper == "NC" ? "NCE" :
                    "";

                query = query.Where(t =>
                    t.FacturaRemision.ToUpper().StartsWith(tipoUpper) ||
                    (alternativo != "" &&
                     t.FacturaRemision.ToUpper().StartsWith(alternativo)));
            }

            // Fecha inicial: incluye todo el día seleccionado.
            if (fechaDesde.HasValue)
            {
                var desde = fechaDesde.Value.Date;
                query = query.Where(t => t.FechaRegistro >= desde);
            }

            // Fecha final: incluye todo el día seleccionado,
            // incluso los registros que tengan hora.
            if (fechaHasta.HasValue)
            {
                var hastaExclusiva = fechaHasta.Value.Date.AddDays(1);
                query = query.Where(t => t.FechaRegistro < hastaExclusiva);
            }

            return query;
        }

        // SINCRONIZAR COSTOS DE FLETES
        private async Task SincronizarCostoFleteAsync(
            int trazabilidadId,
            int? autorizacionId,
            decimal? valorFlete)
        {
            if (valorFlete == null || valorFlete <= 0)
                return;

            CosteFlete? existente;

            if (autorizacionId != null)
            {
                // Flota propia: buscar por autorización.
                existente = await _context.CostosFletes
                    .FirstOrDefaultAsync(
                        c => c.AutorizacionId == autorizacionId);
            }
            else
            {
                // Transportadora externa: buscar por trazabilidad.
                existente = await _context.CostosFletes
                    .FirstOrDefaultAsync(
                        c => c.TrazabilidadId == trazabilidadId);
            }

            if (existente != null)
            {
                existente.Total = valorFlete.Value;
            }
            else
            {
                _context.CostosFletes.Add(new CosteFlete
                {
                    AutorizacionId = autorizacionId,
                    TrazabilidadId = autorizacionId == null
                        ? trazabilidadId
                        : null,
                    FechaRegistro = DateTime.Now,
                    Peajes = 0,
                    Combustible = 0,
                    Parqueos = 0,
                    DescarguesMcia = 0,
                    CargueMateriales = 0,
                    Alimentacion = 0,
                    Hospedaje = 0,
                    Varios = 0,
                    Total = valorFlete.Value,
                    Estado = "Pendiente"
                });
            }

            await _context.SaveChangesAsync();
        }

        // OBTENER TRAZABILIDADES CON FILTROS Y PAGINACIÓN

        public async Task<TrazabilidadPaginadaDto> ObtenerAsync(
            int pagina,
            int porPagina,
            string? buscar,
            string? estado,
            string? entregada,
            string? tipo,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            var query = _context.TrazabilidadFacturas
                .Include(t => t.Autorizacion)
                    .ThenInclude(a => a != null ? a.Conductor : null)
                .Include(t => t.Autorizacion)
                    .ThenInclude(a => a != null ? a.Vehiculo : null)
                .AsQueryable();

            query = AplicarFiltros(
                query,
                buscar,
                estado,
                entregada,
                tipo,
                fechaDesde,
                fechaHasta);

            var total = await query.CountAsync();

            var lista = await query
                .OrderByDescending(t => t.FechaRegistro)
                .ThenByDescending(t => t.Id)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .ToListAsync();

            return new TrazabilidadPaginadaDto
            {
                Data = lista,
                Total = total,
                Pagina = pagina,
                PorPagina = porPagina,
                TotalPaginas = (int)Math.Ceiling(
                    (double)total / porPagina)
            };
        }

        // OBTENER RESUMEN

        public async Task<TrazabilidadResumenDto> ObtenerResumenAsync(
            string? buscar,
            string? estado,
            string? entregada,
            string? tipo,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            var query = _context.TrazabilidadFacturas.AsQueryable();

            query = AplicarFiltros(
                query,
                buscar,
                estado,
                entregada,
                tipo,
                fechaDesde,
                fechaHasta);

            var total = await query.CountAsync();

            var entregadas = await query.CountAsync(
                t => t.FacturaEntregada);

            var pendientesEntrega = total - entregadas;

            var totalFlete = await query.SumAsync(
                t => t.ValorFlete ?? 0);

            return new TrazabilidadResumenDto
            {
                Total = total,
                Entregadas = entregadas,
                PendientesEntrega = pendientesEntrega,
                TotalFlete = totalFlete
            };
        }


        // OBTENER POR ID
        public async Task<TrazabilidadFactura?> ObtenerPorIdAsync(int id)
        {
            return await _context.TrazabilidadFacturas
                .Include(t => t.Autorizacion)
                    .ThenInclude(a => a != null ? a.Conductor : null)
                .Include(t => t.Autorizacion)
                    .ThenInclude(a => a != null ? a.Vehiculo : null)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // CREAR TRAZABILIDAD
        public async Task<TrazabilidadFactura> CrearAsync(
            CrearTrazabilidadDto dto,
            string usuario,
            string rol)
        {
            var trazabilidad = new TrazabilidadFactura
            {
                AutorizacionId = dto.AutorizacionId,
                FacturaRemision = dto.FacturaRemision,
                Cliente = dto.Cliente,
                Conductor = dto.Conductor,
                Transportadora = dto.Transportadora,
                Guia = dto.Guia,
                Vehiculo = dto.Vehiculo,
                PesoKilos = dto.PesoKilos,
                ValorFlete = dto.ValorFlete,
                AjusteRecibido = dto.AjusteRecibido,
                FacturaEntregada = dto.FacturaEntregada,
                FechaEntrega = dto.FacturaEntregada
                    ? DateTime.Now
                    : null,
                Novedad = dto.Novedad,
                Estado = dto.Estado ?? "Pendiente",
                FechaRegistro = DateTime.Now
            };

            _context.TrazabilidadFacturas.Add(trazabilidad);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: usuario,
                rol: rol,
                accion: "Crear",
                modulo: "Trazabilidad",
                detalle:
                    $"Trazabilidad creada — Factura: {dto.FacturaRemision}, Cliente: {dto.Cliente}",
                registroId: trazabilidad.Id
            );

            await SincronizarCostoFleteAsync(
                trazabilidad.Id,
                trazabilidad.AutorizacionId,
                trazabilidad.ValorFlete);

            return trazabilidad;
        }

        // ACTUALIZAR TRAZABILIDAD
        public async Task<TrazabilidadFactura?> ActualizarAsync(
            int id,
            CrearTrazabilidadDto dto,
            string usuario,
            string rol)
        {
            var t = await _context.TrazabilidadFacturas.FindAsync(id);

            if (t == null)
                return null;

            t.FacturaRemision = dto.FacturaRemision;
            t.Cliente = dto.Cliente;
            t.Conductor = dto.Conductor;
            t.Transportadora = dto.Transportadora;
            t.Guia = dto.Guia;
            t.Vehiculo = dto.Vehiculo;
            t.PesoKilos = dto.PesoKilos;
            t.ValorFlete = dto.ValorFlete;
            t.AjusteRecibido = dto.AjusteRecibido;
            t.Novedad = dto.Novedad;
            t.Estado = dto.Estado ?? t.Estado;

            if (dto.FacturaEntregada && !t.FacturaEntregada)
                t.FechaEntrega = DateTime.Now;

            t.FacturaEntregada = dto.FacturaEntregada;

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: usuario,
                rol: rol,
                accion: "Editar",
                modulo: "Trazabilidad",
                detalle:
                    $"Trazabilidad #{id} editada — Factura: {dto.FacturaRemision}",
                registroId: id
            );

            await SincronizarCostoFleteAsync(
                t.Id, t.AutorizacionId, t.ValorFlete);

            return t;
        }

        // ELIMINAR TRAZABILIDAD
        public async Task<bool> EliminarAsync(
            int id,
            string usuario,
            string rol)
        {
            var t = await _context.TrazabilidadFacturas.FindAsync(id);

            if (t == null)
                return false;

            var notas = await _context.NotasTrazabilidad
                .Where(n => n.TrazabilidadId == id)
                .ToListAsync();

            _context.NotasTrazabilidad.RemoveRange(notas);
            _context.TrazabilidadFacturas.Remove(t);

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: usuario,
                rol: rol,
                accion: "Eliminar",
                modulo: "Trazabilidad",
                detalle: $"Trazabilidad #{id} eliminada",
                registroId: id
            );

            return true;
        }

        // OBTENER NOTAS
        public async Task<List<NotaTrazabilidad>> ObtenerNotasAsync(int id)
        {
            return await _context.NotasTrazabilidad
                .Where(n => n.TrazabilidadId == id)
                .OrderByDescending(n => n.Fecha)
                .ToListAsync();
        }

        // AGREGAR NOTA Y CREAR SU REGISTRO NCE
        public async Task<NotaTrazabilidad?> AgregarNotaAsync(
            int id,
            CrearNotaDto dto,
            string usuario,
            string rol)
        {
            var trazabilidad = await _context.TrazabilidadFacturas
                .FindAsync(id);

            if (trazabilidad == null)
                return null;

            var nota = new NotaTrazabilidad
            {
                TrazabilidadId = id,
                NumeroNota = dto.NumeroNota,
                Cliente = dto.Cliente,
                Conductor = dto.Conductor,
                FacturaEntregada = dto.FacturaEntregada,
                Observacion = dto.Observacion,
                Fecha = DateTime.Now
            };

            _context.NotasTrazabilidad.Add(nota);

            var registroNCE = new TrazabilidadFactura
            {
                FechaRegistro = DateTime.Now,
                AutorizacionId = trazabilidad.AutorizacionId,
                FacturaRemision = "NCE-" +
                    dto.NumeroNota
                        .Replace("NCE ", "")
                        .Replace("NCE-", "")
                        .Trim(),
                Cliente = dto.Cliente ?? trazabilidad.Cliente,
                Conductor = dto.Conductor ?? trazabilidad.Conductor,
                Transportadora = trazabilidad.Transportadora,
                Vehiculo = trazabilidad.Vehiculo,
                Estado = "Pendiente",
                AjusteRecibido = false,
                FacturaEntregada = dto.FacturaEntregada,
                Novedad = dto.Observacion
            };

            _context.TrazabilidadFacturas.Add(registroNCE);

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: usuario,
                rol: rol,
                accion: "Crear",
                modulo: "Trazabilidad",
                detalle:
                    $"Nota #{dto.NumeroNota} agregada a trazabilidad #{id}",
                registroId: id
            );

            return nota;
        }

        // EDITAR NOTA
        public async Task<NotaTrazabilidad?> EditarNotaAsync(
            int notaId,
            CrearNotaDto dto)
        {
            var nota = await _context.NotasTrazabilidad
                .FindAsync(notaId);

            if (nota == null)
                return null;

            nota.NumeroNota = dto.NumeroNota;
            nota.Cliente = dto.Cliente;
            nota.Conductor = dto.Conductor;
            nota.FacturaEntregada = dto.FacturaEntregada;
            nota.Observacion = dto.Observacion;

            await _context.SaveChangesAsync();

            return nota;
        }

        // ELIMINAR NOTA
        public async Task<bool> EliminarNotaAsync(int notaId)
        {
            var nota = await _context.NotasTrazabilidad.FindAsync(notaId);

            if (nota == null)
                return false;

            _context.NotasTrazabilidad.Remove(nota);
            await _context.SaveChangesAsync();

            return true;
        }

        // AUTORIZACIONES DISPONIBLES PARA IMPORTAR
        public async Task<List<AutorizacionDisponibleDto>>
            ObtenerAutorizacionesDisponiblesAsync()
        {
            return await _context.Autorizaciones
                .Include(a => a.Conductor)
                .Include(a => a.Vehiculo)
                .Where(a =>
                    a.Estado == "Bodega" ||
                    a.Estado == "Porteria" ||
                    a.Estado == "Autorizado")
                .OrderByDescending(a => a.FechaCreacion)
                .Select(a => new AutorizacionDisponibleDto
                {
                    Id = a.Id,
                    FechaCreacion = a.FechaCreacion,
                    TipoVuelta = a.TipoVuelta,
                    DestinoCompleto = a.DestinoCompleto,
                    NumeroGuia = a.NumeroGuia,
                    FacturasClientes = a.FacturasClientes,
                    Conductor = a.Conductor != null
                        ? a.Conductor.Nombre
                        : "-",
                    Vehiculo = a.Vehiculo != null
                        ? a.Vehiculo.Placa
                        : "-"
                })
                .ToListAsync();
        }
    }
}
