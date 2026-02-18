using FluentAssertions;
using LogiTrack.Entities;
using LogiTrack.IntegrationTests.Helpers;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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

                        services.AddMvc(option => option.Filters.Add(new FakeUserFilter()));

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
        public async Task GetAllCompanies_WithValidQueryParameters_ReturnsOkResult(string queryParameters)
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
        public async Task GetAllCompanies_WithInvalidQueryParameters_ShouldReturnsBadRequestResult(string queryParameters)
        {
            //Act

            var response = await _client.GetAsync($"api/company?{queryParameters}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateCompany_WithValidModel_ShouldReturnsCreatedResult()
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
        public async Task DeleteCompany_WhenCompanyNotExist_ShouldReturnsNotFoundException()
        {
            //Act

            var response = await _client.DeleteAsync("api/company/999");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

    }
}
