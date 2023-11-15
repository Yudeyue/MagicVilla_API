using MagicVilla_API.Filters;
using MagicVilla_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_API.Controllers
{
    [ApiController]
    [Route("api/Tickets")]
    [Version1DiscontinueResourceFilter] // Resource filter
    public class TicketsController : ControllerBase
    {
        [HttpPost]
        //default post endpoint
        public ActionResult<Ticket> PostV1([FromBody] Ticket ticket)
        {
            return Ok(ticket);
        }

        [HttpPost]
        [Route("/api/v2/tickets")]
        [Ticket_EnsureEnterDate]// action filter new added feature
        public ActionResult<Ticket> PostV2([FromBody] Ticket ticket)
        {
            return Ok(ticket);
        }
    }
}
