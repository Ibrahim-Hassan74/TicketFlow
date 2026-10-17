using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Users.Commands.UpdateUser;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandValidatorTests
    {
        private readonly UpdateUserCommandValidator _validator;

        public UpdateUserCommandValidatorTests()
        {
            _validator = new UpdateUserCommandValidator();
        }

        [Fact]
        public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var command = new UpdateUserCommand
            {
                Id = Guid.NewGuid(),
                DisplayName = "Updated Name",
                IsActive = true
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
            var command = new UpdateUserCommand
            {
                Id = Guid.Empty,
                DisplayName = "Valid Name",
                IsActive = true
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("User id can't be blank.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public void Validate_WhenDisplayNameIsBlank_ShouldHaveValidationErrorForDisplayName(string? displayName)
        {
            // Arrange
            var command = new UpdateUserCommand
            {
                Id = Guid.NewGuid(),
                DisplayName = displayName!,
                IsActive = true
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DisplayName)
                  .WithErrorMessage("Name can't be blank");
        }

        [Fact]
        public void Validate_WhenDisplayNameExceeds100Characters_ShouldHaveValidationErrorForDisplayName()
        {
            // Arrange
            var command = new UpdateUserCommand
            {
                Id = Guid.NewGuid(),
                DisplayName = new string('A', 101),
                IsActive = true
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DisplayName)
                  .WithErrorMessage("Name can't be longer than 100 characters");
        }
    }
}
