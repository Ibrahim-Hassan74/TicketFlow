using FluentAssertions;
using Moq;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Models;
using TicketFlow.Application.Features.Users.DTOs;
using TicketFlow.Application.Features.Users.Queries.GetAgents;
using TicketFlow.Domain.Enums;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Users.Queries.GetAgents
{
    public class GetAgentsQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly GetAgentsQueryHandler _handler;

        public GetAgentsQueryHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _handler = new GetAgentsQueryHandler(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_WhenCalled_ShouldDelegateToUserRepositoryAndReturnPaginatedAgents()
        {
            // Arrange
            var query = new GetAgentsQuery
            {
                Page = 1,
                PageSize = 10
            };

            var agents = new List<UserResponse>
            {
                new UserResponse
                {
                    Id = Guid.NewGuid(),
                    Email = "agent@example.com",
                    DisplayName = "Agent Scully",
                    Role = UserRole.SupportAgent.ToString(),
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            var expectedResponse = new PaginatedResponse<UserResponse>
            {
                Items = agents,
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            };

            _userRepositoryMock
                .Setup(r => r.GetAgentsAsync(query.Page, query.PageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().DisplayName.Should().Be("Agent Scully");
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(10);

            _userRepositoryMock.Verify(r => r.GetAgentsAsync(1, 10, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
