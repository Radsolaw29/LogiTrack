using LogiTrack.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace LogiTrack.IntegrationTests
{
    public class FakePolicyEvaluator : IPolicyEvaluator
    {
        public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
        {
            // Odczytujemy rolę bezpośrednio z nagłówka żądania
            var role = context.Request.Headers["Test-Role"].FirstOrDefault() ?? "Admin";

            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, role) // Ustawiamy to, co przyszło w nagłówku
            },  "TestScheme"));

            var ticket = new AuthenticationTicket(claimsPrincipal, "TestScheme");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        public Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object? resource)
        {
            var requiredRoles = policy.Requirements
            .OfType<Microsoft.AspNetCore.Authorization.Infrastructure.RolesAuthorizationRequirement>()
            .SelectMany(x => x.AllowedRoles).ToList();

            if (!requiredRoles.Any()) return Task.FromResult(PolicyAuthorizationResult.Success());

            // Sprawdzamy czy rola z nagłówka (np. "User") jest wśród wymaganych (np. "Admin")
            if (requiredRoles.Any(role => authenticationResult.Principal.IsInRole(role)))
            {
                return Task.FromResult(PolicyAuthorizationResult.Success());
            }

            return Task.FromResult(PolicyAuthorizationResult.Forbid());
        }
    }
}
