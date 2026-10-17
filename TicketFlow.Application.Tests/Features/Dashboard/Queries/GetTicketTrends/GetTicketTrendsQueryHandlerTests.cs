using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Dashboard.Queries.GetTicketTrends;
using TicketFlow.Domain.Enums;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Dashboard.Queries.GetTicketTrends
{
    public class GetTicketTrendsQueryHandlerTests
    {
        private readonly Mock<IDashboardRepository> _dashboardRepositoryMock;
        private readonly Mock<ILogger<GetTicketTrendsQueryHandler>> _loggerMock;
        private readonly GetTicketTrendsQueryHandler _handler;

        public GetTicketTrendsQueryHandlerTests()
        {
            _dashboardRepositoryMock = new Mock<IDashboardRepository>();
            _loggerMock = new Mock<ILogger<GetTicketTrendsQueryHandler>>();
            _handler = new GetTicketTrendsQueryHandler(_dashboardRepositoryMock.Object, _loggerMock.Object);
        }

        [Theory]
        [InlineData("Customer")]
        [InlineData("SupportAgent")]
        public async Task Handle_WhenUserIsNotAdmin_ShouldThrowForbiddenException(string nonAdminRole)
        {
            // Arrange
            var query = new GetTicketTrendsQuery
            {
                Role = nonAdminRole,
                Days = 30
            };

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>()
                .WithMessage("Only administrators can access dashboard metrics.");

            _dashboardRepositoryMock.Verify(r => r.GetTicketTrendsAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenUserIsAdmin_ShouldReturnTicketTrends()
        {
            // Arrange
            var query = new GetTicketTrendsQuery
            {
                Role = nameof(UserRole.Admin),
                Days = 7
            };

            var trends = new List<TicketTrendResponse>
            {
                new TicketTrendResponse
                {
                    Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                    OpenCount = 10,
                    ClosedCount = 8
                }
            };

            _dashboardRepositoryMock
                .Setup(r => r.GetTicketTrendsAsync(7))
                .ReturnsAsync(trends);

            // Act
            var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].OpenCount.Should().Be(10);
            result[0].ClosedCount.Should().Be(8);

            _dashboardRepositoryMock.Verify(r => r.GetTicketTrendsAsync(7), Times.Once);
        }
    }
}
