using System.ComponentModel.DataAnnotations;

namespace MagicVilla_API.Models
{
    public class RefreshTokenLogic
    {
        [Key]
        public int UserId { get; set; } 

        public string UserName { get; set; }
        public int TokenId { get; set; }

        public string RefreshToke { get; set; }   

    }
}
