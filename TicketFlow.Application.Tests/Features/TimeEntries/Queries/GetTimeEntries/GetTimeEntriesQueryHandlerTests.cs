using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.TimeEntries.Queries.GetTimeEntries;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.IdentityEntities;
using Xunit;

namespace TicketFlow.Application.Tests.Features.TimeEntries.Queries.GetTimeEntries
{
    public class GetTimeEntriesQueryHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITicketsRepository> _ticketsRepositoryMock;
        private readonly Mock<ITimeEntryRepository> _timeEntryRepositoryMock;
        private readonly Mock<ILogger<GetTimeEntriesQueryHandler>> _loggerMock;
        private readonly GetTimeEntriesQueryHandler _handler;

        public GetTimeEntriesQueryHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _ticketsRepositoryMock = new Mock<ITicketsRepository>();
            _timeEntryRepositoryMock = new Mock<ITimeEntryRepository>();

            _unitOfWorkMock.Setup(u => u.TicketsRepository).Returns(_ticketsRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.TimeEntryRepository).Returns(_timeEntryRepositoryMock.Object);

            _loggerMock = new Mock<ILogger<GetTimeEntriesQueryHandler>>();

            _handler = new GetTimeEntriesQueryHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenTicketNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var query = new GetTimeEntriesQuery
            {
                TicketId = ticketId,
                UserId = Guid.NewGuid(),
                Role = "Admin"
            };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Ticket?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Ticket not found.");
        }

        [Fact]
        public async Task Handle_WhenUserIsCustomer_ShouldThrowNotFoundException_IDOR()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var query = new GetTimeEntriesQuery
            {
                TicketId = ticketId,
                UserId = customerId,
                Role = UserRole.Customer.ToString()
            };

            var ticket = new Ticket { Id = ticketId, CustomerId = customerId };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Ticket not found.");
        }

        [Fact]
        public async Task Handle_WhenSupportAgentNotAssignedToTicket_ShouldThrowNotFoundException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var otherAgentId = Guid.NewGuid();
            var query = new GetTimeEntriesQuery
            {
                TicketId = ticketId,
                UserId = agentId,
                Role = UserRole.SupportAgent.ToString()
            };

            var ticket = new Ticket { Id = ticketId };
            ticket.AssignAgent(otherAgentId);

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Ticket not found or not assigned to you.");
        }

        [Fact]
        public async Task Handle_WhenAssignedAgentRequests_ShouldReturnOrderedEntriesAndTotalDuration()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var query = new GetTimeEntriesQuery
            {
                TicketId = ticketId,
                UserId = agentId,
                Role = UserRole.SupportAgent.ToString()
            };

            var ticket = new Ticket { Id = ticketId };
            ticket.AssignAgent(agentId);

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            var agentUser = new ApplicationUser { Id = agentId, DisplayName = "Agent Fox" };
            var entry1 = new TimeEntry
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                AgentId = agentId,
                Agent = agentUser,
                WorkDate = new DateOnly(2026, 1, 10),
                DurationMinutes = 60,
                Description = "Investigation",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-5)
            };
            var entry2 = new TimeEntry
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                AgentId = agentId,
                Agent = agentUser,
                WorkDate = new DateOnly(2026, 1, 11),
                DurationMinutes = 90,
                Description = "Fix applied",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-1)
            };

            _timeEntryRepositoryMock
                .Setup(r => r.GetFilteredAsync(It.IsAny<Expression<Func<TimeEntry, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<TimeEntry, object>>[]>()))
                .ReturnsAsync(new List<TimeEntry> { entry2, entry1 }); // Out of order list to test ordering

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalDurationMinutes.Should().Be(150);
            result.Entries.Should().HaveCount(2);
            result.Entries[0].Description.Should().Be("Investigation");
            result.Entries[0].AgentName.Should().Be("Agent Fox");
            result.Entries[1].Description.Should().Be("Fix applied");
        }

        [Fact]
        public async Task Handle_WhenAdminRequests_ShouldReturnEntriesEvenIfNotAssigned()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var query = new GetTimeEntriesQuery
            {
                TicketId = ticketId,
                UserId = adminId,
                Role = UserRole.Admin.ToString()
            };

            var ticket = new Ticket { Id = ticketId };
            ticket.AssignAgent(agentId);

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            _timeEntryRepositoryMock
                .Setup(r => r.GetFilteredAsync(It.IsAny<Expression<Func<TimeEntry, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<TimeEntry, object>>[]>()))
                .ReturnsAsync(new List<TimeEntry>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalDurationMinutes.Should().Be(0);
            result.Entries.Should().BeEmpty();
        }
    }
}
