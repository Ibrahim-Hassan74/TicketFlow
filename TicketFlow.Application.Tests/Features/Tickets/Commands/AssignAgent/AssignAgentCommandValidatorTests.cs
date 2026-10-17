using FluentValidation.TestHelper;
using TicketFlow.Application.Features.Tickets.Commands.AssignAgent;
using Xunit;

namespace TicketFlow.Application.Tests.Features.Tickets.Commands.AssignAgent
{
    public class AssignAgentCommandValidatorTests
    {
        private readonly AssignAgentCommandValidator _validator;

        public AssignAgentCommandValidatorTests()
        {
            _validator = new AssignAgentCommandValidator();
        }

        [Fact]
        public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var command = new AssignAgentCommand
            {
                TicketId = Guid.NewGuid(),
                AgentId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
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
            var command = new AssignAgentCommand
            {
                TicketId = Guid.Empty,
                AgentId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TicketId)
                  .WithErrorMessage("Ticket Id can't be blank.");
        }

        [Fact]
        public void Validate_WhenAgentIdIsEmpty_ShouldHaveValidationErrorForAgentId()
        {
            // Arrange
            var command = new AssignAgentCommand
            {
                TicketId = Guid.NewGuid(),
                AgentId = Guid.Empty,
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.AgentId)
                  .WithErrorMessage("Agent Id can't be blank.");
        }

        [Fact]
        public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationErrorForUserId()
        {
            // Arrange
            var command = new AssignAgentCommand
            {
                TicketId = Guid.NewGuid(),
                AgentId = Guid.NewGuid(),
                UserId = Guid.Empty
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("User Id can't be blank.");
        }
    }
}
