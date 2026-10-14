using MediatR;
using TicketFlow.Application.Features.TimeEntries.DTOs;

namespace TicketFlow.Application.Features.TimeEntries.Commands.AddTimeEntry
{
    public sealed class AddTimeEntryCommand : IRequest<Unit>
    {
        public Guid TicketId { get; set; }
        public Guid AgentId { get; set; }
        public DateOnly WorkDate { get; set; }
        public int DurationMinutes { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
