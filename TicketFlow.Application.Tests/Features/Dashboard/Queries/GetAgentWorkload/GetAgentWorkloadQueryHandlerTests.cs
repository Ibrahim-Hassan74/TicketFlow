using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Dashboard.Queries.GetAgentWorkload;
using TicketFlow.Domain.Enums;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Dashboard.Queries.GetAgentWorkload
{
    public class GetAgentWorkloadQueryHandlerTests
    {
        private readonly Mock<IDashboardRepository> _dashboardRepositoryMock;
        private readonly Mock<ILogger<GetAgentWorkloadQueryHandler>> _loggerMock;
        private readonly GetAgentWorkloadQueryHandler _handler;

        public GetAgentWorkloadQueryHandlerTests()
        {
            _dashboardRepositoryMock = new Mock<IDashboardRepository>();
            _loggerMock = new Mock<ILogger<GetAgentWorkloadQueryHandler>>();
            _handler = new GetAgentWorkloadQueryHandler(_dashboardRepositoryMock.Object, _loggerMock.Object);
        }

        [Theory]
        [InlineData("Customer")]
        [InlineData("SupportAgent")]
        public async Task Handle_WhenUserIsNotAdmin_ShouldThrowForbiddenException(string nonAdminRole)
        {
            // Arrange
            var query = new GetAgentWorkloadQuery { Role = nonAdminRole };

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>()
                .WithMessage("Only administrators can access dashboard metrics.");

            _dashboardRepositoryMock.Verify(r => r.GetAgentWorkloadAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenUserIsAdmin_ShouldReturnAgentWorkload()
        {
            // Arrange
            var query = new GetAgentWorkloadQuery { Role = nameof(UserRole.Admin) };

            var workload = new List<AgentWorkloadResponse>
            {
                new AgentWorkloadResponse
                {
                    AgentId = Guid.NewGuid(),
                    AgentName = "Agent Fox",
                    OpenTickets = 5,
                    InProgressTickets = 3,
                    TotalTimeMinutes = 120
                }
            };

            _dashboardRepositoryMock
                .Setup(r => r.GetAgentWorkloadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(workload);

            // Act
            var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].AgentName.Should().Be("Agent Fox");
            result[0].OpenTickets.Should().Be(5);
            result[0].InProgressTickets.Should().Be(3);
            result[0].TotalTimeMinutes.Should().Be(120);

            _dashboardRepositoryMock.Verify(r => r.GetAgentWorkloadAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
