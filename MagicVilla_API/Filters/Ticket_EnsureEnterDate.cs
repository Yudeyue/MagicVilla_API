using MagicVilla_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MagicVilla_API.Filters
{
    // action filter validation which validate new properties without adding error with exzisting database. and could short circuiting process
    public class Ticket_EnsureEnterDate:ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            var ticket = context.ActionArguments["ticket"] as Ticket;
            if (ticket != null && ticket.EnterDate.HasValue == false && !string.IsNullOrEmpty(ticket.Owner))
            {
                context.ModelState.AddModelError("EnterDate", "Enter date is required");
                context.Result = new BadRequestObjectResult(context.ModelState);
            }

            if (ticket != null && ticket.EnterDate.HasValue == true && string.IsNullOrEmpty(ticket.Owner) && ticket.DueDate.HasValue == true)
            {
                if (ticket.DueDate < ticket.EnterDate)
                {
                    context.ModelState.AddModelError("DueDate", "Due date must be later than enter date.");
                    context.Result = new BadRequestObjectResult(context.ModelState);
                }
            }
        }
    }
}
