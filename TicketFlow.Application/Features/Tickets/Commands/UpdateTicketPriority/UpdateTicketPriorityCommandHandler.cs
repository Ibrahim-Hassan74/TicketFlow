using MediatR;
using Microsoft.Extensions.Logging;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.Tickets.Commands.UpdateTicketPriority
{
    public sealed class UpdateTicketPriorityCommandHandler : IRequestHandler<UpdateTicketPriorityCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateTicketPriorityCommandHandler> _logger;

        public UpdateTicketPriorityCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateTicketPriorityCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Unit> Handle(UpdateTicketPriorityCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _unitOfWork.TicketsRepository.GetByIdAsync(request.TicketId, cancellationToken);
            if (ticket == null)
                throw new NotFoundException("Ticket not found.");

            var oldPriority = ticket.Priority;
            ticket.Priority = request.NewPriority;
            ticket.UpdatedAt = DateTimeOffset.UtcNow;

            if (oldPriority != request.NewPriority)
            {
                var activity = new TicketActivity
                {
                    TicketId = ticket.Id,
                    UserId = request.UserId,
                    Type = ActivityType.PriorityChanged,
                    Description = $"Priority changed from {oldPriority} to {request.NewPriority}",
                    OldValue = oldPriority.ToString(),
                    NewValue = request.NewPriority.ToString(),
                    CreatedAt = DateTimeOffset.UtcNow
                };
                await _unitOfWork.TicketActivityRepository.AddAsync(activity, cancellationToken);
            }

            await _unitOfWork.TicketsRepository.UpdateAsync(ticket, cancellationToken);

            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("Admin {AdminId} updated priority for ticket {TicketId} from {OldPriority} to {NewPriority}.", request.UserId, request.TicketId, oldPriority, request.NewPriority);

            return Unit.Value;
        }
    }
}
