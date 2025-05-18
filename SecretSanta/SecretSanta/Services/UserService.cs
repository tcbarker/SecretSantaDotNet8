using SecretSanta.Data;//ApplicationDbContext
using SecretSanta.Interfaces;
using SecretSanta.Data.Models;
using Microsoft.EntityFrameworkCore;//DbSet

namespace SecretSanta.Services;

public class UserService : IUserService {
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IEmailRepository _emailRepository;
    
    public UserService(IUserRepository userRepository, IEmailRepository emailRepository, ILogger<UserService> logger){
        _userRepository = userRepository;
        _emailRepository = emailRepository;
        _logger = logger;
        _logger.LogTrace("UserService created.");
    }

    ~UserService(){
        _logger.LogTrace("UserService destroyed.");
    }

    public async Task<ApplicationUser> AddEmailToCurrentUserAsync(string mail){
        ApplicationUser appuser = await _userRepository.getApplicationUserAsync();
        Email Email = await _emailRepository.getEmailAsync(mail);
        if(Email.ApplicationUserId==null){
            appuser.Emails.Add(Email);
            await _userRepository.UpdateApplicationUserAsync(appuser);
        } else {
            _logger.LogWarning("email already linked - not linking.");
        }
        return appuser;
    }

}

