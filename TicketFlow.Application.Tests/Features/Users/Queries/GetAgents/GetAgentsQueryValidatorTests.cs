using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Users.Queries.GetAgents;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Users.Queries.GetAgents
{
    public class GetAgentsQueryValidatorTests
    {
        private readonly GetAgentsQueryValidator _validator;

        public GetAgentsQueryValidatorTests()
        {
            _validator = new GetAgentsQueryValidator();
        }

        [Fact]
        public void Validate_WhenQueryHasDefaultValues_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var query = new GetAgentsQuery();

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Validate_WhenPageIsLessThanOne_ShouldHaveValidationErrorForPage(int invalidPage)
        {
            // Arrange
            var query = new GetAgentsQuery { Page = invalidPage };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Page)
                  .WithErrorMessage("Page number must be at least 1.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(101)]
        [InlineData(500)]
        public void Validate_WhenPageSizeIsOutOfRange_ShouldHaveValidationErrorForPageSize(int invalidPageSize)
        {
            // Arrange
            var query = new GetAgentsQuery { PageSize = invalidPageSize };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                  .WithErrorMessage("Page size must be between 1 and 100.");
        }
    }
}
