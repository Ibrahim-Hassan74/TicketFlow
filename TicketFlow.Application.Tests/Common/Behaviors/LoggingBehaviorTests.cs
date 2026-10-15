using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using TicketFlow.Application.Common.Behaviors;
using TicketFlow.Application.Common.Interfaces;
using Xunit;

namespace TicketFlow.Application.Tests.Common.Behaviors
{
    public class LoggingBehaviorTests
    {
        public class SampleCommand : IRequest<int>
        {
            public int Data { get; set; }
        }

        private readonly Mock<ILogger<LoggingBehavior<SampleCommand, int>>> _loggerMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly LoggingBehavior<SampleCommand, int> _behavior;

        public LoggingBehaviorTests()
        {
            _loggerMock = new Mock<ILogger<LoggingBehavior<SampleCommand, int>>>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _behavior = new LoggingBehavior<SampleCommand, int>(_loggerMock.Object, _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task Handle_WhenRequestExecutesNormally_ShouldCallNextAndReturnResponse()
        {
            // Arrange
            _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());
            _currentUserServiceMock.Setup(s => s.Role).Returns("Admin");

            var command = new SampleCommand { Data = 42 };
            var nextCalled = false;
            Task<int> Next(CancellationToken _)
            {
                nextCalled = true;
                return Task.FromResult(42);
            }

            // Act
            var result = await _behavior.Handle(command, Next, CancellationToken.None);

            // Assert
            result.Should().Be(42);
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WhenUserIsUnauthenticated_ShouldLogUnauthenticatedRoleAndCallNext()
        {
            // Arrange
            _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.Empty);
            _currentUserServiceMock.Setup(s => s.Role).Returns((string)null!);

            var command = new SampleCommand { Data = 99 };
            var nextCalled = false;
            Task<int> Next(CancellationToken _)
            {
                nextCalled = true;
                return Task.FromResult(99);
            }

            // Act
            var result = await _behavior.Handle(command, Next, CancellationToken.None);

            // Assert
            result.Should().Be(99);
            nextCalled.Should().BeTrue();
        }
    }
}
