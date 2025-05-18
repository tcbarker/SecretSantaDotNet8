using Microsoft.AspNetCore.Identity;//UserManager<
using SecretSanta.Data.Models;
using SecretSanta.Interfaces;

namespace SecretSanta.Data;

public class UserRepository : IUserRepository {

    private readonly System.Security.Claims.ClaimsPrincipal? _claimsPrincipal;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager, ILogger<UserRepository> logger){
        _claimsPrincipal = httpContextAccessor.HttpContext?.User;
        _userManager = userManager;
        _logger = logger;
        _logger.LogTrace("UserRepository created.");
    }

    ~UserRepository(){
        _logger.LogTrace("UserRepository destroyed.");
    }

    public async Task<ApplicationUser> getApplicationUserAsync(){
        if(_claimsPrincipal==null){
            throw new Exception("Not logged in");
        }
        ApplicationUser? user = await _userManager.GetUserAsync(_claimsPrincipal);
        if(user==null){
            throw new Exception("Not logged in");
        }
        return user;
    }

    public async Task UpdateApplicationUserAsync(ApplicationUser user){
        await _userManager.UpdateAsync(user);
    }

}

