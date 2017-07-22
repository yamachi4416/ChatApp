using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class UserInfoViewModel
    {
        [Required]
        [MaxLength(30)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(30)]
        public string FirstName { get; set; }
    }
}