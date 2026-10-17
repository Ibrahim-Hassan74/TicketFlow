using MediatR;
using TicketFlow.Application.Features.Activities.DTOs;

namespace TicketFlow.Application.Features.Activities.Queries.GetActivities
{
    public sealed class GetActivitiesQuery : IRequest<IEnumerable<ActivityResponse>>
    {
        public Guid TicketId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
