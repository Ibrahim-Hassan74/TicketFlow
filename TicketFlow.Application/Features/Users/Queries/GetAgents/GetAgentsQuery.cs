using MediatR;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Users.DTOs;

namespace TicketFlow.Application.Features.Users.Queries.GetAgents
{
    public sealed class GetAgentsQuery : IRequest<PaginatedResponse<UserResponse>>
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
