using Microsoft.AspNetCore.Authorization;

namespace SistemaFlota.Authorization
{
    public class PermisoRequirement : IAuthorizationRequirement
    {
        public string PolicyName { get; }

        public PermisoRequirement(string policyName)
        {
            PolicyName = policyName;
        }
    }
}