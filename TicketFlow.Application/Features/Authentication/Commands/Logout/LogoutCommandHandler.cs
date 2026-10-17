using MediatR;
using Microsoft.AspNetCore.Identity;
using TicketFlow.Domain.IdentityEntities;

namespace TicketFlow.Application.Features.Authentication.Commands.Logout
{
    public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LogoutCommandHandler(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            if (request.Id != Guid.Empty)
            {
                var user = await _userManager.FindByIdAsync(request.Id.ToString());
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpirationDateTime = DateTimeOffset.MinValue;
                    await _userManager.UpdateAsync(user);
                }
            }

            await _signInManager.SignOutAsync();

            return Unit.Value;
        }
    }
}
