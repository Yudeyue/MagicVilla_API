using System.ComponentModel.DataAnnotations;

namespace MagicVilla_API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }   
        public string Password { get; set; }
        public bool IsActive {  get; set; } 

        public string Role { get; set; }    
    }
}
