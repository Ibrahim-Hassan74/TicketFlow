using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Tickets.Commands.CreateTicket;
using TicketFlow.Domain.Enums;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Tickets.Commands.CreateTicket
{
    public class CreateTicketValidatorTests
    {
        private readonly CreateTicketValidator _validator;

        public CreateTicketValidatorTests()
        {
            _validator = new CreateTicketValidator();
        }

        [Fact]
        public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var command = new CreateTicketCommand
            {
                CustomerId = Guid.NewGuid(),
                Title = "Printer connection issue",
                Description = "Printer on 2nd floor is not responding to network requests.",
                Priority = TicketPriority.High
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WhenCustomerIdIsEmpty_ShouldHaveValidationErrorForCustomerId()
        {
            // Arrange
            var command = new CreateTicketCommand
            {
                CustomerId = Guid.Empty,
                Title = "Valid Title",
                Description = "Valid Description",
                Priority = TicketPriority.Medium
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CustomerId)
                  .WithErrorMessage("Customer Id can't be blank.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public void Validate_WhenTitleIsBlank_ShouldHaveValidationErrorForTitle(string? title)
        {
            // Arrange
            var command = new CreateTicketCommand
            {
                CustomerId = Guid.NewGuid(),
                Title = title!,
                Description = "Valid Description",
                Priority = TicketPriority.Medium
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Title can't be blank.");
        }

        [Fact]
        public void Validate_WhenTitleExceeds200Characters_ShouldHaveValidationErrorForTitle()
        {
            // Arrange
            var command = new CreateTicketCommand
            {
                CustomerId = Guid.NewGuid(),
                Title = new string('A', 201),
                Description = "Valid Description",
                Priority = TicketPriority.Medium
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Title can't be longer than 200 characters.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public void Validate_WhenDescriptionIsBlank_ShouldHaveValidationErrorForDescription(string? description)
        {
            // Arrange
            var command = new CreateTicketCommand
            {
                CustomerId = Guid.NewGuid(),
                Title = "Valid Title",
                Description = description!,
                Priority = TicketPriority.Medium
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description can't be blank.");
        }

        [Fact]
        public void Validate_WhenDescriptionExceeds4000Characters_ShouldHaveValidationErrorForDescription()
        {
            // Arrange
            var command = new CreateTicketCommand
            {
                CustomerId = Guid.NewGuid(),
                Title = "Valid Title",
                Description = new string('B', 4001),
                Priority = TicketPriority.Medium
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description can't be longer than 4000 characters.");
        }

        [Fact]
        public void Validate_WhenPriorityIsInvalidEnum_ShouldHaveValidationErrorForPriority()
        {
            // Arrange
            var command = new CreateTicketCommand
            {
                CustomerId = Guid.NewGuid(),
                Title = "Valid Title",
                Description = "Valid Description",
                Priority = (TicketPriority)999
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Priority)
                  .WithErrorMessage("Invalid ticket priority.");
        }
    }
}
