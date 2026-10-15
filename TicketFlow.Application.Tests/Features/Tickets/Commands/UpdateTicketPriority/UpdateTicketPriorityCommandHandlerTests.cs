using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.Tickets.Commands.UpdateTicketPriority;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Tickets.Commands.UpdateTicketPriority
{
    public class UpdateTicketPriorityCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITicketsRepository> _ticketsRepositoryMock;
        private readonly Mock<ITicketActivityRepository> _ticketActivityRepositoryMock;
        private readonly Mock<ILogger<UpdateTicketPriorityCommandHandler>> _loggerMock;
        private readonly UpdateTicketPriorityCommandHandler _handler;

        public UpdateTicketPriorityCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _ticketsRepositoryMock = new Mock<ITicketsRepository>();
            _ticketActivityRepositoryMock = new Mock<ITicketActivityRepository>();

            _unitOfWorkMock.Setup(u => u.TicketsRepository).Returns(_ticketsRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.TicketActivityRepository).Returns(_ticketActivityRepositoryMock.Object);

            _loggerMock = new Mock<ILogger<UpdateTicketPriorityCommandHandler>>();

            _handler = new UpdateTicketPriorityCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenTicketNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var command = new UpdateTicketPriorityCommand
            {
                TicketId = ticketId,
                UserId = Guid.NewGuid(),
                NewPriority = TicketPriority.High
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
        public async Task Handle_WhenPriorityChanges_ShouldUpdateTicketAndAddActivityAndCommit()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var command = new UpdateTicketPriorityCommand
            {
                TicketId = ticketId,
                UserId = adminId,
                NewPriority = TicketPriority.Critical
            };

            var ticket = new Ticket
            {
                Id = ticketId,
                Priority = TicketPriority.Low,
                Status = TicketStatus.Open,
                UpdatedAt = DateTimeOffset.UtcNow.AddHours(-1)
            };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

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
            ticket.Priority.Should().Be(TicketPriority.Critical);

            capturedActivity.Should().NotBeNull();
            capturedActivity!.TicketId.Should().Be(ticketId);
            capturedActivity.UserId.Should().Be(adminId);
            capturedActivity.Type.Should().Be(ActivityType.PriorityChanged);
            capturedActivity.OldValue.Should().Be(TicketPriority.Low.ToString());
            capturedActivity.NewValue.Should().Be(TicketPriority.Critical.ToString());

            _ticketsRepositoryMock.Verify(t => t.UpdateAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenPriorityIsSame_ShouldUpdateTicketWithoutAddingActivity()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var command = new UpdateTicketPriorityCommand
            {
                TicketId = ticketId,
                UserId = adminId,
                NewPriority = TicketPriority.Medium
            };

            var ticket = new Ticket
            {
                Id = ticketId,
                Priority = TicketPriority.Medium,
                Status = TicketStatus.Open
            };

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
            ticket.Priority.Should().Be(TicketPriority.Medium);

            _ticketActivityRepositoryMock.Verify(a => a.AddAsync(It.IsAny<TicketActivity>(), It.IsAny<CancellationToken>()), Times.Never);
            _ticketsRepositoryMock.Verify(t => t.UpdateAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
