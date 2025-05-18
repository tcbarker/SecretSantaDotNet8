using Microsoft.AspNetCore.Identity;

namespace SecretSanta.Data.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public List<Email> Emails { get; set; } = new List<Email>();
}

