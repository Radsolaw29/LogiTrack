using FluentValidation;
using LogiTrack.Entities;

namespace LogiTrack.Models.Validators
{
    public class AddressQueryValidator : AbstractValidator<AddressQuery>
    {
        private int[] allowedPageSizes = new[] { 5, 10, 20, 50 };
        private string[] allowedSortByColumnNames = { nameof(Address.Country), nameof(Address.City), nameof(Address.Street)};

        public AddressQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).WithMessage("The number of pages must be greater than or equal to 1");

            RuleFor(x => x.PageSize).Custom((value, context) => 
            {
                if (!allowedPageSizes.Contains(value))
                {
                    context.AddFailure("PageSize", $"Page size must be in [{string.Join(",", allowedPageSizes)}]");
                }
            });

            RuleFor(r => r.SortBy).Must(value => string.IsNullOrEmpty(value) || allowedSortByColumnNames.Contains(value))
                .WithMessage($"Sort by is optional, or must be in [{string.Join(",", allowedSortByColumnNames)}]");
        }
    }
}
