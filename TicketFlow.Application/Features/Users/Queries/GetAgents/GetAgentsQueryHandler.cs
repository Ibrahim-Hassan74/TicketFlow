using MediatR;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Users.DTOs;

namespace TicketFlow.Application.Features.Users.Queries.GetAgents
{
    public sealed class GetAgentsQueryHandler : IRequestHandler<GetAgentsQuery, PaginatedResponse<UserResponse>>
    {
        private readonly IUserRepository _userRepository;

        public GetAgentsQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<PaginatedResponse<UserResponse>> Handle(GetAgentsQuery request, CancellationToken cancellationToken)
        {

            return await _userRepository.GetAgentsAsync(request.Page, request.PageSize, cancellationToken);
        }
    }
}
