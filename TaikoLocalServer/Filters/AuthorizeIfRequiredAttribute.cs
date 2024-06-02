using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Settings;

namespace TaikoLocalServer.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AuthorizeIfRequiredAttribute(IOptions<AuthSettings> settings) : Attribute, IAsyncAuthorizationFilter
    {
        private readonly bool loginRequired = settings.Value.AuthenticationRequired;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!loginRequired)
            {
                return; // Skip authorization if login is not required
            }

            var authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
            var policyProvider = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationPolicyProvider>();
            var policy = await policyProvider.GetPolicyAsync(AuthorizationPolicyNames.Default);

            if (policy != null)
            {
                var authResult = await authorizationService.AuthorizeAsync(context.HttpContext.User, policy);
                if (!authResult.Succeeded)
                {
                    context.Result = new UnauthorizedResult();
                }
            }
        }
    }

    public static class AuthorizationPolicyNames
    {
        public const string Default = "Default";
    }
}