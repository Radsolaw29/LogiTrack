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
using System.Security.Claims;

namespace LogiTrack.Tests.Services
{
    public class DriverServiceTests
    {
        private readonly LogiTrackDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly DriverService _sut;
        private readonly Mock<IUserContextService> _userContextServiceMock;
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;

        public DriverServiceTests()
        {
            var options = new DbContextOptionsBuilder<LogiTrackDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new LogiTrackDbContext(options);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Driver, DriverDto>();
                cfg.CreateMap<CreateDriverDto, Driver>();
            });

            _mapper = mapperConfig.CreateMapper();

            _authorizationServiceMock = new Mock<IAuthorizationService>();

            _userContextServiceMock = new Mock<IUserContextService>();

            _userContextServiceMock.Setup(x => x.GetUserId).Returns(1);

            _userContextServiceMock.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));

            var loggerMock = new Mock<ILogger<DriverService>>();

            _sut = new DriverService(_dbContext, _mapper, loggerMock.Object, _authorizationServiceMock.Object, _userContextServiceMock.Object);
        }

        private void SeedCompanyWithDrivers()
        {
            var company = new Company 
            { 
                Id = 1,
                Drivers = new List<Driver> 
                { 
                    new Driver 
                    {
                        Id = 9,
                        FirstName = "Krzysztof",
                        LastName = "Krawczyk",
                        PersonalNumber = "11122233344",
                        DateOfBirth = new DateTime(1980, 5, 15),
                        LicenseDriving = "C+E",
                        PhoneNumber = 987654321,
                        ContactEmail = "krzyszdzis@wp.pl",
                        CompanyId = 1
                    },

                    new Driver
                    {
                        Id = 10,
                        FirstName = "Damian",
                        LastName = "Konrad",
                        PersonalNumber = "11155555544",
                        DateOfBirth = new DateTime(1990, 2, 15),
                        LicenseDriving = "C",
                        PhoneNumber = 111654321,
                        ContactEmail = "damian@wp.pl",
                        CompanyId = 1
                    },

                    new Driver
                    {
                        Id = 11,
                        FirstName = "Radosław",
                        LastName = "Polski",
                        PersonalNumber = "22255555544",
                        DateOfBirth = new DateTime(1999, 3, 15),
                        LicenseDriving = "C+E",
                        PhoneNumber = 887654321,
                        ContactEmail = "rado@wp.pl",
                        CompanyId = 1
                    },

                    new Driver
                    {
                        Id = 12,
                        FirstName = "Julia",
                        LastName = "Dziarska",
                        PersonalNumber = "22257546544",
                        DateOfBirth = new DateTime(2001, 9, 10),
                        LicenseDriving = "C+E",
                        PhoneNumber = 333654321,
                        ContactEmail = "julianna@wp.pl",
                        CompanyId = 1
                    }
                }
            };

            _dbContext.Companies.Add(company);
            _dbContext.SaveChanges();
        }

        [Fact]
        public void GetById_WhenCompanyAndDriverExist_ReturnsDriverDto()
        {
            //Arrange

            var company = new Company { Id = 1 };

            var driver = new Driver { Id = 10, CompanyId = 1 };

            company.Drivers = new List<Driver> { driver };

            _dbContext.Companies.Add(company);
            _dbContext.Drivers.Add(driver);
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

            Action action = () => _sut.GetById(1, 10);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Company not found");
        }

        [Fact]
        public void GetById_WhenDriverDoesNotExist_ThrowsNotFoundException()
        {
            //Arrange

            var company = new Company { Id = 1 };

            _dbContext.Companies.Add(company);
            _dbContext.SaveChanges();

            //Act

            var action = () => _sut.GetById(1, 10);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Driver not found");
        }

        [Fact]
        public void GetById_WhenDriverBelongsToDifferentCompany_ThrowsNotFoundException()
        {
            //Arrange

            var company = new Company { Id = 1 };

            var driver = new Driver { Id = 10, CompanyId = 2 };

            _dbContext.Companies.Add(company);
            _dbContext.Drivers.Add(driver);
            _dbContext.SaveChanges();

            //Act

            var action = () => _sut.GetById(1, 10);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Driver not found");
        }

        [Fact]
        public void CreateDriver_WhenCompanyExist_CreatesDriverAndReturnsId()
        {
            //Arrange

            var company = new Company { Id = 1 };
            _dbContext.Companies.Add(company);
            _dbContext.SaveChanges();

            var dto = new CreateDriverDto
            {
                FirstName = "Jan",
                LastName = "Nowak",
                PersonalNumber = "12345678901",
                DateOfBirth = new DateTime(1990, 1, 1),
                LicenseDriving = "C+E",
                PhoneNumber = 123456789,
                ContactEmail = "mikolajki@wp.pl",
                CompanyId = 1
            };

            //Act

            var result = _sut.CreateDriver(1, dto);

            //Assert

            result.Should().BeGreaterThan(0);

            var driver = _dbContext.Drivers.First();

            driver.FirstName.Should().Be("Jan");
            driver.LastName.Should().Be("Nowak");
            driver.PersonalNumber.Should().Be("12345678901");
            driver.DateOfBirth.Should().Be(new DateTime(1990, 1, 1));
            driver.LicenseDriving.Should().Be("C+E");
            driver.PhoneNumber.Should().Be(123456789);
            driver.ContactEmail.Should().Be("mikolajki@wp.pl");
            driver.CompanyId.Should().Be(1);
            driver.CreatedById.Should().Be(1);
        }

        [Fact]
        public void CreateDriver_WhenCompanyDoesNotExist_ThrowsNotFoundException()
        {
            //Arrange

            var dto = new CreateDriverDto
            {
                FirstName = "Jan",
                LastName = "Nowak",
                PersonalNumber = "12345678901",
                DateOfBirth = new DateTime(1990, 1, 1),
                LicenseDriving = "C+E",
                PhoneNumber = 123456789,
                ContactEmail = "mikolajki@wp.pl",
                CompanyId = 1
            };

            //Act

            Action action = () => _sut.CreateDriver(1, dto);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Company not found");
        }

        [Fact]
        public void CreateDriver_SetsCreatedByIdFromUserContext()
        {
            //Arrange

            var company = new Company { Id = 1 };
            _dbContext.Companies.Add(company);
            _dbContext.SaveChanges();

            _userContextServiceMock.Setup(x => x.GetUserId).Returns(999);

            var dto = new CreateDriverDto
            {
                FirstName = "Jan",
                LastName = "Nowak",
                PersonalNumber = "12345678901",
                DateOfBirth = new DateTime(1990, 1, 1),
                LicenseDriving = "C+E",
                PhoneNumber = 123456789,
                ContactEmail = "mikolajki@wp.pl",
                CompanyId = 1
            };

            //Act

            _sut.CreateDriver(1, dto);

            //Assert

            var driver = _dbContext.Drivers.First();
            driver.CreatedById.Should().Be(999);
        }

        [Fact]
        public void UpdateDriver_WhenUserIsAuthorized_UpdateDriver()
        {
            //Arrange

            var company = new Company { Id = 1 };

            var driver = new Driver
            {
                Id = 10,
                CompanyId = 1,
                FirstName = "Jan",
                LastName = "Nowak",
                PersonalNumber = "12345678901",
                DateOfBirth = new DateTime(1990, 1, 1),
                LicenseDriving = "C+E",
                PhoneNumber = 123456789,
                ContactEmail = "mikolajki@wp.pl"
            };

            _dbContext.Companies.Add(company);
            _dbContext.Drivers.Add(driver);
            _dbContext.SaveChanges();

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var dto = new UpdateDriverDto
            {
                FirstName = "Janek",
                LastName = "Nowak-edit",
                PersonalNumber = "145865421",
                DateOfBirth = new DateTime(1990, 12, 12),
                LicenseDriving = "C",
                PhoneNumber = 111111111,
                ContactEmail = "mikolajki-edit@wp.pl"
            };

            //Act

            _sut.UpdateDriver(1, 10, dto);

            //Assert

            var updateDriver = _dbContext.Drivers.First();

            updateDriver.FirstName.Should().Be("Janek");
            updateDriver.LastName.Should().Be("Nowak-edit");
            updateDriver.PersonalNumber.Should().Be("145865421");
            updateDriver.DateOfBirth.Should().Be(new DateTime(1990, 12, 12));
            updateDriver.LicenseDriving.Should().Be("C");
            updateDriver.PhoneNumber.Should().Be(111111111);
            updateDriver.ContactEmail.Should().Be("mikolajki-edit@wp.pl");
        }

        [Fact]
        public void UpdateDriver_WhenDriverDoesNotExist_ThrowsNotFoundException()
        {
            //Arrange

            var company = new Company { Id = 1 };
            _dbContext.Companies.Add(company);
            _dbContext.SaveChanges();

            var dto = new UpdateDriverDto
            {
                FirstName = "Jan"
            };

            //Act

            Action action = () => _sut.UpdateDriver(1, 1, dto);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Driver not found");
        }

        [Fact]
        public void UpdateDriver_WhenCompanyDoesNotExist_ThrowsNotFoundException()
        {
            //Act

            Action action = () => _sut.UpdateDriver(1, 1, new UpdateDriverDto());

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Company not found");
        }

        [Fact]
        public void UpdateDriver_WhenUserIsNotAuthorized_ThrowsForbidException() 
        {
            //Arrange

            var compny = new Company { Id = 1 };
            var driver = new Driver { Id = 10, CompanyId = 1 };

            _dbContext.Companies.Add(compny);
            _dbContext.Drivers.Add(driver);
            _dbContext.SaveChanges();

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            var dto = new UpdateDriverDto
            {
                FirstName = "Jan"
            };

            //Act

            Action action = () => _sut.UpdateDriver(1, 10, dto);

            //Assert

            action.Should().Throw<ForbidException>().WithMessage("You don't have permission to update this driver");
        }

        [Fact]
        public void DeleteDriver_WhenUserIsAuthorized_DeleteDriver() 
        {
            //Arrange

            var company = new Company { Id = 1 };
            var driver = new Driver { Id = 10, CompanyId = 1 };

            _dbContext.Companies.Add(company);
            _dbContext.Drivers.Add(driver);
            _dbContext.SaveChanges();

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());

            //Act

            _sut.DeleteDriver(1, 10);

            //Assert

            _dbContext.Drivers.Should().BeEmpty();
        }

        [Fact]
        public void DeleteDriver_WhenDriverDoesNotExist_ThrowsNotFoundException()
        {
            var company = new Company { Id = 1 };

            _dbContext.Companies.Add(company);
            _dbContext.SaveChanges();

            //Act

            Action action = () => _sut.DeleteDriver(1, 1);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Driver not found");
        }

        [Fact]
        public void DeleteDriver_WhenCompanyDoesNotExist_ThrowsNotFoundException() 
        {
            //Act

            Action action = () => _sut.DeleteDriver(1, 1);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Company not found");
        }

        [Fact]
        public void DeleteDriver_WhenUserIsNotAuthorized_ThrowsForbidException()
        {
            //Arrange

            var company = new Company { Id = 1 };
            var driver = new Driver { Id = 10, CompanyId = 1 };

            _dbContext.Companies.Add(company);
            _dbContext.Drivers.Add(driver);
            _dbContext.SaveChanges();

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            //Act

            Action action = () => _sut.DeleteDriver(1, 10);

            //Assert

            action.Should().Throw<ForbidException>().WithMessage("You don't have permission to delete this driver");
        }

        [Fact]
        public void DeleteAllDrivers_ForExistingCompanyWithDriers_RemoveAllDrivers()
        {
            //Arrange

            var companyId = 1;
            var company = new Company { Id = companyId };
            var drivers = new List<Driver>()
            {
                new Driver() { Id = 1, CompanyId = companyId },
                new Driver() { Id = 2, CompanyId = companyId },
                new Driver() { Id = 3, CompanyId = companyId },
                new Driver() { Id = 4, CompanyId = companyId },
                new Driver() {Id = 5, CompanyId = 999 }
            };

            _dbContext.Companies.Add(company);
            _dbContext.Drivers.AddRange(drivers);
            _dbContext.SaveChanges();

            //Act

            _sut.DeleteAllDrivers(companyId);

            //Assert

            var remainingDrivers = _dbContext.Drivers.ToList();
            remainingDrivers.Should().HaveCount(1);
            remainingDrivers.Should().Contain(d => d.CompanyId == 999);
            _dbContext.Drivers.Any(d => d.CompanyId == companyId).Should().BeFalse();
        }

        [Fact]
        public void DeleteAllDrivers_ForNonExistingCompany_ThrowsNotFoundException()
        {
            //Arrange

            var nonExistingCompanyId = 999;

            //Act

            Action action = () => _sut.DeleteAllDrivers(nonExistingCompanyId);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Company not found");
        }

        [Theory]
        [InlineData("Krawczyk", 1)]
        [InlineData("KON", 1)]
        [InlineData("@wp.pl", 4)]
        [InlineData("", 4)]
        [InlineData(null, 4)]
        public void GetAll_WithDifferentSearchPhrases_ReturnsExpectedCount(string searchPhrase, int expectedCount)
        {
            //Arrange

            SeedCompanyWithDrivers();

            var query = new DriverQuery
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
        [InlineData(nameof(Driver.FirstName), SortDirection.ASC, "Damian")]
        [InlineData(nameof(Driver.FirstName), SortDirection.DESC, "Radosław")]
        [InlineData(nameof(Driver.LastName), SortDirection.ASC, "Dziarska")]
        [InlineData(nameof(Driver.LastName), SortDirection.DESC, "Polski")]
        [InlineData(nameof(Driver.LicenseDriving), SortDirection.ASC, "C")]
        [InlineData(nameof(Driver.LicenseDriving), SortDirection.DESC, "C+E")]
        [InlineData(nameof(Driver.ContactEmail), SortDirection.ASC, "damian@wp.pl")]
        [InlineData(nameof(Driver.ContactEmail), SortDirection.DESC, "rado@wp.pl")]
        public void GetAll_WithSorting_ReturnsSortedResults(string sortBy, SortDirection direction, string expectedFirstValue)
        {
            //Arrange

            SeedCompanyWithDrivers();

            var query = new DriverQuery
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
                .GetValue(firstItem)
                ?.ToString();

            value.Should().Be(expectedFirstValue);
        }

        [Theory]
        [InlineData(1, 1, "Damian")]
        [InlineData(2, 1, "Julia")]
        [InlineData(3, 1, "Krzysztof")]
        [InlineData(4, 1, "Radosław")]
        public void GetAll_WithPagination_ReturnsCorrectPage(int pageNumber, int pageSize, string expectedFirstName) 
        {
            //Arrange

            SeedCompanyWithDrivers();

            var query = new DriverQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = nameof(Driver.FirstName),
                SortDirection = SortDirection.ASC
            };

            //Act

            var result = _sut.GetAll(1, query);

            //Assert

            result.Items.Should().HaveCount(1);
            result.Items.First().FirstName.Should().Be(expectedFirstName);
            result.TotalItemsCount.Should().Be(4);
        }
    }
}
