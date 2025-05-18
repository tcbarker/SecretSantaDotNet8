using SecretSanta.Data.Models;

namespace SecretSanta.Interfaces;

public interface IEmailRepository {
    
    Task<Email> getEmailAsync(string emailaddress);
    Task SaveChangesAsync();
    void AddEmail(Email email);

}

