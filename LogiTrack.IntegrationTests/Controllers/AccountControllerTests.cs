using FluentAssertions;
using LogiTrack.Entities;
using LogiTrack.IntegrationTests.Helpers;
using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Moq;
using NLog.Config;
using System.Net;

namespace LogiTrack.IntegrationTests.Controllers
{
    public class AccountControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly string _databaseName = Guid.NewGuid().ToString();
        private readonly Mock<IAccountService> _accountServiceMock = new Mock<IAccountService>();

        public AccountControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var dbContextOptions = services.SingleOrDefault(services => services.ServiceType == typeof(DbContextOptions<LogiTrackDbContext>));

                        services.Remove(dbContextOptions);

                        services.AddSingleton<IAccountService>(_accountServiceMock.Object);

                        services.AddDbContext<LogiTrackDbContext>(options => options.UseInMemoryDatabase(_databaseName));
                    });
                });

                _client = _factory.CreateClient();

                using var scope = _factory.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<LogiTrackDbContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
        }

        [Fact]
        public async Task RegisterUser_ForValidModel_ShouldReturnsOk()
        {
            //Arrange

            var registerUser = new RegisterUserDto()
            {
                Email = "testemail@wp.pl",
                Password = "password12!",
                ConfirmPassword = "password12!"
            };

            var httpContent = registerUser.ToJsonHttpContent();

            //Act

            var response = await _client.PostAsync("/api/account/register", httpContent);

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task RegisterUser_ForInvalidModel_ShouldReturnsBadRequest()
        {
            // Arrange

            var registerUser = new RegisterUserDto()
            {
                Password = "Password12!",
                ConfirmPassword = "password1!"
            };

            var httpContent = registerUser.ToJsonHttpContent();

            //Act

            var response = await _client.PostAsync("/api/account/register", httpContent);

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }


        [Fact]
        public async Task LoginUser_ForRegisteredUser_ShouldReturnsOk()
        {
            //Arrange

            _accountServiceMock
                .Setup(x => x.GenerateJwt(It.IsAny<LoginDto>())).Returns("jwt");

            var loginDto = new LoginDto()
            {
                Email = "testowy@wp.pl",
                Password = "Password12!"
            };

            var httpContent = loginDto.ToJsonHttpContent();

            //Act

            var response = await _client.PostAsync("/api/account/login", httpContent);

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
