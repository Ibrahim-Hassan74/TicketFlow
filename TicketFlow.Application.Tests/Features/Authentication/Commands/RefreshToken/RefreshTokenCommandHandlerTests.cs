using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Security.Claims;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Authentication.Commands.RefreshToken;
using TicketFlow.Application.Features.Authentication.DTOs;
using TicketFlow.Domain.IdentityEntities;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Authentication.Commands.RefreshToken
{
    public class RefreshTokenCommandHandlerTests
    {
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly RefreshTokenCommandHandler _handler;

        public RefreshTokenCommandHandlerTests()
        {
            _jwtServiceMock = new Mock<IJwtService>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _handler = new RefreshTokenCommandHandler(_jwtServiceMock.Object, _userManagerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenGetPrincipalThrowsSecurityTokenException_ShouldThrowInvalidTokenException()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                Token = "invalid_token",
                RefreshToken = "refresh_token"
            };

            _jwtServiceMock
                .Setup(j => j.GetPrincipalFromJwtToken(command.Token))
                .Throws(new SecurityTokenException("Malformed"));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidTokenException>()
                .WithMessage("Access token is invalid.");
        }

        [Fact]
        public async Task Handle_WhenPrincipalIsNull_ShouldThrowInvalidTokenException()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                Token = "token",
                RefreshToken = "refresh_token"
            };

            _jwtServiceMock
                .Setup(j => j.GetPrincipalFromJwtToken(command.Token))
                .Returns((ClaimsPrincipal?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidTokenException>()
                .WithMessage("Access token is invalid.");
        }

        [Fact]
        public async Task Handle_WhenEmailClaimIsMissing_ShouldThrowMissingTokenClaimException()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                Token = "token",
                RefreshToken = "refresh_token"
            };

            var identity = new ClaimsIdentity(new[] { new Claim("some_other_claim", "value") });
            var principal = new ClaimsPrincipal(identity);

            _jwtServiceMock
                .Setup(j => j.GetPrincipalFromJwtToken(command.Token))
                .Returns(principal);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<MissingTokenClaimException>()
                .WithMessage("Email claim is missing in token.");
        }

        [Fact]
        public async Task Handle_WhenUserDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                Token = "token",
                RefreshToken = "refresh_token"
            };

            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, "missing@example.com") });
            var principal = new ClaimsPrincipal(identity);

            _jwtServiceMock
                .Setup(j => j.GetPrincipalFromJwtToken(command.Token))
                .Returns(principal);

            _userManagerMock
                .Setup(m => m.FindByEmailAsync("missing@example.com"))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("User does not exist.");
        }

        [Fact]
        public async Task Handle_WhenUserIsDeactivated_ShouldThrowAccountDeactivatedException()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                Token = "token",
                RefreshToken = "refresh_token"
            };

            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, "deactivated@example.com") });
            var principal = new ClaimsPrincipal(identity);

            _jwtServiceMock
                .Setup(j => j.GetPrincipalFromJwtToken(command.Token))
                .Returns(principal);

            var user = new ApplicationUser
            {
                Email = "deactivated@example.com",
                IsDeleted = true
            };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync("deactivated@example.com"))
                .ReturnsAsync(user);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<AccountDeactivatedException>()
                .WithMessage("Your account has been deactivated. Please contact an administrator.");
        }

        [Fact]
        public async Task Handle_WhenRefreshTokenDoesNotMatch_ShouldThrowInvalidRefreshTokenException()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                Token = "token",
                RefreshToken = "wrong_token"
            };

            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, "user@example.com") });
            var principal = new ClaimsPrincipal(identity);

            _jwtServiceMock
                .Setup(j => j.GetPrincipalFromJwtToken(command.Token))
                .Returns(principal);

            var user = new ApplicationUser
            {
                Email = "user@example.com",
                RefreshToken = "correct_token",
                RefreshTokenExpirationDateTime = DateTimeOffset.UtcNow.AddDays(1)
            };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync("user@example.com"))
                .ReturnsAsync(user);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidRefreshTokenException>()
                .WithMessage("Refresh token is invalid or expired.");
        }

        [Fact]
        public async Task Handle_WhenRefreshTokenIsExpired_ShouldThrowInvalidRefreshTokenException()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                Token = "token",
                RefreshToken = "my_token"
            };

            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, "user@example.com") });
            var principal = new ClaimsPrincipal(identity);

            _jwtServiceMock
                .Setup(j => j.GetPrincipalFromJwtToken(command.Token))
                .Returns(principal);

            var user = new ApplicationUser
            {
                Email = "user@example.com",
                RefreshToken = "my_token",
                RefreshTokenExpirationDateTime = DateTimeOffset.UtcNow.AddMinutes(-10) // Expired
            };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync("user@example.com"))
                .ReturnsAsync(user);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidRefreshTokenException>()
                .WithMessage("Refresh token is invalid or expired.");
        }

        [Fact]
        public async Task Handle_WhenTokenIsValid_ShouldRotateRefreshTokenAndReturnAuthResponse()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                Token = "token",
                RefreshToken = "valid_old_refresh"
            };

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "user@example.com"),
                new Claim("remember_me", "true")
            });
            var principal = new ClaimsPrincipal(identity);

            _jwtServiceMock
                .Setup(j => j.GetPrincipalFromJwtToken(command.Token))
                .Returns(principal);

            var user = new ApplicationUser
            {
                Email = "user@example.com",
                RefreshToken = "valid_old_refresh",
                RefreshTokenExpirationDateTime = DateTimeOffset.UtcNow.AddDays(1)
            };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync("user@example.com"))
                .ReturnsAsync(user);

            var newExpiration = DateTimeOffset.UtcNow.AddDays(7);
            var newAuthResponse = new AuthenticationResponse
            {
                Token = "new_jwt_token",
                RefreshToken = "new_rotated_refresh",
                Expiration = DateTimeOffset.UtcNow.AddHours(1),
                RefreshTokenExpirationDateTime = newExpiration
            };

            _jwtServiceMock
                .Setup(j => j.CreateJwtToken(user, true))
                .ReturnsAsync(newAuthResponse);

            _userManagerMock
                .Setup(m => m.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be("new_jwt_token");
            result.RefreshToken.Should().Be("new_rotated_refresh");

            user.RefreshToken.Should().Be("new_rotated_refresh");
            user.RefreshTokenExpirationDateTime.Should().Be(newExpiration);

            _userManagerMock.Verify(m => m.UpdateAsync(user), Times.Once);
        }
    }
}
