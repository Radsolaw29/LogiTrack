using FluentValidation;

namespace LogiTrack.Models.Validators
{
    public class CompanyQueryValidator : AbstractValidator<CompanyQuery>
    {
        private int [] allowedPageSizes = new[] {5, 10, 20, 50};

        public CompanyQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);

            RuleFor(x => x.PageSize).Custom((value, context) =>
            {
                if (!allowedPageSizes.Contains(value)) 
                {
                    context.AddFailure("PageSize", $"PageSize must in [{string.Join(",", allowedPageSizes)}]");
                }
            });
        }
    }
}
