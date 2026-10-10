using MediatR;
using TicketFlow.Application.Features.Authentication.DTOs;

namespace TicketFlow.Application.Features.Authentication.Commands.Register
{
    public class RegisterCommand : IRequest<RegisterResponse>
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
