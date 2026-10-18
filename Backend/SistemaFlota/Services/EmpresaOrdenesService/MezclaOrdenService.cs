using MySqlConnector;

namespace SistemaFlota
{
    public class MezclaOrdenItem
    {
        public string Proceso { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public string Unidad { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; }
        public string? Observacion { get; set; }
    }

    public class MezclaOrdenService
    {
        private readonly string _connectionString;

        public MezclaOrdenService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("EmpresaConnection")
                ?? throw new InvalidOperationException("No se encontró 'EmpresaConnection' en la configuración.");
        }

        public async Task<List<MezclaOrdenItem>> ObtenerAsync(string numeroOp)
        {
            var lista = new List<MezclaOrdenItem>();

            // Las OP de Kasoft son numéricas. Con texto, MySQL lo compararía como 0 y podría traer filas que no son
            var op = numeroOp?.Trim() ?? "";
            if (op.Length == 0 || !op.All(char.IsDigit)) return lista;

            const string sql = @"
                SELECT proceso, refer, descrip, cant, unm, porc, observ
                FROM v_inv_mezclapororden
                WHERE numop = @op
                ORDER BY proceso, porc DESC";

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@op", op);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new MezclaOrdenItem
                {
                    Proceso = reader["proceso"]?.ToString() ?? "",
                    Referencia = reader["refer"]?.ToString() ?? "",
                    Descripcion = reader["descrip"]?.ToString() ?? "",
                    Cantidad = reader["cant"] != DBNull.Value ? Convert.ToDecimal(reader["cant"]) : 0,
                    Unidad = reader["unm"]?.ToString() ?? "",
                    Porcentaje = reader["porc"] != DBNull.Value ? Convert.ToDecimal(reader["porc"]) : 0,
                    Observacion = reader["observ"] == DBNull.Value ? null : reader["observ"]?.ToString()
                });
            }
            return lista;
        }
    }
}