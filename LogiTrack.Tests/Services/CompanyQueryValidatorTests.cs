using LogiTrack.Models;
using LogiTrack.Models.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.TestHelper;

namespace LogiTrack.Tests.Services
{
    public class CompanyQueryValidatorTests
    {
        private readonly CompanyQueryValidator _validator;

        public CompanyQueryValidatorTests()
        {
            _validator = new CompanyQueryValidator();
        }

        [Fact]
        public void Validate_ForPageNumbersLessThanOne_ShouldReturnsValidatorError()
        {
            //Arrange

            var query = new CompanyQuery
            {
                PageNumber = 0,
                PageSize = 10
            };

            //Act

            var result = _validator.TestValidate(query);

            //Assert

            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                .WithErrorMessage("The number of pages must be greater than or equal to 1");
        }

        [Fact]
        public void Validate_ForPageNumberGreaterThanOrEqualToOne_ShouldNotReturnsValidationError()
        {
            //Arrange

            var query = new CompanyQuery
            {
                PageNumber = 1,
                PageSize = 10
            };

            //Act

            var result = _validator.TestValidate(query);

            //Assert

            result.ShouldNotHaveValidationErrorFor(x => x.PageNumber);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(15)]
        [InlineData(25)]
        [InlineData(100)]
        [InlineData(150)]
        public void Validate_ForInvalidPageSizes_ShouldReturnsValidationError(int pageSize)
        {
            //Arrange

            var queru = new CompanyQuery
            {
                PageNumber = 1,
                PageSize = pageSize
            };

            //Act

            var result = _validator.TestValidate(queru);

            //Assert

            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage("PageSize must be in [5,10,20,50]");
        }

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(50)]
        public void Validate_ForValidPageSizes_ShouldNotReturnsValidationError(int pageSize)
        {
            //Arrange

            var query = new CompanyQuery
            {
                PageNumber = 1,
                PageSize = pageSize
            };

            //Act

            var result = _validator.TestValidate(query);

            //Assert

            result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
        }

        [Fact]
        public void Validate_ForNullSortBy_ShouldNotReturnsValidationError()
        {
            //Arrange

            var query = new CompanyQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = null
            };

            //Act

            var result = _validator.TestValidate(query);

            //Assert

            result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
        }

        [Fact]
        public void Validate_ForEmptySortBy_ShouldNotReturnsValidationError()
        {
            //Arragne

            var query = new CompanyQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = string.Empty
            };

            //Act

            var result = _validator.TestValidate(query);

            //Assert

            result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
        }

        [Theory]
        [InlineData("Name")]
        [InlineData("Description")]
        [InlineData("ContactEmail")]
        public void Validate_ForValidSortBy_ShouldNotReturnsValidationError(string sortBy)
        {
            //Arrange

            var query = new CompanyQuery
            {
                PageNumber= 1,
                PageSize = 10,
                SortBy = sortBy
            };

            //Act

            var result = _validator.TestValidate(query);

            //Assert

            result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
        }

        [Fact]
        public void Validate_ForInvalidSortBy_ShouldReturnsValidationError()
        {
            //Arrange

            var query = new CompanyQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = "InvalidColumn"
            };

            //Act

            var result = _validator.TestValidate(query);

            //Assert

            result.ShouldHaveValidationErrorFor(x => x.SortBy)
                .WithErrorMessage("Sort by is optional, or must be in [Name,Description,ContactEmail]");
        }
    }
}
