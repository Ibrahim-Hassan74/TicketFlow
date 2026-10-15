using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using TicketFlow.Application.Features.Tickets.DTOs;
using TicketFlow.Application.Features.Tickets.Queries.GetTickets;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.IdentityEntities;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Tickets.Queries.GetTickets
{
    public class GetTicketsQueryHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITicketsRepository> _ticketsRepositoryMock;
        private readonly GetTicketsQueryHandler _handler;

        public GetTicketsQueryHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _ticketsRepositoryMock = new Mock<ITicketsRepository>();
            _unitOfWorkMock.Setup(u => u.TicketsRepository).Returns(_ticketsRepositoryMock.Object);

            _handler = new GetTicketsQueryHandler(_unitOfWorkMock.Object);
        }

        private static Ticket CreateFakeTicket(string title, TicketStatus status = TicketStatus.Open, TicketPriority priority = TicketPriority.Medium)
        {
            return new Ticket
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = "Test ticket description",
                Status = status,
                Priority = priority,
                CustomerId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Customer = new ApplicationUser { DisplayName = "John Doe" }
            };
        }

        [Fact]
        public async Task Handle_WhenAdminRequestsTickets_ShouldReturnPaginatedTickets()
        {
            // Arrange
            var query = new GetTicketsQuery
            {
                Page = 1,
                PageSize = 10,
                UserId = Guid.NewGuid(),
                Role = nameof(UserRole.Admin)
            };

            var fakeTickets = new List<Ticket>
            {
                CreateFakeTicket("Ticket 1"),
                CreateFakeTicket("Ticket 2")
            };

            _ticketsRepositoryMock.Setup(r => r.GetFilteredTicketsAsync(
                It.IsAny<Expression<Func<Ticket, bool>>?>(),
                It.IsAny<Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>>>(),
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Ticket, object>>>(),
                It.IsAny<Expression<Func<Ticket, object>>>()))
                .ReturnsAsync(fakeTickets);

            _ticketsRepositoryMock.Setup(r => r.CountFilteredTicketsAsync(
                It.IsAny<Expression<Func<Ticket, bool>>?>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalCount.Should().Be(2);
            result.Items.Should().HaveCount(2);
            result.Items.First().Title.Should().Be("Ticket 1");
            result.Items.First().CustomerName.Should().Be("John Doe");

            _ticketsRepositoryMock.Verify(r => r.GetFilteredTicketsAsync(
                It.IsAny<Expression<Func<Ticket, bool>>?>(),
                It.IsAny<Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>>>(),
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Ticket, object>>>(),
                It.IsAny<Expression<Func<Ticket, object>>>()), Times.Once);

            _ticketsRepositoryMock.Verify(r => r.CountFilteredTicketsAsync(
                It.IsAny<Expression<Func<Ticket, bool>>?>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenCustomerRequestsTickets_ShouldScopeToCustomer()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var query = new GetTicketsQuery
            {
                Page = 1,
                PageSize = 5,
                UserId = customerId,
                Role = nameof(UserRole.Customer)
            };

            var fakeTicket = CreateFakeTicket("My Customer Ticket");
            fakeTicket.CustomerId = customerId;

            _ticketsRepositoryMock.Setup(r => r.GetFilteredTicketsAsync(
                It.IsAny<Expression<Func<Ticket, bool>>?>(),
                It.IsAny<Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>>>(),
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Ticket, object>>>(),
                It.IsAny<Expression<Func<Ticket, object>>>()))
                .ReturnsAsync(new List<Ticket> { fakeTicket });

            _ticketsRepositoryMock.Setup(r => r.CountFilteredTicketsAsync(
                It.IsAny<Expression<Func<Ticket, bool>>?>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Items.First().Title.Should().Be("My Customer Ticket");
        }

        [Fact]
        public async Task Handle_WhenSupportAgentRequestsTickets_ShouldScopeToAssignedAgent()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var query = new GetTicketsQuery
            {
                Page = 1,
                PageSize = 10,
                UserId = agentId,
                Role = nameof(UserRole.SupportAgent)
            };

            _ticketsRepositoryMock.Setup(r => r.GetFilteredTicketsAsync(
                It.IsAny<Expression<Func<Ticket, bool>>?>(),
                It.IsAny<Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>>>(),
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Ticket, object>>>(),
                It.IsAny<Expression<Func<Ticket, object>>>()))
                .ReturnsAsync(new List<Ticket>());

            _ticketsRepositoryMock.Setup(r => r.CountFilteredTicketsAsync(
                It.IsAny<Expression<Func<Ticket, bool>>?>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WhenSearchAndStatusFiltersProvided_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = new GetTicketsQuery
            {
                Page = 1,
                PageSize = 10,
                UserId = Guid.NewGuid(),
                Role = nameof(UserRole.Admin),
                Status = TicketStatus.InProgress,
                Priority = TicketPriority.High,
                Search = "Database"
            };

            var matchingTicket = CreateFakeTicket("Database connection failure", TicketStatus.InProgress, TicketPriority.High);

            _ticketsRepositoryMock.Setup(r => r.GetFilteredTicketsAsync(
                It.IsAny<Expression<Func<Ticket, bool>>?>(),
                It.IsAny<Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>>>(),
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Ticket, object>>>(),
                It.IsAny<Expression<Func<Ticket, object>>>()))
                .ReturnsAsync(new List<Ticket> { matchingTicket });

            _ticketsRepositoryMock.Setup(r => r.CountFilteredTicketsAsync(
                It.IsAny<Expression<Func<Ticket, bool>>?>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().Title.Should().Be("Database connection failure");
            result.Items.First().Status.Should().Be(TicketStatus.InProgress.ToString());
            result.Items.First().Priority.Should().Be(TicketPriority.High.ToString());
        }
    }
}
