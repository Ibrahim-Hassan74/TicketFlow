using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Authentication.Commands.RefreshToken;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Authentication.Commands.RefreshToken
{
    public class RefreshTokenCommandValidatorTests
    {
        private readonly RefreshTokenCommandValidator _validator;

        public RefreshTokenCommandValidatorTests()
        {
            _validator = new RefreshTokenCommandValidator();
        }

        [Fact]
        public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                Token = "valid.jwt.token",
                RefreshToken = "valid_refresh_token"
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
        public void Validate_WhenTokenIsBlank_ShouldHaveValidationErrorForToken(string? token)
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                Token = token!,
                RefreshToken = "valid_refresh_token"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Token)
                  .WithErrorMessage("Token is required.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public void Validate_WhenRefreshTokenIsBlank_ShouldHaveValidationErrorForRefreshToken(string? refreshToken)
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                Token = "valid.jwt.token",
                RefreshToken = refreshToken!
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
                  .WithErrorMessage("Refresh Token is required.");
        }
    }
}
