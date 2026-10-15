using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Activities.Queries.GetActivities;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.IdentityEntities;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Activities.Queries.GetActivities
{
    public class GetActivitiesQueryHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITicketActivityRepository> _ticketActivityRepositoryMock;
        private readonly Mock<ITicketAuthorizationService> _ticketAuthMock;
        private readonly Mock<ILogger<GetActivitiesQueryHandler>> _loggerMock;
        private readonly GetActivitiesQueryHandler _handler;

        public GetActivitiesQueryHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _ticketActivityRepositoryMock = new Mock<ITicketActivityRepository>();
            _unitOfWorkMock.Setup(u => u.TicketActivityRepository).Returns(_ticketActivityRepositoryMock.Object);

            _ticketAuthMock = new Mock<ITicketAuthorizationService>();
            _loggerMock = new Mock<ILogger<GetActivitiesQueryHandler>>();

            _handler = new GetActivitiesQueryHandler(_unitOfWorkMock.Object, _loggerMock.Object, _ticketAuthMock.Object);
        }

        [Fact]
        public async Task Handle_WhenUserCannotAccessTicket_ShouldThrowNotFoundException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var query = new GetActivitiesQuery
            {
                TicketId = ticketId,
                UserId = userId,
                Role = "SupportAgent"
            };

            _ticketAuthMock
                .Setup(a => a.CanAccessTicketAsync(ticketId, userId, "SupportAgent", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Ticket not found or you do not have access.");

            _ticketActivityRepositoryMock.Verify(r => r.GetFilteredAsync(
                It.IsAny<Expression<Func<TicketActivity, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<TicketActivity, object>>[]>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenUserHasAccess_ShouldReturnOrderedActivities()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var query = new GetActivitiesQuery
            {
                TicketId = ticketId,
                UserId = userId,
                Role = "Admin"
            };

            _ticketAuthMock
                .Setup(a => a.CanAccessTicketAsync(ticketId, userId, "Admin", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var user = new ApplicationUser { Id = userId, DisplayName = "Admin Bob" };
            var act1 = new TicketActivity
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                UserId = userId,
                User = user,
                Type = ActivityType.Created,
                Description = "Ticket created.",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-3)
            };
            var act2 = new TicketActivity
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                UserId = userId,
                User = user,
                Type = ActivityType.StatusChanged,
                Description = "Status changed from Open to InProgress",
                OldValue = "Open",
                NewValue = "InProgress",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-1)
            };

            _ticketActivityRepositoryMock
                .Setup(r => r.GetFilteredAsync(
                    It.IsAny<Expression<Func<TicketActivity, bool>>>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<Expression<Func<TicketActivity, object>>[]>()))
                .ReturnsAsync(new List<TicketActivity> { act2, act1 }); // Out of order list to test ordering

            // Act
            var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Type.Should().Be(ActivityType.Created.ToString());
            result[0].Description.Should().Be("Ticket created.");
            result[0].UserName.Should().Be("Admin Bob");
            result[1].Type.Should().Be(ActivityType.StatusChanged.ToString());
            result[1].OldValue.Should().Be("Open");
            result[1].NewValue.Should().Be("InProgress");
        }
    }
}
