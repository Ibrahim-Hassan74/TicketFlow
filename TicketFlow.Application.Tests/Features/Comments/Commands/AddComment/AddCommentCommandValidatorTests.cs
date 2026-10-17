using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Comments.Commands.AddComment;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Comments.Commands.AddComment
{
    public class AddCommentCommandValidatorTests
    {
        private readonly AddCommentCommandValidator _validator;

        public AddCommentCommandValidatorTests()
        {
            _validator = new AddCommentCommandValidator();
        }

        [Fact]
        public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TicketId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "Customer",
                Content = "Here are the error logs you requested."
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public void Validate_WhenContentIsBlank_ShouldHaveValidationErrorForContent(string? content)
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TicketId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "Customer",
                Content = content!
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Content)
                  .WithErrorMessage("Comment Content can't be blank.");
        }

        [Fact]
        public void Validate_WhenContentExceeds2000Characters_ShouldHaveValidationErrorForContent()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TicketId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "Customer",
                Content = new string('C', 2001)
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Content)
                  .WithErrorMessage("Comment Content must not exceed 2000 characters.");
        }

        [Fact]
        public void Validate_WhenTicketIdIsEmpty_ShouldHaveValidationErrorForTicketId()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TicketId = Guid.Empty,
                UserId = Guid.NewGuid(),
                Role = "Customer",
                Content = "Valid content"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TicketId)
                  .WithErrorMessage("Ticket Id can't be blank.");
        }

        [Fact]
        public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationErrorForUserId()
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TicketId = Guid.NewGuid(),
                UserId = Guid.Empty,
                Role = "Customer",
                Content = "Valid content"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("User Id can't be blank.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public void Validate_WhenRoleIsBlank_ShouldHaveValidationErrorForRole(string? role)
        {
            // Arrange
            var command = new AddCommentCommand
            {
                TicketId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = role!,
                Content = "Valid content"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Role)
                  .WithErrorMessage("Role can't be blank.");
        }
    }
}
