using System.Security.Claims;
using TicketFlow.Application.Common.Interfaces;

namespace TicketFlow.API.Services
{
    /// <include file='../Docs/ServicesDocs.xml' path='docs/members[@name="CurrentUserService"]/Class/*' />
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <include file='../Docs/ServicesDocs.xml' path='docs/members[@name="CurrentUserService"]/Constructor/*' />
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets the current authenticated user's unique identifier.
        /// Returns Guid.Empty if the user is unauthenticated or has no valid ID claim.
        /// </summary>
        public Guid UserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null)
                    return Guid.Empty;

                var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? user.FindFirst("sub")?.Value
                           ?? user.FindFirst("uid")?.Value;

                return Guid.TryParse(idClaim, out var userId) ? userId : Guid.Empty;
            }
        }

        /// <summary>
        /// Gets the current authenticated user's role (e.g. Admin, SupportAgent, Customer).
        /// Returns string.Empty if unauthenticated.
        /// </summary>
        public string Role
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                return user?.FindFirst(ClaimTypes.Role)?.Value
                    ?? user?.FindFirst("role")?.Value
                    ?? string.Empty;
            }
        }
    }
}
