using AutoMapper;
using Castle.Core.Logging;
using FluentAssertions;
using LogiTrack.Entities;
using LogiTrack.Exceptions;
using LogiTrack.Interfaces;
using LogiTrack.Models;
using LogiTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace LogiTrack.Tests.Services
{
    public class TransportOrderServiceTests
    {
        private readonly LogiTrackDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly TransportOrderService _sut;
        private readonly Mock<IUserContextService> _userContextServiceMock;
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;

        public TransportOrderServiceTests()
        {

            var options = new DbContextOptionsBuilder<LogiTrackDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new LogiTrackDbContext(options);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TransportOrder, TransportOrderDto>();
                cfg.CreateMap<CreateTransportOrderDto, TransportOrder>();
            });

            _mapper = mapperConfig.CreateMapper();

            _authorizationServiceMock = new Mock<IAuthorizationService>();

            _userContextServiceMock = new Mock<IUserContextService>();

            _userContextServiceMock.Setup(x => x.GetUserId).Returns(1);

            _userContextServiceMock.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));

            var loggerMock = new Mock<ILogger<TransportOrderService>>();

            _sut = new TransportOrderService(_dbContext, _mapper, loggerMock.Object, _authorizationServiceMock.Object, _userContextServiceMock.Object);
        }

        private void SeedTransportOrder()
        {
            var company = new Company
            {
                Id = 1,
                Name = "Test Company",
                Description = "Test Description",
                TaxNumber = 123456789,
                PhoneNumber = 123456789,
                ContactEmail = "test@wp.pl"
            };

            var pickupAddress = new Address
            {
                Id = 1,
                Country = "Poland",
                City = "Kraków",
                Street = "Bracka 4",
                PostalCode = "12345"
                
            };

            var deliveryAddress = new Address
            {
                Id = 2,
                Country = "Poland",
                City = "Warszawa",
                Street = "Marszałkowska 1",
                PostalCode = "00044"

            };

            var driver = new Driver
            {
                Id = 1,
                FirstName = "Jan",
                LastName = "Mydło",
                PhoneNumber = 123456789,
                PersonalNumber = "12345678901"
            };

            var truck = new Truck
            {
                Id = 1,
                RegistrationNumber = "KR12345",
                Brand = "Volvo",
                Model = "FH16",
                Year = 2020,
                Mileage = 50000,
                CapacityTons = 25,
                Type = "Tandem"
            };

            var transportOrders = new List<TransportOrder>
            {
                new TransportOrder
                {
                    Id = 1,
                    OrderName = "test order 1",
                    Description = "test description 1",
                    Price = 100,
                    CreatedById = 1,
                    CompanyId = company.Id,
                    Company = company,
                    PickupAddressId = pickupAddress.Id,
                    PickupAddress = pickupAddress,
                    DeliveryAddressId = deliveryAddress.Id,
                    DeliveryAddress = deliveryAddress,
                    DriverId = driver.Id,
                    Driver = driver,
                    TruckId = truck.Id,
                    Truck = truck,

                },

                new TransportOrder
                {
                    Id = 2,
                    OrderName = "test order 2",
                    Description = "test description 2",
                    Price = 200,
                    CreatedById = 1,
                    CompanyId = company.Id,
                    Company = company,
                    PickupAddressId = pickupAddress.Id,
                    PickupAddress = pickupAddress,
                    DeliveryAddressId = deliveryAddress.Id,
                    DeliveryAddress = deliveryAddress,
                    DriverId = driver.Id,
                    Driver = driver,
                    TruckId = truck.Id,
                    Truck = truck,
                },

                new TransportOrder
                {
                    Id = 3,
                    OrderName = "test order 3",
                    Description = "test description 3",
                    Price = 300,
                    CreatedById = 1,
                    CompanyId = company.Id,
                    Company = company,
                    PickupAddressId = pickupAddress.Id,
                    PickupAddress = pickupAddress,
                    DeliveryAddressId = deliveryAddress.Id,
                    DeliveryAddress = deliveryAddress,
                    DriverId = driver.Id,
                    Driver = driver,
                    TruckId = truck.Id,
                    Truck = truck,
                }
            };

            _dbContext.Companies.Add(company);
            _dbContext.Addresses.AddRange(pickupAddress, deliveryAddress);
            _dbContext.Drivers.Add(driver);
            _dbContext.Trucks.Add(truck);
            _dbContext.Orders.AddRange(transportOrders);
            _dbContext.SaveChanges();
        }

        [Theory]
        [InlineData("test order 1", 1)]
        [InlineData("test order 2", 1)]
        [InlineData("test order 3", 1)]
        [InlineData("test description 1", 1)]
        [InlineData("test description 2", 1)]
        [InlineData("test description 3", 1)]
        [InlineData("", 3)]
        public void GetAll_WithDifferentSearchPhrases_ReturnsExpectedCount(string searchPhrase, int expectedCount)
        {
            //Arrange

            SeedTransportOrder();

            var query = new TransportOrderQuery
            {
                SearchPhrase = searchPhrase,
                PageNumber = 1,
                PageSize = 10
            };

            //Act

            var result = _sut.GetAllTransportOrders(1, query);

            //Assert

            result.TotalItemsCount.Should().Be(expectedCount);
        }

        [Theory]
        [InlineData(nameof(TransportOrder.OrderName), SortDirection.ASC, "test order 1")]
        [InlineData(nameof(TransportOrder.OrderName), SortDirection.DESC, "test order 3")]
        [InlineData(nameof(TransportOrder.Description), SortDirection.ASC, "test description 1")]
        [InlineData(nameof(TransportOrder.Description), SortDirection.DESC, "test description 3")]
        public void GetAll_WithSorting_Returns_SortedResults(string sortBy, SortDirection direction, string expectedFirstValue)
        {
            //Arrange

            SeedTransportOrder();

            var query = new TransportOrderQuery
            {
                SortBy = sortBy,
                SortDirection = direction,
                PageNumber = 1,
                PageSize = 10
            };

            //Act

            var result = _sut.GetAllTransportOrders(1, query);

            //Assert

            var firstItem = result.Items.First();

            var value = firstItem.
                GetType()
                .GetProperty(sortBy)!
                .GetValue(firstItem)?
                .ToString();

            value.Should().Be(expectedFirstValue);
        }

        [Theory]
        [InlineData(1, 1, "test order 1")]
        [InlineData(2, 1, "test order 2")]
        [InlineData(3, 1, "test order 3")]
        public void GetAll_WithPagination_ReturnsCorrectPage(int pageNumber, int pageSize, string expectedFirstOrderName)
        {
            //Arrange

            SeedTransportOrder();

            var query = new TransportOrderQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = nameof(TransportOrder.OrderName),
                SortDirection = SortDirection.ASC
            };

            //Act

            var result = _sut.GetAllTransportOrders(1, query);

            //Assert

            result.Items.Should().HaveCount(1);
            result.Items.First().OrderName.Should().Be(expectedFirstOrderName);
            result.TotalItemsCount.Should().Be(3);
        }

        [Fact]
        public void GetTransportOrderById_WhenCompanyAndTransportOrderExist_ShouldReturnsTransportOrderDto()
        {
            //Arrange

            var company = new Company { Id = 1 };

            var transpoertOrder = new TransportOrder { Id = 10, CompanyId = 1 };

            company.TransportOrders = new List<TransportOrder> { transpoertOrder };

            _dbContext.Companies.Add(company);
            _dbContext.Orders.Add(transpoertOrder);
            _dbContext.SaveChanges();

            //Act

            var result = _sut.GetTransportOrderById(1, 10);

            //Assert

            result.Should().NotBeNull();
            result.Id.Should().Be(10);
        }

        [Fact]
        public void GetTransportOrderById_WhenTransportOrderDoesNotExist_ShouldThrowsNotFoundException()
        {
            //Arrange

            var company = new Company { Id = 1 };

            _dbContext.Companies.Add(company);
            _dbContext.SaveChanges();

            //Act

            Action action = () => _sut.GetTransportOrderById(1, 999);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Transport order not found");
        }

        [Fact]
        public void GetTransportOrderById_WhenCompanyDoesNotExist_ShouldThrowsNotFoundException()
        {
            //Act

            Action action = () => _sut.GetTransportOrderById(999, 1);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Company not found");
        }

        [Fact]
        public void GetTransportOrderById_WhenTransportOrderBelongsToDifferentCompany_ShouldThrowsNotFoundException()
        {
            //Arrange

            var comapny = new Company { Id = 1 };

            var transportOrder = new TransportOrder { Id = 1, CompanyId = 2 };

            _dbContext.Companies.Add(comapny);
            _dbContext.Orders.Add(transportOrder);
            _dbContext.SaveChanges();

            //Act

            Action action = () => _sut.GetTransportOrderById(1, 1);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Transport order not found");
        }

        [Fact]
        public void CreateTransportOrder_WhenCompanyExist_ShouldCreateTransportOrderAndRuturnsId()
        {
            //Arrange

            var comapny = new Company { Id = 1 };

            _dbContext.Companies.Add(comapny);
            _dbContext.SaveChanges();

            var dto = new CreateTransportOrderDto
            {
                OrderName = "New test order name",
                Description = "New test description",
                Price = 1000,
                PickupAddressId = 1,
                DeliveryAddressId = 2,
                DriverId = 1,
                TruckId = 1
            };

            //Act

            var result = _sut.CreateTransportOrder(1, dto);

            //Assert

            result.Should().BeGreaterThan(0);

            var transportOrder = _dbContext.Orders.First();

            transportOrder.OrderName.Should().Be("New test order name");
            transportOrder.Description.Should().Be("New test description");
            transportOrder.Price.Should().Be(1000);
            transportOrder.PickupAddressId.Should().Be(1);
            transportOrder.DeliveryAddressId.Should().Be(2);
            transportOrder.DriverId.Should().Be(1);
            transportOrder.TruckId.Should().Be(1);
            transportOrder.CompanyId.Should().Be(1);
            transportOrder.CreatedById.Should().Be(1);
        }

        [Fact]
        public void CreateTransportOrder_WhenComapnyDoesNotExist_ShouldThrowsNotFoundException()
        {
            //Arrange

            var dto = new CreateTransportOrderDto
            {
                OrderName = "New test order name",
                Description = "New test description",
                Price = 1000,
                PickupAddressId = 1,
                DeliveryAddressId = 2,
                DriverId = 1,
                TruckId = 1
            };

            //Act

            Action action = () => _sut.CreateTransportOrder(999, dto);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Company not found");
        }

        [Fact]
        public void CreateTransportOrder_SetsCreatedByIdFromUserContext()
        {
            //Arrange

            var company = new Company { Id = 1 };

            _dbContext.Companies.Add(company);
            _dbContext.SaveChanges();

            _userContextServiceMock.Setup(x => x.GetUserId).Returns(999);

            var dto = new CreateTransportOrderDto
            {
                OrderName = "New test order name",
                Description = "New test description",
                Price = 1000,
                PickupAddressId = 1,
                DeliveryAddressId = 2,
                DriverId = 1,
                TruckId = 1
            };

            //Act

            _sut.CreateTransportOrder(1, dto);

            //Assert

            var transportOrder = _dbContext.Orders.First();
            transportOrder.CreatedById.Should().Be(999);
        }

        [Fact]
        public void UpdateTransportOrder_WhenUserIsAuthorized_ShouldUpdateTransportOrder()
        {
            //Arrange

            var company = new Company { Id = 1 };

            var transportOrder = new TransportOrder 
            {
                Id = 1,
                OrderName = "Old order name",
                Description = "Old description",
                Price = 5000,
                PickupAddressId = 1,
                DeliveryAddressId = 2,
                DriverId = 1,
                TruckId = 1,
                CompanyId = 1
            };

            _dbContext.Companies.Add(company);
            _dbContext.Orders.Add(transportOrder);
            _dbContext.SaveChanges();

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success);

            var dto = new UpdateTransportOrder
            {
                OrderName = "Old order name - update",
                Description = "Old description - update",
                Price = 5500,
            };

            //Act

            _sut.UpdateTransportOrder(1, 1, dto);

            //Assert

            var updatedTransportOrder = _dbContext.Orders.First();

            updatedTransportOrder.OrderName.Should().Be("Old order name - update");
            updatedTransportOrder.Description.Should().Be("Old description - update");
            updatedTransportOrder.Price.Should().Be(5500);
        }

        [Fact]
        public void UpdateTransportOrder_WhenUserIsNotAuthorized_ShouldThrowsForbidException()
        {
            //Arrange

            var company = new Company { Id = 1 };

            var transportOrder = new TransportOrder { Id = 10, CompanyId = 1};

            _dbContext.Companies.Add(company);
            _dbContext.Orders.Add(transportOrder);
            _dbContext.SaveChanges();

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Failed);

            var dto = new UpdateTransportOrder
            {
                OrderName = "Old order name - update",
            };

            //Act

            Action action = () => _sut.UpdateTransportOrder(1, 10, dto);

            //Assert

            action.Should().Throw<ForbidException>().WithMessage("You don't have permission to update this transport order");
        }

        [Fact]
        public void UpdateTransportOrder_WhenTransportOrderDoesNotExist_ShouldThrowsNotFoundException()
        {
            //Arrange

            var company = new Company { Id = 1 };

            _dbContext.Companies.Add(company);
            _dbContext.SaveChanges();

            var dto = new UpdateTransportOrder
            {
                OrderName = "Old order name - update"
            };

            //Act

            Action action = () => _sut.UpdateTransportOrder(1, 999, dto);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Transport order not found");
        }

        [Fact]
        public void UpdateTransportOrder_WhenCompanyDoesNotExist_ShouldThrowsNotFoundException()
        {
            //Arrange

            var transportOrder = new TransportOrder
            {
                Id = 1,
                OrderName = "Old order name",
                Description = "Old description",
                Price = 5000,
                PickupAddressId = 1,
                DeliveryAddressId = 2,
                DriverId = 1,
                TruckId = 1,
                CompanyId = 1
            };

            _dbContext.Orders.Add(transportOrder);
            _dbContext.SaveChanges();

            var dto = new UpdateTransportOrder
            {
                OrderName = "Old order name - update",
                Description = "Old description - update",
                Price = 5500,
            };

            //Act

            Action action = () => _sut.UpdateTransportOrder(999, 1, dto);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Company not found");
        }

        [Fact]
        public void DeleteTransportOrder_WhenUserIsAuthorized_ShouldDeleteTransportOrder()
        {
            //Arrange

            var company = new Company { Id = 1 };

            var transportOrder = new TransportOrder { Id = 1, CompanyId = 1 };

            _dbContext.Companies.Add(company);
            _dbContext.Orders.Add(transportOrder);
            _dbContext.SaveChanges();

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success);

            //Act

            _sut.DeleteTransportOrder(1, 1);

            //Assert

            _dbContext.Orders.Should().BeEmpty();
        }

        [Fact]
        public void DeleteTransportOrder_WhenUserIsNotAuthorized_ShouldThrowsForbidException()
        {
            //Arrange

            var company = new Company { Id = 1 };

            var transportOrder = new TransportOrder { Id = 125, CompanyId = 1 };

            _dbContext.Companies.Add(company);
            _dbContext.Orders.Add(transportOrder);
            _dbContext.SaveChanges();

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Failed);

            //Act

            Action action = () => _sut.DeleteTransportOrder(1, 125);

            //Assert

            action.Should().Throw<ForbidException>().WithMessage("You don't have permission to delete this transport order");
        }

        [Fact]
        public void DeleteTransportOrder_WhenCompanyDoesNotExist_ShouldThrowsNotFoundException()
        {
            //Arrange

            var transportOrder = new TransportOrder { Id = 1, CompanyId = 1 };

            _dbContext.Orders.Add(transportOrder);
            _dbContext.SaveChanges();

            //Act

            Action action = () => _sut.DeleteTransportOrder(999, 1);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Company not found");
        }

        [Fact]
        public void DeleteTransportOrder_WhenTransportOrderDoesNotExist_ShouldThrowsNotFoundException()
        {
            //Arrange

            var company = new Company { Id = 1 };

            _dbContext.Companies.Add(company);
            _dbContext.SaveChanges();

            //Act

            Action action = () => _sut.DeleteTransportOrder(1, 999);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Transport order not found");
        }

        [Fact]
        public void DeleteAllTransportOrders_ForExistingCompanyWithTransportOrders_ShouldRemoveAllTransportOrders()
        {
            //Arrange

            var companyId = 123;
            var company = new Company { Id = companyId };

            var transportOrders = new List<TransportOrder>
            {
                new TransportOrder {Id = 1, CompanyId = companyId },
                new TransportOrder {Id = 2, CompanyId = companyId },
                new TransportOrder {Id = 3, CompanyId = companyId },
                new TransportOrder {Id = 4, CompanyId = companyId },
                new TransportOrder {Id = 5, CompanyId = 999 }
            };

            _dbContext.Companies.Add(company);
            _dbContext.Orders.AddRange(transportOrders);
            _dbContext.SaveChanges();

            //Act

            _sut.DeleteAllTraansportOrders(companyId);

            //Assert

            var remainingTransportOrders = _dbContext.Orders.ToList();
            remainingTransportOrders.Should().HaveCount(1);
            remainingTransportOrders.Should().Contain(x => x.CompanyId == 999);
            _dbContext.Orders.Any(x => x.CompanyId == companyId).Should().BeFalse();
        }

        [Fact]
        public void DeleteAllTransportOrders_WhenFromNonExistingCompany_ShouldThrowsNotFoundException()
        {
            //Arrange

            var nonExistingComapnyId = 999;

            //Act

            Action action = () => _sut.DeleteAllTraansportOrders(nonExistingComapnyId);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Company not found");
        }
    }
}
