using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Activities.Queries.GetActivities;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Activities.Queries.GetActivities
{
    public class GetActivitiesQueryValidatorTests
    {
        private readonly GetActivitiesQueryValidator _validator;

        public GetActivitiesQueryValidatorTests()
        {
            _validator = new GetActivitiesQueryValidator();
        }

        [Fact]
        public void Validate_WhenQueryIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var query = new GetActivitiesQuery
            {
                TicketId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "Admin"
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WhenTicketIdIsEmpty_ShouldHaveValidationErrorForTicketId()
        {
            // Arrange
            var query = new GetActivitiesQuery
            {
                TicketId = Guid.Empty,
                UserId = Guid.NewGuid(),
                Role = "Admin"
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TicketId)
                  .WithErrorMessage("Ticket ID can't be blank.");
        }

        [Fact]
        public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationErrorForUserId()
        {
            // Arrange
            var query = new GetActivitiesQuery
            {
                TicketId = Guid.NewGuid(),
                UserId = Guid.Empty,
                Role = "Admin"
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("User ID can't be blank.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public void Validate_WhenRoleIsBlank_ShouldHaveValidationErrorForRole(string? role)
        {
            // Arrange
            var query = new GetActivitiesQuery
            {
                TicketId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
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
