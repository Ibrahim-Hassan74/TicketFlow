using MediatR;
using TicketFlow.Application.Features.Users.DTOs;

namespace TicketFlow.Application.Features.Users.Queries.GetUserById
{
    public sealed class GetUserByIdQuery : IRequest<UserResponse?>
    {
        public Guid Id { get; set; }
    }
}
