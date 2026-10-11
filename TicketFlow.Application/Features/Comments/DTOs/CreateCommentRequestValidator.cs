using FluentValidation;

namespace TicketFlow.Application.Features.Comments.DTOs
{
    public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
    {
        public CreateCommentRequestValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("{PropertyName} can't be blank.")
                .MaximumLength(2000)
                .WithMessage("{PropertyName} must not exceed 2000 characters.")
                .WithName("Comment Content");
        }
    }
}
