using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.TimeEntries.Commands.AddTimeEntry;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using Xunit;

namespace TicketFlow.Application.Tests.Features.TimeEntries.Commands.AddTimeEntry
{
    public class AddTimeEntryCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITicketsRepository> _ticketsRepositoryMock;
        private readonly Mock<ITimeEntryRepository> _timeEntryRepositoryMock;
        private readonly Mock<ITicketActivityRepository> _ticketActivityRepositoryMock;
        private readonly Mock<ILogger<AddTimeEntryCommandHandler>> _loggerMock;
        private readonly AddTimeEntryCommandHandler _handler;

        public AddTimeEntryCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _ticketsRepositoryMock = new Mock<ITicketsRepository>();
            _timeEntryRepositoryMock = new Mock<ITimeEntryRepository>();
            _ticketActivityRepositoryMock = new Mock<ITicketActivityRepository>();

            _unitOfWorkMock.Setup(u => u.TicketsRepository).Returns(_ticketsRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.TimeEntryRepository).Returns(_timeEntryRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.TicketActivityRepository).Returns(_ticketActivityRepositoryMock.Object);

            _loggerMock = new Mock<ILogger<AddTimeEntryCommandHandler>>();

            _handler = new AddTimeEntryCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenTicketNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var command = new AddTimeEntryCommand
            {
                TicketId = ticketId,
                AgentId = agentId,
                WorkDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DurationMinutes = 60,
                Description = "Work"
            };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Ticket?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Ticket not found or not assigned to you.");
        }

        [Fact]
        public async Task Handle_WhenAgentNotAssignedToTicket_ShouldThrowNotFoundException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var otherAgentId = Guid.NewGuid();
            var command = new AddTimeEntryCommand
            {
                TicketId = ticketId,
                AgentId = agentId,
                WorkDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DurationMinutes = 60,
                Description = "Work"
            };

            var ticket = new Ticket { Id = ticketId };
            ticket.AssignAgent(otherAgentId);

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
        public async Task Handle_WhenWorkDateIsInTheFuture_ShouldThrowBadRequestException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
            var command = new AddTimeEntryCommand
            {
                TicketId = ticketId,
                AgentId = agentId,
                WorkDate = futureDate,
                DurationMinutes = 60,
                Description = "Work"
            };

            var ticket = new Ticket { Id = ticketId };
            ticket.AssignAgent(agentId);

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("Work date cannot be in the future.");
        }

        [Fact]
        public async Task Handle_WhenValid_ShouldAddTimeEntryAndActivityAndCommit()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var workDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var command = new AddTimeEntryCommand
            {
                TicketId = ticketId,
                AgentId = agentId,
                WorkDate = workDate,
                DurationMinutes = 120,
                Description = "Refactored CQRS query handler"
            };

            var ticket = new Ticket { Id = ticketId };
            ticket.AssignAgent(agentId);

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            TimeEntry? capturedEntry = null;
            _timeEntryRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<TimeEntry>(), It.IsAny<CancellationToken>()))
                .Callback<TimeEntry, CancellationToken>((entry, _) => capturedEntry = entry)
                .ReturnsAsync((TimeEntry entry, CancellationToken _) => entry);

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

            capturedEntry.Should().NotBeNull();
            capturedEntry!.TicketId.Should().Be(ticketId);
            capturedEntry.AgentId.Should().Be(agentId);
            capturedEntry.DurationMinutes.Should().Be(120);
            capturedEntry.Description.Should().Be("Refactored CQRS query handler");
            capturedEntry.WorkDate.Should().Be(workDate);

            capturedActivity.Should().NotBeNull();
            capturedActivity!.TicketId.Should().Be(ticketId);
            capturedActivity.UserId.Should().Be(agentId);
            capturedActivity.Type.Should().Be(ActivityType.TimeLogged);
            capturedActivity.Description.Should().Be("Logged 120 minutes of work.");

            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
