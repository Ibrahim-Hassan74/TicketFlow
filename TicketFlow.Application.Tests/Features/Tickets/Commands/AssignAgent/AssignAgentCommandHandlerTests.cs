using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.Tickets.Commands.AssignAgent;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.Exceptions;
using TicketFlow.Domain.IdentityEntities;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Tickets.Commands.AssignAgent
{
    public class AssignAgentCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITicketsRepository> _ticketsRepositoryMock;
        private readonly Mock<ITicketActivityRepository> _ticketActivityRepositoryMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<ILogger<AssignAgentCommandHandler>> _loggerMock;
        private readonly AssignAgentCommandHandler _handler;

        public AssignAgentCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _ticketsRepositoryMock = new Mock<ITicketsRepository>();
            _ticketActivityRepositoryMock = new Mock<ITicketActivityRepository>();

            _unitOfWorkMock.Setup(u => u.TicketsRepository).Returns(_ticketsRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.TicketActivityRepository).Returns(_ticketActivityRepositoryMock.Object);

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _loggerMock = new Mock<ILogger<AssignAgentCommandHandler>>();

            _handler = new AssignAgentCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _userManagerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenTicketNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var command = new AssignAgentCommand
            {
                TicketId = ticketId,
                AgentId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Ticket?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Ticket not found.");
        }

        [Fact]
        public async Task Handle_WhenAgentNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var command = new AssignAgentCommand
            {
                TicketId = ticketId,
                AgentId = agentId,
                UserId = Guid.NewGuid()
            };

            var ticket = new Ticket { Id = ticketId, Status = TicketStatus.Open };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            _userManagerMock
                .Setup(m => m.FindByIdAsync(agentId.ToString()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Agent not found.");
        }

        [Fact]
        public async Task Handle_WhenUserIsNotSupportAgent_ShouldThrowBadRequestException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var command = new AssignAgentCommand
            {
                TicketId = ticketId,
                AgentId = agentId,
                UserId = Guid.NewGuid()
            };

            var ticket = new Ticket { Id = ticketId, Status = TicketStatus.Open };
            var notAgentUser = new ApplicationUser { Id = agentId, DisplayName = "Regular Customer" };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            _userManagerMock
                .Setup(m => m.FindByIdAsync(agentId.ToString()))
                .ReturnsAsync(notAgentUser);

            _userManagerMock
                .Setup(m => m.IsInRoleAsync(notAgentUser, UserRole.SupportAgent.ToString()))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("The specified user is not a Support Agent.");
        }

        [Fact]
        public async Task Handle_WhenTicketIsClosed_ShouldThrowDomainValidationException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var command = new AssignAgentCommand
            {
                TicketId = ticketId,
                AgentId = agentId,
                UserId = Guid.NewGuid()
            };

            var ticket = new Ticket { Id = ticketId, Status = TicketStatus.Closed };
            var agentUser = new ApplicationUser { Id = agentId, DisplayName = "Agent Fox" };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            _userManagerMock
                .Setup(m => m.FindByIdAsync(agentId.ToString()))
                .ReturnsAsync(agentUser);

            _userManagerMock
                .Setup(m => m.IsInRoleAsync(agentUser, UserRole.SupportAgent.ToString()))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainValidationException>()
                .WithMessage("Cannot assign an agent to a ticket that is already closed.");
        }

        [Fact]
        public async Task Handle_WhenValidSupportAgent_ShouldAssignAgentRecordActivityAndCommit()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var command = new AssignAgentCommand
            {
                TicketId = ticketId,
                AgentId = agentId,
                UserId = adminId
            };

            var ticket = new Ticket
            {
                Id = ticketId,
                Status = TicketStatus.Open,
                UpdatedAt = DateTimeOffset.UtcNow.AddHours(-2)
            };
            var agentUser = new ApplicationUser { Id = agentId, DisplayName = "Special Agent Mulder" };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            _userManagerMock
                .Setup(m => m.FindByIdAsync(agentId.ToString()))
                .ReturnsAsync(agentUser);

            _userManagerMock
                .Setup(m => m.IsInRoleAsync(agentUser, UserRole.SupportAgent.ToString()))
                .ReturnsAsync(true);

            TicketActivity? capturedActivity = null;
            _ticketActivityRepositoryMock
                .Setup(a => a.AddAsync(It.IsAny<TicketActivity>(), It.IsAny<CancellationToken>()))
                .Callback<TicketActivity, CancellationToken>((act, _) => capturedActivity = act)
                .ReturnsAsync((TicketActivity act, CancellationToken _) => act);

            _unitOfWorkMock
                .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            ticket.AssignedAgentId.Should().Be(agentId);

            capturedActivity.Should().NotBeNull();
            capturedActivity!.TicketId.Should().Be(ticketId);
            capturedActivity.UserId.Should().Be(adminId);
            capturedActivity.Type.Should().Be(ActivityType.AgentAssigned);
            capturedActivity.NewValue.Should().Be(agentId.ToString());
            capturedActivity.Description.Should().Be("Ticket assigned to agent Special Agent Mulder");

            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
