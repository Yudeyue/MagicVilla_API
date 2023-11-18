using MagicVilla_API.CustomAttributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicVilla_API.Models
{
    public class Ticket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? EnterDate { get; set; } = DateTime.Now;
        public string? Owner {  get; set; }

        //[Ticket_EnsureDueDateForTicketOwner]
        public DateTime? DueDate { get; set; }

        public bool ValidateFutureDueDate()
        {
            if (Id != 0) return true;
            if (!DueDate.HasValue) return true;

            return (DueDate.Value > DateTime.Now);
        }

        public bool ValidateReportDatePresence()
        {
            if (string.IsNullOrEmpty(Owner)) return true;
            return false;
        }
    }
}
