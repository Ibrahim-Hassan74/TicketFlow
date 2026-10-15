using MediatR;
using Microsoft.Extensions.Logging;
using System.Data;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.TimeEntries.DTOs;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Application.Features.TimeEntries.Queries.GetTimeEntries
{
    public sealed class GetTimeEntriesQueryHandler : IRequestHandler<GetTimeEntriesQuery, TicketTimeEntriesResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetTimeEntriesQueryHandler> _logger;
        public GetTimeEntriesQueryHandler(IUnitOfWork unitOfWork, ILogger<GetTimeEntriesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<TicketTimeEntriesResponse> Handle(GetTimeEntriesQuery request, CancellationToken cancellationToken)
        {
            var ticket = await _unitOfWork.TicketsRepository.GetByIdAsync(request.TicketId, cancellationToken);
            if (ticket == null)
                throw new NotFoundException("Ticket not found.");

            if (request.Role == UserRole.Customer.ToString())
            {
                _logger.LogWarning("Customer {UserId} denied access to get time entries for ticket {TicketId}.", request.UserId, request.TicketId);
                throw new NotFoundException("Ticket not found.");
            }

            if (request.Role == UserRole.SupportAgent.ToString() && ticket.AssignedAgentId != request.UserId)
            {
                _logger.LogWarning("Agent {AgentId} denied access to get time entries for ticket {TicketId}.", request.UserId, request.TicketId);
                throw new NotFoundException("Ticket not found or not assigned to you.");
            }

            var timeEntries = (await _unitOfWork.TimeEntryRepository.GetTimeEntriesByTicketIdAsync(
                request.TicketId,
                cancellationToken)).ToList();

            var response = new TicketTimeEntriesResponse
            {
                TotalDurationMinutes = timeEntries.Sum(t => t.DurationMinutes),
                Entries = timeEntries.Select(t => new TimeEntryResponse
                {
                    Id = t.Id,
                    TicketId = t.TicketId,
                    AgentId = t.AgentId,
                    AgentName = t.Agent?.DisplayName ?? "Unknown",
                    WorkDate = t.WorkDate,
                    DurationMinutes = t.DurationMinutes,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt
                }).ToList()
            };

            return response;
        }
    }
}
