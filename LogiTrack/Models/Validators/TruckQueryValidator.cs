using FluentValidation;
using LogiTrack.Entities;

namespace LogiTrack.Models.Validators
{
    public class TruckQueryValidator : AbstractValidator<TruckQuery>
    {
        private int[] allowedPageSizes = new[] { 5, 10, 20, 50 };
        private string[] allowedSortByColumnNames = { nameof(Truck.RegistrationNumber), nameof(Truck.Brand), nameof(Truck.Model) };

        public TruckQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).WithMessage("The number of pages must be greater than or equal to 1");

            RuleFor(x => x.PageSize).Custom((value, context) =>
            {
                if (!allowedPageSizes.Contains(value)) 
                {
                    context.AddFailure("PageSize", $"Page size must be in [{string.Join(",", allowedPageSizes)}]");
                }
            });

            RuleFor(x => x.SortBy).Must(value => string.IsNullOrEmpty(value) || allowedSortByColumnNames.Contains(value))
                .WithMessage($"Sort by is optional or must be in [{string.Join(",", allowedSortByColumnNames)}]");
        }
    }
}
