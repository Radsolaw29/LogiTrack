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
    public class CompanyControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly string _databaseName = Guid.NewGuid().ToString();

        public CompanyControllerTests(WebApplicationFactory<Program> factory)
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

        private void SeedCompany(Company company)
        {
            var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetService<LogiTrackDbContext>();

            _dbContext.Companies.Add(company);
            _dbContext.SaveChanges();
        }

        [Theory]
        [InlineData("pageSize=5&pageNumber=1")]
        [InlineData("pageSize=10&pageNumber=2")]
        [InlineData("pageSize=20&pageNumber=3")]
        [InlineData("pageSize=50&pageNumber=4")]
        public async Task GetAllCompanies_WithValidQueryParameters_ReturnsOk(string queryParameters)
        {
            //Act

            var response = await _client.GetAsync($"api/company?{queryParameters}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("pageSize=0&pageNumber=1")]
        [InlineData("pageSize=1&pageNumber=1")]
        [InlineData("pageSize=155&pageNumber=3")]
        [InlineData("")]
        [InlineData(null)]
        public async Task GetAllCompanies_WithInvalidQueryParameters_ShouldReturnsBadRequest(string queryParameters)
        {
            //Act

            var response = await _client.GetAsync($"api/company?{queryParameters}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("Test name123")]
        [InlineData("Test name456")]
        [InlineData("Test name789")]
        [InlineData("Test desc 1")]
        [InlineData("Test desc 2")]
        [InlineData("Test desc 3")]
        public async Task GetAllCompanies_WithSearchPhrase_ShouldReturnsMatchingResults(string searchPhrase)
        {
            //Arrange

            await TestDbSeeder.ResetDatabase(_factory);
            await TestDbSeeder.SeedBasicCompanies(_factory);

            var encodedPhrase = Uri.EscapeDataString(searchPhrase);

            var url = $"api/company?searchPhrase={encodedPhrase}&pageSize=5&pageNumber=1";

            //Act

            var response = await _client.GetAsync(url);

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<PageResult<CompanyDto>>(content);
            result.Items.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();

            result.Items.Should().OnlyContain(c =>
                c.Name.ToLower().Contains(searchPhrase.ToLower()) ||
                c.Description.ToLower().Contains(searchPhrase.ToLower()));
        }

        [Theory]
        [InlineData("zzz")]
        [InlineData("notexisting")]
        [InlineData("123456")]
        public async Task GetAllCompanies_WithSearchPhrase_ShouldReturnEmpty(string searchPhrase)
        {
            // Arrange

            await TestDbSeeder.ResetDatabase(_factory);
            await TestDbSeeder.SeedBasicCompanies(_factory);

            var encodedPhrase = Uri.EscapeDataString(searchPhrase);

            var url = $"api/company?searchPhrase={encodedPhrase}&pageSize=5&pageNumber=1";

            // Act

            var response = await _client.GetAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<PageResult<CompanyDto>>(content);

            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
            result.Items.Should().BeEmpty();
        }

        [Theory]
        [InlineData(SortDirection.ASC)]
        [InlineData(SortDirection.DESC)]
        public async Task GetAllCompanies_WithSorting_ShouldReturnSorted(SortDirection direction)
        {
            // Arrange

            await TestDbSeeder.ResetDatabase(_factory);
            await TestDbSeeder.SeedBasicCompanies(_factory);

            // Act

            var response = await _client.GetAsync($"api/company?pageNumber=1&pageSize=10&sortBy=Name&sortDirection={direction}");

            var content = await response.Content.ReadAsStringAsync();

            // Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK, content);

            var result = JsonConvert.DeserializeObject<PageResult<CompanyDto>>(content);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(3);

            var names = result.Items.Select(c => c.Name).ToList();

            if (direction == SortDirection.ASC)
                names.Should().BeInAscendingOrder();
            else
                names.Should().BeInDescendingOrder();
        }

        [Theory]
        [InlineData("Name")]
        [InlineData("Description")]
        [InlineData("ContactEmail")]
        public async Task GetAllCompanies_WithDifferentSortColumns_ShouldWork(string sortBy)
        {
            // Arrange

            await TestDbSeeder.ResetDatabase(_factory);
            await TestDbSeeder.SeedBasicCompanies(_factory);

            // Act

            var response = await _client.GetAsync($"api/company?pageNumber=1&pageSize=10&sortBy={sortBy}&sortDirection=ASC");

            var content = await response.Content.ReadAsStringAsync();

            // Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK, content);

            var result = JsonConvert.DeserializeObject<PageResult<CompanyDto>>(content);

            result.Items.Should().HaveCount(3);

            var values = sortBy switch
            {
                "Name" => result.Items.Select(x => x.Name),
                "Description" => result.Items.Select(x => x.Description),
                "ContactEmail" => result.Items.Select(x => x.ContactEmail),
                _ => throw new Exception("Invalid sort column")
            };

            values.Should().BeInAscendingOrder();
        }

        [Theory]
        [InlineData("Invalid")]
        [InlineData("123")]
        [InlineData("Name123")]
        public async Task GetAllCompanies_WithInvalidSortBy_ShouldReturnBadRequest(string sortBy)
        {
            // Act

            var response = await _client.GetAsync($"api/company?sortBy={sortBy}");

            // Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetCompanyById_ReturnsOk_WithCorrectData()
        {
            // Arrange

            var address = new Address
            {
                Country = "Poland",
                City = "Warsaw",
                Street = "Test 1",
                PostalCode = "00-001"
            };

            var company = new Company
            {
                Name = "Test Company",
                Description = "Test Description",
                TaxNumber = 123456789,
                PhoneNumber = 123456789,
                ContactEmail = "test@test.com",
                CreatedById = 1,
                Address = address
            };

            SeedCompany(company);

            // Act

            var response = await _client.GetAsync($"api/company/{company.Id}");

            // Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<CompanyDto>(content);

            result.Should().NotBeNull();
            result.Id.Should().Be(company.Id);
            result.Name.Should().Be(company.Name);
        }

        [Fact]
        public async Task GetCompanyById_WhenComapnyDoesNotExist_ShouldReturnsNotFound()
        {
            //Act

            var response = await _client.GetAsync("api/company/999");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetCompanyById_ShouldAllowAnonymousAccess()
        {
            //Arrange

            await TestDbSeeder.ResetDatabase(_factory);
            await TestDbSeeder.SeedBasicCompanies(_factory);

            var unauthorizedClient = _factory.CreateUnauthorizedClient();

            //Act

            var response = await unauthorizedClient.GetAsync($"api/company/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateCompany_WithValidModel_ShouldReturnsCreated()
        {
            //Arrange

            var dto = new CreateCompanyDto()
            {
                Name = "CompanyTest123",
                Description = "Description Test123",
                TaxNumber = 1235469877,
                PhoneNumber = 111222333,
                ContactEmail = "testcompany@wp.pl",
                Country = "Poland",
                City = "Sopot",
                Street = "Łokietka 17c/1",
                PostalCode = "12345"
            };

            var httpContent = dto.ToJsonHttpContent();

            //Act

            var response = await _client.PostAsync("api/company", httpContent);

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateCompany_WithInvalidModel_ShouldReturnsBadRequest()
        {
            //Arrange

            var dto = new CreateCompanyDto()
            {
                Description = "Test decription",
                PhoneNumber = 111222333,
                ContactEmail = "test@test.pl"
            };

            var httpContent = dto.ToJsonHttpContent();

            //Act

            var response = await _client.PostAsync("api/company", httpContent);

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadAsStringAsync();

            var problem = JsonConvert.DeserializeObject<ValidationProblemDetails>(content);

            problem.Errors.Should().ContainKey("Name");
            problem.Errors.Should().ContainKey("PostalCode");
        }

        [Fact]
        public async Task CreateCompany_WhenUserIsNotAdmin_ShouldReturnsForbidden()
        {
            //Arrange

            var dto = new CreateCompanyDto()
            {
                Name = "Test company - NoAdmin",
                Description = "Test description",
                TaxNumber = 111222333,
                PhoneNumber = 444555667,
                ContactEmail = "test@test.pl",
                Country = "Poland",
                City = "Sopot",
                Street = "Pańska 7",
                PostalCode = "70-874"
            };

            var client = _factory.CreateClientWithRole("User");

            //Act

            var response = await client.PostAsync("api/company", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateCompany_WhenUserIsNotLoggedIn_ShouldReturnUnauthorized()
        {
            //Arrange

            var client = _factory.CreateUnauthorizedClient();

            //Act

            var response = await client.PostAsync("api/company", new CreateCompanyDto().ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateCopmany_WithValidModel_ShouldReturnsOk()
        {
            //Arrange

            var company = new Company
            {
                Name = "Test company name 123",
                Description = "Test description",
                TaxNumber = 123654123,
                PhoneNumber = 555444777,
                ContactEmail = "test@wp.pl",
                CreatedById = 1
            };

            SeedCompany(company);

            var dto = new UpdateCompanyDto
            {
                Name = "Test company update",
                Description = "Test description",
                TaxNumber = 111111111,
                ContactEmail = "testupdate@wp.pl"
            };

            //Act

            var response = await _client.PutAsync($"api/company/{company.Id}", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LogiTrackDbContext>();
            var updatedCompany = await dbContext.Companies.FirstOrDefaultAsync(c => c.Id == company.Id);

            updatedCompany.Should().NotBeNull();
            updatedCompany.Name.Should().Be(dto.Name);
            updatedCompany.Description.Should().Be(dto.Description);
            updatedCompany.TaxNumber.Should().Be(dto.TaxNumber);
            updatedCompany.ContactEmail.Should().Be(dto.ContactEmail);
        }

        [Fact]
        public async Task UpdateCompany_WithInvalidModel_ShouldReturnsBadRequest()
        {
            //Arrange

            var company = new Company
            {
                CreatedById = 1,
                Description = "Test desctription"
            };

            SeedCompany(company);

            var dto = new UpdateCompanyDto
            {
                Description = "Comapny name"
            };

            //Act

            var response = await _client.PutAsync($"api/company/{company.Id}", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateCompany_WhenCompanyDoesNotExist_ShouldReturnsNotFound()
        {
            //Arrange

            var dto = new UpdateCompanyDto
            {
                Name = "test company name"
            };

            //Act

            var response = await _client.PutAsync("api/company/999", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateCompany_WhenUserIsNotAdmin_ShouldReturnsForbidden()
        {
            //Arrange

            var company = new Company
            {
                CreatedById = 1,
                Name = "Test company name"
            };

            SeedCompany(company);

            var dto = new UpdateCompanyDto
            {
                Name = "update company name"
            };

            var client = _factory.CreateClientWithRole("User");

            //Act

            var response = await client.PutAsync($"api/company/{company.Id}", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdateCompany_WhenUserIsNotLoggedIn_ShouldReturnsUnauthorized()
        {
            //Arrange

            var company = new Company
            {
                CreatedById = 1,
                Name = "Test company name"
            };

            SeedCompany(company);

            var dto = new UpdateCompanyDto
            {
                Name = "New company name"
            };

            var unauthorizedClient = _factory.CreateUnauthorizedClient();

            //Act

            var response = await unauthorizedClient.PutAsync($"api/company/{company.Id}", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteCompany_ForCompanyOwner_ShouldReturnsNoContent()
        {
            //Arrange

            var company = new Company
            {
                CreatedById = 1,
                Name = "Test company name"
            };

            SeedCompany(company);

            //Act

            var response = await _client.DeleteAsync($"api/company/{company.Id}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteCompany_ForNonCompanyOwner_ShouldReturnsForbidden()
        {
            //Arrange

            var company = new Company
            {
                CreatedById = 999,
                Name = "Test company name"
            };

            SeedCompany(company);

            //Act

            var response = await _client.DeleteAsync($"api/company/{company.Id}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteCompany_WhenUserIsNotAdmin_SholudReturnsForbidden()
        {
            //Arrange

            var client = _factory.CreateClientWithRole("User");

            //Act

            var response = await client.DeleteAsync($"api/company/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteCompany_WhenUserIsNotLoggedIn_ShouldReturnUnauthorized()
        {
            //Arrange

            var client = _factory.CreateUnauthorizedClient();

            //Act

            var response = await client.DeleteAsync("api/company/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteCompany_WhenCompanyNotExist_ShouldReturnsNotFoundException()
        {
            //Act

            var response = await _client.DeleteAsync("api/company/999");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}