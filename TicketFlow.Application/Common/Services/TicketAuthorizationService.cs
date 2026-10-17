using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Domain.Contracts;

namespace TicketFlow.Application.Common.Services
{
    public class TicketAuthorizationService : ITicketAuthorizationService
    {
        private readonly IUnitOfWork _unitOfWork;
        public TicketAuthorizationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> CanAccessTicketAsync(Guid ticketId, Guid userId, string role, CancellationToken cancellationToken = default)
        {
            var ticket = await _unitOfWork.TicketsRepository.GetByIdAsync(ticketId, cancellationToken);
            if (ticket == null)
                return false;
            return ticket.CanBeAccessedBy(userId, role);
        }
    }
}
