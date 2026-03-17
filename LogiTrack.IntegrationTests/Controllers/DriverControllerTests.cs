using FluentAssertions;
using LogiTrack.Entities;
using LogiTrack.IntegrationTests.Builders;
using LogiTrack.IntegrationTests.Extensions;
using LogiTrack.IntegrationTests.Helpers;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace LogiTrack.IntegrationTests.Controllers
{
    public class DriverControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {

        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly string _databaseName = Guid.NewGuid().ToString();

        public DriverControllerTests(WebApplicationFactory<Program> factory)
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
        public async Task GetAllDrivers_WithValidQueryParameters_ShouldReturnsOk(string queryParameters)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync($"/api/company/1/driver?{queryParameters}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("pageSize=0&pageNumber=1")]
        [InlineData("pageSize=1&pageNumber=1")]
        [InlineData("pageSize=155&pageNumber=3")]
        [InlineData("")]
        [InlineData(null)]
        public async Task GetAllDrivers_WithInvalidQueryParameters_ShouldReturnsBadRequest(string queryParameters)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync($"/api/company/1/driver?{queryParameters}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("Jan")]
        [InlineData("Karol")]
        [InlineData("Adam")]
        [InlineData("Kowalski")]
        [InlineData("Podolski")]
        [InlineData("Cymbal")]
        [InlineData("Jan@wp.pl")]
        [InlineData("Karol@wp.pl")]
        [InlineData("Adam@wp.pl")]
        public async Task GetAllDrivers_WithSearchPhrase_ShouldReturnsMatchingResults(string searchPhrase)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var encodedPhrase = Uri.EscapeDataString(searchPhrase);

            //Act

            var response = await _client.GetAsync($"/api/company/1/driver?searchPhrase={encodedPhrase}&pageSize=5&pageNumber=1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.DeserializeAsync<PageResult<DriverDto>>();

            result.Items.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();

            result.Items.Should().OnlyContain(c =>
                c.FirstName.ToLower().Contains(searchPhrase.ToLower()) ||
                c.LastName.ToLower().Contains(searchPhrase.ToLower()) ||
                c.ContactEmail.ToLower().Contains(searchPhrase.ToLower()));
        }

        [Theory]
        [InlineData("zzz")]
        [InlineData("notexisting")]
        [InlineData("123456")]
        public async Task GetAllDrivers_WithBadSearchPhrase_ShouldReturnsEmpty(string searchPhrase)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var encodedPhrase = Uri.EscapeDataString(searchPhrase);

            //Act

            var response = await _client.GetAsync($"/api/company/1/driver?searchPhrase={encodedPhrase}&pageSize=5&pageNumber=1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.DeserializeAsync<PageResult<DriverDto>>();

            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
            result.Items.Should().BeEmpty();
        }

        [Theory]
        [InlineData(SortDirection.ASC)]
        [InlineData(SortDirection.DESC)]
        public async Task GetAllDrivers_WithSorting_ShouldReturnsSorted(SortDirection direction)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync($"/api/company/1/driver?pageSize=5&pageNumber=1&sortBy=FirstName&sortDirection={direction}");

            var content = await response.Content.ReadAsStringAsync();

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK, content);

            var result = await response.DeserializeAsync<PageResult<DriverDto>>();

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(3);

            var firstNames = result.Items.Select(c => c.FirstName).ToList();

            if (direction == SortDirection.ASC)
                firstNames.Should().BeInAscendingOrder();
            else
                firstNames.Should().BeInDescendingOrder();
        }

        [Theory]
        [InlineData("FirstName")]
        [InlineData("LastName")]
        [InlineData("ContactEmail")]
        public async Task GetAllDrivers_WithDifferentSortColumns_ShouldWork(string sortBy)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //act

            var response = await _client.GetAsync($"/api/company/1/driver?pageSize=5&pageNumber=1&sortBy={sortBy}");

            var content = await response.Content.ReadAsStringAsync();

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK, content);

            var result = await response.DeserializeAsync<PageResult<DriverDto>>();

            result.Items.Should().HaveCount(3);

            var values = sortBy switch
            {
                "FirstName" => result.Items.Select(x => x.FirstName),
                "LastName" => result.Items.Select(x => x.LastName),
                "ContactEmail" => result.Items.Select(x => x.ContactEmail),
                _ => throw new Exception("Invalid sort column")
            };

            values.Should().BeInAscendingOrder();
        }

        [Theory]
        [InlineData("Invalid")]
        [InlineData("123")]
        [InlineData("FirstName123")]
        public async Task GetAllDrivers_WithInvalidSortBy_ShouldReturnsBadRequest(string sortBy)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync($"/api/company/1/driver?pageSize=5&pageNumber=1&sortBy={sortBy}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetDriverById_ShouldReturnsOk_WithCorrectData()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync("/api/company/1/driver/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.DeserializeAsync<DriverDto>();

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.FirstName.Should().Be("Jan");
        }

        [Fact]
        public async Task GetDriverById_WhenDriverDoesNotExist_ShouldReturnsNotFound()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync("/api/company/1/driver/999");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetDriverById_ShouldAllAnonymousAccess()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var unauthorizedClient = _factory.CreateUnauthorizedClient();

            //Act

            var response = await _client.GetAsync("/api/company/1/driver/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateDriver_WithValidModel_ShouldReturnsCreated()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new CreateDriverBuilder().Build();

            //Act

            var response = await _client.PostAsync("/api/company/1/driver", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateDriver_WithInvalidModel_ShouldReturnsBadRequest()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new CreateDriverDto
            {
                LastName = "Nowak",
                ContactEmail = "Janek@wp.pl"
            };

            //Act

            var response = await _client.PostAsync("/api/company/1/driver", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var problem = await response.DeserializeAsync<ValidationProblemDetails>();

            problem.Errors.Should().ContainKey(nameof(CreateDriverDto.FirstName));
            problem.Errors.Should().ContainKey(nameof(CreateDriverDto.PersonalNumber));
            problem.Errors.Should().ContainKey("companyId");
        }

        [Fact]
        public async Task CreateDriver_WhenUserIsNotAdmin_ShouldReturnsForbidden()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new CreateDriverBuilder().Build();

            var client = _factory.CreateClientWithRole("User");

            //Act

            var response = await client.PostAsync("/api/company/1/driver", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateDriver_WhenUserIsNotLoggedIn_ShouldReturnsUnauthorized()
        {
            //Arrange

            var client = _factory.CreateUnauthorizedClient();

            //Act

            var response = await client.PostAsync("/api/company/1/driver", new CreateDriverDto().ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateDriver_WithValidModel_ShouldReturnsOk()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new UpdateDriverBuilder().Build();

            //Act

            var response = await _client.PutAsync("/api/company/1/driver/1", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LogiTrackDbContext>();
            var updatedDriver = await dbContext.Drivers.FirstOrDefaultAsync(x => x.Id == 1);

            updatedDriver.Should().NotBeNull();
            updatedDriver.FirstName.Should().Be(dto.FirstName);
            updatedDriver.LastName.Should().Be(dto.LastName);
            updatedDriver.PhoneNumber.Should().Be(dto.PhoneNumber);
            updatedDriver.DateOfBirth.Should().Be(dto.DateOfBirth);
            updatedDriver.LicenseDriving.Should().Be(dto.LicenseDriving);
            updatedDriver.PhoneNumber.Should().Be(dto.PhoneNumber);
            updatedDriver.ContactEmail.Should().Be(dto.ContactEmail);
        }

        [Fact]
        public async Task UpdateDriver_WithInvalidModel_ShouldReturnsBadRequest()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new UpdateDriverDto
            {
                LastName = "Kołatko",
                ContactEmail = "michałkołotko@wp.pl"
            };

            //Act

            var response = await _client.PutAsync("/api/company/1/driver/1", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateDriver_WhenDriverDoesNotExist_ShouldReturnsNotFound()
        {
            //Arrange

            var dto = new UpdateDriverBuilder().Build();

            //Act

            var response = await _client.PutAsync("/api/company/1/driver/999", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateDriver_WhenUserIsNotAdmin_ShouldReturnsForbidden()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new UpdateDriverBuilder().Build();

            var client = _factory.CreateClientWithRole("User");

            //Act

            var response = await client.PutAsync("/api/company/1/driver/1", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdateDriver_WhenUserIsNotLoggedIn_ShouldReturnsUnauthorized()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new UpdateDriverBuilder().Build();

            var ununauthorizedClient = _factory.CreateUnauthorizedClient();

            //Act

            var response = await ununauthorizedClient.PutAsync("/api/company/1/driver/1", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteDriver_ForComapnyOwner_ShouldReturnsNoCOntent()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.DeleteAsync("/api/company/1/driver/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteDriver_ForNonComapnyOwner_ShouldReturnsForbidden()
        {
            //Arrange

            var company = new Company
            {
                Id = 1,
                Name = "Other Company",
                CreatedById = 999
            };

            await _factory.SeedAsync(company);

            var driver = new DriverBuilder().WithId(1).Build();

            driver.CompanyId = 1;
            driver.CreatedById = 999;

            await _factory.SeedAsync(driver);

            //Act

            var response = await _client.DeleteAsync("/api/company/1/driver/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        }

        [Fact]
        public async Task DeleteDriver_WhenUserIsNotAdmin_ShouldReturnsForbidden()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var client = _factory.CreateClientWithRole("User");

            //Act

            var response = await client.DeleteAsync("/api/company/1/driver/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteDriver_WhenUserIsNotLoggedIn_ShouldReturnsUnauthorized()
        {
            //Arrange

            var client = _factory.CreateUnauthorizedClient();

            //Act

            var response = await client.DeleteAsync("/api/company/1/driver/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteDriver_WhenDriverDoesNotExist_ShouldReturnsNotFound()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.DeleteAsync("/api/company/1/driver/999");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
