using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace mock_sns_core2.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(256)]
        [Required]
        public string ApplicationUserName { get; set; }
    }
}
