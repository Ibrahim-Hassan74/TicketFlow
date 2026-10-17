using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TicketFlow.API.Controllers;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Users.Commands.CreateUser;
using TicketFlow.Application.Features.Users.Commands.UpdateUser;
using TicketFlow.Application.Features.Users.DTOs;
using TicketFlow.Application.Features.Users.Queries.GetAgents;
using TicketFlow.Application.Features.Users.Queries.GetUserById;
using TicketFlow.Application.Features.Users.Queries.GetUsers;
using TicketFlow.Domain.Enums;
using Xunit;

namespace TicketFlow.API.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new UsersController(_mediatorMock.Object);
        }

        [Fact]
        public async Task GetUsers_ShouldReturnOk_WithPaginatedUsers()
        {
            // Arrange
            var query = new GetUsersQuery { Page = 1, PageSize = 10, Role = UserRole.SupportAgent };
            var users = new List<UserResponse>
            {
                new UserResponse { Id = Guid.NewGuid(), DisplayName = "Agent Smith", Role = "SupportAgent" }
            };
            var paginatedResponse = new PaginatedResponse<UserResponse>
            {
                Items = users,
                Page = 1,
                PageSize = 10,
                TotalCount = 1
            };

            _mediatorMock
                .Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paginatedResponse);

            // Act
            var result = await _controller.GetUsers(query);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedData = okResult.Value.Should().BeOfType<PaginatedResponse<UserResponse>>().Subject;
            returnedData.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnOk_WithUserResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userResponse = new UserResponse
            {
                Id = userId,
                DisplayName = "John Doe",
                Email = "john@example.com",
                Role = "Customer"
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetUserByIdQuery>(q => q.Id == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userResponse);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedData = okResult.Value.Should().BeOfType<UserResponse>().Subject;
            returnedData.Id.Should().Be(userId);
            returnedData.DisplayName.Should().Be("John Doe");
        }

        [Fact]
        public async Task CreateUser_ShouldReturnCreatedAtAction_WithCreatedUser()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                DisplayName = "New Agent",
                Email = "newagent@example.com",
                Password = "Password123!",
                Role = "SupportAgent"
            };
            var createdUserId = Guid.NewGuid();
            var userResponse = new UserResponse
            {
                Id = createdUserId,
                DisplayName = "New Agent",
                Email = "newagent@example.com",
                Role = "SupportAgent"
            };

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userResponse);

            // Act
            var result = await _controller.CreateUser(command);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.StatusCode.Should().Be(201);
            createdResult.ActionName.Should().Be(nameof(UsersController.GetUserById));
            createdResult.RouteValues!["id"].Should().Be(createdUserId);
            var returnedData = createdResult.Value.Should().BeOfType<UserResponse>().Subject;
            returnedData.Id.Should().Be(createdUserId);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WithUpdatedUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new UpdateUserRequest
            {
                DisplayName = "Updated Name",
                IsActive = true
            };
            var userResponse = new UserResponse
            {
                Id = userId,
                DisplayName = "Updated Name",
                IsActive = true
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<UpdateUserCommand>(c =>
                    c.Id == userId &&
                    c.DisplayName == "Updated Name" &&
                    c.IsActive == true), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userResponse);

            // Act
            var result = await _controller.UpdateUser(userId, request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedData = okResult.Value.Should().BeOfType<UserResponse>().Subject;
            returnedData.DisplayName.Should().Be("Updated Name");
        }

        [Fact]
        public async Task GetAgents_ShouldReturnOk_WithPaginatedAgents()
        {
            // Arrange
            var agents = new List<UserResponse>
            {
                new UserResponse { Id = Guid.NewGuid(), DisplayName = "Agent Fox", Role = "SupportAgent" }
            };
            var paginatedResponse = new PaginatedResponse<UserResponse>
            {
                Items = agents,
                Page = 1,
                PageSize = 10,
                TotalCount = 1
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetAgentsQuery>(q => q.Page == 1 && q.PageSize == 10), It.IsAny<CancellationToken>()))
                .ReturnsAsync(paginatedResponse);

            // Act
            var result = await _controller.GetAgents(1, 10);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedData = okResult.Value.Should().BeOfType<PaginatedResponse<UserResponse>>().Subject;
            returnedData.Items.Should().HaveCount(1);
        }
    }
}
