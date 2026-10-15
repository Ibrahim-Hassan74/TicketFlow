using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Authentication.Commands.Login;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Authentication.Commands.Login
{
    public class LoginCommandValidatorTests
    {
        private readonly LoginCommandValidator _validator;

        public LoginCommandValidatorTests()
        {
            _validator = new LoginCommandValidator();
        }

        [Fact]
        public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "user@example.com",
                Password = "Password123!"
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
        public void Validate_WhenEmailIsBlank_ShouldHaveValidationErrorForEmail(string? email)
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = email!,
                Password = "Password123!"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Email is required.");
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("user@")]
        public void Validate_WhenEmailFormatIsInvalid_ShouldHaveValidationErrorForEmail(string invalidEmail)
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = invalidEmail,
                Password = "Password123!"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Invalid email format.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public void Validate_WhenPasswordIsBlank_ShouldHaveValidationErrorForPassword(string? password)
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "user@example.com",
                Password = password!
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Password is required.");
        }

        [Fact]
        public void Validate_WhenPasswordIsShorterThan8Characters_ShouldHaveValidationErrorForPassword()
        {
            // Arrange
            var command = new LoginCommand
            {
                Email = "user@example.com",
                Password = "short"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Password must be at least 8 characters long.");
        }
    }
}
