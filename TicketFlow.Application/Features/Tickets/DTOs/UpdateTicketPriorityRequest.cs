using FluentValidation;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Tickets.DTOs
{
    public class UpdateTicketPriorityRequest
    {
        public TicketPriority Priority { get; set; }
    }
    public class UpdateTicketPriorityRequestValidator : AbstractValidator<UpdateTicketPriorityRequest>
    {
        public UpdateTicketPriorityRequestValidator()
        {
            RuleFor(x => x.Priority)
                .NotEmpty()
                .WithMessage("Priority can't be blank.");
        }
    }
}
