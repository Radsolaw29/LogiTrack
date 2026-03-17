using LogiTrack.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LogiTrack.UnitTests.Context
{
    public class UserContextServiceTests
    {

        [Fact]
        public void GetUserId_ShouldReturnsNull_WhenHttpContextIsNull()
        {
            //Arrange

            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns((HttpContext)null);

            var service = new UserContextService(accessorMock.Object);

            //Act

            var result = service.GetUserId;

            //Assert

            Assert.Null(result);
        }

        [Fact]
        public void GetUserId_ShouldReturnsNull_WhenUserIsNull()
        {
            //Arrange

            var context = new DefaultHttpContext
            {
                User = null
            };

            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(context);

            var service = new UserContextService(accessorMock.Object);

            //Act

            var result = service.GetUserId;

            //Assert

            Assert.Null(result);
        }

        [Fact]
        public void GetUserId_ShouldReturnsUserId_WhenClaimsExists()
        {
            //Arrange

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "123")
            };

            var identity = new ClaimsIdentity(claims);

            var user = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext
            {
                User = user
            };

            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(context);

            var service = new UserContextService(accessorMock.Object);

            //Act

            var result = service.GetUserId;

            //Assert

            Assert.Equal(123, result);
        }
    }
}
