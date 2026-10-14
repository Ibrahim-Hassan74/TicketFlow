using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.Application.Common.Exceptions;
using TicketFlow.Domain.Exceptions;

namespace TicketFlow.API.Exceptions
{
    public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var problem = exception switch
            {
                // 400 - FluentValidation (MediatR ValidationBehavior)
                ValidationException ex => new ValidationProblemDetails(
                    ex.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()))
                {
                    Title = "Validation Failed",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "One or more validation errors occurred."
                },

                // 400 - Identity failed with identity error list
                IdentityOperationException ex => new ProblemDetails
                {
                    Title = "Identity Error",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = ex.Message,
                    Extensions = { ["errors"] = ex.Errors }
                },

                // 400 - Domain Rule / Bad Request
                DomainValidationException or BadRequestException => new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = exception.Message
                },

                // 401 - Authentication Exceptions
                UnauthorizedException or InvalidCredentialsException
                    or InvalidTokenException or InvalidRefreshTokenException
                    or MissingTokenClaimException => new ProblemDetails
                    {
                        Title = "Unauthorized",
                        Status = StatusCodes.Status401Unauthorized,
                        Detail = exception.Message
                    },

                // 403 - Forbidden Exceptions
                ForbiddenException or AccountDeactivatedException
                    or AccountLockedException or LoginNotAllowedException => new ProblemDetails
                    {
                        Title = "Forbidden",
                        Status = StatusCodes.Status403Forbidden,
                        Detail = exception.Message
                    },

                // 404 - Not Found
                NotFoundException => new ProblemDetails
                {
                    Title = "Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = exception.Message
                },

                // 409 - Conflict
                ConflictException or EmailAlreadyExistsException => new ProblemDetails
                {
                    Title = "Conflict",
                    Status = StatusCodes.Status409Conflict,
                    Detail = exception.Message
                },

                // 500 - Unhandled Server Errors
                _ => new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An unexpected error occurred. Please contact support."
                }
            };

            // 2. Smart Logging
            if (problem.Status >= 500)
            {
                logger.LogError(exception, "Unhandled server error occurred: {Message}", exception.Message);
            }
            else
            {
                logger.LogWarning("Handled application exception ({StatusCode}): {Message}", problem.Status, problem.Detail);
            }

            // 3. Write Response
            httpContext.Response.StatusCode = problem.Status!.Value;

            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem,
                Exception = exception
            });

            return true;
        }
    }
}
