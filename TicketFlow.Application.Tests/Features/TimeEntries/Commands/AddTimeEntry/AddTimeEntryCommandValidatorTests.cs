using FluentValidation.TestHelper;
using TicketFlow.Application.Features.TimeEntries.Commands.AddTimeEntry;
using Xunit;

namespace TicketFlow.Application.Tests.Features.TimeEntries.Commands.AddTimeEntry
{
    public class AddTimeEntryCommandValidatorTests
    {
        private readonly AddTimeEntryCommandValidator _validator;

        public AddTimeEntryCommandValidatorTests()
        {
            _validator = new AddTimeEntryCommandValidator();
        }

        [Fact]
        public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var command = new AddTimeEntryCommand
            {
                TicketId = Guid.NewGuid(),
                AgentId = Guid.NewGuid(),
                WorkDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DurationMinutes = 60,
                Description = "Investigated logs and deployed fix"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WhenTicketIdIsEmpty_ShouldHaveValidationErrorForTicketId()
        {
            // Arrange
            var command = new AddTimeEntryCommand
            {
                TicketId = Guid.Empty,
                AgentId = Guid.NewGuid(),
                WorkDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DurationMinutes = 60,
                Description = "Work description"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TicketId)
                  .WithErrorMessage("Ticket Id can't be blank.");
        }

        [Fact]
        public void Validate_WhenAgentIdIsEmpty_ShouldHaveValidationErrorForAgentId()
        {
            // Arrange
            var command = new AddTimeEntryCommand
            {
                TicketId = Guid.NewGuid(),
                AgentId = Guid.Empty,
                WorkDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DurationMinutes = 60,
                Description = "Work description"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.AgentId)
                  .WithErrorMessage("Agent Id can't be blank.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        [InlineData(1441)]
        [InlineData(5000)]
        public void Validate_WhenDurationMinutesIsOutOfRange_ShouldHaveValidationErrorForDuration(int invalidDuration)
        {
            // Arrange
            var command = new AddTimeEntryCommand
            {
                TicketId = Guid.NewGuid(),
                AgentId = Guid.NewGuid(),
                WorkDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DurationMinutes = invalidDuration,
                Description = "Work description"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DurationMinutes)
                  .WithErrorMessage("Duration must be between 1 and 1440 minutes (24 hours).");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public void Validate_WhenDescriptionIsBlank_ShouldHaveValidationErrorForDescription(string? description)
        {
            // Arrange
            var command = new AddTimeEntryCommand
            {
                TicketId = Guid.NewGuid(),
                AgentId = Guid.NewGuid(),
                WorkDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DurationMinutes = 60,
                Description = description!
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description can't be blank.");
        }

        [Fact]
        public void Validate_WhenDescriptionExceeds500Characters_ShouldHaveValidationErrorForDescription()
        {
            // Arrange
            var command = new AddTimeEntryCommand
            {
                TicketId = Guid.NewGuid(),
                AgentId = Guid.NewGuid(),
                WorkDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DurationMinutes = 60,
                Description = new string('W', 501)
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description must not exceed 500 characters.");
        }
    }
}
