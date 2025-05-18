using SecretSanta.Data.Models;

namespace SecretSanta.Interfaces;

public interface IUserRepository {
    
    Task<ApplicationUser> getApplicationUserAsync();
    Task UpdateApplicationUserAsync(ApplicationUser user);

}

