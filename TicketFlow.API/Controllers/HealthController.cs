using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TicketFlow.API.Controllers
{
    /// <summary>
    /// Health and deployment verification endpoint.
    /// </summary>
    [ApiVersion(1.0)]
    public class HealthController : CustomControllerBase
    {
        public HealthController(IMediator mediator) : base(mediator)
        {
        }

        /// <summary>
        /// Checks the health status of the API and verifies deployment.
        /// </summary>
        /// <returns>Service health status and deployment metadata.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetHealth()
        {
            return Ok(new
            {
                status = "Healthy",
                service = "TicketFlow API",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                deployedAtUtc = DateTimeOffset.UtcNow,
                version = "1.0.0"
            });
        }
    }
}
