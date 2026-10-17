using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Dashboard.Queries.GetTicketTrends;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Dashboard.Queries.GetTicketTrends
{
    public class GetTicketTrendsQueryValidatorTests
    {
        private readonly GetTicketTrendsQueryValidator _validator;

        public GetTicketTrendsQueryValidatorTests()
        {
            _validator = new GetTicketTrendsQueryValidator();
        }

        [Fact]
        public void Validate_WhenQueryIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var query = new GetTicketTrendsQuery
            {
                Days = 30,
                Role = "Admin"
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(366)]
        [InlineData(1000)]
        public void Validate_WhenDaysOutOfRange_ShouldHaveValidationErrorForDays(int invalidDays)
        {
            // Arrange
            var query = new GetTicketTrendsQuery
            {
                Days = invalidDays,
                Role = "Admin"
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Days)
                  .WithErrorMessage("Days parameter must be between 1 and 365.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public void Validate_WhenRoleIsBlank_ShouldHaveValidationErrorForRole(string? role)
        {
            // Arrange
            var query = new GetTicketTrendsQuery
            {
                Days = 7,
                Role = role!
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Role)
                  .WithErrorMessage("Role can't be blank.");
        }
    }
}
