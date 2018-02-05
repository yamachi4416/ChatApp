using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(128)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(128)]
        public string LastName { get; set; }

        public Guid? UserAvatarId { get; set; }
    }
}
