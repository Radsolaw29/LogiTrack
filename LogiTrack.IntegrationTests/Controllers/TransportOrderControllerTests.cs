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
using System.Net;

namespace LogiTrack.IntegrationTests.Controllers
{
    public class TransportOrderControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {

        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly string _databaseName = Guid.NewGuid().ToString();

        public TransportOrderControllerTests(WebApplicationFactory<Program> factory)
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
        public async Task GetAllTransportOrders_WithValidQueryParameters_ShouldReturnsOk(string queryParameters)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync($"/api/company/1/transportOrder?{queryParameters}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("pageSize=0&pageNumber=1")]
        [InlineData("pageSize=1&pageNumber=1")]
        [InlineData("pageSize=155&pageNumber=3")]
        [InlineData("")]
        [InlineData(null)]
        public async Task GetAllTransportOrders_WithInvalidQueryParameters_ShouldReturnsBadRequest(string queryParameters)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync($"/api/company/1/transportOrder?{queryParameters}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("First order")]
        [InlineData("Second order")]
        [InlineData("Third order")]
        [InlineData("Description test")]
        [InlineData("Description test 2")]
        [InlineData("Description test 3")]
        public async Task GetAllTransportOrders_WithSearchPhrase_ShouldReturnsMatchingResults(string searchPhrase)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var encodedPhrase = Uri.EscapeDataString(searchPhrase);

            //Act

            var response = await _client.GetAsync($"/api/company/1/transportOrder?searchPhrase={encodedPhrase}&pageSize=5&pageNumber=1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.DeserializeAsync<PageResult<TransportOrderDto>>();
            result.Items.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();

            result.Items.Should().OnlyContain(c =>
                c.OrderName.ToLower().Contains(searchPhrase.ToLower()) ||
                c.Description.ToLower().Contains(searchPhrase.ToLower()));
        }

        [Theory]
        [InlineData("zzz")]
        [InlineData("notexisting")]
        [InlineData("123456")]
        public async Task GetAllTransportOrders_WithBadSearchPhrase_ShouldReturnsEmpty(string searchPhrase)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var encodedPhrase = Uri.EscapeDataString(searchPhrase);

            //Act

            var response = await _client.GetAsync($"/api/company/1/transportOrder?searchPhrase={encodedPhrase}&pageSize=5&pageNumber=1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.DeserializeAsync<PageResult<TransportOrderDto>>();

            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
            result.Items.Should().BeEmpty();
        }

        [Theory]
        [InlineData(SortDirection.ASC)]
        [InlineData(SortDirection.DESC)]
        public async Task GetAllTransportOrders_WithSorting_ShouldReturnsSorted(SortDirection direction)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync($"/api/company/1/transportOrder?pageSize=5&pageNumber=1&sortBy=OrderName&sortDirection={direction}");

            var content = await response.Content.ReadAsStringAsync();

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK, content);

            var result = await response.DeserializeAsync<PageResult<TransportOrderDto>>();

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(3);

            var orderNames = result.Items.Select(c => c.OrderName).ToList();

            if (direction == SortDirection.ASC)
                orderNames.Should().BeInAscendingOrder();
            else
                orderNames.Should().BeInDescendingOrder();
        }

        [Theory]
        [InlineData("OrderName")]
        [InlineData("Description")]
        public async Task GetAllTransportOrders_WithDifferentSortColumns_ShouldWork(string sortBy)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync($"/api/company/1/transportOrder?pageSize=5&pageNumber=1&sortBy={sortBy}");

            var content = await response.Content.ReadAsStringAsync();

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK, content);

            var result = await response.DeserializeAsync<PageResult<TransportOrderDto>>();

            result.Items.Should().HaveCount(3);

            var values = sortBy switch
            {
                "OrderName" => result.Items.Select(x => x.OrderName),
                "Description" => result.Items.Select(x => x.Description),
                _ => throw new Exception("Invalid sort column")
            };

