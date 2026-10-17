using MySqlConnector;

namespace SistemaFlota
{
    public class EntradaBodegaResumen
    {
        public int NumEntrada { get; set; }
        public string Seccion { get; set; } = string.Empty;
        public DateTime Primera { get; set; }
        public DateTime Ultima { get; set; }
        public int Bultos { get; set; }
        public decimal SumaUnidades { get; set; }
        public decimal TotalUnidades { get; set; }
        public decimal SumaKilos { get; set; }
        public decimal TotalKilos { get; set; }
        public List<string> Operarios { get; set; } = new();
        public string? Troquel { get; set; }   // SI | NO | MIXTO | null

        public bool Completa =>
            Bultos > 0
            && Math.Abs(SumaUnidades - TotalUnidades) <= 0.01m
            && Math.Abs(SumaKilos - TotalKilos) <= 0.05m;
    }

    public class EntradasBodegaService
    {
        private readonly string _connectionString;

        public EntradasBodegaService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("EmpresaConnection")
                ?? throw new InvalidOperationException("No se encontró 'EmpresaConnection' en la configuración.");
        }

        // OP con al menos un bulto registrado desde la fecha indicada
        public async Task<List<string>> ObtenerOpsConActividadAsync(DateTime desde)
        {
            const string sql = @"
                SELECT DISTINCT numero_op
                FROM v_inv_entradas
                WHERE fechahora >= @desde AND seccion IS NOT NULL";

            var ops = new List<string>();

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@desde", desde);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var op = reader.GetValue(0)?.ToString();
                if (!string.IsNullOrWhiteSpace(op)) ops.Add(op);
            }
            return ops;
        }

        // Todas las entradas de una OP (de cualquier fecha), una por num_entrada y sección
        public async Task<List<EntradaBodegaResumen>> ObtenerEntradasDeOpAsync(string numeroOp)
        {
            const string sql = @"
                SELECT num_entrada,
                       seccion,
                       MIN(fechahora) AS primera,
                       MAX(fechahora) AS ultima,
                       COUNT(*) AS bultos,
                       COALESCE(SUM(unidades), 0) AS suma_unidades,
                       COALESCE(MAX(total_unidades), 0) AS total_unidades,
                       COALESCE(SUM(peso), 0) AS suma_kilos,
                       COALESCE(MAX(total_peso), 0) AS total_kilos,
                       GROUP_CONCAT(DISTINCT operario SEPARATOR '|') AS operarios,
                       MIN(troquel) AS troquel_min,
                       MAX(troquel) AS troquel_max
                FROM v_inv_entradas
                WHERE numero_op = @op AND seccion IS NOT NULL
                GROUP BY num_entrada, seccion
                ORDER BY MIN(fechahora)";

            var lista = new List<EntradaBodegaResumen>();

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@op", numeroOp);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var operarios = (reader["operarios"]?.ToString() ?? "")
                    .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var troqueles = new[]
                {
                    reader["troquel_min"]?.ToString()?.Trim().ToUpperInvariant(),
                    reader["troquel_max"]?.ToString()?.Trim().ToUpperInvariant()
                }
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .ToList();

                lista.Add(new EntradaBodegaResumen
                {
                    NumEntrada = Convert.ToInt32(reader["num_entrada"]),
                    Seccion = reader["seccion"]?.ToString() ?? "",
                    Primera = Convert.ToDateTime(reader["primera"]),
                    Ultima = Convert.ToDateTime(reader["ultima"]),
                    Bultos = Convert.ToInt32(reader["bultos"]),
                    SumaUnidades = Convert.ToDecimal(reader["suma_unidades"]),
                    TotalUnidades = Convert.ToDecimal(reader["total_unidades"]),
                    SumaKilos = Convert.ToDecimal(reader["suma_kilos"]),
                    TotalKilos = Convert.ToDecimal(reader["total_kilos"]),
                    Operarios = operarios,
                    Troquel = troqueles.Count == 0 ? null
                            : troqueles.Count == 1 ? troqueles[0]
                            : "MIXTO"
                });
            }
            return lista;
        }
    }
}