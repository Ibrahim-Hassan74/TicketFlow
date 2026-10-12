using MediatR;
using TicketFlow.Application.Features.TimeEntries.DTOs;

namespace TicketFlow.Application.Features.TimeEntries.Queries.GetTimeEntries
{
    public sealed class GetTimeEntriesQuery : IRequest<TicketTimeEntriesResponse>
    {
        public Guid TicketId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
