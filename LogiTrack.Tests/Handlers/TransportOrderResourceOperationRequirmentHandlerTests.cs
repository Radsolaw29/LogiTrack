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
    public class TransportOrderResourceOperationRequirmentHandlerTests
    {

        private readonly TransportOrderResourceOperationRequirmentHandler _handler;

        public TransportOrderResourceOperationRequirmentHandlerTests()
        {
            _handler = new TransportOrderResourceOperationRequirmentHandler();
        }

        private static AuthorizationHandlerContext CreateContext(ResourceOperation resourceOperation, TransportOrder transportOrder, int userId)
        {
            var requirement = new ResourcerceOperationRequirement(resourceOperation);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }));

            return new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                transportOrder);
        }

        [Theory]
        [InlineData(ResourceOperation.Read)]
        [InlineData(ResourceOperation.Create)]
        public async Task HandleRequirementAsync_ForReadOrCreate_ShouldSucceed(ResourceOperation resourceOperation)
        {
            //Arrange

            var transpoerOrder = new TransportOrder { CreatedById = 999 };

            var context = CreateContext(resourceOperation, transpoerOrder, userId: 1);

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

            var transportOrder = new TransportOrder { CreatedById = 999 };

            var context = CreateContext(ResourceOperation.Update, transportOrder, userId: 999);

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

            var transportOrder = new TransportOrder { CreatedById = 999 };

            var context = CreateContext(ResourceOperation.Delete, transportOrder, userId: 1);

            //Act

            await _handler.HandleAsync(context);

            //Assert

            context.HasSucceeded.Should().BeFalse();
        }
    }
}
