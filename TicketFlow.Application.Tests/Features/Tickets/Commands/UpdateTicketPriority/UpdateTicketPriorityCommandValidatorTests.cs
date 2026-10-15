using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Tickets.Commands.UpdateTicketPriority;
using TicketFlow.Domain.Enums;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Tickets.Commands.UpdateTicketPriority
{
    public class UpdateTicketPriorityCommandValidatorTests
    {
        private readonly UpdateTicketPriorityCommandValidator _validator;

        public UpdateTicketPriorityCommandValidatorTests()
        {
            _validator = new UpdateTicketPriorityCommandValidator();
        }

        [Fact]
        public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var command = new UpdateTicketPriorityCommand
            {
                TicketId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                NewPriority = TicketPriority.High
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WhenTicketIdIsEmpty_ShouldHaveValidationErrorForTicketId()
        {
            // Arrange
            var command = new UpdateTicketPriorityCommand
            {
                TicketId = Guid.Empty,
                UserId = Guid.NewGuid(),
                NewPriority = TicketPriority.High
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TicketId)
                  .WithErrorMessage("Ticket Id can't be blank.");
        }

        [Fact]
        public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationErrorForUserId()
        {
            // Arrange
            var command = new UpdateTicketPriorityCommand
            {
                TicketId = Guid.NewGuid(),
                UserId = Guid.Empty,
                NewPriority = TicketPriority.High
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("User Id can't be blank.");
        }

        [Fact]
        public void Validate_WhenNewPriorityIsInvalidEnum_ShouldHaveValidationErrorForNewPriority()
        {
            // Arrange
            var command = new UpdateTicketPriorityCommand
            {
                TicketId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                NewPriority = (TicketPriority)999
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.NewPriority)
                  .WithErrorMessage("Invalid ticket priority.");
        }
    }
}
