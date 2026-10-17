using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.IdentityEntities;

namespace TicketFlow.Application.Features.Tickets.Commands.AssignAgent
{
    public sealed class AssignAgentCommandHandler : IRequestHandler<AssignAgentCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AssignAgentCommandHandler> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public AssignAgentCommandHandler(IUnitOfWork unitOfWork, ILogger<AssignAgentCommandHandler> logger, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<Unit> Handle(AssignAgentCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _unitOfWork.TicketsRepository.GetByIdAsync(request.TicketId, cancellationToken);

            if (ticket == null)
                throw new NotFoundException("Ticket not found.");

            var agent = await _userManager.FindByIdAsync(request.AgentId.ToString());

            if (agent == null)
            {
                _logger.LogWarning("Agent {AgentId} not found while attempting to assign ticket {TicketId}.", request.AgentId, request.TicketId);
                throw new NotFoundException("Agent not found.");
            }

            var isAgent = await _userManager.IsInRoleAsync(agent, UserRole.SupportAgent.ToString());

            if (!isAgent)
            {
                _logger.LogWarning("User {UserId} is not a support agent. Cannot assign ticket {TicketId}.", agent.Id, request.TicketId);
                throw new BadRequestException("The specified user is not a Support Agent.");
            }

            ticket.AssignAgent(request.AgentId);
            ticket.UpdatedAt = DateTimeOffset.UtcNow;

            var activity = new TicketActivity
            {
                TicketId = ticket.Id,
                UserId = request.UserId,
                Type = ActivityType.AgentAssigned,
                Description = $"Ticket assigned to agent {agent.DisplayName}",
                OldValue = null,
                NewValue = request.AgentId.ToString(),
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.TicketActivityRepository.AddAsync(activity, cancellationToken);

            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("Admin {AdminId} successfully assigned ticket {TicketId} to agent {AgentId}.", request.UserId, request.TicketId, request.AgentId);

            return Unit.Value;
        }
    }
}
