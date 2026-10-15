using FluentAssertions;
using Moq;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Users.DTOs;
using TicketFlow.Application.Features.Users.Queries.GetUserById;
using TicketFlow.Domain.Enums;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly GetUserByIdQueryHandler _handler;

        public GetUserByIdQueryHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_WhenUserDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetUserByIdQuery { Id = userId };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserResponse?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("User not found.");
        }

        [Fact]
        public async Task Handle_WhenUserExists_ShouldReturnUserResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetUserByIdQuery { Id = userId };

            var userResponse = new UserResponse
            {
                Id = userId,
                Email = "user@example.com",
                DisplayName = "John Doe",
                Role = UserRole.Customer.ToString(),
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userResponse);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(userId);
            result.Email.Should().Be("user@example.com");
            result.DisplayName.Should().Be("John Doe");
            result.Role.Should().Be(UserRole.Customer.ToString());
            result.IsActive.Should().BeTrue();
        }
    }
}
