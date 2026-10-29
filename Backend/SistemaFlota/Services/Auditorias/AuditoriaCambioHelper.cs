using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace SistemaFlota.Services.Auditorias
{
    public static class AuditoriaCambioHelper
    {
        private static readonly HashSet<string> PropiedadesSensibles =
            new(StringComparer.OrdinalIgnoreCase)
            {
            "Password",
            "Contrasena",
            "Clave",
            "Token",
            "RefreshToken",
            "Secret"
            };

        public static string CrearDetalle(EntityEntry entrada)
        {
            var cambios = new List<object>();

            foreach (var propiedad in entrada.Properties)
            {
                var nombre = propiedad.Metadata.Name;

                // Evitar guardar información sensible.
                if (PropiedadesSensibles.Any(nombre.Contains))
                    continue;

                object? anterior = null;
                object? nuevo = null;

                switch (entrada.State)
                {
                    case EntityState.Added:
                        nuevo = propiedad.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        anterior = propiedad.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (!propiedad.IsModified)
                            continue;

                        anterior = propiedad.OriginalValue;
                        nuevo = propiedad.CurrentValue;

                        if (Equals(anterior, nuevo))
                            continue;

                        break;

                    default:
                        continue;
                }

                cambios.Add(new
                {
                    campo = nombre,
                    anterior,
                    nuevo
                });
            }

            var detalle = new
            {
                entidad = entrada.Metadata.ClrType.Name,
                operacion = entrada.State.ToString(),
                cambios
            };

            return JsonSerializer.Serialize(detalle);
        }
    }
}
