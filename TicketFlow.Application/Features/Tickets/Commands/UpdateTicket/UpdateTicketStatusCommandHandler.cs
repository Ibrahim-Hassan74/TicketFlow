using MediatR;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Domain.Contracts;

namespace TicketFlow.Application.Features.Tickets.Commands.UpdateTicket
{
    public sealed class UpdateTicketStatusCommandHandler : IRequestHandler<UpdateTicketCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTicketStatusCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _unitOfWork.TicketsRepository.GetByIdAsync(request.Id, cancellationToken);
            if (ticket == null)
                throw new NotFoundException("Ticket not found.");

            ticket.Title = request.UpdateTicket.Title;
            ticket.Description = request.UpdateTicket.Description;
            ticket.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.CompleteAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
