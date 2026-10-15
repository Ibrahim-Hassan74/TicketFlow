using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Comments.Commands.AddComment;
using TicketFlow.Domain.Contracts;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Comments.Commands.AddComment
{
    public class AddCommentCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITicketCommentRepository> _ticketCommentRepositoryMock;
        private readonly Mock<ITicketActivityRepository> _ticketActivityRepositoryMock;
        private readonly Mock<ITicketAuthorizationService> _ticketAuthMock;
        private readonly Mock<ILogger<AddCommentCommandHandler>> _loggerMock;
        private readonly AddCommentCommandHandler _handler;

        public AddCommentCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _ticketCommentRepositoryMock = new Mock<ITicketCommentRepository>();
            _ticketActivityRepositoryMock = new Mock<ITicketActivityRepository>();

            _unitOfWorkMock.Setup(u => u.TicketCommentRepository).Returns(_ticketCommentRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.TicketActivityRepository).Returns(_ticketActivityRepositoryMock.Object);

            _ticketAuthMock = new Mock<ITicketAuthorizationService>();
            _loggerMock = new Mock<ILogger<AddCommentCommandHandler>>();

            _handler = new AddCommentCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object, _ticketAuthMock.Object);
        }

        [Fact]
        public async Task Handle_WhenUserCannotAccessTicket_ShouldThrowNotFoundException()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var command = new AddCommentCommand
            {
                TicketId = ticketId,
                UserId = userId,
                Role = "Customer",
                Content = "Help please"
            };

            _ticketAuthMock
                .Setup(a => a.CanAccessTicketAsync(ticketId, userId, "Customer", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Ticket not found or you do not have access.");

            _ticketCommentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TicketComment>(), It.IsAny<CancellationToken>()), Times.Never);
            _ticketActivityRepositoryMock.Verify(a => a.AddAsync(It.IsAny<TicketActivity>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenUserHasAccess_ShouldAddCommentAndActivityAndCommit()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var command = new AddCommentCommand
            {
                TicketId = ticketId,
                UserId = userId,
                Role = "Customer",
                Content = "Uploaded the diagnostic file."
            };

            _ticketAuthMock
                .Setup(a => a.CanAccessTicketAsync(ticketId, userId, "Customer", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            TicketComment? capturedComment = null;
            _ticketCommentRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<TicketComment>(), It.IsAny<CancellationToken>()))
                .Callback<TicketComment, CancellationToken>((c, _) => capturedComment = c)
                .ReturnsAsync((TicketComment c, CancellationToken _) => c);

            TicketActivity? capturedActivity = null;
            _ticketActivityRepositoryMock
                .Setup(a => a.AddAsync(It.IsAny<TicketActivity>(), It.IsAny<CancellationToken>()))
                .Callback<TicketActivity, CancellationToken>((a, _) => capturedActivity = a)
                .ReturnsAsync((TicketActivity a, CancellationToken _) => a);

            _unitOfWorkMock
                .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Message.Should().Be("Comment added successfully.");

            capturedComment.Should().NotBeNull();
            capturedComment!.TicketId.Should().Be(ticketId);
            capturedComment.UserId.Should().Be(userId);
            capturedComment.Content.Should().Be("Uploaded the diagnostic file.");

            capturedActivity.Should().NotBeNull();
            capturedActivity!.TicketId.Should().Be(ticketId);
            capturedActivity.UserId.Should().Be(userId);
            capturedActivity.Type.Should().Be(ActivityType.CommentAdded);
            capturedActivity.Description.Should().Be("A new comment was added.");

            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
