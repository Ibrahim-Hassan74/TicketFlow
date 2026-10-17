using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TicketFlow.API.Controllers;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.TimeEntries.Commands.AddTimeEntry;
using TicketFlow.Application.Features.TimeEntries.DTOs;
using TicketFlow.Application.Features.TimeEntries.Queries.GetTimeEntries;
using Xunit;

namespace TicketFlow.API.Tests.Controllers
{
    public class TicketTimeEntriesControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly TicketTimeEntriesController _controller;

        public TicketTimeEntriesControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _controller = new TicketTimeEntriesController(_mediatorMock.Object, _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task GetTimeEntries_ShouldReturnOk_WithTimeEntriesResponse()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            _currentUserServiceMock.Setup(s => s.UserId).Returns(agentId);
            _currentUserServiceMock.Setup(s => s.Role).Returns("SupportAgent");

            var response = new TicketTimeEntriesResponse
            {
                TotalDurationMinutes = 120,
                Entries = new List<TimeEntryResponse>
                {
                    new TimeEntryResponse
                    {
                        Id = Guid.NewGuid(),
                        TicketId = ticketId,
                        DurationMinutes = 120,
                        Description = "Debugging code"
                    }
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetTimeEntriesQuery>(q =>
                    q.TicketId == ticketId &&
                    q.UserId == agentId &&
                    q.Role == "SupportAgent"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetTimeEntries(ticketId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedData = okResult.Value.Should().BeOfType<TicketTimeEntriesResponse>().Subject;
            returnedData.TotalDurationMinutes.Should().Be(120);
            returnedData.Entries.Should().HaveCount(1);
        }

        [Fact]
        public async Task AddTimeEntry_ShouldReturnNoContent()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            _currentUserServiceMock.Setup(s => s.UserId).Returns(agentId);

            var request = new CreateTimeEntryRequest
            {
                WorkDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DurationMinutes = 45,
                Description = "Investigated network traffic"
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<AddTimeEntryCommand>(c =>
                    c.TicketId == ticketId &&
                    c.AgentId == agentId &&
                    c.DurationMinutes == 45 &&
                    c.Description == "Investigated network traffic"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await _controller.AddTimeEntry(ticketId, request);

            // Assert
            var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
            noContentResult.StatusCode.Should().Be(204);
        }
    }
}
