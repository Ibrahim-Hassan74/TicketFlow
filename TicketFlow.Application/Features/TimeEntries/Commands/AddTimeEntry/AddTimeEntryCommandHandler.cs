using MediatR;
using Microsoft.Extensions.Logging;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.TimeEntries.Commands.AddTimeEntry
{
    public sealed class AddTimeEntryCommandHandler : IRequestHandler<AddTimeEntryCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AddTimeEntryCommandHandler> _logger;
        public AddTimeEntryCommandHandler(IUnitOfWork unitOfWork, ILogger<AddTimeEntryCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handle(AddTimeEntryCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _unitOfWork.TicketsRepository.GetByIdAsync(request.TicketId);
            if (ticket == null || ticket.AssignedAgentId != request.AgentId)
            {
                _logger.LogWarning("Agent {AgentId} denied access to add time entry to ticket {TicketId}.", request.AgentId, request.TicketId);
                throw new NotFoundException("Ticket not found or not assigned to you.");
            }

            if (request.CreateTimeEntry.WorkDate.ToDateTime(TimeOnly.MinValue) > DateTime.UtcNow)
                throw new BadRequestException("Work date cannot be in the future.");

            var timeEntry = new TimeEntry
            {
                TicketId = request.TicketId,
                AgentId = request.AgentId,
                WorkDate = request.CreateTimeEntry.WorkDate,
                DurationMinutes = request.CreateTimeEntry.DurationMinutes,
                Description = request.CreateTimeEntry.Description,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.TimeEntryRepository.AddAsync(timeEntry);

            var activity = new TicketActivity
            {
                TicketId = request.TicketId,
                UserId = request.AgentId,
                Type = ActivityType.TimeLogged,
                Description = $"Logged {request.CreateTimeEntry.DurationMinutes} minutes of work.",
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _unitOfWork.TicketActivityRepository.AddAsync(activity);

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Agent {AgentId} logged {Duration} minutes on ticket {TicketId}.", request.AgentId, request.CreateTimeEntry.DurationMinutes, request.TicketId);
        }
    }
}
