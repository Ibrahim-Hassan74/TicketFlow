using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TicketFlow.API.Controllers;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Tickets.Commands.AssignAgent;
using TicketFlow.Application.Features.Tickets.Commands.CreateTicket;
using TicketFlow.Application.Features.Tickets.Commands.UpdateTicket;
using TicketFlow.Application.Features.Tickets.Commands.UpdateTicketPriority;
using TicketFlow.Application.Features.Tickets.Commands.UpdateTicketStatus;
using TicketFlow.Application.Features.Tickets.DTOs;
using TicketFlow.Application.Features.Tickets.Queries.GetTicketById;
using TicketFlow.Application.Features.Tickets.Queries.GetTickets;
using TicketFlow.Domain.Enums;
using Xunit;

namespace TicketFlow.API.Tests.Controllers
{
    public class TicketsControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly TicketsController _controller;

        public TicketsControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _controller = new TicketsController(_mediatorMock.Object, _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task GetTickets_ShouldReturnOk_WithPaginatedTickets()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);
            _currentUserServiceMock.Setup(s => s.Role).Returns("Customer");

            var request = new TicketQueryRequest
            {
                Page = 1,
                PageSize = 10,
                Search = "Printer"
            };

            var ticketResponses = new List<TicketResponse>
            {
                new TicketResponse
                {
                    Id = Guid.NewGuid(),
                    Title = "Printer Issue",
                    Description = "Paper jam",
                    Status = TicketStatus.Open.ToString(),
                    Priority = TicketPriority.Medium.ToString()
                }
            };
            var paginatedResponse = new PaginatedResponse<TicketResponse>
            {
                Items = ticketResponses,
                Page = 1,
                PageSize = 10,
                TotalCount = 1
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetTicketsQuery>(q =>
                    q.Page == 1 &&
                    q.PageSize == 10 &&
                    q.Search == "Printer" &&
                    q.UserId == currentUserId &&
                    q.Role == "Customer"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(paginatedResponse);

            // Act
            var result = await _controller.GetTickets(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedData = okResult.Value.Should().BeOfType<PaginatedResponse<TicketResponse>>().Subject;
            returnedData.Items.Should().HaveCount(1);
            returnedData.Items.First().Title.Should().Be("Printer Issue");
        }

        [Fact]
        public async Task GetTicketById_ShouldReturnOk_WithTicketResponse()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();
            _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);
            _currentUserServiceMock.Setup(s => s.Role).Returns("Admin");

            var response = new TicketResponse
            {
                Id = ticketId,
                Title = "Network Issue",
                Description = "VPN down",
                Status = TicketStatus.InProgress.ToString(),
                Priority = TicketPriority.High.ToString()
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetTicketByIdQuery>(q =>
                    q.Id == ticketId &&
                    q.UserId == currentUserId &&
                    q.Role == "Admin"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetTicketById(ticketId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedData = okResult.Value.Should().BeOfType<TicketResponse>().Subject;
            returnedData.Id.Should().Be(ticketId);
            returnedData.Title.Should().Be("Network Issue");
        }

        [Fact]
        public async Task CreateTicket_ShouldReturnCreatedAtAction_WithCreatedTicket()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            var createdTicketId = Guid.NewGuid();
            _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

            var request = new CreateTicketRequest
            {
                Title = "Software Crash",
                Description = "App crashes on launch",
                Priority = TicketPriority.High
            };

            var createdResponse = new TicketResponse
            {
                Id = createdTicketId,
                Title = "Software Crash",
                Description = "App crashes on launch",
                CustomerId = currentUserId,
                Status = TicketStatus.Open.ToString(),
                Priority = TicketPriority.High.ToString()
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<CreateTicketCommand>(c =>
                    c.CustomerId == currentUserId &&
                    c.Title == "Software Crash" &&
                    c.Description == "App crashes on launch" &&
                    c.Priority == TicketPriority.High), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdResponse);

            // Act
            var result = await _controller.CreateTicket(request);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.StatusCode.Should().Be(201);
            createdResult.ActionName.Should().Be(nameof(TicketsController.GetTicketById));
            createdResult.RouteValues!["id"].Should().Be(createdTicketId);
            var returnedData = createdResult.Value.Should().BeOfType<TicketResponse>().Subject;
            returnedData.Id.Should().Be(createdTicketId);
        }

        [Fact]
        public async Task UpdateTicket_ShouldReturnNoContent()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();
            _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

            var request = new UpdateTicketRequest
            {
                Title = "Updated Title",
                Description = "Updated Description"
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<UpdateTicketCommand>(c =>
                    c.Id == ticketId &&
                    c.UserId == currentUserId &&
                    c.Title == "Updated Title" &&
                    c.Description == "Updated Description"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await _controller.UpdateTicket(ticketId, request);

            // Assert
            var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
            noContentResult.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task UpdateTicketStatus_ShouldReturnNoContent()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();
            _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);
            _currentUserServiceMock.Setup(s => s.Role).Returns("SupportAgent");

            var request = new UpdateTicketStatusRequest
            {
                Status = TicketStatus.InProgress
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<UpdateTicketStatusCommand>(c =>
                    c.TicketId == ticketId &&
                    c.NewStatus == TicketStatus.InProgress &&
                    c.UserId == currentUserId &&
                    c.Role == "SupportAgent"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await _controller.UpdateTicketStatus(ticketId, request);

            // Assert
            var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
            noContentResult.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task UpdateTicketPriority_ShouldReturnNoContent()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();
            _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

            var request = new UpdateTicketPriorityRequest
            {
                Priority = TicketPriority.Critical
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<UpdateTicketPriorityCommand>(c =>
                    c.TicketId == ticketId &&
                    c.NewPriority == TicketPriority.Critical &&
                    c.UserId == currentUserId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await _controller.UpdateTicketPriority(ticketId, request);

            // Assert
            var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
            noContentResult.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task AssignAgent_ShouldReturnNoContent()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();
            _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

            var request = new AssignTicketRequest
            {
                AgentId = agentId
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<AssignAgentCommand>(c =>
                    c.TicketId == ticketId &&
                    c.AgentId == agentId &&
                    c.UserId == currentUserId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await _controller.AssignAgent(ticketId, request);

            // Assert
            var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
            noContentResult.StatusCode.Should().Be(204);
        }
    }
}
