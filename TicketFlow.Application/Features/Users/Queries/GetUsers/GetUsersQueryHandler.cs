using MediatR;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Users.DTOs;

namespace TicketFlow.Application.Features.Users.Queries.GetUsers
{
    public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedResponse<UserResponse>>
    {
        private readonly IUserRepository _userRepository;
        public GetUsersQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<PaginatedResponse<UserResponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            if (request.Page < 1)
                request.Page = 1;

            if (request.PageSize < 1)
                request.PageSize = 10;

            if (request.PageSize > 100)
                request.PageSize = 100;

            return await _userRepository.GetUsersAsync(
                request.Page,
                request.PageSize,
                request.Role,
                request.Search,
                cancellationToken);
        }
    }
}
