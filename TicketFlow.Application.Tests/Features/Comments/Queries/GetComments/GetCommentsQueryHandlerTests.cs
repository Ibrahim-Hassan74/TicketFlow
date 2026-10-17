using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Comments.Queries.GetComments;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.IdentityEntities;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Comments.Queries.GetComments
{
    public class GetCommentsQueryHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITicketCommentRepository> _ticketCommentRepositoryMock;
        private readonly Mock<ITicketAuthorizationService> _ticketAuthMock;
        private readonly Mock<ILogger<GetCommentsQueryHandler>> _loggerMock;
        private readonly GetCommentsQueryHandler _handler;

        public GetCommentsQueryHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _ticketCommentRepositoryMock = new Mock<ITicketCommentRepository>();
            _unitOfWorkMock.Setup(u => u.TicketCommentRepository).Returns(_ticketCommentRepositoryMock.Object);

            _ticketAuthMock = new Mock<ITicketAuthorizationService>();
            _loggerMock = new Mock<ILogger<GetCommentsQueryHandler>>();

            _handler = new GetCommentsQueryHandler(_unitOfWorkMock.Object, _loggerMock.Object, _ticketAuthMock.Object);
        }

        [Fact]
        public async Task Handle_WhenUserCannotAccessTicket_ShouldThrowNotFoundException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var query = new GetCommentsQuery
            {
                TicketId = ticketId,
                UserId = userId,
                Role = "Customer"
            };

            _ticketAuthMock
                .Setup(a => a.CanAccessTicketAsync(ticketId, userId, "Customer", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Ticket not found or you do not have access.");

            _ticketCommentRepositoryMock.Verify(r => r.GetCommentsByTicketIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenUserHasAccess_ShouldReturnOrderedComments()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var query = new GetCommentsQuery
            {
                TicketId = ticketId,
                UserId = userId,
                Role = "Admin"
            };

            _ticketAuthMock
                .Setup(a => a.CanAccessTicketAsync(ticketId, userId, "Admin", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var author = new ApplicationUser { Id = userId, DisplayName = "Sarah Admin" };
            var comment1 = new TicketComment
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                UserId = userId,
                User = author,
                Content = "First Comment",
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-30)
            };
            var comment2 = new TicketComment
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                UserId = userId,
                User = author,
                Content = "Second Comment",
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
            };

            _ticketCommentRepositoryMock
                .Setup(r => r.GetCommentsByTicketIdAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TicketComment> { comment1, comment2 });

            // Act
            var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Content.Should().Be("First Comment");
            result[0].UserName.Should().Be("Sarah Admin");
            result[1].Content.Should().Be("Second Comment");

            _ticketCommentRepositoryMock.Verify(r => r.GetCommentsByTicketIdAsync(
                ticketId,
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
