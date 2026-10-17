using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Dashboard.Queries.GetStats;
using TicketFlow.Domain.Enums;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Dashboard.Queries.GetStats
{
    public class GetStatsQueryHandlerTests
    {
        private readonly Mock<IDashboardRepository> _dashboardRepositoryMock;
        private readonly Mock<ILogger<GetStatsQueryHandler>> _loggerMock;
        private readonly GetStatsQueryHandler _handler;

        public GetStatsQueryHandlerTests()
        {
            _dashboardRepositoryMock = new Mock<IDashboardRepository>();
            _loggerMock = new Mock<ILogger<GetStatsQueryHandler>>();
            _handler = new GetStatsQueryHandler(_dashboardRepositoryMock.Object, _loggerMock.Object);
        }

        [Theory]
        [InlineData("Customer")]
        [InlineData("SupportAgent")]
        [InlineData("Manager")]
        public async Task Handle_WhenUserIsNotAdmin_ShouldThrowForbiddenException(string nonAdminRole)
        {
            // Arrange
            var query = new GetStatsQuery { Role = nonAdminRole };

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>()
                .WithMessage("Only administrators can access dashboard metrics.");

            _dashboardRepositoryMock.Verify(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenUserIsAdmin_ShouldReturnDashboardStats()
        {
            // Arrange
            var query = new GetStatsQuery { Role = nameof(UserRole.Admin) };

            var expectedStats = new DashboardStatsResponse
            {
                TotalTickets = 100,
                OpenTickets = 25,
                InProgressTickets = 35,
                ResolvedTickets = 30,
                ClosedTickets = 10
            };

            _dashboardRepositoryMock
                .Setup(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedStats);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalTickets.Should().Be(100);
            result.OpenTickets.Should().Be(25);
            result.InProgressTickets.Should().Be(35);
            result.ResolvedTickets.Should().Be(30);
            result.ClosedTickets.Should().Be(10);

            _dashboardRepositoryMock.Verify(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
