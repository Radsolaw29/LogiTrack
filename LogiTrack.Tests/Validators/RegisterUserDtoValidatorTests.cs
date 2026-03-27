using FluentValidation.TestHelper;
using LogiTrack.Entities;
using LogiTrack.Models;
using LogiTrack.Models.Validators;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogiTrack.UnitTests.Validators
{
    public class RegisterUserDtoValidatorTests
    {
        private readonly RegisterUserDtoValidator _validateor;
        private readonly LogiTrackDbContext _dbContext;

        public RegisterUserDtoValidatorTests()
        {
            var options = new DbContextOptionsBuilder<LogiTrackDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new LogiTrackDbContext(options);

            _validateor = new RegisterUserDtoValidator(_dbContext);
        }

        [Fact]
        public void Validate_ForEmptyEmail_ShouldReturnsValidationError()
        {
            // Arrange

            var dto = new RegisterUserDto
            {
                Email = "",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act

            var result = _validateor.TestValidate(dto);

            // Assert

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Validate_ForInvalidEmailFormat_ShouldReturnsValidationError()
        {
            // Arrange

            var dto = new RegisterUserDto
            {
                Email = "invalid-email",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act

            var result = _validateor.TestValidate(dto);

            // Assert

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Validate_ForEmailAlreadyInUse_ShouldReturnsValidationError()
        {
            //Arrange

            _dbContext.Users.Add(new User
            {
                Email = "Testowyemail@wp.pl"
            });

            _dbContext.SaveChanges();

            var dto = new RegisterUserDto
            {
                Email = "Testowyemail@wp.pl",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act

            var result = _validateor.TestValidate(dto);

            // Assert

            result.ShouldHaveValidationErrorFor(x => x.Email)
                .WithErrorMessage("That e-mail is taken");
        }

        [Fact]
        public void Validate_ForTooShortPassword_ShouldReturnsValidationError()
        {
            //Arrange

            var dto = new RegisterUserDto
            {
                Email = "test@wp.pl",
                Password = "123",
                ConfirmPassword = "123"
            };

            //Act

            var result = _validateor.TestValidate(dto);

            //Assert

            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("The password must be at least 6 characters long.");
        }

        [Fact]
        public void Validate_ForPasswordsDoesNotMatch_ShouldReturnsValidationError()
        {
            //Arrange

            var dto = new RegisterUserDto
            {
                Email = "test@wp.pl",
                Password = "Password123!",
                ConfirmPassword = "differentPassword"
            };

            //Act

            var result = _validateor.TestValidate(dto);

            //Assert

            result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
                .WithErrorMessage("Password and ConfirmPassword must match.");
        }

        [Fact]
        public void Validate_ForValidDto_ShouldNotReturnsAnyValidationErrors()
        {
            //Arrange

            var dto = new RegisterUserDto
            {
                Email = "testowy123@wp.pl",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            //Act

            var result = _validateor.TestValidate(dto);

            //Assert

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}