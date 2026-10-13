using FluentValidation;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Tickets.DTOs
{
    public class UpdateTicketStatusRequest
    {
        public TicketStatus Status { get; set; }
    }
    public class UpdateTicketStatusRequestValidator : AbstractValidator<UpdateTicketStatusRequest>
    {
        public UpdateTicketStatusRequestValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty()
                .WithMessage("Status can't be blank.");
        }
    }
}
