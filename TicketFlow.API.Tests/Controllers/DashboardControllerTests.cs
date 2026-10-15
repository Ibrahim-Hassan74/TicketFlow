using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TicketFlow.API.Controllers;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Dashboard.Queries.GetAgentWorkload;
using TicketFlow.Application.Features.Dashboard.Queries.GetStats;
using TicketFlow.Application.Features.Dashboard.Queries.GetTicketTrends;
using Xunit;

namespace TicketFlow.API.Tests.Controllers
{
    public class DashboardControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly DashboardController _controller;

        public DashboardControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _controller = new DashboardController(_mediatorMock.Object, _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task GetStats_ShouldReturnOk_WithStatsResponse()
        {
            // Arrange
            _currentUserServiceMock.Setup(s => s.Role).Returns("Admin");

            var statsResponse = new DashboardStatsResponse
            {
                TotalTickets = 50,
                OpenTickets = 15,
                InProgressTickets = 20,
                ResolvedTickets = 10,
                ClosedTickets = 5
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetStatsQuery>(q => q.Role == "Admin"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(statsResponse);

            // Act
            var result = await _controller.GetStats();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedData = okResult.Value.Should().BeOfType<DashboardStatsResponse>().Subject;
            returnedData.TotalTickets.Should().Be(50);
        }

        [Fact]
        public async Task GetAgentWorkload_ShouldReturnOk_WithWorkloadList()
        {
            // Arrange
            _currentUserServiceMock.Setup(s => s.Role).Returns("Admin");

            var workload = new List<AgentWorkloadResponse>
            {
                new AgentWorkloadResponse
                {
                    AgentId = Guid.NewGuid(),
                    AgentName = "Agent Smith",
                    OpenTickets = 4,
                    InProgressTickets = 2,
                    TotalTimeMinutes = 180
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetAgentWorkloadQuery>(q => q.Role == "Admin"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(workload);

            // Act
            var result = await _controller.GetAgentWorkload();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedData = okResult.Value.Should().BeAssignableTo<IEnumerable<AgentWorkloadResponse>>().Subject;
            returnedData.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetTicketTrends_ShouldReturnOk_WithTrendsList()
        {
            // Arrange
            _currentUserServiceMock.Setup(s => s.Role).Returns("Admin");

            var trends = new List<TicketTrendResponse>
            {
                new TicketTrendResponse
                {
                    Date = DateOnly.FromDateTime(DateTime.UtcNow),
                    OpenCount = 5,
                    ClosedCount = 3
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetTicketTrendsQuery>(q => q.Role == "Admin" && q.Days == 14), It.IsAny<CancellationToken>()))
                .ReturnsAsync(trends);

            // Act
            var result = await _controller.GetTicketTrends(14);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedData = okResult.Value.Should().BeAssignableTo<IEnumerable<TicketTrendResponse>>().Subject;
            returnedData.Should().HaveCount(1);
        }
    }
}
