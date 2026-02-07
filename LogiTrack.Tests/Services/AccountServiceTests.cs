using FluentAssertions;
using LogiTrack.Entities;
using LogiTrack.Exceptions;
using LogiTrack.Models;
using LogiTrack.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LogiTrack.Tests.Services
{
    public class AccountServiceTests
    {
        private readonly LogiTrackDbContext _dbContext;
        private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
        private readonly AuthenticationSettings _authenticationSettingsMock;
        private readonly AccountService _sut;

        public AccountServiceTests()
        {
            var options = new DbContextOptionsBuilder<LogiTrackDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new LogiTrackDbContext(options);

            _dbContext.Roles.AddRange(new List<Role>
            {
                new Role { Id = 1, Name = "User" },
                new Role { Id = 2, Name = "Manager" },
                new Role { Id = 3, Name = "Admin" }
            });

            _dbContext.SaveChanges();

            _passwordHasherMock = new Mock<IPasswordHasher<User>>();

            _authenticationSettingsMock = new AuthenticationSettings
            {
                JwtKey = "THIS_IS_A_SECRET_KEY_12345678910_THIS_IS_A_SECRET_KEY_12345678910",
                JwtIssuer = "TestIssuer",
                JwtExpireDays = 7
            };

            _sut = new AccountService(_dbContext, _passwordHasherMock.Object, _authenticationSettingsMock);
        }

        [Theory]
        [InlineData(1, "User")]
        [InlineData(2, "Manager")]
        [InlineData(3, "Admin")]
        public void RegisterUser_WithDifferentRoles_ShouldAddUserToDataBase(int roleId, string roleName)
        {
            //Arrange

            var dto = new RegisterUserDto
            {
                Email = $"test_{roleName.ToLower()}@test.com",
                Password = "password",
                ConfirmPassword = "password",
                DateOfBirth = new DateTime(1990, 1, 1),
                Nationality = "Polish",
                RoleId = roleId
            };

            _passwordHasherMock
                .Setup(x => x.HashPassword(
                    It.IsAny<User>(),
                    dto.Password))
                .Returns("HASHED_PASSWORD");

            //Act

            _sut.RegisterUser(dto);

            //Assert

            var user = _dbContext.Users.Single(u => u.Email == dto.Email);

            user.Should().NotBeNull();
            user.PasswordHash.Should().Be("HASHED_PASSWORD");
            user.RoleId.Should().Be(roleId);
            user.Nationality.Should().Be("Polish");
            user.DateOfBirth.Should().Be(new DateTime(1990, 1, 1));
            user.Role.Name.Should().Be(roleName);
        }

        [Theory]
        [InlineData(1, "User")]
        [InlineData(2, "Manager")]
        [InlineData(3, "Admin")]
        public void GenerateJwtTokenForValidCredentials_ShouldReturnsTokenAndContainCorrectRoleName(int roleId, string roleName)
        {
            //Arrange

            var email = $"jwt_{roleName.ToLower()}@test.com";
            var password = "password";

            var user = new User
            {
                Email = email,
                PasswordHash = "HASHED_PASSWORD",
                RoleId = roleId,
                DateOfBirth = new DateTime(1990, 1, 1),
                Nationality = "Polish"
            };

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            _passwordHasherMock
                .Setup(x => x.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    password))
                .Returns(PasswordVerificationResult.Success);

            var dto = new LoginDto
            {
                Email = email,
                Password = password
            };

            //Act

            var token = _sut.GenerateJwt(dto);

            //Assert

            token.Should().NotBeNull();
            token.Should().NotBeEmpty();

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            jwt.Claims.Single(c => c.Type == ClaimTypes.Role).Value.Should().Be(roleName);
        }

        [Fact]
        public void GenerateJwt_WhenUserDoesNotExist_ShouldThrowBadRequestException()
        {
            //Arrange

            var dto = new LoginDto
            {
                Email = "notexists@test.com",
                Password = "password"
            };

            //Act

            Action action = () => _sut.GenerateJwt(dto);

            //Assert

            action.Should().Throw<BadRequestException>().WithMessage("Invalid username or password");
        }

        [Fact]
        public void GenerateJwt_WhenPasswordIsInvalid_ShouldThrowBadRequestException()
        {
            //Arrange

            var user = new User
            {
                Email = "testowy@wp.pl",
                PasswordHash = "HASHED_PASSWORD",
                RoleId = 1,
                DateOfBirth = new DateTime(1990, 1, 1),
                Nationality = "Polish"
            };

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            _passwordHasherMock
                .Setup(x => x.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    "WrongPassword"))
                .Returns(PasswordVerificationResult.Failed);

            var dto = new LoginDto
            {
                Email = "testowy@wp.pl",
                Password = "WrongPassword"
            };

            //Act

            Action action = () => _sut.GenerateJwt(dto);

            //Assert

            action.Should().Throw<BadRequestException>().WithMessage("Invalid username or password");
        }
    }
}
