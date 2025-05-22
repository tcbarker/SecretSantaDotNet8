using Microsoft.AspNetCore.Identity;//UserManager<
using Microsoft.EntityFrameworkCore;//DbSet
using SecretSanta.Data.Models;
using SecretSanta.Interfaces;
using System.Text.Json;//JsonSerializer

namespace SecretSanta.Data;

public class UserRepository : IUserRepository {

    private readonly System.Security.Claims.ClaimsPrincipal? _claimsPrincipal;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager, ApplicationDbContext applicationDbContext, ILogger<UserRepository> logger){
        _claimsPrincipal = httpContextAccessor.HttpContext?.User;
        _userManager = userManager;
        _dbContext = applicationDbContext;
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
        ApplicationUser? user = await _userManager.GetUserAsync(_claimsPrincipal);//doesn't .Include Emails...
        if(user==null){
            throw new Exception("Not logged in");
        }
        return user;
    }

    public async Task UpdateApplicationUserAsync(ApplicationUser user){
        await _userManager.UpdateAsync(user);
    }

    public async Task<ApplicationUser> getApplicationUserWithEmailsAsync(bool includecampaigns){
        _logger.LogInformation($"Getting current user with includecampaigns={includecampaigns}");
        ApplicationUser appuser = await getApplicationUserAsync();
        IQueryable<Email> findquery = _dbContext.Emails
                .Where(em => em.ApplicationUserId == appuser.Id);
        if(includecampaigns){

            //todo - need to .AsSplitQuery() - ? https://go.microsoft.com/fwlink/?linkid=2134277

            findquery = findquery
                .Include(em => em.CampaignMembers)!//CHECKNULL (s)
                    .ThenInclude(mem => mem.Campaign)!
                        .ThenInclude(cam => cam!.CampaignGuid)
                .Include(em => em.CampaignMembers)!
                    .ThenInclude(mem => mem.Campaign)!
                        .ThenInclude(cam => cam!.Members)
                            .ThenInclude(mem => mem.Email)
                .Include(em => em.CampaignMembers)!//really? only way to do it?
                    .ThenInclude(mem => mem.Campaign)!
                        .ThenInclude(cam => cam!.Members)
                            .ThenInclude(mem => mem.DisplayEmail);
        }
        appuser.Emails = await findquery.ToListAsync();//why necessary? todo.
        _logger.LogTrace("User:\r{user}",JsonSerializer.Serialize(appuser, new JsonSerializerOptions{ ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles, WriteIndented = true }));
        return appuser;
    }

}

