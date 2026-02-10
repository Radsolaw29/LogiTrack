using FluentValidation;
using LogiTrack.Entities;

namespace LogiTrack.Models.Validators
{
    public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
    {
        public RegisterUserDtoValidator(LogiTrackDbContext dbContext)
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .MinimumLength(6)
                .WithMessage("The password must be at least 6 characters long.");

            RuleFor(x => x.ConfirmPassword)
                .Equal(e => e.Password)
                .WithMessage("Password and ConfirmPassword must match.");

            RuleFor(x => x.Email)
                .Custom((value, context) =>
                {
                   var emailInUse = dbContext.Users.Any(u => u.Email == value);
                    if (emailInUse)
                    {
                        context.AddFailure("Email", "That e-mail is taken");
                    }
                });
        }
    }
}
