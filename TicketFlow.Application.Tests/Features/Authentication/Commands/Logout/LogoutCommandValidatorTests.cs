using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Authentication.Commands.Logout;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Authentication.Commands.Logout
{
    public class LogoutCommandValidatorTests
    {
        private readonly LogoutCommandValidator _validator;

        public LogoutCommandValidatorTests()
        {
            _validator = new LogoutCommandValidator();
        }

        [Fact]
        public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var command = new LogoutCommand
            {
                Id = Guid.NewGuid()
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
            var command = new LogoutCommand
            {
                Id = Guid.Empty
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("User Id can't be blank.");
        }
    }
}
