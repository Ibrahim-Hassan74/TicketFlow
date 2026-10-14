using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TicketFlow.API.Controllers
{
    /// <summary>
    /// Base controller for the Support Ticket Management API, providing common functionality for all controllers.
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CustomControllerBase : ControllerBase
    {
        protected readonly IMediator _mediator;
        /// <summary>
        /// Constructor for CustomControllerBase, initializing the unit of work.
        /// </summary>

        public CustomControllerBase(IMediator mediator)
        {
            _mediator = mediator;
            // This constructor can be used to initialize common services or properties
            // that all derived controllers might need.
        }
    }
}
