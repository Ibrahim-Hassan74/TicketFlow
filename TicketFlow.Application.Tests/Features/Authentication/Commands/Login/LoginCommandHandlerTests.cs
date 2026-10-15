using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Authentication.Commands.Login;
using TicketFlow.Application.Features.Authentication.DTOs;
using TicketFlow.Domain.IdentityEntities;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Authentication.Commands.Login
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<ILogger<LoginCommandHandler>> _loggerMock;
        private readonly LoginCommandHandler _handler;

        public LoginCommandHandlerTests()
        {
            _jwtServiceMock = new Mock<IJwtService>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(_userManagerMock.Object, contextAccessor.Object, claimsFactory.Object, null!, null!, null!, null!);

            _loggerMock = new Mock<ILogger<LoginCommandHandler>>();

            _handler = new LoginCommandHandler(_jwtServiceMock.Object, _userManagerMock.Object, _loggerMock.Object, _signInManagerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "missing@example.com",
                Password = "Password123!"
            };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(command.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("User not found.");
        }

        [Fact]
        public async Task Handle_WhenUserIsDeactivated_ShouldThrowAccountDeactivatedException()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "deactivated@example.com",
                Password = "Password123!"
            };

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "deactivated@example.com",
                IsDeleted = true
            };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(command.Email))
                .ReturnsAsync(user);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<AccountDeactivatedException>()
                .WithMessage("Your account has been deactivated. Please contact an administrator.");
        }

        [Fact]
        public async Task Handle_WhenAccountIsLockedOut_ShouldThrowAccountLockedException()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "locked@example.com",
                Password = "Password123!"
            };

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "locked@example.com",
                IsDeleted = false
            };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(command.Email))
                .ReturnsAsync(user);

            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync(user, command.Password, command.RememberMe, true))
                .ReturnsAsync(SignInResult.LockedOut);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<AccountLockedException>()
                .WithMessage("Your account is temporarily locked due to multiple failed login attempts. Please try again later.");
        }

        [Fact]
        public async Task Handle_WhenLoginIsNotAllowed_ShouldThrowLoginNotAllowedException()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "notallowed@example.com",
                Password = "Password123!"
            };

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "notallowed@example.com",
                IsDeleted = false
            };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(command.Email))
                .ReturnsAsync(user);

            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync(user, command.Password, command.RememberMe, true))
                .ReturnsAsync(SignInResult.NotAllowed);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<LoginNotAllowedException>()
                .WithMessage("User is not allowed to login.");
        }

        [Fact]
        public async Task Handle_WhenPasswordIsIncorrect_ShouldThrowInvalidCredentialsException()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "user@example.com",
                Password = "WrongPassword"
            };

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                IsDeleted = false
            };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(command.Email))
                .ReturnsAsync(user);

            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync(user, command.Password, command.RememberMe, true))
                .ReturnsAsync(SignInResult.Failed);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidCredentialsException>()
                .WithMessage("Incorrect email or password.");
        }

        [Fact]
        public async Task Handle_WhenCredentialsValid_ShouldGenerateJwtUpdateRefreshTokenAndReturnResponse()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "user@example.com",
                Password = "CorrectPassword123!",
                RememberMe = true
            };

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                DisplayName = "Valid User",
                IsDeleted = false
            };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(command.Email))
                .ReturnsAsync(user);

            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync(user, command.Password, command.RememberMe, true))
                .ReturnsAsync(SignInResult.Success);

            var expiration = DateTimeOffset.UtcNow.AddDays(7);
            var authResponse = new AuthenticationResponse
            {
                Token = "jwt.token.here",
                RefreshToken = "new_refresh_token",
                Expiration = DateTimeOffset.UtcNow.AddHours(1),
                RefreshTokenExpirationDateTime = expiration
            };

            _jwtServiceMock
                .Setup(j => j.CreateJwtToken(user, true))
                .ReturnsAsync(authResponse);

            _userManagerMock
                .Setup(m => m.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be("jwt.token.here");
            result.RefreshToken.Should().Be("new_refresh_token");

            user.RefreshToken.Should().Be("new_refresh_token");
            user.RefreshTokenExpirationDateTime.Should().Be(expiration);

            _userManagerMock.Verify(m => m.UpdateAsync(user), Times.Once);
        }
    }
}
