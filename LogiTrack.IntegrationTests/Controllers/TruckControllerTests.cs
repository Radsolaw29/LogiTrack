using FluentAssertions;
using LogiTrack.Entities;
using LogiTrack.IntegrationTests.Builders;
using LogiTrack.IntegrationTests.Extensions;
using LogiTrack.IntegrationTests.Helpers;
using LogiTrack.IntegrationTests.Seed;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
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
        [InlineData("GD11111")]
        [InlineData("GD22222")]
        [InlineData("Volvo")]
        [InlineData("Saab")]
        [InlineData("Mercedes")]
        [InlineData("FH16")]
        [InlineData("FH18")]
        [InlineData("FH20")]
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

        [Theory]
        [InlineData("zzz")]
        [InlineData("notexisting")]
        [InlineData("123456")]
        public async Task GetAllTrucks_WithBadSearchPhrase_ShouldReturEmpty(string searchPhrase)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var encodedPhrase = Uri.EscapeDataString(searchPhrase);

            //Act

            var response = await _client.GetAsync($"/api/company/1/truck?searchPhrase={encodedPhrase}&pageSize=5&pageNumber=1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.DeserializeAsync<PageResult<TruckDto>>();

            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
            result.Items.Should().BeEmpty();
        }

        [Theory]
        [InlineData(SortDirection.ASC)]
        [InlineData(SortDirection.DESC)]
        public async Task GetAllTrucks_WithSorting_ShouldReturnsSorted(SortDirection direction)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync($"/api/company/1/truck?pageSize=5&pageNumber=1&sortBy=Brand&sortDirection={direction}");

            var content = await response.Content.ReadAsStringAsync();

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK, content);

            var result = await response.DeserializeAsync<PageResult<TruckDto>>();

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(3);

            var truckBrands = result.Items.Select(c => c.Brand).ToList();

            if (direction == SortDirection.ASC)
                truckBrands.Should().BeInAscendingOrder();
            else
                truckBrands.Should().BeInDescendingOrder();
        }

        [Theory]
        [InlineData("RegistrationNumber")]
        [InlineData("Brand")]
        [InlineData("Model")]
        public async Task GetAllTrucks_WithDifferentSortColumns_ShouldWork(string sortBy)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync($"/api/company/1/truck?pageSize=5&pageNumber=1&sortBy={sortBy}");

            var content = await response.Content.ReadAsStringAsync();

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK, content);

            var result = await response.DeserializeAsync<PageResult<TruckDto>>();

            result.Items.Should().HaveCount(3);

            var values = sortBy switch
            {
                "RegistrationNumber" => result.Items.Select(x => x.RegistrationNumber),
                "Brand" => result.Items.Select(x => x.Brand),
                "Model" => result.Items.Select(x => x.Model),
                _ => throw new Exception("Invalid sort column")
            };

            values.Should().BeInAscendingOrder();
        }

        [Theory]
        [InlineData("Invalid")]
        [InlineData("123")]
        [InlineData("Brand123")]
        public async Task GetAllTrucks_WithInvalidSortBy_ShouldReturnsBadRequest(string sortBy)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync($"/api/company/1/truck?pageSize=5&pageNumber=1&sortBy={sortBy}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetTruckById_ShouldReturnsOk_WithCorrectData()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync($"/api/company/1/truck/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.DeserializeAsync<TruckDto>();

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Brand.Should().Be("Volvo");
        }

        [Fact]
        public async Task GetTruckById_WhenTruckDoesNotExist_ShouldReturnsNotFound()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync("/api/company/1/truck/999");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetTruckById_ShouldAllowAnonymousAccess()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var unauthorizedClient = _factory.CreateUnauthorizedClient();

            //Act

            var response = await _client.GetAsync("/api/company/1/truck/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateTruck_WithValidModel_ShouldReturnsCreated()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new CreateTruckBuilder().Build();

            //Act

            var response = await _client.PostAsync("/api/company/1/truck", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateTruck_WithInvalidModel_ShouldReturnsBadRequest()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new CreateTruckDto
            {
                Brand = "Saab",
                Model = "FH20"
            };

            //Act

            var response = await _client.PostAsync("/api/company/1/truck", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var problem = await response.DeserializeAsync<ValidationProblemDetails>();

            problem.Errors.Should().ContainKey(nameof(CreateTruckDto.RegistrationNumber));
        }

        [Fact]
        public async Task CreateTruck_WhenUserIsNotAdmin_ShouldReturnsForbidden()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new CreateTruckBuilder().Build();

            var client = _factory.CreateClientWithRole("User");

            //Act

            var response = await client.PostAsync("/api/company/1/truck", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateTruck_WhenUserIsNotLoggedIn_ShouldReturnsUnauthorized()
        {
            //Arrange

            var client = _factory.CreateUnauthorizedClient();

            //Act

            var response = await client.PostAsync("/api/company/1/truck", new CreateTruckDto().ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateTruck_WithValidModel_ShouldReturnsOk()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new UpdateTruckBuilder().Build();


            //Act

            var response = await _client.PutAsync("/api/company/1/truck/1", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LogiTrackDbContext>();
            var updatedTruck = await db.Trucks.FirstOrDefaultAsync(x => x.Id == 1);

            updatedTruck.Should().NotBeNull();
            updatedTruck.RegistrationNumber.Should().Be(dto.RegistrationNumber);
            updatedTruck.Brand.Should().Be(dto.Brand);
            updatedTruck.Model.Should().Be(dto.Model);
            updatedTruck.Mileage.Should().Be(dto.Mileage);
            updatedTruck.CapacityTons.Should().Be(dto.CapacityTons);
        }

        [Fact]
        public async Task UpdateTruck_WithInvalidModel_ShouldReturnsBadRequest()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new UpdateTruckDto
            {
                Model = "Opel"
            };

            //Act

            var response = await _client.PutAsync($"/api/company/1/truck/1", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateTruck_WhenTruckDoesNotExist_ShouldReturnsNotFound()
        {
            //Arrange

            var dto = new UpdateTruckBuilder().Build();

            //Act

            var response = await _client.PutAsync("/api/company/1/truck/999", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateTruck_WhenUserIsNotAdmin_ShouldReturnsForbidden()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new UpdateTruckBuilder().Build();

            var client = _factory.CreateClientWithRole("User");

            //Act

            var response = await client.PutAsync("/api/company/1/truck/1", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdateTruck_WhenUserIsNotLoggedIn_shouldReturnsUnauthorized()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new UpdateTruckBuilder().Build();

            var unauthorizedClient = _factory.CreateUnauthorizedClient();

            //Act

            var response = await unauthorizedClient.PutAsync($"/api/company/1/truck/1", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteTruck_ForCompanyOwner_ShouldReturnsNoContent()
        {
            //Assert

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.DeleteAsync($"/api/company/1/truck/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteTruck_WhenUserIsNotAdmin_ShouldReturnsForbidden()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var client = _factory.CreateClientWithRole("User");

            //Act

            var response = await client.DeleteAsync("/api/company/1/truck/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeteleTruck_WhenUserIsNotLoggedIn_ShouldReturnsUnauthorized()
        {
            //Arrange

            var client = _factory.CreateUnauthorizedClient();

            //Act

            var response = await client.DeleteAsync("/api/company/1/truck/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteTruck_WhenTransportOrderNotExist_ShouldReturnsNotFound()
        {
            //Assert

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.DeleteAsync("/api/company/1/truck/999");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
