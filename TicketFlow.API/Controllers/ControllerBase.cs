using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TicketFlow.API.Controllers
{
    /// <include file='../Docs/ControllerBaseDocs.xml' path='docs/members[@name="CustomControllerBase"]/Class/*' />
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CustomControllerBase : ControllerBase
    {
        /// <include file='../Docs/ControllerBaseDocs.xml' path='docs/members[@name="CustomControllerBase"]/FieldMediator/*' />
        protected readonly IMediator _mediator;

        /// <include file='../Docs/ControllerBaseDocs.xml' path='docs/members[@name="CustomControllerBase"]/Constructor/*' />
        public CustomControllerBase(IMediator mediator)
        {
            _mediator = mediator;
        }
    }
}
