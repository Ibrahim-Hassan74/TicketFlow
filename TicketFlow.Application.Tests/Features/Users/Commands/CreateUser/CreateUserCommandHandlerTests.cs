using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Features.Users.Commands.CreateUser;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.IdentityEntities;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandlerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
        private readonly Mock<ILogger<CreateUserCommandHandler>> _loggerMock;
        private readonly CreateUserCommandHandler _handler;

        public CreateUserCommandHandlerTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
            _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(roleStoreMock.Object, null!, null!, null!, null!);

            _loggerMock = new Mock<ILogger<CreateUserCommandHandler>>();

            _handler = new CreateUserCommandHandler(_userManagerMock.Object, _roleManagerMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenRoleIsInvalidEnum_ShouldThrowBadRequestException()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                Password = "Password123!",
                DisplayName = "Test User",
                Role = "SuperHacker"
            };

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("*Invalid role 'SuperHacker'*");
        }

        [Fact]
        public async Task Handle_WhenEmailAlreadyExists_ShouldThrowConflictException()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "existing@example.com",
                Password = "Password123!",
                DisplayName = "Existing User",
                Role = UserRole.SupportAgent.ToString()
            };

            var existingUser = new ApplicationUser { Email = "existing@example.com" };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(command.Email))
                .ReturnsAsync(existingUser);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ConflictException>()
                .WithMessage("A user with this email already exists.");
        }

        [Fact]
        public async Task Handle_WhenCreateAsyncFails_ShouldThrowIdentityOperationException()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "newuser@example.com",
                Password = "WeakPassword",
                DisplayName = "New User",
                Role = UserRole.Customer.ToString()
            };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(command.Email))
                .ReturnsAsync((ApplicationUser?)null);

            var identityError = new IdentityError { Description = "Passwords must have at least one non alphanumeric character." };
            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
                .ReturnsAsync(IdentityResult.Failed(identityError));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            var ex = await act.Should().ThrowAsync<IdentityOperationException>()
                .WithMessage("Failed to create user.");
            ex.Which.Errors.Should().Contain("Passwords must have at least one non alphanumeric character.");
        }

        [Fact]
        public async Task Handle_WhenValidRequest_ShouldCreateUserEnsureRoleAndReturnUserResponse()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "agent.dan@example.com",
                Password = "StrongPassword123!",
                DisplayName = "Dan Agent",
                Role = UserRole.SupportAgent.ToString()
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
                .Setup(m => m.RoleExistsAsync(UserRole.SupportAgent.ToString()))
                .ReturnsAsync(false); // Role doesn't exist yet, should create it

            _roleManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationRole>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), UserRole.SupportAgent.ToString()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be("agent.dan@example.com");
            result.DisplayName.Should().Be("Dan Agent");
            result.Role.Should().Be("SupportAgent");
            result.IsActive.Should().BeTrue();

            createdUser.Should().NotBeNull();
            createdUser!.EmailConfirmed.Should().BeTrue();

            _roleManagerMock.Verify(m => m.CreateAsync(It.Is<ApplicationRole>(r => r.Name == "SupportAgent")), Times.Once);
            _userManagerMock.Verify(m => m.AddToRoleAsync(createdUser, "SupportAgent"), Times.Once);
        }
    }
}
