using FluentValidation;

namespace TicketFlow.Application.Features.Comments.Commands.AddComment
{
    public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
    {
        public AddCommentCommandValidator()
        {
            RuleFor(x => x.Content)
               .NotEmpty()
               .WithMessage("{PropertyName} can't be blank.")
               .MaximumLength(2000)
               .WithMessage("{PropertyName} must not exceed 2000 characters.")
               .WithName("Comment Content");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("{PropertyName} can't be blank.")
                .WithName("User Id");

            RuleFor(x => x.Role)
                .NotEmpty()
                .WithMessage("{PropertyName} can't be blank.")
                .WithName("Role");

            RuleFor(x => x.TicketId)
                .NotEmpty()
                .WithMessage("{PropertyName} can't be blank.")
                .WithName("Ticket Id");

        }
    }
}