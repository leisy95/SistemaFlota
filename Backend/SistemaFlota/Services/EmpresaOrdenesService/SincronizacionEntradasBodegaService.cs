using Microsoft.EntityFrameworkCore;
using SistemaFlota.Services.InformacionKasoft.OrdenesProduccion;

namespace SistemaFlota
{
    public class SincronizacionEntradasBodegaService : BackgroundService
    {
        private static readonly TimeSpan HoraInicio = new(6, 0, 0);
        private static readonly TimeSpan HoraFin = new(19, 0, 0);
        private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(5);
        private static readonly DateTime FechaArranque = new(2026, 9, 1);
        private const int DiasCicloNormal = 2;
        private const int DiasRevisionDiaria = 60;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<SincronizacionEntradasBodegaService> _logger;
        private readonly TimeZoneInfo _zona = ObtenerZonaBogota();
        private DateTime _ultimaRevisionDiaria = DateTime.MinValue;

        public SincronizacionEntradasBodegaService(
            IServiceScopeFactory scopeFactory,
            IConfiguration config,
            ILogger<SincronizacionEntradasBodegaService> logger)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!(_config.GetValue<bool?>("SincronizacionFgc008:Habilitada") ?? true))
            {
                _logger.LogInformation("Sincronización de entradas de Bodega desactivada por configuración");
                return;
            }

            try
            {
                // Deja que la aplicación termine de arrancar y aplicar migraciones
                await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await CicloAsync(stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error en la sincronización de entradas de Bodega");
                    }

                    await Task.Delay(Intervalo, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // El servicio se está deteniendo
            }
        }

        private async Task CicloAsync(CancellationToken ct)
        {
            var ahora = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _zona);
            if (ahora.TimeOfDay < HoraInicio || ahora.TimeOfDay >= HoraFin) return;

            var revisionDiaria = _ultimaRevisionDiaria != ahora.Date;
            var ops = new HashSet<string>();

            using (var scope = _scopeFactory.CreateScope())
            {
                var entradas = scope.ServiceProvider.GetRequiredService<EntradasBodegaService>();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                if (revisionDiaria)
                {
                    // Primera vez: carga desde la fecha de arranque. Después: últimos 60 días
                    var hayFilas = await db.FormatosFGC008.AnyAsync(f => f.Origen == "Bodega", ct);
                    var desde = hayFilas ? ahora.Date.AddDays(-DiasRevisionDiaria) : FechaArranque;

                    foreach (var op in await entradas.ObtenerOpsConActividadAsync(desde))
                        ops.Add(op);

                    // También las OP que ya tienen filas, por si Bodega borró alguna entrada
                    var conFilas = await db.FormatosFGC008
                        .Where(f => f.Origen == "Bodega" && f.DiaEntrada >= desde)
                        .Select(f => f.OrdenProduccion)
                        .Distinct()
                        .ToListAsync(ct);
                    foreach (var op in conFilas) ops.Add(op);
                }
                else
                {
                    foreach (var op in await entradas.ObtenerOpsConActividadAsync(ahora.Date.AddDays(-DiasCicloNormal)))
                        ops.Add(op);
                }
            }

            int creadas = 0, actualizadas = 0, fallidas = 0;

            foreach (var op in ops)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    // Un alcance por OP: si una falla, no contamina a las demás
                    using var scopeOp = _scopeFactory.CreateScope();
                    var sync = scopeOp.ServiceProvider.GetRequiredService<SincronizacionFgc008Service>();
                    var r = await sync.SincronizarOpAsync(op);
                    creadas += r.FilasCreadas;
                    actualizadas += r.FilasActualizadas;
                }
                catch (Exception ex)
                {
                    fallidas++;
                    _logger.LogError(ex, "No se pudo sincronizar la OP {Op}", op);
                }
            }

            if (revisionDiaria) _ultimaRevisionDiaria = ahora.Date;

            if (creadas > 0 || actualizadas > 0 || fallidas > 0)
                _logger.LogInformation(
                    "Sincronización F-GC-008: {Ops} OP revisadas, {Creadas} filas creadas, {Actualizadas} actualizadas, {Fallidas} con error",
                    ops.Count, creadas, actualizadas, fallidas);
        }

        private static TimeZoneInfo ObtenerZonaBogota()
        {
            foreach (var id in new[] { "America/Bogota", "SA Pacific Standard Time" })
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }
            return TimeZoneInfo.CreateCustomTimeZone("Bogota", TimeSpan.FromHours(-5), "Bogota", "Bogota");
        }
    }
}