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

        private void SeedAddress()
        {
            var addresses = new List<Address>
            {
                new Address
                {
                    Id = 1,
                    Country = "Poland",
                    City = "Sopot",
                    Street = "Aleja Zwycięstwa 115",
                    PostalCode = "12345",
                    CreatedById = 1
                },

                new Address
                {
                    Id = 2,
                    Country = "England",
                    City = "London",
                    Street = "Oxford Street 15",
                    PostalCode = "55777",
                    CreatedById = 2
                },

                new Address
                {
                    Id = 3,
                    Country = "Sweeden",
                    City = "Stockholm",
                    Street = "Drottninggatan 22",
                    PostalCode = "11111",
                    CreatedById = 3
                },

                new Address
                {
                    Id = 4,
                    Country = "Sweeden",
                    City = "Stockholm",
                    Street = "Karls 74",
                    PostalCode = "11114",
                    CreatedById = 4
                }
            };

            _dbContext.Addresses.AddRange(addresses);
            _dbContext.SaveChanges();
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

            SeedAddress();

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
        [InlineData(nameof(Address.Street), SortDirection.ASC, "Aleja Zwycięstwa 115")]
        [InlineData(nameof(Address.Street), SortDirection.DESC, "Oxford Street 15")]
        public void GetAll_WithSorting_Returns_SortedResults(string sortBy, SortDirection direction, string expectedFirstValue)
        {
            //Arrange

            SeedAddress();

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
        [InlineData(1, 1, "Aleja Zwycięstwa 115")]
        [InlineData(2, 1, "Drottninggatan 22")]
        [InlineData(3, 1, "Karls 74")]
        [InlineData(4, 1, "Oxford Street 15")]
        public void GetAll_WithPagination_ReturnsCorrectPage(int pageNumber, int pageSize, string expectedFirstStreet)
        {
            //Arrange

            SeedAddress();

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

            SeedAddress();

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

            var dto = new CreateAddressDto
            {
                Country = "Portugal",
                City = "Lizbona",
                Street = "Polna 147",
                PostalCode = "12345"
            };

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

            var dto = new CreateAddressDto
            {
                Country = "Portugal",
                City = "Lizbona",
                Street = "Polna 147",
                PostalCode = "12345"
            };

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

            SeedAddress();

            var dto = new UpdateAddressDto
            {
                Country = "Portugal",
                City = "Lizbona",
                Street = "Polna 147",
                PostalCode = "12345"
            };

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success);

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

            SeedAddress();

            var dto = new UpdateAddressDto
            {
                Country = "Portugal",
                City = "Lizbona",
                Street = "Polna 147",
                PostalCode = "12345"
            };

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Failed);

            //Act

            Action action = () => _sut.UpdateAddress(4, dto);

            //Assert

            action.Should().Throw<ForbidException>().WithMessage("You don't have permission to update this address");
        }

        [Fact]
        public void UpdateAddress_WhenAddressDoesNotExist_ThrowsNotFoundException()
        {
            //Arrange

            var dto = new UpdateAddressDto
            {
                Country = "Portugal",
                City = "Lizbona",
                Street = "Polna 147",
                PostalCode = "12345"
            };

            //Act

            Action action = () => _sut.UpdateAddress(999, dto);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Address not found");
        }

        [Fact]
        public void DeleteAddress_WhenUserIsAuthorize_ShouldDeleteAddress()
        {
            //Arrnge

            SeedAddress();

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success);

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

            SeedAddress();

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Failed);

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
