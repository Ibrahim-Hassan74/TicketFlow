using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Tickets.Queries.GetTicketById;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Tickets.Queries.GetTicketById
{
    public class GetTicketByIdQueryValidatorTests
    {
        private readonly GetTicketByIdQueryValidator _validator;

        public GetTicketByIdQueryValidatorTests()
        {
            _validator = new GetTicketByIdQueryValidator();
        }

        [Fact]
        public void Validate_WhenQueryIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var query = new GetTicketByIdQuery(Guid.NewGuid(), Guid.NewGuid(), "Admin");

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WhenIdIsEmpty_ShouldHaveValidationErrorForId()
        {
            // Arrange
            var query = new GetTicketByIdQuery(Guid.Empty, Guid.NewGuid(), "Admin");

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Ticket Id can't be blank.");
        }

        [Fact]
        public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationErrorForUserId()
        {
            // Arrange
            var query = new GetTicketByIdQuery(Guid.NewGuid(), Guid.Empty, "Admin");

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("User Id can't be blank.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null!)]
        public void Validate_WhenRoleIsBlank_ShouldHaveValidationErrorForRole(string? role)
        {
            // Arrange
            var query = new GetTicketByIdQuery(Guid.NewGuid(), Guid.NewGuid(), role!);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Role)
                  .WithErrorMessage("Role can't be blank.");
        }
    }
}
