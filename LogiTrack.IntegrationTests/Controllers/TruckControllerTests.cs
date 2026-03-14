using FluentAssertions;
using LogiTrack.Entities;
using LogiTrack.IntegrationTests.Extensions;
using LogiTrack.IntegrationTests.Helpers;
using LogiTrack.IntegrationTests.Seed;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace LogiTrack.IntegrationTests.Controllers
{
    public class TruckControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {

        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly string _databaseName = Guid.NewGuid().ToString();

        public TruckControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var dbContextOptions = services.SingleOrDefault(services => services.ServiceType == typeof(DbContextOptions<LogiTrackDbContext>));

                    services.Remove(dbContextOptions);

                    services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();

                    services.AddDbContext<LogiTrackDbContext>(options => options.UseInMemoryDatabase(_databaseName));
                });
            });

            _client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LogiTrackDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }

        [Theory]
        [InlineData("pageSize=5&pageNumber=1")]
        [InlineData("pageSize=10&pageNumber=2")]
        [InlineData("pageSize=20&pageNumber=3")]
        [InlineData("pageSize=50&pageNumber=4")]
        public async Task GetAllTrucks_WithValidQueryParameters_ShouldReturnsOk(string queryParameters)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync($"/api/company/1/truck?{queryParameters}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("pageSize=0&pageNumber=1")]
        [InlineData("pageSize=1&pageNumber=1")]
        [InlineData("pageSize=155&pageNumber=3")]
        [InlineData("")]
        [InlineData(null)]
        public async Task GetAllTrucks_WithInvalidQueryParameters_ShouldReturnsBadRequest(string queryParameters)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync($"/api/company/1/truck?{queryParameters}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }

        [Theory]
        [InlineData("GD12345")]
        [InlineData("Volvo")]
        [InlineData("FH16")]
        public async Task GetAllTrucks_WithSearchPhrase_ShouldReturnsMatchingResults(string searchPhrase)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var encodedPhrase = Uri.EscapeDataString(searchPhrase);

            //Act

            var response = await _client.GetAsync($"/api/company/1/truck?searchPhrase={encodedPhrase}&pageSize=5&pageNumber=1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.DeserializeAsync<PageResult<TruckDto>>();
            result.Items.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();

            result.Items.Should().OnlyContain(c =>
                c.RegistrationNumber.ToLower().Contains(searchPhrase.ToLower()) ||
                c.Brand.ToLower().Contains(searchPhrase.ToLower()) ||
                c.Model.ToLower().Contains(searchPhrase.ToLower()));
        }
    }
}
