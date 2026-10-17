using MediatR;
using TicketFlow.Application.Features.Tickets.DTOs;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Tickets.Commands.CreateTicket
{
    public sealed class CreateTicketCommand : IRequest<TicketResponse>
    {
        public Guid CustomerId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    }
}
