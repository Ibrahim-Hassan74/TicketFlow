using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Tickets.Commands.UpdateTicket;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Tickets.Commands.UpdateTicket
{
    public class UpdateTicketCommandValidatorTests
    {
        private readonly UpdateTicketCommandValidator _validator;

        public UpdateTicketCommandValidatorTests()
        {
            _validator = new UpdateTicketCommandValidator();
        }

        [Fact]
        public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var command = new UpdateTicketCommand
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Title = "Updated Ticket Title",
                Description = "Updated Ticket Description"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WhenIdIsEmpty_ShouldHaveValidationErrorForId()
        {
            // Arrange
            var command = new UpdateTicketCommand
            {
                Id = Guid.Empty,
                UserId = Guid.NewGuid(),
                Title = "Valid Title",
                Description = "Valid Description"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Ticket Id can't be blank.");
        }

        [Fact]
        public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationErrorForUserId()
        {
            // Arrange
            var command = new UpdateTicketCommand
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Empty,
                Title = "Valid Title",
                Description = "Valid Description"
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
        public void Validate_WhenTitleIsBlank_ShouldHaveValidationErrorForTitle(string? title)
        {
            // Arrange
            var command = new UpdateTicketCommand
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Title = title!,
                Description = "Valid Description"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Title can't be blank.");
        }

        [Fact]
        public void Validate_WhenTitleExceeds200Characters_ShouldHaveValidationErrorForTitle()
        {
            // Arrange
            var command = new UpdateTicketCommand
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Title = new string('T', 201),
                Description = "Valid Description"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Title can't be more than 200 char.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public void Validate_WhenDescriptionIsBlank_ShouldHaveValidationErrorForDescription(string? description)
        {
            // Arrange
            var command = new UpdateTicketCommand
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Title = "Valid Title",
                Description = description!
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description can't be blank.");
        }

        [Fact]
        public void Validate_WhenDescriptionExceeds4000Characters_ShouldHaveValidationErrorForDescription()
        {
            // Arrange
            var command = new UpdateTicketCommand
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Title = "Valid Title",
                Description = new string('D', 4001)
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description can't be more than 4000 char.");
        }
    }
}
