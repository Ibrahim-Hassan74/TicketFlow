using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TicketFlow.API.Controllers;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Features.Comments.Commands.AddComment;
using TicketFlow.Application.Features.Comments.DTOs;
using TicketFlow.Application.Features.Comments.Queries.GetComments;
using Xunit;

namespace TicketFlow.API.Tests.Controllers
{
    public class TicketCommentsControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly TicketCommentsController _controller;

        public TicketCommentsControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _controller = new TicketCommentsController(_mediatorMock.Object, _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task GetComments_ShouldReturnOk_WithListOfComments()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
            _currentUserServiceMock.Setup(s => s.Role).Returns("Customer");

            var comments = new List<CommentResponse>
            {
                new CommentResponse
                {
                    Id = Guid.NewGuid(),
                    TicketId = ticketId,
                    Content = "Please look at this asap",
                    UserName = "Alice"
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetCommentsQuery>(q =>
                    q.TicketId == ticketId &&
                    q.UserId == userId &&
                    q.Role == "Customer"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(comments);

            // Act
            var result = await _controller.GetComments(ticketId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedData = okResult.Value.Should().BeAssignableTo<IEnumerable<CommentResponse>>().Subject;
            returnedData.Should().HaveCount(1);
        }

        [Fact]
        public async Task AddComment_ShouldReturnOk_WithAddCommentResponse()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
            _currentUserServiceMock.Setup(s => s.Role).Returns("Customer");

            var request = new CreateCommentRequest
            {
                Content = "Here are the log files."
            };

            var response = new AddCommentResponse
            {
                Message = "Comment added successfully."
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<AddCommentCommand>(c =>
                    c.TicketId == ticketId &&
                    c.UserId == userId &&
                    c.Role == "Customer" &&
                    c.Content == "Here are the log files."), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.AddComment(ticketId, request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            var returnedData = okResult.Value.Should().BeOfType<AddCommentResponse>().Subject;
            returnedData.Message.Should().Be("Comment added successfully.");
        }
    }
}
