using SecretSanta.Interfaces;
using SecretSanta.Data.Models;

namespace SecretSanta.Tests.Repos;

public class UserRepositoryFake : IUserRepository {

    ApplicationUser? user = null;

    public void FakeSetUser(string[] emails, IEnumerable<Campaign>? campaigns = null){
        campaigns ??= Array.Empty<Campaign>();//because can't create compile-time constants of object references
        if(emails.Length==0){
            user = null;
            return;
        }
        user = new ApplicationUser{
            Emails = new List<Email>()
        };
        foreach(string emailaddress in emails){
            user.Emails.Add(new Email{
                Address = emailaddress,
                Id = 0,
                CampaignMembers = new List<CampaignMember>()
            });
        }
        foreach(Campaign campaign in campaigns){
            //foreach, check if in?, unnecessary.
            int i = 0;//(not uint?)
            user.Emails[i].CampaignMembers!
                .Add(new CampaignMember{
                    Email = user.Emails[i],
                    Campaign = campaign
                });
        }
    }

    public Task<ApplicationUser> getApplicationUserWithEmailsAsync(bool ignored){
        if(user==null){
            throw new Exception("Not logged in");
        }
        return Task.FromResult<ApplicationUser>(user);
    }

    public Task<ApplicationUser> getApplicationUserAsync(){
        throw new NotImplementedException();
    }

    public Task UpdateApplicationUserAsync(ApplicationUser user){
        throw new NotImplementedException();
    }

}

