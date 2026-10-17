using FluentAssertions;
using Moq;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Users.DTOs;
using TicketFlow.Application.Features.Users.Queries.GetUsers;
using TicketFlow.Domain.Enums;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Users.Queries.GetUsers
{
    public class GetUsersQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly GetUsersQueryHandler _handler;

        public GetUsersQueryHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _handler = new GetUsersQueryHandler(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_WhenCalled_ShouldDelegateToUserRepositoryAndReturnPaginatedUsers()
        {
            // Arrange
            var query = new GetUsersQuery
            {
                Page = 2,
                PageSize = 15,
                Role = UserRole.SupportAgent,
                Search = "Smith"
            };

            var userResponses = new List<UserResponse>
            {
                new UserResponse
                {
                    Id = Guid.NewGuid(),
                    Email = "smith@example.com",
                    DisplayName = "Agent Smith",
                    Role = UserRole.SupportAgent.ToString(),
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            var expectedResponse = new PaginatedResponse<UserResponse>
            {
                Items = userResponses,
                TotalCount = 1,
                Page = 2,
                PageSize = 15
            };

            _userRepositoryMock
                .Setup(r => r.GetUsersAsync(query.Page, query.PageSize, query.Role, query.Search, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().DisplayName.Should().Be("Agent Smith");
            result.Page.Should().Be(2);
            result.PageSize.Should().Be(15);
            result.TotalCount.Should().Be(1);

            _userRepositoryMock.Verify(r => r.GetUsersAsync(2, 15, UserRole.SupportAgent, "Smith", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
