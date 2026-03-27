using Microsoft.AspNetCore.Authorization;

namespace LogiTrack.Authorization
{
    public enum ResourceOperation
    {
        Create,
        Read,
        Update,
        Delete
    }

    public class ResourcerceOperationRequirement : IAuthorizationRequirement
    {
        public ResourcerceOperationRequirement(ResourceOperation resourceOperation)
        {
            ResourceOperation = resourceOperation;
        }

        public ResourceOperation ResourceOperation { get; }
    }
}
