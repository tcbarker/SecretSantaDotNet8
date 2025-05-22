using SecretSanta.Data;//ApplicationDbContext
using SecretSanta.Interfaces;
using SecretSanta.Data.Models;
using Microsoft.EntityFrameworkCore;//DbSet

namespace SecretSanta.Services;

public class UserService : IUserService {
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IEmailRepository _emailRepository;
    private readonly JWTService _jWTService;
    
    public UserService(IUserRepository userRepository, IEmailRepository emailRepository, ILogger<UserService> logger, JWTService jWTService){
        _userRepository = userRepository;
        _emailRepository = emailRepository;
        _jWTService = jWTService;
        _logger = logger;
        _logger.LogTrace("UserService created.");
    }

    ~UserService(){
        _logger.LogTrace("UserService destroyed.");
    }

    public async Task<string[]> getUserEMailsAsync(){
        var getmails = new Func<Task<string[]>>( async () => {
            try {
                return (await _userRepository.getApplicationUserWithEmailsAsync(false)).Emails.Select(em => em.Address).ToArray();
            } catch {//(Exception e) {
                return Array.Empty<string>();
            }
        });
        //_currentUserEmails ??= await getmails();
        return await getmails();
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

    public async Task<string> createJWTAddNewEMailAsync(string newemail){
        return _jWTService.CreateJWTAddNewEMail(newemail, (await _userRepository.getApplicationUserAsync()).Id);
    }

    public async Task<string> addUserEmailFromJWTAsync(string jwtoken){
        string? result = _jWTService.ValidateJWTAddNewEMail(jwtoken, out var email, out var userid);
        if(result!=null){
            return result;
        }
        if(await linkEmail(email!,userid!)){
            return email+" has been successfully linked.";
        }
        return "Email cannot be linked";
    }

    public async Task<bool> linkEmail(string emailaddress, string applicationUserId ){
        Email Email = await _emailRepository.getEmailAsync(emailaddress);
        if(Email.ApplicationUserId!=null){
            return false;//already linked
        }
        Email.ApplicationUserId = applicationUserId;
        if(Email.Id==null){
            _emailRepository.AddEmail(Email);
        }
        await _emailRepository.SaveChangesAsync();
        if(Email.Id==null){
            throw new Exception("email can't be added?");
        }
        return true;
    }

}

