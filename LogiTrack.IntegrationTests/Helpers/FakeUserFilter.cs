using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace LogiTrack.IntegrationTests.Helpers
{
    public class FakeUserFilter : IAsyncActionFilter
    {
        private readonly string _role;
        private readonly string _userId;

        public FakeUserFilter(string role = "Admin", string userId = "1")
        {
            _role = role;
            _userId = userId;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _userId),
                new Claim(ClaimTypes.Role, _role)
            };

            // "TestScheme" jest wymagane, by Identity.IsAuthenticated było true
            var identity = new ClaimsIdentity(claims, "TestScheme");
            var user = new ClaimsPrincipal(identity);

            // Wstrzykujemy użytkownika do HttpContext
            context.HttpContext.User = user;

            await next();
        }
    }
}
