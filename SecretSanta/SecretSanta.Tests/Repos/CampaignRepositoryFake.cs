using SecretSanta.Interfaces;
using SecretSanta.Data.Models;

namespace SecretSanta.Tests.Repos;

public class CampaignRepositoryFake : ICampaignRepository {

    public Campaign? campaignindatabase = null;
    //public bool saved = false;

    public Task SaveChangesAsync(){
        //saved = true;
        return Task.CompletedTask;
    }

    public void AddCampaign(Campaign campaign){
        campaignindatabase = campaign;
    }

    public Task<Campaign> getCampaignByGuidAsync(Guid guid){
        if(campaignindatabase==null){
            throw new Exception("Campaign not found");
        }
        return Task.FromResult<Campaign>(campaignindatabase);
    }

}

