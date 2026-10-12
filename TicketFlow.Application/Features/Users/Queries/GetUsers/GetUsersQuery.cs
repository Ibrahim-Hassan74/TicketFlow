using MediatR;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Users.DTOs;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Users.Queries.GetUsers
{
    public sealed class GetUsersQuery : IRequest<PaginatedResponse<UserResponse>>
    {
        public UserRole? Role { get; set; }

        public string? Search { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
