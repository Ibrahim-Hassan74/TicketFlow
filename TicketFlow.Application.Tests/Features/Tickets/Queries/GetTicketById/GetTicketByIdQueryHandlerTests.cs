using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.Tickets.Queries.GetTicketById;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.IdentityEntities;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Tickets.Queries.GetTicketById
{
    public class GetTicketByIdQueryHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITicketsRepository> _ticketsRepositoryMock;
        private readonly GetTicketByIdQueryHandler _handler;

        public GetTicketByIdQueryHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _ticketsRepositoryMock = new Mock<ITicketsRepository>();
            _unitOfWorkMock.Setup(u => u.TicketsRepository).Returns(_ticketsRepositoryMock.Object);

            _handler = new GetTicketByIdQueryHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_WhenTicketDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var query = new GetTicketByIdQuery(ticketId, Guid.NewGuid(), "Admin");

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Ticket, object>>[]>()))
                .ReturnsAsync((Ticket?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Ticket not found.");
        }

        [Fact]
        public async Task Handle_WhenCustomerAccessesOtherCustomerTicket_ShouldThrowNotFoundException_IDOR()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var otherCustomerId = Guid.NewGuid();

            var query = new GetTicketByIdQuery(ticketId, customerId, UserRole.Customer.ToString());

            var ticket = new Ticket
            {
                Id = ticketId,
                Title = "Network Issue",
                Description = "WiFi disconnected",
                CustomerId = otherCustomerId, // Different customer
                Customer = new ApplicationUser { Id = otherCustomerId, DisplayName = "Alice" }
            };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Ticket, object>>[]>()))
                .ReturnsAsync(ticket);

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Ticket not found.");
        }

        [Fact]
        public async Task Handle_WhenSupportAgentAccessesUnassignedTicket_ShouldThrowNotFoundException_IDOR()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var otherAgentId = Guid.NewGuid();

            var query = new GetTicketByIdQuery(ticketId, agentId, UserRole.SupportAgent.ToString());

            var ticket = new Ticket
            {
                Id = ticketId,
                Title = "Server Crash",
                Description = "Prod is down",
                CustomerId = Guid.NewGuid(),
                Customer = new ApplicationUser { DisplayName = "Bob" }
            };
            ticket.AssignAgent(otherAgentId); // Assigned to someone else

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Ticket, object>>[]>()))
                .ReturnsAsync(ticket);

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Ticket not found.");
        }

        [Fact]
        public async Task Handle_WhenCustomerAccessesOwnTicket_ShouldReturnTicketResponse()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var query = new GetTicketByIdQuery(ticketId, customerId, UserRole.Customer.ToString());

            var ticket = new Ticket
            {
                Id = ticketId,
                Title = "Billing Question",
                Description = "Overcharged on invoice",
                CustomerId = customerId,
                Customer = new ApplicationUser { Id = customerId, DisplayName = "Charlie" },
                Priority = TicketPriority.High,
                Status = TicketStatus.Open,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Ticket, object>>[]>()))
                .ReturnsAsync(ticket);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(ticketId);
            result.Title.Should().Be("Billing Question");
            result.CustomerName.Should().Be("Charlie");
            result.Priority.Should().Be(TicketPriority.High.ToString());
            result.Status.Should().Be(TicketStatus.Open.ToString());
        }

        [Fact]
        public async Task Handle_WhenSupportAgentAccessesAssignedTicket_ShouldReturnTicketResponse()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var query = new GetTicketByIdQuery(ticketId, agentId, UserRole.SupportAgent.ToString());

            var agent = new ApplicationUser { Id = agentId, DisplayName = "Agent Smith" };
            var ticket = new Ticket
            {
                Id = ticketId,
                Title = "Hardware Glitch",
                Description = "Screen flickering",
                CustomerId = customerId,
                Customer = new ApplicationUser { Id = customerId, DisplayName = "Dave" },
                AssignedAgent = agent,
                Priority = TicketPriority.Low,
                Status = TicketStatus.InProgress,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            ticket.AssignAgent(agentId);

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Ticket, object>>[]>()))
                .ReturnsAsync(ticket);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(ticketId);
            result.AssignedAgentId.Should().Be(agentId);
            result.AssignedAgentName.Should().Be("Agent Smith");
        }

        [Fact]
        public async Task Handle_WhenAdminAccessesAnyTicket_ShouldReturnTicketResponse()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var query = new GetTicketByIdQuery(ticketId, adminId, UserRole.Admin.ToString());

            var ticket = new Ticket
            {
                Id = ticketId,
                Title = "Database Migration",
                Description = "Upgrade postgres",
                CustomerId = Guid.NewGuid(),
                Customer = new ApplicationUser { DisplayName = "Eve" },
                Priority = TicketPriority.Critical,
                Status = TicketStatus.Open,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _ticketsRepositoryMock
                .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Ticket, object>>[]>()))
                .ReturnsAsync(ticket);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(ticketId);
            result.Title.Should().Be("Database Migration");
            result.CustomerName.Should().Be("Eve");
            result.Priority.Should().Be(TicketPriority.Critical.ToString());
        }
    }
}
