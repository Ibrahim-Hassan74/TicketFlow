using MediatR;
using TicketFlow.Application.Features.TimeEntries.DTOs;

namespace TicketFlow.Application.Features.TimeEntries.Commands.AddTimeEntry
{
    public sealed class AddTimeEntryCommand : IRequest
    {
        public Guid TicketId { get; set; }
        public CreateTimeEntryRequest CreateTimeEntry { get; set; }
        public Guid AgentId { get; set; }
    }
}
