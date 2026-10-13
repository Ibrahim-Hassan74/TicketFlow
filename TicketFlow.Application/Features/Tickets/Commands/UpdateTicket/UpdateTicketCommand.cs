using MediatR;
using TicketFlow.Application.Features.Tickets.DTOs;

namespace TicketFlow.Application.Features.Tickets.Commands.UpdateTicket
{
    public sealed class UpdateTicketCommand : IRequest<Unit>
    {
        public UpdateTicketRequest UpdateTicket { get; set; }
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
    }
}
