using SecretSanta.Interfaces;
using SecretSanta.Data.Models;

namespace SecretSanta.Tests.Repos;

public class EmailRepositoryFake : IEmailRepository {

    public Task<Email> getEmailAsync(string emailaddress){
        return Task.FromResult<Email>(new Email {
            Address = emailaddress
        });
    }

    public void AddEmail(Email email){
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(){
        throw new NotImplementedException();
        //return Task.CompletedTask;
    }

}