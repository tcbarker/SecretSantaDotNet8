using SecretSanta.Shared.Models;

namespace SecretSanta.Shared.Interfaces;

public interface ICampaignService {    
    Task<IEnumerable<CampaignDTO>> GetAllCampaignsAsync();//return Task, not declared async in interface
    Task<CampaignDTO> GetCampaignAsync(Guid guid);
    Task<CampaignActionDTO> UpdateCampaignAsync(Guid guid, CampaignDTO updatedcampaigndto, string? action);
    Task<CampaignActionDTO> CreateCampaignAsync(CampaignDTO newcampaigndto, string? action);

}

