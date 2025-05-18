using SecretSanta.Interfaces;
using SecretSanta.Data.Models;
using Microsoft.EntityFrameworkCore;//DbSet
using SecretSanta.Shared;//EmailHelper

namespace SecretSanta.Data;//ApplicationDbContext

public class EmailRepository : IEmailRepository {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<EmailRepository> _logger;

    public EmailRepository(ApplicationDbContext applicationDbContext, ILogger<EmailRepository> logger){
        _dbContext = applicationDbContext;
        _logger = logger;
        _logger.LogTrace("EmailRepository created.");
    }

    ~EmailRepository(){
        _logger.LogTrace("EmailRepository destroyed.");
    }

    public async Task SaveChangesAsync(){
        await _dbContext.SaveChangesAsync();
    }

    public void AddEmail(Email email){
        _dbContext.Add(email);
    }

    public async Task<Email> getEmailAsync(string emailaddress){
        Email? Email = await _dbContext.Emails.FirstOrDefaultAsync(i => i.Address == emailaddress);
        if(Email==null){
            if(!EmailHelper.IsValid(emailaddress)){
                throw new Exception("Invalid email address");
            }
            Email = new Email { Address = emailaddress };
            //_dbContext.Emails.Update(Email);//?
        }
        return Email;
    }

}

