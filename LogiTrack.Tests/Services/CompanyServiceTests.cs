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
    public class CompanyServiceTests
    {
        private readonly LogiTrackDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly CompanyService _sut;
        private readonly Mock<IUserContextService> _userContextServiceMock;
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;

        public CompanyServiceTests()
        {
            var options = new DbContextOptionsBuilder<LogiTrackDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new LogiTrackDbContext(options);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Company, CompanyDto>();
                cfg.CreateMap<CreateCompanyDto, Company>()
                    .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new Address
                    {
                        Country = src.Country,
                        City = src.City,
                        Street = src.Street,
                        PostalCode = src.PostalCode
                    }));
            });

            _mapper = mapperConfig.CreateMapper();

            _authorizationServiceMock = new Mock<IAuthorizationService>();

            _userContextServiceMock = new Mock<IUserContextService>();

            _userContextServiceMock.Setup(x => x.GetUserId).Returns(1);

            _userContextServiceMock.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));

            var loggerMock = new Mock<ILogger<CompanyService>>();

            _sut = new CompanyService(_dbContext, _mapper, loggerMock.Object, _authorizationServiceMock.Object, _userContextServiceMock.Object);
        }

        [Theory]
        [InlineData("Eagle Trans", 1)]
        [InlineData("Nordic Logistics", 1)]
        [InlineData("Norway Logistics", 1)]
        [InlineData("Scandinavian transport services", 2)]
        [InlineData("A transport company serving all of Europe.", 1)]
        public void GetAll_WithDifferentSearchPhrases_ReturnsExpectedCount(string searchPhrase, int expectedCount)
        {
            //Arrange

            CompanySeeder.SeedCompanies(_dbContext);

            var query = new CompanyQuery
            {
                SearchPhrase = searchPhrase,
                PageNumber = 1,
                PageSize = 10
            };

            //Act

            var result = _sut.GetAll(query);

            //Assert

            result.Items.Count.Should().Be(expectedCount);
        }

        [Theory]
        [InlineData(nameof(Company.Name), SortDirection.ASC, "Eagle Trans")]
        [InlineData(nameof(Company.Name), SortDirection.DESC, "Norway Logistics")]
        [InlineData(nameof(Company.Description), SortDirection.ASC, "A transport company serving all of Europe.")]
        [InlineData(nameof(Company.Description),SortDirection.DESC, "Scandinavian transport services.")]
        [InlineData(nameof(Company.ContactEmail), SortDirection.ASC, "contact@nordic.com")]
        [InlineData(nameof(Company.ContactEmail), SortDirection.DESC, "transport@wp.pl")]
        public void GetAll_WithSorting_ReturnsSortedResults(string sortBy, SortDirection direction, string expectedFirstValue)
        {
            //Arrange

            CompanySeeder.SeedCompanies(_dbContext);

            var query = new CompanyQuery
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
        [InlineData(1, 1, "Eagle Trans")]
        [InlineData(2, 1, "Nordic Logistics")]
        [InlineData(3, 1, "Norway Logistics")]
        public void GetAll_WithPagination_ReturnsCorrectPage(int pageNumber, int pageSize, string expectedFirstCompanyName)
        {
            //Arrange

            CompanySeeder.SeedCompanies(_dbContext);

            var query = new CompanyQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = nameof(Company.Name),
                SortDirection = SortDirection.ASC
            };

            //Act

            var result = _sut.GetAll(query);

            //Assert

            result.Items.Should().HaveCount(1);
            result.Items.First().Name.Should().Be(expectedFirstCompanyName);
            result.TotalItemsCount.Should().Be(3);
        }

        [Fact]
        public void GetById_WhenCompanyExist_ReturnsCompanyDto()
        {
            //Arrange

            CompanySeeder.SeedCompanies(_dbContext);

            //Act

            var result = _sut.GetById(3);

            //Assert

            result.Should().NotBeNull();
            result.Id.Should().Be(3);
        }

        [Fact]
        public void GetById_WhenCompanyDoesNotExist_ThrowsNotFoundException()
        {
            //Act

            Action action = () => _sut.GetById(999);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Company not found");
        }

        [Fact]
        public void CreateCompany_WithValidData_CreatCompanyAndReturnsId()
        {
            //Arrange

            var dto = new CreateCompanyBuilder().Build();

            //Act

            var result = _sut.CreateCompany(dto);

            //Assert

            result.Should().BeGreaterThan(0);

            var company = _dbContext.Companies.Include(c => c.Address).First();

            company.Should().NotBeNull();
            
            company.Name.Should().Be("Eagle Trans");
            company.Description.Should().Be("A transport company serving all of Europe.");
            company.TaxNumber.Should().Be(123456789);
            company.PhoneNumber.Should().Be(123456789);
            company.ContactEmail.Should().Be("transport@wp.pl");
            company.Address.Should().NotBeNull();
            company.Address.Country.Should().Be(dto.Country);
            company.Address.City.Should().Be(dto.City);
            company.Address.Street.Should().Be(dto.Street);
            company.Address.PostalCode.Should().Be(dto.PostalCode);
            company.Id.Should().Be(1);
            company.CreatedById.Should().Be(1);
        }

        [Fact]
        public void CreateCompany_SetsCreatedByIdFromUserContext()
        {
            //Arrange

            var expectedUserId = 999;
            _userContextServiceMock.Setup(x => x.GetUserId).Returns(expectedUserId);

            var dto = new CreateCompanyBuilder().Build();

            //Act

            var result = _sut.CreateCompany(dto);

            //Assert

            var company = _dbContext.Companies.First(x => x.Id == result);

            company.Should().NotBeNull();
            company.CreatedById.Should().Be(expectedUserId);
        }

        [Fact]
        public void UpdateCompany_WhenUserIsAuthorized_UpdateCompany()
        {
            //Arrange

            CompanySeeder.SeedCompanies(_dbContext);

            var dto = new UpdateCompanyBuilder().Build();

            _authorizationServiceMock.SetupSuccess();

            //Act

            _sut.UpdateCompany(1, dto);

            //Assert

            var company = _dbContext.Companies.First(c => c.Id == 1);

            company.Name.Should().Be(dto.Name);
            company.Description.Should().Be(dto.Description);
            company.ContactEmail.Should().Be(dto.ContactEmail);
            company.TaxNumber.Should().Be(dto.TaxNumber);
        }

        [Fact]
        public void UpdateCompany_WhenUserIsNotAuthorized_ThrowsForbidException()
        {
            //Arrange

            CompanySeeder.SeedCompanies(_dbContext);

            var dto = new UpdateCompanyBuilder().Build();

            _authorizationServiceMock.SetupFail();

            //Act

            Action action = () => _sut.UpdateCompany(1, dto);

            //Assert

            action.Should().Throw<ForbidException>().WithMessage("You do not have permission to access this resource.");
        }

        [Fact]
        public void UpdateCompany_WhenCompanyDoesNotExist_ThrowsNotFoundException()
        {
            //Arrange

            var dto = new UpdateCompanyBuilder().Build();

            //Act

            Action action = () => _sut.UpdateCompany(999, dto);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Company not found");
        }

        [Fact]
        public void DeleteCompany_WhenUserIsAuthorized_ShouldDeleteCompany()
        {
            //Arrange

            CompanySeeder.SeedCompanies(_dbContext);

            _authorizationServiceMock.SetupSuccess();

            //Act

            _sut.DeleteCompany(1);

            //Assert

            var comapny = _dbContext.Companies.FirstOrDefault(x => x.Id == 1);
            comapny.Should().BeNull();

            _dbContext.Companies.Count().Should().Be(2);
        }

        [Fact]
        public void DeleteCompany_WhenUserIsNotAuthorized_ShouldThrowsForbidException()
        {
            //Arrange

            CompanySeeder.SeedCompanies(_dbContext);

            _authorizationServiceMock.SetupFail();

            //Act

            Action action = () => _sut.DeleteCompany(1);

            //Assert

            action.Should().Throw<ForbidException>().WithMessage("You do not have permission to access this resource.");
        }

        [Fact]
        public void DeleteCompany_WhenCompanyDoesNotExist_ShouldThrowsNotFoundException()
        {
            //Act

            Action action = () => _sut.DeleteCompany(999);

            //Assert

            action.Should().Throw<NotFoundException>().WithMessage("Company not found");
        }
    }
}