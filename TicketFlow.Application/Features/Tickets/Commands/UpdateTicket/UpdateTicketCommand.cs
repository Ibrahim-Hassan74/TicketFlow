using MediatR;

namespace TicketFlow.Application.Features.Tickets.Commands.UpdateTicket
{
    public sealed class UpdateTicketCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
