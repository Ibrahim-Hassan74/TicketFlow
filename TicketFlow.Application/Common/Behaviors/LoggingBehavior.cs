using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TicketFlow.Application.Common.Interfaces;

namespace TicketFlow.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
        private readonly ICurrentUserService _currentUserService;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, ICurrentUserService currentUserService)
        {
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _currentUserService.UserId;
            var role = _currentUserService.Role ?? "Unauthenticated";

            _logger.LogInformation("TicketFlow Request: Starting {RequestName} [User: {UserId} | Role: {Role}]", requestName, userId, role);

            var timer = new Stopwatch();

            timer.Start();

            var response = await next(cancellationToken);

            timer.Stop();

            var timeTaken = timer.ElapsedMilliseconds;

            if (timeTaken > 500)
            {
                _logger.LogWarning("TicketFlow Long Running Request: {RequestName} took {TimeTaken}ms. [User: {UserId}]",
                    requestName, timeTaken, userId);
            }
            else
            {
                _logger.LogInformation("TicketFlow Request: Completed {RequestName} in {TimeTaken}ms",
                    requestName, timeTaken);
            }

            return response;
        }
    }
}
