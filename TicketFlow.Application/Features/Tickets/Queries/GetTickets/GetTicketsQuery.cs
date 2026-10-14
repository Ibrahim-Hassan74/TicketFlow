using MediatR;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Tickets.DTOs;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Tickets.Queries.GetTickets
{
    public sealed class GetTicketsQuery : IRequest<PaginatedResponse<TicketResponse>>
    {
        public TicketStatus? Status { get; init; }
        public TicketPriority? Priority { get; init; }
        public string? Search { get; init; }
        public SortByOptions? SortBy { get; init; }
        public SortOrderOptions? SortOrder { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;

        // Current User Context
        public Guid UserId { get; init; }
        public string Role { get; init; } = string.Empty;
    }
}
