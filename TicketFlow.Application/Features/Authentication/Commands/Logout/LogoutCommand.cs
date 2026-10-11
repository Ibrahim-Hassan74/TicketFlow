using MediatR;

namespace TicketFlow.Application.Features.Authentication.Commands.Logout
{
    public sealed class LogoutCommand : IRequest
    {
        public Guid Id { get; set; }
    }
}
