using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace SistemaFlota.Authorization
{
    public class PermisoPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermisoPolicyProvider(
            IOptions<AuthorizationOptions> options)
            : base(options)
        {
        }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(
            string policyName)
        {
            if (policyName.StartsWith("Permiso:"))
            {
                return new AuthorizationPolicyBuilder()
                    .AddRequirements(
                        new PermisoRequirement(policyName))
                    .Build();
            }

            return await base.GetPolicyAsync(policyName);
        }
    }
}