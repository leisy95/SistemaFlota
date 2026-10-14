using Microsoft.AspNetCore.Authorization;

namespace SistemaFlota.Authorization
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method,
        AllowMultiple = false,
        Inherited = true)]
    public class PermisoAlternativoAttribute : AuthorizeAttribute
    {
        public string[] Modulos { get; }
        public string Accion { get; }

        public PermisoAlternativoAttribute(
            string modulo1,
            string modulo2,
            string accion)
        {
            Modulos = new[] { modulo1, modulo2 };
            Accion = accion;

            Policy = $"PermisoAlternativo:{modulo1}:{modulo2}:{accion}";
        }
    }
}
