using LogiTrack.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LogiTrack.Authorization
{
    public class ResourcerceOperationRequirementHandler : AuthorizationHandler<ResourcerceOperationRequirement, Company>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourcerceOperationRequirement requirement, Company company)
        {
            if (requirement.ResourceOperation == ResourceOperation.Read || requirement.ResourceOperation == ResourceOperation.Create)
                context.Succeed(requirement);

            var userId = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (company.CreatedById == int.Parse(userId))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
