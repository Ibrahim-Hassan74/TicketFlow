using MediatR;
using TicketFlow.Application.Common.Models;

namespace TicketFlow.Application.Features.Dashboard.Queries.GetAgentWorkload
{
    public sealed class GetAgentWorkloadQuery : IRequest<IEnumerable<AgentWorkloadResponse>>
    {
        public string Role { get; set; } = string.Empty;
    }
}
