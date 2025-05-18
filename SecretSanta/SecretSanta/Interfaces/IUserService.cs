using SecretSanta.Data.Models;

namespace SecretSanta.Interfaces;

public interface IUserService {
    
    Task<ApplicationUser> AddEmailToCurrentUserAsync(string mail);
    Task<string> createJWTAddNewEMailAsync(string newemail);
    Task<string> addUserEmailFromJWTAsync(string jwtoken);

}

