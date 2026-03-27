using FluentAssertions;
using LogiTrack.Authorization;
using LogiTrack.Entities;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LogiTrack.UnitTests.Handlers
{
    public class ResourcerceOperationRequirementHandlerTests
    {

        private readonly ResourcerceOperationRequirementHandler _handler;

        public ResourcerceOperationRequirementHandlerTests()
        {
            _handler = new ResourcerceOperationRequirementHandler();
        }

        private static AuthorizationHandlerContext CreateContext(ResourceOperation resourceOperation, Company company, int userId)
        {
            var requirement = new ResourcerceOperationRequirement(resourceOperation);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }));

            return new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                company);
        }

        [Theory]
        [InlineData(ResourceOperation.Read)]
        [InlineData(ResourceOperation.Create)]
        public async Task HandleRequirementAsync_ForReadOrCreate_ShouldSucceed(ResourceOperation resourceOperation)
        {
            //Arrange

            var company = new Company { CreatedById = 999 };

            var context = CreateContext(resourceOperation, company, userId: 1);

            //Act

            await _handler.HandleAsync(context);

            //Assert

            context.HasSucceeded.Should().BeTrue();
            context.HasFailed.Should().BeFalse();
        }

        [Fact]
        public async Task HandleRequirementAsync_WhenUserIsOwner_ShouldSucceed()
        {
            //Arrange

            var company = new Company { CreatedById = 999 };

            var context = CreateContext(ResourceOperation.Update, company, userId: 999);

            //Act

            await _handler.HandleAsync(context);

            //Assert

            context.HasSucceeded.Should().BeTrue();
            context.HasFailed.Should().BeFalse();
        }

        [Fact]
        public async Task HandleRequirementAsync_WhenUserIsNotOwner_ShouldFail()
        {
            //Arrange

            var company = new Company { CreatedById = 999 };

            var context = CreateContext(ResourceOperation.Delete, company, userId: 1);

            //Act

            await _handler.HandleAsync(context);

            //Assert

            context.HasSucceeded.Should().BeFalse();
        }
    }
}
