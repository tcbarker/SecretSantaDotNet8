using SecretSanta.Data.Models;

namespace SecretSanta.Interfaces;

public interface ICampaignRepository {
    
    void AddCampaign(Campaign campaign);
    Task SaveChangesAsync();
    Task<Campaign> getCampaignByGuidAsync(Guid guid);
}

