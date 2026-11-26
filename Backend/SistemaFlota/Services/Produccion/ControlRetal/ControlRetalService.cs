using MySqlConnector;
using SistemaFlota.DTOs.Produccion.ControlRetal;

namespace SistemaFlota.Services.Produccion.ControlRetal
{
    public class ControlRetalService : IControlRetalService
    {
        private readonly string _connectionString;

        public ControlRetalService(IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString("EmpresaConnection")
                ?? throw new InvalidOperationException(
                    "No está configurada la conexión EmpresaConnection."
                );
        }

        public async Task<ConsultarOrdenControlRetalDto?> ObtenerReferenciaPorOrdenAsync(int numop)
        {
            await using var conexion =
                new MySqlConnection(_connectionString);

            await conexion.OpenAsync();

            const string sql = @"
                SELECT numop, refer
                FROM v_inv_ordenesproduccion
                WHERE numop = @numop
                LIMIT 1;
            ";

            await using var comando = new MySqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("@numop", numop);

            await using var reader = await comando.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new ConsultarOrdenControlRetalDto
            {
                Numop = Convert.ToInt32(reader["numop"]),
                Refer = reader["refer"]?.ToString() ?? string.Empty
            };
        }
    }
}