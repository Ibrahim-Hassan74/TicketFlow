using MediatR;
using TicketFlow.Application.Features.Users.DTOs;

namespace TicketFlow.Application.Features.Users.Commands.CreateUser
{
    public sealed class CreateUserCommand : IRequest<UserResponse>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
