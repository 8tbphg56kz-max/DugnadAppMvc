using Microsoft.AspNetCore.Identity;

namespace DugnadAppMvc.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public bool IsActivated { get; set; } = false;

        public DateTime? ActivatedDate { get; set; }       
    }
}
