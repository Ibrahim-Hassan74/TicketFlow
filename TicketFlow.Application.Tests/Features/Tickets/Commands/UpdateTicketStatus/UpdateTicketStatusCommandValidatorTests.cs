using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Tickets.Commands.UpdateTicketStatus;
using TicketFlow.Domain.Enums;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Tickets.Commands.UpdateTicketStatus
{
    public class UpdateTicketStatusCommandValidatorTests
    {
        private readonly UpdateTicketStatusCommandValidator _validator;

        public UpdateTicketStatusCommandValidatorTests()
        {
            _validator = new UpdateTicketStatusCommandValidator();
        }

        [Fact]
        public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var command = new UpdateTicketStatusCommand
            {
                TicketId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "Admin",
                NewStatus = TicketStatus.InProgress
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WhenNewStatusIsInvalidEnum_ShouldHaveValidationErrorForNewStatus()
        {
            // Arrange
            var command = new UpdateTicketStatusCommand
            {
                TicketId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "Admin",
                NewStatus = (TicketStatus)999
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.NewStatus)
                  .WithErrorMessage("Invalid ticket status.");
        }

        [Fact]
        public void Validate_WhenTicketIdIsEmpty_ShouldHaveValidationErrorForTicketId()
        {
            // Arrange
            var command = new UpdateTicketStatusCommand
            {
                TicketId = Guid.Empty,
                UserId = Guid.NewGuid(),
                Role = "Admin",
                NewStatus = TicketStatus.Resolved
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
            var command = new UpdateTicketStatusCommand
            {
                TicketId = Guid.NewGuid(),
                UserId = Guid.Empty,
                Role = "Admin",
                NewStatus = TicketStatus.Resolved
            };

            // Act
            var result = _validator.TestValidate(command);

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
            var command = new UpdateTicketStatusCommand
            {
                TicketId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = role!,
                NewStatus = TicketStatus.Resolved
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Role)
                  .WithErrorMessage("Role can't be blank.");
        }
    }
}
