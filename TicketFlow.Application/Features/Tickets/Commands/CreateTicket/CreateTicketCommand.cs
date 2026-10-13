using MediatR;
using TicketFlow.Application.Features.Tickets.DTOs;

namespace TicketFlow.Application.Features.Tickets.Commands.CreateTicket
{
    public sealed class CreateTicketCommand : IRequest<TicketResponse>
    {
        public Guid CustomerId { get; set; }
        public CreateTicketRequest CreateTicket { get; set; }
    }
}
