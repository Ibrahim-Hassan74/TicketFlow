using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Users.Commands.CreateUser;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandValidatorTests
    {
        private readonly CreateUserCommandValidator _validator;

        public CreateUserCommandValidatorTests()
        {
            _validator = new CreateUserCommandValidator();
        }

        [Fact]
        public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "agent@ticketflow.com",
                Password = "SecurePassword123!",
                DisplayName = "Special Agent",
                Role = "SupportAgent"
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
            var command = new CreateUserCommand
            {
                Email = email!,
                Password = "SecurePassword123!",
                DisplayName = "Special Agent",
                Role = "SupportAgent"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Email can't be blank.");
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("agent@")]
        [InlineData("@example.com")]
        public void Validate_WhenEmailFormatIsInvalid_ShouldHaveValidationErrorForEmail(string invalidEmail)
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = invalidEmail,
                Password = "SecurePassword123!",
                DisplayName = "Special Agent",
                Role = "SupportAgent"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Invalid email address.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public void Validate_WhenPasswordIsBlank_ShouldHaveValidationErrorForPassword(string? password)
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "agent@ticketflow.com",
                Password = password!,
                DisplayName = "Special Agent",
                Role = "SupportAgent"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Password can't be blank.");
        }

        [Fact]
        public void Validate_WhenPasswordIsShorterThan8Characters_ShouldHaveValidationErrorForPassword()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "agent@ticketflow.com",
                Password = "short",
                DisplayName = "Special Agent",
                Role = "SupportAgent"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Password must be at least 8 characters long.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public void Validate_WhenDisplayNameIsBlank_ShouldHaveValidationErrorForDisplayName(string? displayName)
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "agent@ticketflow.com",
                Password = "SecurePassword123!",
                DisplayName = displayName!,
                Role = "SupportAgent"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DisplayName)
                  .WithErrorMessage("Display Name can't be blank.");
        }

        [Fact]
        public void Validate_WhenDisplayNameExceeds100Characters_ShouldHaveValidationErrorForDisplayName()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "agent@ticketflow.com",
                Password = "SecurePassword123!",
                DisplayName = new string('N', 101),
                Role = "SupportAgent"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DisplayName)
                  .WithErrorMessage("Display Name can't be longer than 100 characters.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public void Validate_WhenRoleIsBlank_ShouldHaveValidationErrorForRole(string? role)
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "agent@ticketflow.com",
                Password = "SecurePassword123!",
                DisplayName = "Special Agent",
                Role = role!
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Role)
                  .WithErrorMessage("Role can't be blank.");
        }
    }
}
