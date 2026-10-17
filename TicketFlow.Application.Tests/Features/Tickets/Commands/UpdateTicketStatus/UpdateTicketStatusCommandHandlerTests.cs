using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.Tickets.Commands.UpdateTicketStatus;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Tickets.Commands.UpdateTicketStatus
{
    public class UpdateTicketStatusCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITicketsRepository> _ticketsRepositoryMock;
        private readonly Mock<ITicketActivityRepository> _ticketActivityRepositoryMock;
        private readonly Mock<ILogger<UpdateTicketStatusCommandHandler>> _loggerMock;
        private readonly UpdateTicketStatusCommandHandler _handler;

        public UpdateTicketStatusCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _ticketsRepositoryMock = new Mock<ITicketsRepository>();
            _ticketActivityRepositoryMock = new Mock<ITicketActivityRepository>();

            _unitOfWorkMock.Setup(u => u.TicketsRepository).Returns(_ticketsRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.TicketActivityRepository).Returns(_ticketActivityRepositoryMock.Object);

            _loggerMock = new Mock<ILogger<UpdateTicketStatusCommandHandler>>();

            _handler = new UpdateTicketStatusCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenTicketNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var command = new UpdateTicketStatusCommand
            {
                TicketId = ticketId,
                UserId = Guid.NewGuid(),
                Role = "Admin",
                NewStatus = TicketStatus.InProgress
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
        public async Task Handle_WhenSupportAgentIsNotAssignedToTicket_ShouldThrowNotFoundException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var command = new UpdateTicketStatusCommand
            {
                TicketId = ticketId,
                UserId = agentId,
                Role = UserRole.SupportAgent.ToString(),
                NewStatus = TicketStatus.InProgress
            };

            var ticket = new Ticket
            {
                Id = ticketId,
                Status = TicketStatus.Open
            };
            ticket.AssignAgent(Guid.NewGuid()); // Assigned to different agent

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Ticket not found or not assigned to you.");
        }

        [Fact]
        public async Task Handle_WhenCustomerDoesNotOwnTicket_ShouldThrowNotFoundException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var command = new UpdateTicketStatusCommand
            {
                TicketId = ticketId,
                UserId = customerId,
                Role = UserRole.Customer.ToString(),
                NewStatus = TicketStatus.Closed
            };

            var ticket = new Ticket
            {
                Id = ticketId,
                CustomerId = Guid.NewGuid(), // Different customer
                Status = TicketStatus.Resolved
            };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Ticket not found or does not belong to you.");
        }

        [Theory]
        [InlineData(TicketStatus.InProgress)]
        [InlineData(TicketStatus.Resolved)]
        public async Task Handle_WhenCustomerAttemptsToSetInProgressOrResolved_ShouldThrowBadRequestException(TicketStatus invalidStatus)
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var command = new UpdateTicketStatusCommand
            {
                TicketId = ticketId,
                UserId = customerId,
                Role = UserRole.Customer.ToString(),
                NewStatus = invalidStatus
            };

            var ticket = new Ticket
            {
                Id = ticketId,
                CustomerId = customerId,
                Status = TicketStatus.Open
            };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("Customers cannot set status to InProgress or Resolved.");
        }

        [Fact]
        public async Task Handle_WhenSupportAgentAttemptsToCloseTicket_ShouldThrowBadRequestException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var command = new UpdateTicketStatusCommand
            {
                TicketId = ticketId,
                UserId = agentId,
                Role = UserRole.SupportAgent.ToString(),
                NewStatus = TicketStatus.Closed
            };

            var ticket = new Ticket
            {
                Id = ticketId,
                Status = TicketStatus.Resolved
            };
            ticket.AssignAgent(agentId);

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("Support agents are not authorized to close tickets. Only customers or admins can close.");
        }

        [Fact]
        public async Task Handle_WhenTransitioningFromOpenDirectlyToResolved_ShouldThrowBadRequestException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var command = new UpdateTicketStatusCommand
            {
                TicketId = ticketId,
                UserId = adminId,
                Role = UserRole.Admin.ToString(),
                NewStatus = TicketStatus.Resolved
            };

            var ticket = new Ticket
            {
                Id = ticketId,
                Status = TicketStatus.Open
            };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("Cannot resolve an open ticket. Must be In Progress first.");
        }

        [Fact]
        public async Task Handle_WhenTransitioningFromClosedToAnythingOtherThanOpen_ShouldThrowBadRequestException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var command = new UpdateTicketStatusCommand
            {
                TicketId = ticketId,
                UserId = adminId,
                Role = UserRole.Admin.ToString(),
                NewStatus = TicketStatus.InProgress
            };

            var ticket = new Ticket
            {
                Id = ticketId,
                Status = TicketStatus.Closed
            };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("A closed ticket can only be reopened (set to Open).");
        }

        [Fact]
        public async Task Handle_WhenStatusIsUnchanged_ShouldReturnUnitValueWithoutModifyingDatabase()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var command = new UpdateTicketStatusCommand
            {
                TicketId = ticketId,
                UserId = adminId,
                Role = UserRole.Admin.ToString(),
                NewStatus = TicketStatus.Open
            };

            var ticket = new Ticket
            {
                Id = ticketId,
                Status = TicketStatus.Open
            };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _ticketActivityRepositoryMock.Verify(a => a.AddAsync(It.IsAny<TicketActivity>(), It.IsAny<CancellationToken>()), Times.Never);
            _ticketsRepositoryMock.Verify(t => t.UpdateAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenSupportAgentTransitionsFromOpenToInProgress_ShouldUpdateStatusAndLogActivity()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var command = new UpdateTicketStatusCommand
            {
                TicketId = ticketId,
                UserId = agentId,
                Role = UserRole.SupportAgent.ToString(),
                NewStatus = TicketStatus.InProgress
            };

            var ticket = new Ticket
            {
                Id = ticketId,
                Status = TicketStatus.Open
            };
            ticket.AssignAgent(agentId);

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            TicketActivity? recordedActivity = null;
            _ticketActivityRepositoryMock
                .Setup(a => a.AddAsync(It.IsAny<TicketActivity>(), It.IsAny<CancellationToken>()))
                .Callback<TicketActivity, CancellationToken>((act, _) => recordedActivity = act)
                .ReturnsAsync((TicketActivity act, CancellationToken _) => act);

            _unitOfWorkMock
                .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            ticket.Status.Should().Be(TicketStatus.InProgress);

            recordedActivity.Should().NotBeNull();
            recordedActivity!.TicketId.Should().Be(ticketId);
            recordedActivity.UserId.Should().Be(agentId);
            recordedActivity.Type.Should().Be(ActivityType.StatusChanged);
            recordedActivity.OldValue.Should().Be(TicketStatus.Open.ToString());
            recordedActivity.NewValue.Should().Be(TicketStatus.InProgress.ToString());

            _ticketsRepositoryMock.Verify(t => t.UpdateAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenTransitioningToResolved_ShouldSetResolvedAtTimestamp()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var command = new UpdateTicketStatusCommand
            {
                TicketId = ticketId,
                UserId = agentId,
                Role = UserRole.SupportAgent.ToString(),
                NewStatus = TicketStatus.Resolved
            };

            var ticket = new Ticket
            {
                Id = ticketId,
                Status = TicketStatus.InProgress
            };
            ticket.AssignAgent(agentId);

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            _unitOfWorkMock
                .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            ticket.Status.Should().Be(TicketStatus.Resolved);
            ticket.ResolvedAt.Should().NotBeNull();
            ticket.ResolvedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenCustomerClosesResolvedTicket_ShouldSetClosedAtAndLogClosedActivity()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var command = new UpdateTicketStatusCommand
            {
                TicketId = ticketId,
                UserId = customerId,
                Role = UserRole.Customer.ToString(),
                NewStatus = TicketStatus.Closed
            };

            var ticket = new Ticket
            {
                Id = ticketId,
                CustomerId = customerId,
                Status = TicketStatus.Resolved
            };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            TicketActivity? recordedActivity = null;
            _ticketActivityRepositoryMock
                .Setup(a => a.AddAsync(It.IsAny<TicketActivity>(), It.IsAny<CancellationToken>()))
                .Callback<TicketActivity, CancellationToken>((act, _) => recordedActivity = act)
                .ReturnsAsync((TicketActivity act, CancellationToken _) => act);

            _unitOfWorkMock
                .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            ticket.Status.Should().Be(TicketStatus.Closed);
            ticket.ClosedAt.Should().NotBeNull();
            ticket.ClosedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

            recordedActivity.Should().NotBeNull();
            recordedActivity!.Type.Should().Be(ActivityType.Closed);

            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
