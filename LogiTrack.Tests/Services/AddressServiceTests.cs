using AutoMapper;
using FluentAssertions;
using LogiTrack.Entities;
using LogiTrack.Exceptions;
using LogiTrack.Interfaces;
using LogiTrack.Models;
using LogiTrack.Services;
using LogiTrack.UnitTests.Builders;
using LogiTrack.UnitTests.Helpers;
using LogiTrack.UnitTests.TestData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace LogiTrack.Tests.Services
{
    public class AddressServiceTests
    {
        private readonly LogiTrackDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly AddressService _sut;
        private readonly Mock<IUserContextService> _userContextServiceMock;
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;

        public AddressServiceTests()
        {
            var options = new DbContextOptionsBuilder<LogiTrackDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new LogiTrackDbContext(options);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Address, AddressDto>();
                cfg.CreateMap<CreateAddressDto, Address>();
            });

            _mapper = mapperConfig.CreateMapper();

            _authorizationServiceMock = new Mock<IAuthorizationService>();

            _userContextServiceMock = new Mock<IUserContextService>();

            _userContextServiceMock.Setup(x => x.GetUserId).Returns(1);

            _userContextServiceMock.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));

            var loggerMock = new Mock<ILogger<AddressService>>();

            _sut = new AddressService(_dbContext, _mapper, loggerMock.Object, _authorizationServiceMock.Object, _userContextServiceMock.Object);
        }

        [Theory]
        [InlineData("Sweeden", 2)]
        [InlineData("England", 1)]
        [InlineData("Poland", 1)]
        [InlineData("Stockholm", 2)]
        [InlineData("Sopot", 1)]
        [InlineData("London", 1)]
        [InlineData("Aleja Zwycięstwa 115", 1)]
        [InlineData("Oxford Street 15", 1)]
        [InlineData("Drottninggatan 22", 1)]
        [InlineData("Karls 74", 1)]
        [InlineData("", 4)]

        public void GetAll_WithDifferentSearchPhrases_ReturnsExpectedCount(string searchPhrase, int expectedCount)
        {
            //Arrange

            AddressSeeder.SeedAddress(_dbContext);

            var query = new AddressQuery
            {
                SearchPhrase = searchPhrase,
                PageNumber = 1,
                PageSize = 10
            };

            //Act

            var result = _sut.GetAll(query);

            //Assert

            result.TotalItemsCount.Should().Be(expectedCount);
        }

        [Theory]
        [InlineData(nameof(Address.Country), SortDirection.ASC, "England")]
        [InlineData(nameof(Address.Country), SortDirection.DESC, "Sweeden")]
        [InlineData(nameof(Address.City), SortDirection.ASC, "London")]
        [InlineData(nameof(Address.City), SortDirection.DESC, "Stockholm")]
        [InlineData(nameof(Address.Street), SortDirection.ASC, "ADrottninggatan 22")]
        [InlineData(nameof(Address.Street), SortDirection.DESC, "Oxford Street 15")]
        public void GetAll_WithSorting_Returns_SortedResults(string sortBy, SortDirection direction, string expectedFirstValue)
        {
            //Arrange

            AddressSeeder.SeedAddress(_dbContext);

            var query = new AddressQuery
            {
                SortBy = sortBy,
                SortDirection = direction,
                PageNumber = 1,
                PageSize = 10
            };

            //Act

            var result = _sut.GetAll(query);

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
        [InlineData(1, 1, "ADrottninggatan 22")]
        [InlineData(2, 1, "Aleja Zwycięstwa 115")]
        [InlineData(3, 1, "Karls 74")]
        [InlineData(4, 1, "Oxford Street 15")]
        public void GetAll_WithPagination_ReturnsCorrectPage(int pageNumber, int pageSize, string expectedFirstStreet)
        {
            //Arrange

            AddressSeeder.SeedAddress(_dbContext);

            var query = new AddressQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = nameof(Address.Street),
                SortDirection = SortDirection.ASC
            };

            //Act

            var result = _sut.GetAll(query);

            //Assert

            result.Items.Should().HaveCount(1);
            result.Items.First().Street.Should().Be(expectedFirstStreet);
            result.TotalItemsCount.Should().Be(4);
        }

        [Fact]
        public void GetById_WhenAddressExist_ReturnsAddressDto()
        {
            //Arrange

            AddressSeeder.SeedAddress(_dbContext);

            //Act

            var result = _sut.GetById(2);

            //Assert

            result.Should().NotBeNull();
            result.Id.Should().Be(2);
        }

        [Fact]
        public void GetById_WhenAddressDoesNotExist_ThrowsNotFoundException()
        {
            //Act

            Action action = () => _sut.GetById(777);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Address not found");
        }

        [Fact]
        public void CreateAddress_WithValidData_CreateAddressAndReturnsId()
        {
            //Arrange

            var dto = new CreateAddressBuilder().Build();

            //Act

            var result = _sut.CreateAddress(dto);

            //Assert

            result.Should().BeGreaterThan(0);

            var address = _dbContext.Addresses.First();

            address.Country.Should().Be("Portugal");
            address.City.Should().Be("Lizbona");
            address.Street.Should().Be("Polna 147");
            address.PostalCode.Should().Be("12345");
            address.Id.Should().Be(1);
            address.CreatedById.Should().Be(1);
        }

        [Fact]
        public void CreateAddress_SetsCreatedByIdFromUserContext()
        {
            //Arrange

            var expectedUserId = 999;
            _userContextServiceMock.Setup(x => x.GetUserId).Returns(expectedUserId);

            var dto = new CreateAddressBuilder().Build();

            //Act

            var result = _sut.CreateAddress(dto);

            //Assert

            var address = _dbContext.Addresses.First(x => x.Id == result);

            address.Should().NotBeNull();
            address.CreatedById.Should().Be(expectedUserId);
        }

        [Fact]
        public void UpdateAddress_WhenUserIsAuthorized_UpdateAddress()
        {
            //Arrange

            AddressSeeder.SeedAddress(_dbContext);

            var dto = new UpdateAddressBuilder().Build();

            _authorizationServiceMock.SetupSuccess();

            //Act

            _sut.UpdateAddress(4, dto);

            //Assert

            var address = _dbContext.Addresses.First(x => x.Id == 4);

            address.Country.Should().Be("Portugal");
            address.City.Should().Be("Lizbona");
            address.Street.Should().Be("Polna 147");
            address.PostalCode.Should().Be("12345");
        }

        [Fact]
        public void UpdateAddress_WhenUserIsNotAuthorized_ThrowsForbidException()
        {
            //Arrange

            AddressSeeder.SeedAddress(_dbContext);

            var dto = new UpdateAddressBuilder().Build();

            _authorizationServiceMock.SetupFail();

            //Act

            Action action = () => _sut.UpdateAddress(4, dto);

            //Assert

            action.Should().Throw<ForbidException>().WithMessage("You don't have permission to update this address");
        }

        [Fact]
        public void UpdateAddress_WhenAddressDoesNotExist_ThrowsNotFoundException()
        {
            //Arrange

            var dto = new UpdateAddressBuilder().Build();

            //Act

            Action action = () => _sut.UpdateAddress(999, dto);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Address not found");
        }

        [Fact]
        public void DeleteAddress_WhenUserIsAuthorize_ShouldDeleteAddress()
        {
            //Arrnge

            AddressSeeder.SeedAddress(_dbContext);

            _authorizationServiceMock.SetupSuccess();

            //Act

            _sut.DeleteAdderss(2);

            //Assert

            var address = _dbContext.Addresses.FirstOrDefault(x => x.Id == 2);
            address.Should().BeNull();

            _dbContext.Addresses.Count().Should().Be(3);
        }

        [Fact]
        public void DeleteAddress_WhenUserIsNotAuthorized_ShouldThrowsForbidException()
        {
            //Arrange

            AddressSeeder.SeedAddress(_dbContext);

            _authorizationServiceMock.SetupFail();

            //Act

            Action action = () => _sut.DeleteAdderss(1);

            //Assert

            action.Should().Throw<ForbidException>().WithMessage("You don't have permission to delete this address");
        }

        [Fact]
        public void DeleteAddress_WhenAddressDoesNotExist_ShouldThrowsNotFoundException()
        {
            //Act

            Action action = () => _sut.DeleteAdderss(999);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Address not found");
        }
    }
}