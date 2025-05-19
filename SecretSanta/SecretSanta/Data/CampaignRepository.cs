using SecretSanta.Interfaces;
using SecretSanta.Data.Models;
using Microsoft.EntityFrameworkCore;//DbSet
using System.Text.Json;//JsonSerializer

namespace SecretSanta.Data;//ApplicationDbContext

public class CampaignRepository : ICampaignRepository {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CampaignRepository> _logger;

    public CampaignRepository(ApplicationDbContext applicationDbContext, ILogger<CampaignRepository> logger){
        _dbContext = applicationDbContext;
        _logger = logger;
        _logger.LogTrace("CampaignRepository created.");
    }

    ~CampaignRepository(){
        _logger.LogTrace("CampaignRepository destroyed.");
    }

    public async Task SaveChangesAsync(){
        await _dbContext.SaveChangesAsync();
    }

    public void AddCampaign(Campaign campaign){
        _dbContext.Add(campaign);
    }

    public async Task<Campaign> getCampaignByGuidAsync(Guid guid){
        _logger.LogInformation("Getting campaign {guid.ToString()}", guid.ToString());
        Campaign? result =  await _dbContext.Campaigns
            .Include(cam => cam.Members)
                .ThenInclude(mem => mem.Email)
            .Include(cam => cam.Members)
                .ThenInclude(mem => mem.DisplayEmail)
            .Include(cam => cam.CampaignGuid)
            .FirstOrDefaultAsync(i => i.CampaignGuid!=null && i.CampaignGuid.Id == guid);        
        
        _logger.LogTrace("Campaign:\r {canyounotjustdestructuretheresultyourself}",
        JsonSerializer.Serialize(result, new JsonSerializerOptions{ ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles, WriteIndented = true }));
        
        if(result==null){
            throw new Exception("Campaign not found");
        }
        return result;
    }

    public Task<IEnumerable<Campaign>> getAllDbCampaignsAsync(){//test. not for production - remove todo.
        _logger.LogInformation("Getting ALL campaigns in db.");
        IEnumerable<Campaign> result = _dbContext.Campaigns
            .Include(cam => cam.Members)
                .ThenInclude(mem => mem.Email)
            .Include(cam => cam.Members)
                .ThenInclude(mem => mem.DisplayEmail)
            .Include(cam => cam.CampaignGuid)
            .ToList();
        return Task.FromResult<IEnumerable<Campaign>>(result);
    }

}

