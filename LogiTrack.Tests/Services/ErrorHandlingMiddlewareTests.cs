using Castle.Core.Logging;
using FluentAssertions;
using LogiTrack.Exceptions;
using LogiTrack.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogiTrack.Tests.Services
{
    public class ErrorHandlingMiddlewareTests
    {

        private readonly Mock<ILogger<ErrorHandlingMiddleware>> _loggerMock;
        private readonly ErrorHandlingMiddleware _middleware;

        public ErrorHandlingMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
            _middleware = new ErrorHandlingMiddleware(_loggerMock.Object);
        }

        private static HttpContext CreateHtppContext()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            return context;
        }

        private static async Task<string> ReadRespondBody(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            return await reader.ReadToEndAsync();
        }

        [Fact]
        public async Task InvokeAsync_WhenForbidExceptionThrown_ShouldReturn403()
        {
            //Arrange

            var context = CreateHtppContext();

            RequestDelegate next = _ => throw new ForbidException("Forbidden");

            //Act

            await _middleware.InvokeAsync(context, next);

            //Assert

            context.Response.StatusCode.Should().Be(403);
            (await ReadRespondBody(context)).Should().Be("Forbidden");
        }

        [Fact]
        public async Task InvokeAsync_WhenBadRequestExceptionThrown_ShouldReturn400()
        {
            //Arrange

            var context = CreateHtppContext();

            RequestDelegate next = _ => throw new BadRequestException("Bad Request");

            //Act

            await _middleware.InvokeAsync(context, next);

            //Assert

            context.Response.StatusCode.Should().Be(400);
            (await ReadRespondBody(context)).Should().Be("Bad Request");
        }

        [Fact]
        public async Task InvokeAsync_WhenNotFoundExceptionThrown_ShouldReturn404()
        {
            //Arrange

            var context = CreateHtppContext();

            RequestDelegate next = _ => throw new NotFoundException("Not Found");

            //Act

            await _middleware.InvokeAsync(context, next);

            //Assert

            context.Response.StatusCode.Should().Be(404);
            (await ReadRespondBody(context)).Should().Be("Not Found");
        }

        [Fact]
        public async Task InvokeAsync_WhenUnhandledExceptionThrown_ShouldReturn500_AndLogError()
        {
            //Arrange

            var context = CreateHtppContext();

            var exception = new Exception("Unhandled exception");

            RequestDelegate next = _ => throw exception;

            //Act

            await _middleware.InvokeAsync(context, next);

            //Assert

            context.Response.StatusCode.Should().Be(500);
            (await ReadRespondBody(context)).Should().Be("Something went wrong...");

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Unhandled exception")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WhenNoExceptionThrown_ShouldCallNext()
        {
            //Arrange

            var context = CreateHtppContext();
            var wasCalled = false;

            RequestDelegate next = _ =>
            {
                wasCalled = true;
                return Task.CompletedTask;
            };

            //Act

            await _middleware.InvokeAsync(context, next);

            //Assert

            wasCalled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(200);
        }
    }
}
