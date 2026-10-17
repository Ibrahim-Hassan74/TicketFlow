using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Users.Queries.GetUserById;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryValidatorTests
    {
        private readonly GetUserByIdQueryValidator _validator;

        public GetUserByIdQueryValidatorTests()
        {
            _validator = new GetUserByIdQueryValidator();
        }

        [Fact]
        public void Validate_WhenIdIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var query = new GetUserByIdQuery { Id = Guid.NewGuid() };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WhenIdIsEmpty_ShouldHaveValidationErrorForId()
        {
            // Arrange
            var query = new GetUserByIdQuery { Id = Guid.Empty };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("User Id can't be blank.");
        }
    }
}
