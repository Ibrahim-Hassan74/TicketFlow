using MediatR;
using TicketFlow.Application.Features.Users.DTOs;

namespace TicketFlow.Application.Features.Users.Commands.UpdateUser
{
    public sealed class UpdateUserCommand : IRequest<UserResponse>
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
