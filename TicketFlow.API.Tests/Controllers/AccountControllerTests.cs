using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TicketFlow.API.Controllers;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Authentication.Commands.Login;
using TicketFlow.Application.Features.Authentication.Commands.Logout;
using TicketFlow.Application.Features.Authentication.Commands.RefreshToken;
using TicketFlow.Application.Features.Authentication.Commands.Register;
using TicketFlow.Application.Features.Authentication.DTOs;
using TicketFlow.Application.Features.Users.DTOs;
using TicketFlow.Application.Features.Users.Queries.GetUserById;
using Xunit;

namespace TicketFlow.API.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _controller = new AccountController(_mediatorMock.Object, _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task PostRegister_ShouldReturnOk_WithRegisterResponse()
        {
            // Arrange
            var command = new RegisterCommand
            {
                UserName = "Jane",
                Email = "jane@example.com",
                Phone = "1234567890",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var response = new RegisterResponse
            {
                Message = "Registration successful."
            };

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.PostRegister(command);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedData = okResult.Value.Should().BeOfType<RegisterResponse>().Subject;
            returnedData.Message.Should().Be("Registration successful.");
        }

        [Fact]
        public async Task PostLogin_ShouldReturnOk_WithAuthenticationResponse()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "user@example.com",
                Password = "Password123!"
            };

            var response = new AuthenticationResponse
            {
                Token = "jwt.token",
                RefreshToken = "refresh.token"
            };

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.PostLogin(command);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedData = okResult.Value.Should().BeOfType<AuthenticationResponse>().Subject;
            returnedData.Token.Should().Be("jwt.token");
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnOk_WithNewAuthenticationResponse()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                Token = "old.jwt.token",
                RefreshToken = "old.refresh.token"
            };

            var response = new AuthenticationResponse
            {
                Token = "new.jwt.token",
                RefreshToken = "new.refresh.token"
            };

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.RefreshToken(command);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedData = okResult.Value.Should().BeOfType<AuthenticationResponse>().Subject;
            returnedData.Token.Should().Be("new.jwt.token");
        }

        [Fact]
        public async Task PostLogout_ShouldReturnNoContent()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

            _mediatorMock
                .Setup(m => m.Send(It.Is<LogoutCommand>(c => c.Id == currentUserId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await _controller.PostLogout();

            // Assert
            var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
            noContentResult.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task GetCurrentUser_ShouldReturnOk_WithCurrentUserProfile()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

            var userResponse = new UserResponse
            {
                Id = currentUserId,
                DisplayName = "Current Active User",
                Email = "current@example.com",
                Role = "SupportAgent"
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetUserByIdQuery>(q => q.Id == currentUserId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userResponse);

            // Act
            var result = await _controller.GetCurrentUser();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedData = okResult.Value.Should().BeOfType<UserResponse>().Subject;
            returnedData.Id.Should().Be(currentUserId);
            returnedData.DisplayName.Should().Be("Current Active User");
        }
    }
}
