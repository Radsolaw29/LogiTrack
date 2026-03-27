using FluentAssertions;
using LogiTrack.Entities;
using LogiTrack.IntegrationTests.Builders;
using LogiTrack.IntegrationTests.Extensions;
using LogiTrack.IntegrationTests.Helpers;
using LogiTrack.IntegrationTests.Seed;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;

namespace LogiTrack.IntegrationTests.Controllers
{
    public class AddressControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {

        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly string _databaseName = Guid.NewGuid().ToString();

        public AddressControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory
                .WithWebHostBuilder(builder =>
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

        private void SeedAddress(Address address)
        {
            var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetService<LogiTrackDbContext>();

            _dbContext.Addresses.Add(address);
            _dbContext.SaveChanges();
        }

        [Theory]
        [InlineData("pageSize=5&pageNumber=1")]
        [InlineData("pageSize=10&pageNumber=2")]
        [InlineData("pageSize=20&pageNumber=3")]
        [InlineData("pageSize=50&pageNumber=4")]
        public async Task GetAllAddresses_WithValidQueryParameter_ReturnsOk(string queryParameters)
        {
            //Act

            var response = await _client.GetAsync($"api/address?{queryParameters}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("pageSize=0&pageNumber=1")]
        [InlineData("pageSize=1&pageNumber=1")]
        [InlineData("pageSize=155&pageNumber=3")]
        [InlineData("")]
        [InlineData(null)]
        public async Task GetAllAddresses_WithInvalidQueryParameters_ReturnsBadRequest(string queryParameters)
        {
            //Act

            var response = await _client.GetAsync($"api/address?{queryParameters}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("Sweden")]
        [InlineData("Poland")]
        [InlineData("England")]
        [InlineData("Stockholm")]
        [InlineData("Sopot")]
        [InlineData("London")]
        [InlineData("Kungsgatan")]
        [InlineData("Pańska")]
        [InlineData("Downing Street")]
        public async Task GetAllAddresses_WithSearchPhrases_ShouldReturnsMatchingResults(string searchPhrase)
        {
            //Arrange

            await SeedBasicData.SeedAddressAsync(_factory);

            var encodedPhrase = Uri.EscapeDataString(searchPhrase);

            //Act

            var response = await _client.GetAsync($"api/address?searchPhrase={encodedPhrase}&pageSize=5&pageNumber=1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.DeserializeAsync<PageResult<AddressDto>>();

            result.Items.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();

            result.Items.Should().OnlyContain(c =>
                c.Country.ToLower().Contains(searchPhrase.ToLower()) ||
                c.City.ToLower().Contains(searchPhrase.ToLower()) ||
                c.Street.ToLower().Contains(searchPhrase.ToLower()));
        }

        [Theory]
        [InlineData("zzz")]
        [InlineData("notexisting")]
        [InlineData("123456")]
        [InlineData("Germany")]
        public async Task GetAllAddresses_WithNotMatchingSearchPhrase_ShouldReturnEmpty(string searchPhrase)
        {
            //Arrange

            await SeedBasicData.SeedAddressAsync(_factory);

            var encodedPhrase = Uri.EscapeDataString(searchPhrase);

            //Act

            var response = await _client.GetAsync($"api/address?searchPhrase={encodedPhrase}&pageSize=5&pageNumber=1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.DeserializeAsync<PageResult<AddressDto>>();

            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
            result.Items.Should().BeEmpty();
        }

        [Theory]
        [InlineData(SortDirection.ASC)]
        [InlineData(SortDirection.DESC)]
        public async Task GetAllAddresses_WithSorting_ShouldReturnsSorted(SortDirection direction)
        {
            //Arrange

            await SeedBasicData.SeedAddressAsync(_factory);

            //Act

            var response = await _client.GetAsync($"api/address?pageNumber=1&pageSize=10&sortBy=Country&sortDirection={direction}");

            var content = await response.Content.ReadAsStringAsync();

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK, content);

            var result = await response.DeserializeAsync<PageResult<AddressDto>>();

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(3);

            var countries = result.Items.Select(c => c.Country).ToList();

            if (direction == SortDirection.ASC)
                countries.Should().BeInAscendingOrder();
            else 
                countries.Should().BeInDescendingOrder();
        }

        [Theory]
        [InlineData("Country")]
        [InlineData("City")]
        [InlineData("Street")]
        public async Task GetAllAdresses_WithDifferentSortColumns_ShouldWork(string sortBy)
        {
            //Arrange

            await SeedBasicData.SeedAddressAsync(_factory);

            //Act

            var response = await _client.GetAsync($"api/address?pageNumber=1&pageSize=10&sortBy={sortBy}&sortDirection=ASC");

            var content = await response.Content.ReadAsStringAsync();

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK, content);

            var result = await response.DeserializeAsync<PageResult<AddressDto>>();

            result.Items.Should().HaveCount(3);

            var values = sortBy switch
            {
                "Country" => result.Items.Select(x => x.Country),
                "City" => result.Items.Select(x => x.City),
                "Street" => result.Items.Select(x => x.Street),
                _ => throw new Exception("Invalid sort column")
            };

            values.Should().BeInAscendingOrder();
        }

        [Theory]
        [InlineData("Invalid")]
        [InlineData("123")]
        [InlineData("Country123")]
        public async Task GetAllAddresses_WithInvalidSortBy_ShouldReturnsBadRequest(string sortBy)
        {
            //Act

            var response = await _client.GetAsync($"api/address?sortBy={sortBy}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetAddressById_ReturnsOk_WithCorrectData()
        {
            //Arrange

            await SeedBasicData.SeedAddressAsync(_factory);

            //Act

            var response = await _client.GetAsync($"api/address/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<AddressDto>(content);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task GetAddressById_WhenAddressDoesNotExist_ShouldReturnsNotFound()
        {
            //Act

            var response = await _client.GetAsync("api/address/999");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetAddressById_ShouldAllowAnnonymousAccess()
        {
            //Arrange

            await SeedBasicData.SeedAddressAsync(_factory);

            var unauthorizedClient = _factory.CreateUnauthorizedClient();

            //Act

            var response = await unauthorizedClient.GetAsync($"api/company/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateAddress_WithValidModel_ShouldReturnsCreated()
        {
            //Arrange

            var dto = new CreateAddressBuilder().Build();

            //Act

            var response = await _client.PostAsync("api/address", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAddress_WithInvalidModel_ShouldReturnsBadRequest()
        {
            //Arrange

            var dto = new CreateAddressDto
            {
                Country = "Poland",
                City = "Warsaw",
                Street = "Wojska Polskiego 13c"
            };

            //Act

            var response = await _client.PostAsync("api/address", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadAsStringAsync();

            var problem = JsonConvert.DeserializeObject<ValidationProblemDetails>(content);

            problem.Errors.Should().ContainKey("PostalCode");
        }

        [Fact]
        public async Task CreateAddress_WhenUserIsNotAdmin_ShouldReturnsForbidden()
        {
            //Arrange

            var dto = new CreateAddressBuilder().Build();

            var client = _factory.CreateClientWithRole("User");

            //Act

            var response = await client.PostAsync("api/address", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateAddress_WhenUserIsNotLoggedIn_ShouldReturnsUnauthorized()
        {
            //Arrange

            var client = _factory.CreateUnauthorizedClient();

            //Act

            var response = await client.PostAsync("api/address", new CreateAddressDto().ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateAddress_WithValidModel_ShouldReturnsOk()
        {
            //Arrange

            await SeedBasicData.SeedAddressAsync(_factory);

            var dto = new UpdateAddressBuilder().Build();

            //Act

            var response = await _client.PutAsync($"api/address/1", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LogiTrackDbContext>();
            var updatedAddress = await dbContext.Addresses.FirstOrDefaultAsync(c => c.Id == 1);

            updatedAddress.Should().NotBeNull();
            updatedAddress.Country.Should().Be(dto.Country);
            updatedAddress.City.Should().Be(dto.City);
            updatedAddress.Street.Should().Be(dto.Street);
            updatedAddress.PostalCode.Should().Be(dto.PostalCode);
        }

        [Fact]
        public async Task UpdateAddress_WithInvalidModel_ShouldReturnsBadRequest()
        {
            //Arrange

            await SeedBasicData.SeedAddressAsync(_factory);

            var dto = new UpdateAddressDto
            {
                Country = "Polska",
                City = "Gdańsk",
                Street = "Dluga 1"
            };

            //Act

            var response = await _client.PutAsync($"api/address/1", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateAddress_WhenAddressDoesNotExist_ShouldReturnsNotFound()
        {
            //Arrange

            var dto = new UpdateAddressBuilder().Build();

            //Act

            var response = await _client.PutAsync("api/address/999", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateAddress_WhenUserIsNotAdmin_ShouldReturnsForbidden()
        {
            //Arrange

            await TestDbSeeder.SeedBasicAddresses(_factory);

            var dto = new UpdateAddressBuilder().Build();

            var client = _factory.CreateClientWithRole("User");

            //Act

            var response = await client.PutAsync($"api/address/1", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdateAddress_WhenUserIsNotLoggedIn_ShouldReturnsUnauthorized()
        {
            //Arrange

            await SeedBasicData.SeedAddressAsync(_factory);

            var dto = new UpdateAddressBuilder().Build();

            var unauthorizedClient = _factory.CreateUnauthorizedClient();

            //Act

            var response = await unauthorizedClient.PutAsync($"api/address/1", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteAddress_ForCompanyOwner_ShouldReturnsNoContent()
        {
            //Arrange

            await SeedBasicData.SeedAddressAsync(_factory);

            //Act

            var response = await _client.DeleteAsync($"api/address/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteAddress_ForNonCompanyOwner_ShouldReturnsForbidden()
        {
            //Arrange

            var address = new Address
            {
                Country = "Polska",
                City = "Gdańsk",
                Street = "Długa 1",
                PostalCode = "80-150",
                CreatedById = 999,
            };

            SeedAddress(address);

            //Act

            var response = await _client.DeleteAsync($"api/address/{address.Id}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteAddress_WhenUserIsNotAdmin_SholudReturnsForbidden()
        {
            //Arrange

            await SeedBasicData.SeedAddressAsync(_factory);

            var client = _factory.CreateClientWithRole("User");

            //Act

            var response = await client.DeleteAsync($"api/address/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteAddress_WhenUserIsNotLoggedIn_ShouldReturnUnauthorized()
        {
            //Arrange

            var client = _factory.CreateUnauthorizedClient();

            //Act

            var response = await client.DeleteAsync("api/address/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteAddress_WhenAddressDoesNotExist_ShouldReturnsNotFound()
        {
            //Act

            var response = await _client.DeleteAsync("api/address/999");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
