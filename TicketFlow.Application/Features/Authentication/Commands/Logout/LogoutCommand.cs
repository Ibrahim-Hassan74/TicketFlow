using MediatR;

namespace TicketFlow.Application.Features.Authentication.Commands.Logout
{
    public class LogoutCommand : IRequest
    {
        public Guid Id { get; set; }
    }
}
