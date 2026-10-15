using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TicketFlow.API.Exceptions;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Domain.Exceptions;
using Xunit;

namespace TicketFlow.API.Tests.Exceptions
{
    public class GlobalExceptionHandlerTests
    {
        private readonly Mock<IProblemDetailsService> _problemDetailsServiceMock;
        private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
        private readonly GlobalExceptionHandler _handler;

        public GlobalExceptionHandlerTests()
        {
            _problemDetailsServiceMock = new Mock<IProblemDetailsService>();
            _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
            _handler = new GlobalExceptionHandler(_problemDetailsServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task TryHandleAsync_WhenValidationException_SetsStatus400AndWritesValidationProblemDetails()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var failures = new List<ValidationFailure>
            {
                new("Title", "Title is required."),
                new("Title", "Title must be at least 5 characters."),
                new("Priority", "Invalid priority.")
            };
            var exception = new ValidationException(failures);

            ProblemDetailsContext? capturedContext = null;
            _problemDetailsServiceMock
                .Setup(p => p.WriteAsync(It.IsAny<ProblemDetailsContext>()))
                .Callback<ProblemDetailsContext>(ctx => capturedContext = ctx)
                .Returns(ValueTask.CompletedTask);

            // Act
            var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

            // Assert
            handled.Should().BeTrue();
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            capturedContext.Should().NotBeNull();
            capturedContext!.ProblemDetails.Should().BeOfType<ValidationProblemDetails>();

            var validationProblem = (ValidationProblemDetails)capturedContext.ProblemDetails;
            validationProblem.Title.Should().Be("Validation Failed");
            validationProblem.Status.Should().Be(StatusCodes.Status400BadRequest);
            validationProblem.Detail.Should().Be("One or more validation errors occurred.");
            validationProblem.Errors.Should().ContainKey("Title");
            validationProblem.Errors["Title"].Should().HaveCount(2);
            validationProblem.Errors.Should().ContainKey("Priority");
        }

        [Fact]
        public async Task TryHandleAsync_WhenIdentityOperationException_SetsStatus400AndExtensionErrors()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var identityErrors = new[] { "Password requires non-alphanumeric character.", "Password too short." };
            var exception = new IdentityOperationException("User creation failed.", identityErrors);

            ProblemDetailsContext? capturedContext = null;
            _problemDetailsServiceMock
                .Setup(p => p.WriteAsync(It.IsAny<ProblemDetailsContext>()))
                .Callback<ProblemDetailsContext>(ctx => capturedContext = ctx)
                .Returns(ValueTask.CompletedTask);

            // Act
            var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

            // Assert
            handled.Should().BeTrue();
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            capturedContext.Should().NotBeNull();
            capturedContext!.ProblemDetails.Title.Should().Be("Identity Error");
            capturedContext.ProblemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
            capturedContext.ProblemDetails.Detail.Should().Be("User creation failed.");
            capturedContext.ProblemDetails.Extensions.Should().ContainKey("errors");
            capturedContext.ProblemDetails.Extensions["errors"].Should().BeEquivalentTo(identityErrors);
        }

        [Theory]
        [InlineData("Domain rule broken", false)]
        [InlineData("Bad request payload", true)]
        public async Task TryHandleAsync_WhenDomainOrBadRequestException_SetsStatus400(string message, bool isBadRequest)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            Exception exception = isBadRequest
                ? new BadRequestException(message)
                : new DomainValidationException(message);

            ProblemDetailsContext? capturedContext = null;
            _problemDetailsServiceMock
                .Setup(p => p.WriteAsync(It.IsAny<ProblemDetailsContext>()))
                .Callback<ProblemDetailsContext>(ctx => capturedContext = ctx)
                .Returns(ValueTask.CompletedTask);

            // Act
            var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

            // Assert
            handled.Should().BeTrue();
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            capturedContext.Should().NotBeNull();
            capturedContext!.ProblemDetails.Title.Should().Be("Bad Request");
            capturedContext.ProblemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
            capturedContext.ProblemDetails.Detail.Should().Be(message);
        }

        public static IEnumerable<object[]> AuthenticationExceptions()
        {
            yield return new object[] { new UnauthorizedException("Unauthorized access.") };
            yield return new object[] { new InvalidCredentialsException("Invalid username or password.") };
            yield return new object[] { new InvalidTokenException("Token is malformed.") };
            yield return new object[] { new InvalidRefreshTokenException("Refresh token expired.") };
            yield return new object[] { new MissingTokenClaimException("Required claim is missing.") };
        }

        [Theory]
        [MemberData(nameof(AuthenticationExceptions))]
        public async Task TryHandleAsync_WhenAuthenticationExceptions_SetsStatus401(Exception exception)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            ProblemDetailsContext? capturedContext = null;
            _problemDetailsServiceMock
                .Setup(p => p.WriteAsync(It.IsAny<ProblemDetailsContext>()))
                .Callback<ProblemDetailsContext>(ctx => capturedContext = ctx)
                .Returns(ValueTask.CompletedTask);

            // Act
            var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

