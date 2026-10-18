using Microsoft.EntityFrameworkCore;

namespace SistemaFlota
{
    public class ResultadoSincronizacionFgc008
    {
        public string OrdenProduccion { get; set; } = string.Empty;
        public int EntradasLeidas { get; set; }
        public int FilasCreadas { get; set; }
        public int FilasActualizadas { get; set; }
    }

    public class SincronizacionFgc008Service
    {
        // Evita que el proceso automático y el botón escriban la misma OP al mismo tiempo
        private static readonly SemaphoreSlim _candado = new(1, 1);

        private readonly AppDbContext _context;
        private readonly EmpresaOrdenesService _empresa;
        private readonly EntradasBodegaService _entradas;

        public SincronizacionFgc008Service(
            AppDbContext context,
            EmpresaOrdenesService empresa,
            EntradasBodegaService entradas)
        {
            _context = context;
            _empresa = empresa;
            _entradas = entradas;
        }

        // Solo se manejan kilos y unidades: K = kilos; cualquier otra unidad de (ML, UN) = unidades
        private static string UnidadDeConteo(string? unidadOrden, string seccion)
        {
            var u = unidadOrden?.Trim().ToUpperInvariant();
            if (!string.IsNullOrEmpty(u))
                return u.StartsWith("K") ? "K" : "UN";

            // Si la orden no trae unidad, se decide por la sección
            return seccion is "SELLADO" or "DOBLADO" ? "UN" : "K";
        }

        private static string? NormalizarUnidad(string? unidad)
        {
            var u = unidad?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(u)) return null;
            return u.Length > 10 ? u[..10] : u;
        }

        public async Task<ResultadoSincronizacionFgc008> SincronizarOpAsync(string op)
        {
            await _candado.WaitAsync();
            try
            {
                return await SincronizarInternoAsync(op);
            }
            finally
            {
                _candado.Release();
            }
        }

        private async Task<ResultadoSincronizacionFgc008> SincronizarInternoAsync(string op)
        {
            var resultado = new ResultadoSincronizacionFgc008 { OrdenProduccion = op };

            var entradas = await _entradas.ObtenerEntradasDeOpAsync(op);
            resultado.EntradasLeidas = entradas.Count;
            if (entradas.Count == 0) return resultado;

            var orden = await _empresa.BuscarPorNumero(op);
            var ahora = DateTime.Now;

            var grupos = entradas
                .GroupBy(e => new
                {
                    Seccion = e.Seccion.Trim().ToUpperInvariant(),
                    Dia = e.Primera.Date
                })
                .ToList();

            var filas = await _context.FormatosFGC008
                .Where(f => f.OrdenProduccion == op && f.Origen == "Bodega")
                .ToListAsync();

            var vistos = new HashSet<string>();

            foreach (var g in grupos)
            {
                var seccion = g.Key.Seccion;
                var dia = g.Key.Dia;
                vistos.Add($"{seccion}|{dia:yyyyMMdd}");

                var unidad = UnidadDeConteo(orden?.Unidad, seccion);
                var bultos = g.Sum(e => e.Bultos);
                var unidades = Math.Round(g.Sum(e => e.SumaUnidades), 2);
                var kilos = Math.Round(g.Sum(e => e.SumaKilos), 2);
                var real = unidad == "UN" ? unidades : kilos;

                var operarios = string.Join(", ",
                    g.SelectMany(e => e.Operarios).Distinct(StringComparer.OrdinalIgnoreCase));
                if (operarios.Length > 300) operarios = operarios[..300];

                var troqueles = g.Select(e => e.Troquel)
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Distinct()
                    .ToList();
                var troquel = troqueles.Count == 0 ? null
                            : troqueles.Count == 1 ? troqueles[0]
                            : "MIXTO";

                var fila = filas.FirstOrDefault(f =>
                    f.Seccion == seccion
                    && f.DiaEntrada.HasValue
                    && f.DiaEntrada.Value.Date == dia);

                if (fila == null)
                {
                    fila = new FormatoFGC008
                    {
                        OrdenProduccion = op,
                        Origen = "Bodega",
                        Seccion = seccion,
                        DiaEntrada = dia,
                        Fecha = g.Min(e => e.Primera)
                    };
                    _context.FormatosFGC008.Add(fila);
                    filas.Add(fila);
                    resultado.FilasCreadas++;
                }
                else
                {
                    var cambio = fila.Bultos != bultos
                              || fila.UnidadesEntrada != unidades
                              || fila.KilosEntrada != kilos;

                    if (cambio)
                    {
                        resultado.FilasActualizadas++;
                        // Si una persona ya había revisado esta fila, queda marcada para revisarla otra vez
                        if (!string.IsNullOrEmpty(fila.RevisadoPor))
                            fila.CambioDespuesDeRevisar = true;
                    }
                }

                if (orden != null)
                {
                    fila.Cliente = orden.Cliente ?? fila.Cliente;
                    fila.Referencia = orden.Referencia ?? fila.Referencia;
                    fila.CantidadOP = orden.CantidadOP;
                    fila.UnidadOP = NormalizarUnidad(orden.Unidad);
                }

                fila.UnidadReal = unidad;
                fila.CantidadReal = real;
                fila.Bultos = bultos;
                fila.UnidadesEntrada = unidades;
                fila.KilosEntrada = kilos;
                fila.OperariosBodega = operarios;
                fila.Troquel = troquel;
                fila.UltimoNumEntrada = g.Max(e => e.NumEntrada);
                fila.EnProceso = g.Any(e => !e.Completa);
                fila.UltimaSincronizacion = ahora;
            }

            // Si Bodega borró o movió una entrada, la fila de ese día se pone en cero.
            // No se elimina, para no perder verificaciones ya hechas.
            foreach (var fila in filas.Where(f =>
                !vistos.Contains($"{f.Seccion}|{(f.DiaEntrada ?? DateTime.MinValue):yyyyMMdd}")))
            {
                if (fila.Bultos == 0 && fila.CantidadReal == 0) continue;

                fila.Bultos = 0;
                fila.UnidadesEntrada = 0;
                fila.KilosEntrada = 0;
                fila.CantidadReal = 0;
                fila.EnProceso = false;
                fila.UltimaSincronizacion = ahora;
                resultado.FilasActualizadas++;

                if (!string.IsNullOrEmpty(fila.RevisadoPor))
                    fila.CambioDespuesDeRevisar = true;
            }

            await _context.SaveChangesAsync();
            return resultado;
        }
    }
}