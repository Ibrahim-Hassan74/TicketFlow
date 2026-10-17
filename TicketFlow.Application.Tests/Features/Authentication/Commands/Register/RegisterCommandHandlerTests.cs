using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.Authentication.Commands.Register;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.IdentityEntities;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Authentication.Commands.Register
{
    public class RegisterCommandHandlerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
        private readonly Mock<ILogger<RegisterCommandHandler>> _loggerMock;
        private readonly RegisterCommandHandler _handler;

        public RegisterCommandHandlerTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
            _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(roleStoreMock.Object, null!, null!, null!, null!);

            _loggerMock = new Mock<ILogger<RegisterCommandHandler>>();

            _handler = new RegisterCommandHandler(_userManagerMock.Object, _loggerMock.Object, _roleManagerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenEmailAlreadyExists_ShouldThrowEmailAlreadyExistsException()
        {
            // Arrange
            var command = new RegisterCommand
            {
                UserName = "JaneDoe",
                Email = "jane@example.com",
                Phone = "1234567890",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var existingUser = new ApplicationUser { Email = "jane@example.com" };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(command.Email))
                .ReturnsAsync(existingUser);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<EmailAlreadyExistsException>()
                .WithMessage("This email is already in use.");

            _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenIdentityCreationFails_ShouldThrowIdentityOperationException()
        {
            // Arrange
            var command = new RegisterCommand
            {
                UserName = "JaneDoe",
                Email = "jane@example.com",
                Phone = "1234567890",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(command.Email))
                .ReturnsAsync((ApplicationUser?)null);

            var error = new IdentityError { Description = "Password requires a digit." };
            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
                .ReturnsAsync(IdentityResult.Failed(error));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            var ex = await act.Should().ThrowAsync<IdentityOperationException>()
                .WithMessage("Registration failed.");
            ex.Which.Errors.Should().Contain("Password requires a digit.");
        }

        [Fact]
        public async Task Handle_WhenRegistrationSucceeds_ShouldCreateRoleIfNotExistsAssignRoleAndReturnSuccess()
        {
            // Arrange
            var command = new RegisterCommand
            {
                UserName = "JaneDoe",
                Email = "jane@example.com",
                Phone = "1234567890",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(command.Email))
                .ReturnsAsync((ApplicationUser?)null);

            ApplicationUser? createdUser = null;
            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
                .Callback<ApplicationUser, string>((u, _) =>
                {
                    u.Id = Guid.NewGuid();
                    createdUser = u;
                })
                .ReturnsAsync(IdentityResult.Success);

            _roleManagerMock
                .Setup(m => m.RoleExistsAsync(UserRole.Customer.ToString()))
                .ReturnsAsync(false); // Customer role not yet present

            _roleManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationRole>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), UserRole.Customer.ToString()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Message.Should().Be("Registration successful.");

            createdUser.Should().NotBeNull();
            createdUser!.DisplayName.Should().Be("JaneDoe");
            createdUser.Email.Should().Be("jane@example.com");
            createdUser.PhoneNumber.Should().Be("1234567890");
            createdUser.EmailConfirmed.Should().BeTrue();

            _roleManagerMock.Verify(m => m.CreateAsync(It.Is<ApplicationRole>(r => r.Name == UserRole.Customer.ToString())), Times.Once);
            _userManagerMock.Verify(m => m.AddToRoleAsync(createdUser, UserRole.Customer.ToString()), Times.Once);
        }
    }
}