            values.Should().BeInAscendingOrder();
        }

        [Theory]
        [InlineData("Invalid")]
        [InlineData("123")]
        [InlineData("OrderName123")]
        public async Task GetAllTransportOrders_WithInvalidSortBy_ShouldReturnsBadRequest(string sortBy)
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync($"/api/company/1/transportOrder?pageSize=5&pageNumber=1&sortBy={sortBy}");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetTransportOrderById_ShouldReturnsOk_WithCorrectData()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.GetAsync($"/api/company/1/transportOrder/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.DeserializeAsync<TransportOrderDto>();

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.OrderName.Should().Be("First order");
        }

        [Fact]
        public async Task GetTransportOrderById_WhenTransportOrderDoesNotExist_ShouldReturnsNotFound()
        {
            //Act

            var response = await _client.GetAsync("/api/company/1/transportOrder/999");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetTransportOrderById_ShouldAllowAnonymousAccess()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var unauthorizedClient = _factory.CreateUnauthorizedClient();

            //Act

            var response = await unauthorizedClient.GetAsync("/api/company/1/transportOrder/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateTransportOrder_WithValidModel_ShouldReturnsCreated()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new CreateTransportOrderBuilder().Build();

            //Act

            var response = await _client.PostAsync("/api/company/1/transportOrder", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateTransportOrder_WithInvalidModel_ShouldReturnsBadRequest()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new CreateTransportOrderDto
            {
                OrderName = "Test invalid model",
                Description = "Invalid",
                Price = 1500
            };

            //Act

            var response = await _client.PostAsync("/api/company/1/transportOrder", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var problem = await response.DeserializeAsync<ValidationProblemDetails>();

            problem.Errors.Should().ContainKey(nameof(CreateTransportOrderDto.PickupAddressId));
            problem.Errors.Should().ContainKey(nameof(CreateTransportOrderDto.DeliveryAddressId));
            problem.Errors.Should().ContainKey(nameof(CreateTransportOrderDto.DriverId));
            problem.Errors.Should().ContainKey(nameof(CreateTransportOrderDto.TruckId));
        }

        [Fact]
        public async Task CreateTransportOrder_WhenUserIsNotAdmin_ShouldReturnsForbidden()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new CreateTransportOrderBuilder().Build();

            var client = _factory.CreateClientWithRole("User");

            //Act

            var response = await client.PostAsync("/api/company/1/transportOrder", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateTransportOrder_WhenUserIsNotLoggedIn_ShouldReturnUnauthorized()
        {
            //Arrange

            var client = _factory.CreateUnauthorizedClient();

            //Act

            var response = await client.PostAsync("/api/company/1/transportOrder", new CreateCompanyDto().ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateTransportOrder_WithValidModel_ShouldReturnsOk()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new UpdateTransportOrderBuilder().Build();

            //Act

            var response = await _client.PutAsync($"/api/company/1/transportOrder/1", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LogiTrackDbContext>();
            var updateTransportOrder = await dbContext.Orders.FirstOrDefaultAsync(c => c.Id == 1);

            updateTransportOrder.Should().NotBeNull();
            updateTransportOrder.OrderName.Should().Be(dto.OrderName);
            updateTransportOrder.Description.Should().Be(dto.Description);
            updateTransportOrder.Price.Should().Be(dto.Price);
        }

        [Fact]
        public async Task UpdateTransportOrder_WithInvalidModel_ShouldReturnsBadRequest()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new UpdateTransportOrder
            {
                Description = "Description",

            };

            //Act

            var response = await _client.PutAsync($"/api/company/1/transportOrder/1", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateTransportOrder_WhenTransportOrderDoesNotExist_ShouldReturnsNotFound()
        {
            //Arrange

            var dto = new UpdateTransportOrderBuilder().Build();

            //Act

            var response = await _client.PutAsync($"/api/company/1/transportOrder/999", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateTransportOrder_WhenUserIsNotAdmin_ShouldReturnsForbidden()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new UpdateTransportOrderBuilder().Build();

            var client = _factory.CreateClientWithRole("User");

            //Act

            var response = await client.PutAsync($"/api/company/1/transportOrder/1", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdateTransportOrder_WhenUserIsNotLoggedIn_shouldReturnsUnauthorized()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var dto = new UpdateTransportOrderBuilder().Build();

            var unauthorizedClient = _factory.CreateUnauthorizedClient();

            //Act

            var response = await unauthorizedClient.PutAsync($"/api/company/1/transportOrder/1", dto.ToJsonHttpContent());

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteTransportOrder_ForCompanyOwner_ShouldReturnsNoContent()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.DeleteAsync("/api/company/1/transportOrder/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteTransportOrder_ForNonCompanyOwner_ShouldReturnsForbidden()
        {
            //Arrange

            await TestDbSeeder.ResetDatabase(_factory);
            await TestDbSeeder.SeedBasicOrderDependencies(_factory);

            var company = new Company
            {
                Id = 1,
                Name = "Other Company",
                CreatedById = 999
            };

            await _factory.SeedAsync(company);

            var transportOrder = new TransportOrderBuilder().WithId(1).Build();

            transportOrder.CompanyId = 1;
            transportOrder.CreatedById = 999;

            await _factory.SeedAsync(transportOrder);

            //Act

            var response = await _client.DeleteAsync($"/api/company/1/transportOrder/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteTransportOrder_WhenUserIsNotAdmin_ShouldReturnsForbidden()
        {
            //Arrange

            await SeedBasicData.SeedAsync(_factory);

            var client = _factory.CreateClientWithRole("User");

            //Act

            var response = await client.DeleteAsync("/api/company/1/transportOrder/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeteleTransportOrder_WhenUserIsNotLoggedIn_ShouldReturnsUnauthorized()
        {
            //Arrange

            var client = _factory.CreateUnauthorizedClient();

            //Act

            var response = await client.DeleteAsync("/api/company/1/transportOrder/1");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteTransportOrder_WhenTransportOrderNotExist_ShouldReturnsNotFound()
        {
            //Assert

            await SeedBasicData.SeedAsync(_factory);

            //Act

            var response = await _client.DeleteAsync("/api/company/1/transportOrder/999");

            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}