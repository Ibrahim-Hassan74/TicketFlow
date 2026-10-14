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

            RuleFor(x => x.Role)
                .NotEmpty()
                .WithMessage("Role can't be blank.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User Id can't be blank.");

            RuleFor(x => x.TicketId)
                .NotEmpty()
                .WithMessage("Ticket Id can't be blank.");


        }
    }
}
