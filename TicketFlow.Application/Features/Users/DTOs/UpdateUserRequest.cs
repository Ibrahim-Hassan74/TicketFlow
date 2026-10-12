using FluentValidation;

namespace TicketFlow.Application.Features.Users.DTOs
{
    public class UpdateUserRequest
    {
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .WithMessage("Name can't be blank")
                .MaximumLength(100)
                .WithMessage("Name can't be longer than 100 characters");
        }
    }
}
