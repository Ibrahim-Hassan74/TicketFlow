using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using TicketFlow.Application.Features.Authentication.Commands.Logout;
using TicketFlow.Domain.IdentityEntities;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Authentication.Commands.Logout
{
    public class LogoutCommandHandlerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly LogoutCommandHandler _handler;

        public LogoutCommandHandlerTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(_userManagerMock.Object, contextAccessor.Object, claimsFactory.Object, null!, null!, null!, null!);

            _handler = new LogoutCommandHandler(_userManagerMock.Object, _signInManagerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenUserIdIsEmpty_ShouldOnlyCallSignOutAsync()
        {
            // Arrange
            var command = new LogoutCommand { Id = Guid.Empty };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _userManagerMock.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
            _signInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenUserIdProvidedAndUserFound_ShouldResetRefreshTokenAndSignOut()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new LogoutCommand { Id = userId };

            var user = new ApplicationUser
            {
                Id = userId,
                RefreshToken = "active_refresh_token",
                RefreshTokenExpirationDateTime = DateTimeOffset.UtcNow.AddDays(5)
            };

            _userManagerMock
                .Setup(m => m.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            user.RefreshToken.Should().BeNull();
            user.RefreshTokenExpirationDateTime.Should().Be(DateTimeOffset.MinValue);

            _userManagerMock.Verify(m => m.UpdateAsync(user), Times.Once);
            _signInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenUserIdProvidedAndUserNotFound_ShouldStillCallSignOut()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new LogoutCommand { Id = userId };

            _userManagerMock
                .Setup(m => m.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
            _signInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);
        }
    }
}
