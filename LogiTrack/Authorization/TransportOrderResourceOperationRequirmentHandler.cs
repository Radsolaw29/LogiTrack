using LogiTrack.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LogiTrack.Authorization
{
    public class TransportOrderResourceOperationRequirmentHandler : AuthorizationHandler<ResourcerceOperationRequirement, TransportOrder>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourcerceOperationRequirement requirement, TransportOrder transportOrder)
        {
            if (requirement.ResourceOperation == ResourceOperation.Read || requirement.ResourceOperation == ResourceOperation.Create)
                context.Succeed(requirement);

            var userId = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (transportOrder.CreatedById == int.Parse(userId))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
