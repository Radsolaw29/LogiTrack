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

namespace LogiTrack.Tests.Services
{
    public class AddressResourceOperationRequirementHandlerTests
    {

        private readonly AddressResourceOperationRequirementHandler _handler;

        public AddressResourceOperationRequirementHandlerTests()
        {
            _handler = new AddressResourceOperationRequirementHandler();
        }

        private static AuthorizationHandlerContext CreateContext(ResourceOperation resourceOperation, Address address, int userId)
        {
            var requirement = new ResourcerceOperationRequirement(resourceOperation);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }));

            return new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                address);
        }

        [Theory]
        [InlineData(ResourceOperation.Read)]
        [InlineData(ResourceOperation.Create)]
        public async Task HandleRequirementAsync_ForReadOrCreate_ShouldSucceed(ResourceOperation resourceOperation)
        {
            //Arrange

            var address = new Address { CreatedById = 999 };

            var context = CreateContext(resourceOperation, address, userId: 1);

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

            var address = new Address { CreatedById = 999 };

            var context = CreateContext(ResourceOperation.Update, address, userId: 999);

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

            var address = new Address { CreatedById = 999 };

            var context = CreateContext(ResourceOperation.Delete, address, userId: 1);

            //Act

            await _handler.HandleAsync(context);

            //Assert

            context.HasSucceeded.Should().BeFalse();
        }
    }
}
