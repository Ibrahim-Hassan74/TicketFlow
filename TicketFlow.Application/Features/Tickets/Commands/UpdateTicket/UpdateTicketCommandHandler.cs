using MediatR;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Domain.Contracts;

namespace TicketFlow.Application.Features.Tickets.Commands.UpdateTicket
{
    public sealed class UpdateTicketCommandHandler : IRequestHandler<UpdateTicketCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTicketCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _unitOfWork.TicketsRepository.GetByIdAsync(request.Id, cancellationToken);
            if (ticket == null)
                throw new NotFoundException("Ticket not found.");

            ticket.Title = request.Title;
            ticket.Description = request.Description;
            ticket.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.CompleteAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
