using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.Users.Commands.UpdateUser;
using TicketFlow.Domain.IdentityEntities;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandHandlerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<ILogger<UpdateUserCommandHandler>> _loggerMock;
        private readonly UpdateUserCommandHandler _handler;

        public UpdateUserCommandHandlerTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            _loggerMock = new Mock<ILogger<UpdateUserCommandHandler>>();

            _handler = new UpdateUserCommandHandler(_userManagerMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new UpdateUserCommand
            {
                Id = userId,
                DisplayName = "Updated Name",
                IsActive = true
            };

            _userManagerMock
                .Setup(m => m.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("User not found.");
        }

        [Fact]
        public async Task Handle_WhenUpdateAsyncFails_ShouldThrowIdentityOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new UpdateUserCommand
            {
                Id = userId,
                DisplayName = "Updated Name",
                IsActive = true
            };

            var user = new ApplicationUser { Id = userId, DisplayName = "Old Name" };

            _userManagerMock
                .Setup(m => m.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            var error = new IdentityError { Description = "Concurrency failure, object has been modified." };
            _userManagerMock
                .Setup(m => m.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Failed(error));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            var ex = await act.Should().ThrowAsync<IdentityOperationException>()
                .WithMessage("Failed to update user.");
            ex.Which.Errors.Should().Contain("Concurrency failure, object has been modified.");
        }

        [Fact]
        public async Task Handle_WhenUpdateSucceeds_ShouldUpdateUserAndReturnUserResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new UpdateUserCommand
            {
                Id = userId,
                DisplayName = "Brand New Display Name",
                IsActive = false // Deactivating user
            };

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "user@example.com",
                DisplayName = "Old Display Name",
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-1)
            };

            _userManagerMock
                .Setup(m => m.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "SupportAgent" });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(userId);
            result.DisplayName.Should().Be("Brand New Display Name");
            result.IsActive.Should().BeFalse();
            result.Role.Should().Be("SupportAgent");

            user.DisplayName.Should().Be("Brand New Display Name");
            user.IsDeleted.Should().BeTrue();

            _userManagerMock.Verify(m => m.UpdateAsync(user), Times.Once);
        }
    }
}
