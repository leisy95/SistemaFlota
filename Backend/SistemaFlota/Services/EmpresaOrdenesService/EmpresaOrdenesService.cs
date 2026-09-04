using MySqlConnector;
using SistemaFlota.Models;
namespace SistemaFlota
{
    public class EmpresaOrdenesService : IProveedorOrdenesProduccion
    {
        private readonly string _connectionString;
        public EmpresaOrdenesService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("EmpresaConnection")
                ?? throw new InvalidOperationException("No se encontró 'EmpresaConnection' en la configuración.");
        }
        public async Task<OrdenProduccionExterna?> BuscarPorNumero(string numeroOP)
        {
            const string sql = @"
                SELECT numop, nombrecliente, refer, descrip, canprog, um
                FROM v_inv_ordenesproduccion
                WHERE numop = @numeroOP
                LIMIT 1";
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@numeroOP", numeroOP);
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new OrdenProduccionExterna
                {
                    NumeroOP = reader["numop"]?.ToString() ?? "",
                    Cliente = reader["nombrecliente"]?.ToString(),
                    Referencia = reader["refer"]?.ToString(),
                    Descripcion = reader["descrip"]?.ToString(),
                    CantidadOP = reader["canprog"] != DBNull.Value
                        ? Convert.ToInt32(reader["canprog"])
                        : 0,
                    Unidad = reader["um"]?.ToString(),
                    FechaImportacion = DateTime.Now
                };
            }
            return null;
        }

        public async Task<decimal> ObtenerCantidadRealAsync(string numeroOP)
        {
            const string sql = @"
                SELECT COALESCE(SUM(kilos), 0) AS total
                FROM v_inv_registroproduccion
                WHERE numop = @numeroOP";

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@numeroOP", numeroOP);

            var resultado = await command.ExecuteScalarAsync();
            return resultado != null && resultado != DBNull.Value ? Convert.ToDecimal(resultado) : 0;
        }

        // Esta fuente no soporta importación por archivo — no aplica aquí.
        public Task<List<OrdenProduccionExterna>> ImportarDesdeArchivo(Stream archivo, string nombreArchivo)
        {
            throw new NotSupportedException(
                "La importación desde archivo no aplica para la fuente de base de datos de la empresa.");
        }
    }
}