            // Assert
            handled.Should().BeTrue();
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            capturedContext.Should().NotBeNull();
            capturedContext!.ProblemDetails.Title.Should().Be("Unauthorized");
            capturedContext.ProblemDetails.Status.Should().Be(StatusCodes.Status401Unauthorized);
            capturedContext.ProblemDetails.Detail.Should().Be(exception.Message);
        }

        [Fact]
        public async Task TryHandleAsync_WhenAccountLockedException_SetsStatus423()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var exception = new AccountLockedException("Account is temporarily locked.");

            ProblemDetailsContext? capturedContext = null;
            _problemDetailsServiceMock
                .Setup(p => p.WriteAsync(It.IsAny<ProblemDetailsContext>()))
                .Callback<ProblemDetailsContext>(ctx => capturedContext = ctx)
                .Returns(ValueTask.CompletedTask);

            // Act
            var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

            // Assert
            handled.Should().BeTrue();
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status423Locked);
            capturedContext.Should().NotBeNull();
            capturedContext!.ProblemDetails.Title.Should().Be("Account Locked");
            capturedContext.ProblemDetails.Status.Should().Be(StatusCodes.Status423Locked);
            capturedContext.ProblemDetails.Detail.Should().Be("Account is temporarily locked.");
        }

        public static IEnumerable<object[]> ForbiddenExceptions()
        {
            yield return new object[] { new ForbiddenException("Access forbidden.") };
            yield return new object[] { new AccountDeactivatedException("Account has been deactivated.") };
            yield return new object[] { new LoginNotAllowedException("Login is not allowed.") };
        }

        [Theory]
        [MemberData(nameof(ForbiddenExceptions))]
        public async Task TryHandleAsync_WhenForbiddenExceptions_SetsStatus403(Exception exception)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            ProblemDetailsContext? capturedContext = null;
            _problemDetailsServiceMock
                .Setup(p => p.WriteAsync(It.IsAny<ProblemDetailsContext>()))
                .Callback<ProblemDetailsContext>(ctx => capturedContext = ctx)
                .Returns(ValueTask.CompletedTask);

            // Act
            var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

            // Assert
            handled.Should().BeTrue();
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
            capturedContext.Should().NotBeNull();
            capturedContext!.ProblemDetails.Title.Should().Be("Forbidden");
            capturedContext.ProblemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
            capturedContext.ProblemDetails.Detail.Should().Be(exception.Message);
        }

        [Fact]
        public async Task TryHandleAsync_WhenNotFoundException_SetsStatus404()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var exception = new NotFoundException("Ticket not found.");

            ProblemDetailsContext? capturedContext = null;
            _problemDetailsServiceMock
                .Setup(p => p.WriteAsync(It.IsAny<ProblemDetailsContext>()))
                .Callback<ProblemDetailsContext>(ctx => capturedContext = ctx)
                .Returns(ValueTask.CompletedTask);

            // Act
            var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

            // Assert
            handled.Should().BeTrue();
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            capturedContext.Should().NotBeNull();
            capturedContext!.ProblemDetails.Title.Should().Be("Not Found");
            capturedContext.ProblemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
            capturedContext.ProblemDetails.Detail.Should().Be("Ticket not found.");
        }

        public static IEnumerable<object[]> ConflictExceptions()
        {
            yield return new object[] { new ConflictException("Ticket was modified concurrently.") };
            yield return new object[] { new EmailAlreadyExistsException("Email is already taken.") };
        }

        [Theory]
        [MemberData(nameof(ConflictExceptions))]
        public async Task TryHandleAsync_WhenConflictExceptions_SetsStatus409(Exception exception)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            ProblemDetailsContext? capturedContext = null;
            _problemDetailsServiceMock
                .Setup(p => p.WriteAsync(It.IsAny<ProblemDetailsContext>()))
                .Callback<ProblemDetailsContext>(ctx => capturedContext = ctx)
                .Returns(ValueTask.CompletedTask);

            // Act
            var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

            // Assert
            handled.Should().BeTrue();
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
            capturedContext.Should().NotBeNull();
            capturedContext!.ProblemDetails.Title.Should().Be("Conflict");
            capturedContext.ProblemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
            capturedContext.ProblemDetails.Detail.Should().Be(exception.Message);
        }

        [Fact]
        public async Task TryHandleAsync_WhenUnhandledException_SetsStatus500AndLogsError()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var exception = new InvalidOperationException("Something blew up unexpectedly.");

            ProblemDetailsContext? capturedContext = null;
            _problemDetailsServiceMock
                .Setup(p => p.WriteAsync(It.IsAny<ProblemDetailsContext>()))
                .Callback<ProblemDetailsContext>(ctx => capturedContext = ctx)
                .Returns(ValueTask.CompletedTask);

            // Act
            var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

            // Assert
            handled.Should().BeTrue();
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            capturedContext.Should().NotBeNull();
            capturedContext!.ProblemDetails.Title.Should().Be("Internal Server Error");
            capturedContext.ProblemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);
            capturedContext.ProblemDetails.Detail.Should().Be("An unexpected error occurred. Please contact support.");

            // Verify error logging for >= 500 status
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
