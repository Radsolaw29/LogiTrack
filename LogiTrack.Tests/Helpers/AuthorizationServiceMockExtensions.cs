using Microsoft.AspNetCore.Authorization;
using Moq;
using System.Security.Claims;

namespace LogiTrack.UnitTests.Helpers
{
    public static class AuthorizationServiceMockExtensions
    {
        public static void SetupSuccess(this Mock<IAuthorizationService> mock)
        {
            mock.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success);
        }

        public static void SetupFail(this Mock<IAuthorizationService> mock)
        {
            mock.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Failed);
        }
    }
}
