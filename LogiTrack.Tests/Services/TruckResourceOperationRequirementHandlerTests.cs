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
    public class TruckResourceOperationRequirementHandlerTests
    {

        private readonly TruckResourceOperationRequirementHandler _handler;

        public TruckResourceOperationRequirementHandlerTests()
        {
            _handler = new TruckResourceOperationRequirementHandler();
        }

        private static AuthorizationHandlerContext CreateContext(ResourceOperation resourceOperation, Truck truck, int userId)
        {
            var requirement = new ResourcerceOperationRequirement(resourceOperation);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }));

            return new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                truck);
        }

        [Theory]
        [InlineData(ResourceOperation.Read)]
        [InlineData(ResourceOperation.Create)]
        public async Task HandleRequirementAsync_ForReadOrCreate_ShouldSucceed(ResourceOperation resourceOperation)
        {
            //Arrange

            var truck = new Truck { CreatedById = 999 };

            var context = CreateContext(resourceOperation, truck, userId: 1);

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

            var truck = new Truck { CreatedById = 999 };

            var context = CreateContext(ResourceOperation.Update, truck, userId: 999);

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

            var truck = new Truck { CreatedById = 999 };

            var context = CreateContext(ResourceOperation.Delete, truck, userId: 1);

            //Act

            await _handler.HandleAsync(context);

            //Assert

            context.HasSucceeded.Should().BeFalse();
        }
    }
}
