using Microsoft.AspNetCore.Mvc;

namespace TicketFlow.API.Attributes
{
    /// <summary>
    /// Shorthand attribute for HTTP 400 with ValidationProblemDetails.
    /// </summary>
    public class ProducesValidationProblemAttribute()
        : ProducesResponseTypeAttribute(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest);
    /// <summary>
    /// Shorthand attribute for any HTTP status code with ProblemDetails.
    /// </summary>
    public class ProducesProblemAttribute(int statusCode)
        : ProducesResponseTypeAttribute(typeof(ProblemDetails), statusCode);
}
