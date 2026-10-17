using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Users.Queries.GetUsers;
using TicketFlow.Domain.Enums;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Users.Queries.GetUsers
{
    public class GetUsersQueryValidatorTests
    {
        private readonly GetUsersQueryValidator _validator;

        public GetUsersQueryValidatorTests()
        {
            _validator = new GetUsersQueryValidator();
        }

        [Fact]
        public void Validate_WhenQueryHasDefaultValues_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var query = new GetUsersQuery();

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
            var query = new GetUsersQuery { Page = invalidPage };

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
            var query = new GetUsersQuery { PageSize = invalidPageSize };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                  .WithErrorMessage("Page size must be between 1 and 100.");
        }

        [Fact]
        public void Validate_WhenRoleIsInvalidEnum_ShouldHaveValidationErrorForRole()
        {
            // Arrange
            var query = new GetUsersQuery { Role = (UserRole)999 };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Role)
                  .WithErrorMessage("Invalid user role.");
        }

        [Fact]
        public void Validate_WhenRoleIsNull_ShouldNotHaveValidationErrorForRole()
        {
            // Arrange
            var query = new GetUsersQuery { Role = null };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Role);
        }
    }
}
