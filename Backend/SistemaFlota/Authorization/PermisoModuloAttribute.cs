using Microsoft.AspNetCore.Authorization;

namespace SistemaFlota.Authorization
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method,
        AllowMultiple = true,
        Inherited = true)]
    public class PermisoAttribute : AuthorizeAttribute
    {
        public string Modulo { get; }
        public string Accion { get; }

        public PermisoAttribute(string modulo, string accion)
        {
            Modulo = modulo;
            Accion = accion;

            Policy = $"Permiso:{modulo}:{accion}";
        }
    }
}