using LogiTrack.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LogiTrack.Authorization
{
    public class AddressResourceOperationRequirementHandler : AuthorizationHandler<ResourcerceOperationRequirement, Address>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourcerceOperationRequirement requirement, Address address)
        {
            if (requirement.ResourceOperation == ResourceOperation.Read || requirement.ResourceOperation == ResourceOperation.Create)
                context.Succeed(requirement);

            var userId = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (address.CreatedById == int.Parse(userId))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
