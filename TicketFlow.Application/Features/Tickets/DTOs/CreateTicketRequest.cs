using FluentValidation;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Tickets.DTOs
{
    public class CreateTicketRequest
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    }
    public sealed class CreateTicketRequestValidator : AbstractValidator<CreateTicketRequest>
    {
        public CreateTicketRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title can't be blank.")
                .MaximumLength(200)
                .WithMessage("Title can't be longer than 200 characters.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description can't be blank.")
                .MaximumLength(4000)
                .WithMessage("Description can't be longer than 4000 characters.");

            RuleFor(x => x.Priority)
                .IsInEnum()
                .WithMessage("Invalid ticket priority.");
        }
    }
}
