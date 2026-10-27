
using MySqlConnector;
using SistemaFlota.DTOs.InformacionKasoft;
using System.Globalization;
using System.Reflection;

namespace SistemaFlota.Services.InformacionKasoft
{
    public class InformacionKasoftService : IInformacionKasoftService
    {
        private readonly string _connectionString;

        public InformacionKasoftService(IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString("EmpresaConnection")
                ?? throw new InvalidOperationException(
                    "No está configurada la variable ConnectionStrings__EmpresaConnection."
                );
        }

        // Consultas generales de las siete vistas
        public Task<List<StockReferenciaDto>> ObtenerStocksPorReferenciaAsync()
            => ConsultarAsync<StockReferenciaDto>("v_inv_stocksxreferencia");

        public Task<List<EntradaDto>> ObtenerEntradasAsync()
            => ConsultarAsync<EntradaDto>("v_inv_entradas");

        public Task<List<MezclaPorOrdenDto>> ObtenerMezclaPorOrdenAsync()
            => ConsultarAsync<MezclaPorOrdenDto>("v_inv_mezclapororden");

        public Task<List<MateriasPrimasDto>> ObtenerMateriasPrimasAsync()
            => ConsultarAsync<MateriasPrimasDto>("v_inv_materiasprimas");

        public Task<List<OrdenesProduccionDto>> ObtenerOrdenesProduccionAsync()
            => ConsultarAsync<OrdenesProduccionDto>("v_inv_ordenesproduccion");

        public Task<List<ReferenciasDto>> ObtenerReferenciasAsync()
            => ConsultarAsync<ReferenciasDto>("v_inv_referencias");

        public Task<List<RegistroProduccionDto>> ObtenerRegistroProduccionAsync()
            => ConsultarAsync<RegistroProduccionDto>("v_inv_registroproduccion");


        // Consulta genérica reutilizable para todas las vistas
        private async Task<List<T>> ConsultarAsync<T>(string vista)
            where T : new()
        {
            var resultados = new List<T>();

            await using var conexion =
                new MySqlConnection(_connectionString);

            await conexion.OpenAsync();

            await using var comando = new MySqlCommand(
                $"SELECT * FROM `{vista}`",
                conexion);

            await using var reader = await comando.ExecuteReaderAsync();

            var propiedades = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToDictionary(
                    p => NormalizarNombre(p.Name),
                    p => p);

            while (await reader.ReadAsync())
            {
                var objeto = new T();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var nombreColumna =
                        NormalizarNombre(reader.GetName(i));

                    if (!propiedades.TryGetValue(
                        nombreColumna, out var propiedad))
                        continue;

                    if (reader.IsDBNull(i))
                    {
                        var tipoNullable = Nullable.GetUnderlyingType(
                            propiedad.PropertyType);

                        if (!propiedad.PropertyType.IsValueType ||
                            tipoNullable != null)
                        {
                            propiedad.SetValue(objeto, null);
                        }

                        continue;
                    }

                    var valor = reader.GetValue(i);

                    var tipoDestino = Nullable.GetUnderlyingType(
                        propiedad.PropertyType)
                        ?? propiedad.PropertyType;

                    if (tipoDestino == typeof(string))
                    {
                        valor = Convert.ToString(
                            valor,
                            CultureInfo.InvariantCulture)?.Trim();
                    }
                    else if (tipoDestino == typeof(DateTime))
                    {
                        valor = Convert.ToDateTime(
                            valor,
                            CultureInfo.InvariantCulture);
                    }
                    else if (tipoDestino.IsEnum)
                    {
                        valor = Enum.ToObject(tipoDestino, valor);
                    }
                    else if (tipoDestino != valor.GetType())
                    {
                        valor = Convert.ChangeType(
                            valor,
                            tipoDestino,
                            CultureInfo.InvariantCulture);
                    }

                    propiedad.SetValue(objeto, valor);
                }

                resultados.Add(objeto);
            }

            return resultados;
        }

        private static string NormalizarNombre(string nombre)
        {
            return new string(
                nombre.Where(char.IsLetterOrDigit)
                    .Select(char.ToLowerInvariant)
                    .ToArray());
        }
    }
}
