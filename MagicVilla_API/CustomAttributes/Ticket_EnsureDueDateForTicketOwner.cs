using MagicVilla_API.Models;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace MagicVilla_API.CustomAttributes
{
    public class Ticket_EnsureDueDateForTicketOwner:ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var ticket = validationContext.ObjectInstance as Ticket;
            if (ticket != null && !string.IsNullOrEmpty(ticket.Owner))
            {
                if (!ticket.DueDate.HasValue)
                    return new ValidationResult("Due date is required when having a owner");
            }

            if (ticket is not null && ticket.Id == 0)
            {
                if (ticket.DueDate.HasValue && ticket.DueDate.Value < DateTime.Now)
                    return new ValidationResult("Due date has to be in the future");
            }
            return ValidationResult.Success;
        }
    }
}
