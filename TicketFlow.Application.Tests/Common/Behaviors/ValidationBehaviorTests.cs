using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using TicketFlow.Application.Common.Behaviors;
using Xunit;

namespace TicketFlow.Application.Tests.Common.Behaviors
{
    public class ValidationBehaviorTests
    {
        public class SampleRequest : IRequest<string>
        {
            public string Value { get; set; } = string.Empty;
        }

        [Fact]
        public async Task Handle_WhenNoValidators_ShouldInvokeNextAndReturnResponse()
        {
            // Arrange
            var validators = Enumerable.Empty<IValidator<SampleRequest>>();
            var behavior = new ValidationBehavior<SampleRequest, string>(validators);

            var request = new SampleRequest { Value = "test" };
            var nextCalled = false;
            Task<string> Next(CancellationToken _)
            {
                nextCalled = true;
                return Task.FromResult("Success");
            }

            // Act
            var result = await behavior.Handle(request, Next, CancellationToken.None);

            // Assert
            result.Should().Be("Success");
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WhenValidationPasses_ShouldInvokeNextAndReturnResponse()
        {
            // Arrange
            var validatorMock = new Mock<IValidator<SampleRequest>>();
            validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SampleRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var behavior = new ValidationBehavior<SampleRequest, string>(new[] { validatorMock.Object });

            var request = new SampleRequest { Value = "valid" };
            var nextCalled = false;
            Task<string> Next(CancellationToken _)
            {
                nextCalled = true;
                return Task.FromResult("Success");
            }

            // Act
            var result = await behavior.Handle(request, Next, CancellationToken.None);

            // Assert
            result.Should().Be("Success");
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WhenValidationFails_ShouldThrowValidationExceptionAndNotCallNext()
        {
            // Arrange
            var failure = new ValidationFailure("Value", "Value is invalid");
            var validatorMock = new Mock<IValidator<SampleRequest>>();
            validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SampleRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[] { failure }));

            var behavior = new ValidationBehavior<SampleRequest, string>(new[] { validatorMock.Object });

            var request = new SampleRequest { Value = "invalid" };
            var nextCalled = false;
            Task<string> Next(CancellationToken _)
            {
                nextCalled = true;
                return Task.FromResult("Success");
            }

            // Act
            Func<Task> act = async () => await behavior.Handle(request, Next, CancellationToken.None);

            // Assert
            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Should().Contain(f => f.PropertyName == "Value" && f.ErrorMessage == "Value is invalid");
            nextCalled.Should().BeFalse();
        }
    }
}
