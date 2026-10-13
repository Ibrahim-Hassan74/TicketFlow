using FluentValidation;

namespace TicketFlow.Application.Features.Tickets.DTOs
{
    public class UpdateTicketRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateTicketRequestValidator : AbstractValidator<UpdateTicketRequest>
    {
        public UpdateTicketRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title can't be blank.")
                .MaximumLength(200)
                .WithMessage("Title can't be more than 200 char.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description can't be blank.")
                .MaximumLength(4000)
                .WithMessage("Description can't be more than 200 char.");

        }
    }
}
