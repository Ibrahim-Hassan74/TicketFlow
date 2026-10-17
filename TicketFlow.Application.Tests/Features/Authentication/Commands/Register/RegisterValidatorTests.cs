using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Authentication.Commands.Register;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Authentication.Commands.Register
{
    public class RegisterValidatorTests
    {
        private readonly RegisterValidator _validator;

        public RegisterValidatorTests()
        {
            _validator = new RegisterValidator();
        }

        [Fact]
        public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var command = new RegisterCommand
            {
                UserName = "johndoe",
                Email = "john@example.com",
                Phone = "1234567890",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
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
        public void Validate_WhenUserNameIsBlank_ShouldHaveValidationErrorForUserName(string? userName)
        {
            // Arrange
            var command = new RegisterCommand
            {
                UserName = userName!,
                Email = "john@example.com",
                Phone = "1234567890",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserName)
                  .WithErrorMessage("User Name can't be blank");
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("john@")]
        public void Validate_WhenEmailFormatIsInvalid_ShouldHaveValidationErrorForEmail(string invalidEmail)
        {
            // Arrange
            var command = new RegisterCommand
            {
                UserName = "johndoe",
                Email = invalidEmail,
                Phone = "1234567890",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Email should be in a proper email address format");
        }

        [Theory]
        [InlineData("123-abc")]
        [InlineData("+123456")]
        [InlineData("letters")]
        public void Validate_WhenPhoneContainsNonDigits_ShouldHaveValidationErrorForPhone(string invalidPhone)
        {
            // Arrange
            var command = new RegisterCommand
            {
                UserName = "johndoe",
                Email = "john@example.com",
                Phone = invalidPhone,
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Phone)
                  .WithErrorMessage("Phone should contains numbers only");
        }

        [Fact]
        public void Validate_WhenPasswordIsShorterThan8Characters_ShouldHaveValidationErrorForPassword()
        {
            // Arrange
            var command = new RegisterCommand
            {
                UserName = "johndoe",
                Email = "john@example.com",
                Phone = "1234567890",
                Password = "short",
                ConfirmPassword = "short"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Password should be at least 8 characters");
        }

        [Fact]
        public void Validate_WhenConfirmPasswordDoesNotMatchPassword_ShouldHaveValidationErrorForConfirmPassword()
        {
            // Arrange
            var command = new RegisterCommand
            {
                UserName = "johndoe",
                Email = "john@example.com",
                Phone = "1234567890",
                Password = "Password123!",
                ConfirmPassword = "DifferentPassword456!"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
                  .WithErrorMessage("Passwords do not match");
        }
    }
}
