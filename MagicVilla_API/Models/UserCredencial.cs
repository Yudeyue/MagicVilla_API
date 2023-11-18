using System.ComponentModel.DataAnnotations;

namespace MagicVilla_API.Models
{
    public class UserCredencial
    {
        [Key]
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
