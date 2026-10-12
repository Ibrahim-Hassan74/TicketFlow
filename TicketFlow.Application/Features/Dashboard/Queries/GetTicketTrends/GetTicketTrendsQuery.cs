using MediatR;
using TicketFlow.Application.Common.Models;

namespace TicketFlow.Application.Features.Dashboard.Queries.GetTicketTrends
{
    public sealed class GetTicketTrendsQuery : IRequest<IEnumerable<TicketTrendResponse>>
    {
        public string Role { get; set; } = string.Empty;
        public int Days { get; set; }
    }
}
