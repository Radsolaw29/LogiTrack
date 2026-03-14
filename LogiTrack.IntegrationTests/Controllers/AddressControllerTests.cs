using FluentAssertions;
using LogiTrack.Entities;
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

            await TestDbSeeder.ResetDatabase(_factory);
            await TestDbSeeder.SeedBasicAddresses(_factory);

            var encodedPhrase = Uri.EscapeDataString(searchPhrase);

            var url = $"api/address?searchPhrase={encodedPhrase}&pageSize=5&pageNumber=1";

            //Act

            var response = await _client.GetAsync(url);

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<PageResult<AddressDto>>(content);
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

            await TestDbSeeder.ResetDatabase(_factory);
            await TestDbSeeder.SeedBasicAddresses(_factory);

            var encodedPhrase = Uri.EscapeDataString(searchPhrase);

            var url = $"api/address?searchPhrase={encodedPhrase}&pageSize=5&pageNumber=1";

            //Act

            var response = await _client.GetAsync(url);

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<PageResult<AddressDto>>(content);

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

            await TestDbSeeder.ResetDatabase(_factory);
            await TestDbSeeder.SeedBasicAddresses(_factory);

            //Act

            var response = await _client.GetAsync($"api/address?pageNumber=1&pageSize=10&sortBy=Country&sortDirection={direction}");

            var content = await response.Content.ReadAsStringAsync();

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK, content);

            var result = JsonConvert.DeserializeObject<PageResult<AddressDto>>(content);

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
            await TestDbSeeder.ResetDatabase(_factory);
            await TestDbSeeder.SeedBasicAddresses(_factory);

            var url = $"api/address?pageNumber=1&pageSize=10&sortBy={sortBy}&sortDirection=ASC";

            //Act

            var response = await _client.GetAsync(url);

            var content = await response.Content.ReadAsStringAsync();

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK, content);

            var result = JsonConvert.DeserializeObject<PageResult<AddressDto>>(content);

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
            //Arrange

            var url = $"api/address?sortBy={sortBy}";

            //Act

            var response = await _client.GetAsync(url);

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetAddressById_ReturnsOk_WithCorrectData()
        {
            //Arrange

            var address = new Address
            {
                Country = "Poland",
                City = "Warsaw",
                Street = "Test 1",
                PostalCode = "00-001"
            };

            SeedAddress(address);

            //Act

            var response = await _client.GetAsync($"api/address/{address.Id}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<AddressDto>(content);

            result.Should().NotBeNull();
            result.Id.Should().Be(address.Id);
            result.Street.Should().Be(address.Street);
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

            await TestDbSeeder.ResetDatabase(_factory);
            await TestDbSeeder.SeedBasicAddresses(_factory);

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

            var dto = new CreateAddressDto
            {
                Country = "Poland",
                City = "Warsaw",
                Street = "Test 1",
                PostalCode = "00-001"
            };

            var httpContent = dto.ToJsonHttpContent();

            //Act

            var response = await _client.PostAsync("api/address", httpContent);

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

            var httpContent = dto.ToJsonHttpContent();

            //Act

            var response = await _client.PostAsync("api/address", httpContent);

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

            var dto = new CreateAddressDto
            {
                Country = "Poland",
                City = "Warsaw",
                Street = "Wojska Polskiego 13c",
                PostalCode = "11-256"
            };

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

            var address = new Address
            {
                Country = "Poland",
                City = "Sopot",
                Street = "Polna 12",
                PostalCode = "22-877",
                CreatedById = 1
            };

            SeedAddress(address);

            var dto = new UpdateAddressDto
            {
                Country = "Poland",
                City = "Warszawa",
                Street = "Wojska Polskiego 12",
                PostalCode = "00-001"
            };

            //Act

            var response = await _client.PutAsync($"api/address/{address.Id}", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LogiTrackDbContext>();
            var updatedAddress = await dbContext.Addresses.FirstOrDefaultAsync(c => c.Id == address.Id);

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

            await TestDbSeeder.ResetDatabase(_factory);
            await TestDbSeeder.SeedBasicAddresses(_factory);

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

            var dto = new UpdateAddressDto
            {
                Country = "Poland",
                City = "Sopot",
                Street = "Łokietka 1",
                PostalCode = "80-123"
            };

            //Act

            var response = await _client.PutAsync("api/address/999", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateAddress_WhenUserIsNotAdmin_ShouldReturnsForbidden()
        {
            //Arrange

            await TestDbSeeder.ResetDatabase(_factory);
            await TestDbSeeder.SeedBasicAddresses(_factory);

            var dto = new UpdateAddressDto
            {
                Country = "Polska",
                City = "Gdańsk",
                Street = "Długa 1",
                PostalCode = "80-150"
            };

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

            await TestDbSeeder.ResetDatabase(_factory);
            await TestDbSeeder.SeedBasicAddresses(_factory);

            var dto = new UpdateAddressDto
            {
                Country = "Polska",
                City = "Gdańsk",
                Street = "Długa 1",
                PostalCode = "80-150"
            };

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

            await TestDbSeeder.ResetDatabase(_factory);
            await TestDbSeeder.SeedBasicAddresses(_factory);

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

            await TestDbSeeder.ResetDatabase(_factory);
            await TestDbSeeder.SeedBasicAddresses(_factory);

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
