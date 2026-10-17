using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TicketFlow.API.Controllers;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Activities.DTOs;
using TicketFlow.Application.Features.Activities.Queries.GetActivities;
using Xunit;

namespace TicketFlow.API.Tests.Controllers
{
    public class TicketActivitiesControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly TicketActivitiesController _controller;

        public TicketActivitiesControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _controller = new TicketActivitiesController(_mediatorMock.Object, _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task GetActivities_ShouldReturnOk_WithListOfActivities()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
            _currentUserServiceMock.Setup(s => s.Role).Returns("Admin");

            var activities = new List<ActivityResponse>
            {
                new ActivityResponse
                {
                    Id = Guid.NewGuid(),
                    TicketId = ticketId,
                    UserId = userId,
                    UserName = "AdminUser",
                    Type = "StatusChanged",
                    Description = "Status changed from Open to InProgress",
                    OldValue = "Open",
                    NewValue = "InProgress",
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetActivitiesQuery>(q =>
                    q.TicketId == ticketId &&
                    q.UserId == userId &&
                    q.Role == "Admin"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(activities);

            // Act
            var result = await _controller.GetActivities(ticketId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedData = okResult.Value.Should().BeAssignableTo<IEnumerable<ActivityResponse>>().Subject;
            returnedData.Should().HaveCount(1);
            returnedData.First().Type.Should().Be("StatusChanged");
        }
    }
}
