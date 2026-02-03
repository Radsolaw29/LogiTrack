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

namespace LogiTrack.Tests.Services
{
    public class TruckServiceTests
    {
        private readonly LogiTrackDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly TruckService _sut;
        private readonly Mock<IUserContextService> _userContextServiceMock;
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;

        public TruckServiceTests()
        {
            var options = new DbContextOptionsBuilder<LogiTrackDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new LogiTrackDbContext(options);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Truck, TruckDto>();
                cfg.CreateMap<CreateTruckDto, Truck>();
            });

            _mapper = mapperConfig.CreateMapper();

            _authorizationServiceMock = new Mock<IAuthorizationService>();

            _userContextServiceMock = new Mock<IUserContextService>();

            _userContextServiceMock.Setup(x => x.GetUserId).Returns(1);

            _userContextServiceMock.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));

            var loggerMock = new Mock<ILogger<TruckService>>();

            _sut = new TruckService(_dbContext, _mapper, loggerMock.Object, _authorizationServiceMock.Object, _userContextServiceMock.Object);
        }

        private void SeedCompanyWithTrucks()
        {
            var company = new Company
            {
                Id = 1,
                Trucks = new List<Truck>
                {
                    new Truck
                    {
                        Id = 5,
                        RegistrationNumber = "GDA 1111",
                        Brand = "Volvo",
                        Model = "FH18",
                        Year = 2017,
                        Mileage = 450000,
                        CapacityTons = 25,
                        Type = "Semi-trailer",
                        CompanyId = 1
                    },

                    new Truck
                    {
                        Id = 6,
                        RegistrationNumber = "GD 2222",
                        Brand = "Volvo",
                        Model = "FH16",
                        Year = 2015,
                        Mileage = 100000,
                        CapacityTons = 21,
                        Type = "Semi-trailer",
                        CompanyId = 1
                    },

                    new Truck
                    {
                        Id = 7,
                        RegistrationNumber = "WW 5454",
                        Brand = "Volvo",
                        Model = "FH12",
                        Year = 2020,
                        Mileage = 200000,
                        CapacityTons = 18,
                        Type = "Semi-trailer",
                        CompanyId = 1
                    },

                    new Truck
                    {
                        Id = 8,
                        RegistrationNumber = "GSP 1452",
                        Brand = "MAN",
                        Model = "FH16",
                        Year = 2019,
                        Mileage = 300000,
                        CapacityTons = 20,
                        Type = "Semi-trailer",
                        CompanyId = 1
                    }
                }
            };

            _dbContext.Companies.Add(company);
            _dbContext.SaveChanges();
        }

        [Theory]
        [InlineData("GD 2222", 1)]
        [InlineData("Volvo", 3)]
        [InlineData("FH16", 2)]
        [InlineData("", 4)]
        [InlineData(null, 4)]
        public void GetAll_WithDifferentSearchPhrases_ReturnsExpectedCount(string searchPhrase, int expectedCount)
        {
            //Arrange

            SeedCompanyWithTrucks();

            var query = new TruckQuery
            {
                SearchPhrase = searchPhrase,
                PageNumber = 1,
                PageSize = 10
            };

            //Act

            var result = _sut.GetAll(1, query);

            //Assert

            result.Items.Count.Should().Be(expectedCount);
            result.TotalItemsCount.Should().Be(expectedCount);
        }

        [Theory]
        [InlineData(nameof(Truck.RegistrationNumber), SortDirection.ASC, "GD 2222")]
        [InlineData(nameof(Truck.RegistrationNumber), SortDirection.DESC, "WW 5454")]
        [InlineData(nameof(Truck.Brand), SortDirection.ASC, "MAN")]
        [InlineData(nameof(Truck.Brand), SortDirection.DESC, "Volvo")]
        [InlineData(nameof(Truck.Model), SortDirection.ASC, "FH12")]
        [InlineData(nameof(Truck.Model), SortDirection.DESC, "FH18")]
        public void GetAll_WithSorting_ReturnsSortedResults(string sortBy, SortDirection direction, string expectedFirstValue)
        {
            //Arrange

            SeedCompanyWithTrucks();

            var query = new TruckQuery
            {
                SortBy = sortBy,
                SortDirection = direction,
                PageNumber = 1,
                PageSize = 10
            };

            //Act

            var result = _sut.GetAll(1, query);

            //Assert

            var firstItem = result.Items.First();

            var value = firstItem
                .GetType()
                .GetProperty(sortBy)!
                .GetValue(firstItem)?
                .ToString();

            value.Should().Be(expectedFirstValue);
        }

        [Theory]
        [InlineData(1, 1, "GD 2222")]
        [InlineData(2, 1, "GDA 1111")]
        [InlineData(3, 1, "GSP 1452")]
        [InlineData(4, 1, "WW 5454")]
        public void GetAll_WithPagination_ReturnsCorrectPage(int pageNumber, int pageSize, string expectedFirstRegistrationNumber)
        {
            //Arrange

            SeedCompanyWithTrucks();

            var query = new TruckQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = nameof(Truck.RegistrationNumber),
                SortDirection = SortDirection.ASC
            };

            //Act

            var result = _sut.GetAll(1, query);

            //Assert

            result.Items.Should().HaveCount(1);
            result.Items.First().RegistrationNumber.Should().Be(expectedFirstRegistrationNumber);
            result.TotalItemsCount.Should().Be(4);
        }

        [Fact]
        public void GetById_WhenCompanyAndTruckExist_ReturnsTruckDto()
        {
            //Arrange

            var comapny = new Company { Id = 1 };

            var truck = new Truck { Id = 10, CompanyId = 1 };

            comapny.Trucks = new List<Truck> { truck };

            _dbContext.Companies.Add(comapny);
            _dbContext.Trucks.Add(truck);
            _dbContext.SaveChanges();

            //Act

            var result = _sut.GetById(1, 10);

            //Assert

            result.Should().NotBeNull();
            result.Id.Should().Be(10);
        }

        [Fact]
        public void GetById_WhenCompanyDoesNotExist_ThrowsNotFoundException()
        {
            //Act

            Action action = () => _sut.GetById(999, 10);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Company not found");
        }

        [Fact]
        public void GetById_WhenTruckDoesNotExist_ThrowsNotFoundException()
        {
            //Arrange

            var company = new Company { Id = 1 };

            _dbContext.Companies.Add(company);
            _dbContext.SaveChanges();

            //Act

            Action action = () => _sut.GetById(1, 10);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Truck not found");
        }

        [Fact]
        public void GetById_WhenTruckBelongsToDifferentCompany_ThrowsNotFoundException()
        {
            //Arrange

            var company = new Company { Id = 1 };

            var truck = new Truck { Id = 10, CompanyId = 2 };

            _dbContext.Companies.Add(company);
            _dbContext.Trucks.Add(truck);
            _dbContext.SaveChanges();

            //Act

            Action action = () => _sut.GetById(1, 10);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Truck not found");
        }

        [Fact]
        public void CreateTruck_WhenCompanyExist_CreatesTruckAndReturnsId()
        {
            //Arrange

            var company = new Company { Id = 1 };
            _dbContext.Companies.Add(company);
            _dbContext.SaveChanges();

            var dto = new CreateTruckDto
            {
                RegistrationNumber = "GD 78954",
                Brand = "Scania",
                Model = "R500",
                Year = 2018,
                Mileage = 150000,
                CapacityTons = 22,
                Type = "Semi-trailer",
                CompanyId = 1
            };

            //Act

            var result = _sut.CreateTruck(1, dto);

            //Assert

            result.Should().BeGreaterThan(0);

            var truck = _dbContext.Trucks.First();

            truck.RegistrationNumber.Should().Be("GD 78954");
            truck.Brand.Should().Be("Scania");
            truck.Model.Should().Be("R500");
            truck.Year.Should().Be(2018);
            truck.Mileage.Should().Be(150000);
            truck.CapacityTons.Should().Be(22);
            truck.Type.Should().Be("Semi-trailer");
            truck.CompanyId.Should().Be(1);
            truck.CreatedById.Should().Be(1);
        }

        [Fact]
        public void CreateTruck_WhenCompanyDoesNotExist_ThrowsNotFoundException()
        {
            //Arrange

            var dto = new CreateTruckDto
            {
                RegistrationNumber = "GD 78954",
                Brand = "Scania",
                Model = "R500",
                Year = 2018,
                Mileage = 150000,
                CapacityTons = 22,
                Type = "Semi-trailer",
                CompanyId = 1
            };

            //Act

            Action action = () => _sut.CreateTruck(1, dto);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Company not found");
        }

        [Fact]
        public void CreateTruck_SetsCreatedByIdFromUserContext()
        {
            //Arrange

            var company = new Company { Id = 1 };

            _dbContext.Companies.Add(company);
            _dbContext.SaveChanges();

            _userContextServiceMock.Setup(x => x.GetUserId).Returns(999);

            var dto = new CreateTruckDto
            {
                RegistrationNumber = "GD 78954",
                Brand = "Scania",
                Model = "R500",
                Year = 2018,
                Mileage = 150000,
                CapacityTons = 22,
                Type = "Semi-trailer",
                CompanyId = 1
            };

            //Act

            _sut.CreateTruck(1, dto);

            //Assert

            var truck = _dbContext.Trucks.First();
            truck.CreatedById.Should().Be(999);
        }

        [Fact]
        public void UpdateTruck_WhenUserIsAuthorized_UpdateTruck()
        {
            //Arrange

            var company = new Company { Id = 1 };

            var truck = new Truck
            {
                Id = 15,
                CompanyId = 1,
                RegistrationNumber = "GD 78954",
                Brand = "Scania",
                Model = "R500",
                Year = 2018,
                Mileage = 150000,
                CapacityTons = 22,
                Type = "Semi-trailer",
            };

            _dbContext.Companies.Add(company);
            _dbContext.Trucks.Add(truck);
            _dbContext.SaveChanges();

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var dto = new UpdateTruckDto
            {
                RegistrationNumber = "GD 11111",
                Brand = "Scania - edit",
                Model = "R450",
                Year = 2024,
                Mileage = 90000,
                CapacityTons = 20,
                Type = "Semi-trailer"
            };

            //Act

            _sut.UpdateTruck(1, 15, dto);

            //Assert

            var updateTruck = _dbContext.Trucks.First();

            updateTruck.RegistrationNumber.Should().Be("GD 11111");
            updateTruck.Brand.Should().Be("Scania - edit");
            updateTruck.Model.Should().Be("R450");
            updateTruck.Year.Should().Be(2024);
            updateTruck.Mileage.Should().Be(90000);
            updateTruck.CapacityTons.Should().Be(20);
            updateTruck.Type.Should().Be("Semi-trailer");
        }

        [Fact]
        public void UpdateTruck_WhenUserIsNotAuthorized_ThrowsForbidException()
        {
            //Arrange

            var company = new Company { Id = 1 };

            var truck = new Truck { Id = 10, CompanyId = 1 };

            _dbContext.Companies.Add(company);
            _dbContext.Trucks.Add(truck);
            _dbContext.SaveChanges();

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            var dto = new UpdateTruckDto
            {
                RegistrationNumber = "GD 11114",
            };

            //Act

            Action action = () => _sut.UpdateTruck(1, 10, dto);

            //Assert

            action.Should().Throw<ForbidException>().WithMessage("You don't have permission to update this truck");
        }

        [Fact]
        public void UpdateTruck_WhenCompanyDoesNotExist_ThrowsNotFoundException()
        {
            //Act

            Action action = () => _sut.UpdateTruck(999, 1, new UpdateTruckDto());

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Company not found");
        }

        [Fact]
        public void UpdateTruck_WhenTruckDoesNotExist_ThrowsNotFoundException()
        {
            //Arrange

            var comapny = new Company { Id = 1 };

            _dbContext.Companies.Add(comapny);
            _dbContext.SaveChanges();

            var dto = new UpdateTruckDto
            {
                RegistrationNumber = "GD 11117",
            };

            //Act

            Action action = () => _sut.UpdateTruck(1, 999, dto);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Truck not found");
        }

        [Fact]
        public void DeleteTruck_WhenUserIsAuthorized_DeleteTruck()
        {
            //Arrange

            var company = new Company { Id = 1 };
            var truck = new Truck { Id = 10, CompanyId = 1 };

            _dbContext.Companies.Add(company);
            _dbContext.Trucks.Add(truck);
            _dbContext.SaveChanges();

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());

            //Act

            _sut.DeleteTruck(1, 10);

            //Assert

            _dbContext.Trucks.Should().BeEmpty();
        }

        [Fact]
        public void DeleteTruck_WhenUserIsNotAuthorized_ThrowsForbidException() 
        {
            //Arrange

            var company = new Company { Id = 1 };
            var truck = new Truck { Id = 12, CompanyId = 1 };

            _dbContext.Companies.Add(company);
            _dbContext.Trucks.Add(truck);
            _dbContext.SaveChanges();

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            //Act

            Action action = () => _sut.DeleteTruck(1, 12);

            //Assert

            action.Should().Throw<ForbidException>().WithMessage("You don't have permission to delete this truck");
        }

        [Fact]
        public void DeleteTruck_WhenCompanyDoesNotExist_ThrowsNotFoundException()
        {
            //Act

            Action action = () => _sut.DeleteTruck(1, 12);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Company not found");
        }

        [Fact]
        public void DeleteTruck_WhenTruckDoesNotExist_ThrowsNotFoundException()
        {
            //Arrange

            var company = new Company { Id = 1 };

            _dbContext.Companies.Add(company);
            _dbContext.SaveChanges();

            //Act

            Action action = () => _sut.DeleteTruck(1, 12);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Truck not found");
        }

        [Fact]
        public void DeleteAllTrucks_ForExistingCompanyWithTrucks_RemoveAllTrucks()
        {
            //Arrange

            var companyId = 1;
            var company = new Company { Id = companyId };
            var trucks = new List<Truck>()
            {
                new Truck() { Id = 1, CompanyId = companyId },
                new Truck() { Id = 2, CompanyId = companyId },
                new Truck() { Id = 3, CompanyId = companyId },
                new Truck() { Id = 4, CompanyId = companyId },
                new Truck() { Id = 5, CompanyId =  999 }
            };

            _dbContext.Companies.Add(company);
            _dbContext.Trucks.AddRange(trucks);
            _dbContext.SaveChanges();

            //Act

            _sut.DeleteAllTrucks(companyId);

            //Assert

            var remainingTrucks = _dbContext.Trucks.ToList();
            remainingTrucks.Should().HaveCount(1);
            remainingTrucks.Should().Contain(x => x.CompanyId == 999);
            _dbContext.Trucks.Any(x => x.CompanyId == companyId).Should().BeFalse();
        }

        [Fact]
        public void DeleteAllTrucks_ForNonExistingCompany_ThrowsNotFoundException()
        {
            //Arrange

            var nonExistingCompanyId = 999;

            //Act

            Action action = () => _sut.DeleteAllTrucks(nonExistingCompanyId);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Company not found");
        }
    }
}
