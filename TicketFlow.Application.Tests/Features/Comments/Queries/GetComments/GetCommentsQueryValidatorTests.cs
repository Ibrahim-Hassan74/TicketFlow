using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Comments.Queries.GetComments;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Comments.Queries.GetComments
{
    public class GetCommentsQueryValidatorTests
    {
        private readonly GetCommentsQueryValidator _validator;

        public GetCommentsQueryValidatorTests()
        {
            _validator = new GetCommentsQueryValidator();
        }

        [Fact]
        public void Validate_WhenQueryIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var query = new GetCommentsQuery
            {
                TicketId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "Customer"
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
            var query = new GetCommentsQuery
            {
                TicketId = Guid.Empty,
                UserId = Guid.NewGuid(),
                Role = "Customer"
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TicketId)
                  .WithErrorMessage("Ticket Id can't be blank.");
        }

        [Fact]
        public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationErrorForUserId()
        {
            // Arrange
            var query = new GetCommentsQuery
            {
                TicketId = Guid.NewGuid(),
                UserId = Guid.Empty,
                Role = "Customer"
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("User Id can't be blank.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public void Validate_WhenRoleIsBlank_ShouldHaveValidationErrorForRole(string? role)
        {
            // Arrange
            var query = new GetCommentsQuery
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
