using MediatR;
using TicketFlow.Application.Common.Models;

namespace TicketFlow.Application.Features.Dashboard.Queries.GetStats
{
    public sealed class GetStatsQuery : IRequest<DashboardStatsResponse>
    {
        public string Role { get; set; } = string.Empty;
    }
}
