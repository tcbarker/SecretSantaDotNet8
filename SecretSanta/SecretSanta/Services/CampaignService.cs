using SecretSanta.Interfaces;
using SecretSanta.Data.Models;
using SecretSanta.Shared.Interfaces;
using SecretSanta.Shared.Models;

namespace SecretSanta.Services;

public class CampaignService : ICampaignService {
    private readonly ICampaignRepository _campaignRepository;
    private readonly IEmailRepository _emailRepository;
    private readonly ILogger<CampaignService> _logger;

    public CampaignService(ICampaignRepository campaignRepository, IEmailRepository emailRepository, ILogger<CampaignService> logger){
        _campaignRepository = campaignRepository;
        _emailRepository = emailRepository;
        _logger = logger;
        _logger.LogTrace("Campaign service created.");
    }

    ~CampaignService(){
        _logger.LogTrace("Campaign service destroyed.");
    }

    CampaignDTO toCampaignDTO (Campaign fromthis, bool organiserview, string[] useremails, bool hidenames = false){
        if(fromthis.CampaignGuid==null){
            _logger.LogWarning($"Campaign {fromthis.Id} has No GUID");//create somewhere if not present? todo.
        }
        return new CampaignDTO{
            CreatedDate = fromthis.CreatedDate,
            Name = fromthis.Name,
            WelcomeMessage = fromthis.WelcomeMessage,
            Members  = fromthis.Members
                .Select(member =>new CampaignMemberDTO {
                    Email = organiserview?
                        member.Email.Address
                        :useremails.Contains(member.Email.Address)?
                            member.Email.Address
                            :null,
                    Organiser = member.Organiser,
                    DisplayEmail = hidenames&&member.Organiser==false?
                                    null
                                    :member.DisplayEmail?.Address,
                    DisplayName = hidenames?
                                    null
                                    :member.DisplayName,
                    Accept = member.Accept
                }).ToArray(),
            JoinPublic = fromthis.JoinPublic,
            RequireVerification = fromthis.RequireVerification,
            OrganiserView = organiserview,
            Guid = fromthis.CampaignGuid?.Id
        };
    }

    async Task<CampaignMember> fromCampaignMemberDTO (CampaignMemberDTO fromthis, bool allowedOrganiser, string[] useremails){
        if(fromthis.Email==null){
            throw new Exception("No Email in UserDTO");
        }
        return new CampaignMember{
            Email = await _emailRepository.getEmailAsync(fromthis.Email),
            Organiser = allowedOrganiser?
                fromthis.Organiser==true?
                    null
                    :fromthis.Organiser
                :false,
            Accept = useremails.Contains(fromthis.Email)?
                fromthis.Accept
                :null,
            DisplayName = useremails.Contains(fromthis.Email)?
                fromthis.DisplayName
                :null,
            DisplayEmail = useremails.Contains(fromthis.Email) &&
                            fromthis.DisplayEmail!=null?
                                await _emailRepository.getEmailAsync(fromthis.DisplayEmail)
                                :null
        };
    }

    async Task<Campaign> fromCampaignDTO (CampaignDTO fromthis, string[] useremails){     
        //var waitformembers = fromthis.Members.Select(member => fromCampaignMemberDTO(member,true,useremails) );
        //List<CampaignMember> Members = (await Task.WhenAll(waitformembers)).ToList();//A second operation was started on this context instance before a previous operation completed
        List<CampaignMember> Members = new List<CampaignMember>();
        foreach(CampaignMemberDTO member in fromthis.Members){
            Members.Add(await fromCampaignMemberDTO(member,true,useremails));
        }
        return new Campaign{
            Name = fromthis.Name,
            CreatedDate = DateTime.Now,
            WelcomeMessage = fromthis.WelcomeMessage,
            Members = Members,
            JoinPublic = fromthis.JoinPublic,
            RequireVerification = fromthis.RequireVerification,
            CampaignGuid = new CampaignGuid()
        };
    }

    public async Task<IEnumerable<CampaignDTO>> GetAllCampaignsAsync(){
        List<CampaignDTO> toreturn = new List<CampaignDTO>();
        foreach(Campaign campaign in await _campaignRepository.getAllDbCampaignsAsync()){
            toreturn.Add(toCampaignDTO(campaign,true,[]));
        }
        return toreturn;
    }

    public async Task<CampaignDTO> GetCampaignAsync(Guid guid){
        try {
            return toCampaignDTO(
                await _campaignRepository.getCampaignByGuidAsync(guid),
                true, []
            );
        } catch (Exception e) {
            throw new Exception("Get - Exception: "+e.Message);
        }
    }

    public Task<CampaignActionDTO> UpdateCampaignAsync(Guid guid, CampaignDTO updatedcampaigndto, string? action) {
        throw new NotImplementedException();
    }

    public async Task<CampaignActionDTO> CreateCampaignAsync(CampaignDTO newcampaigndto, string? action = null){
        string[] useremails = [action!];//test - actually get from logged in user
        Campaign? newcampaign = null;
        try {
            newcampaign = await fromCampaignDTO(newcampaigndto,useremails);
        } catch(Exception e) {
            throw new Exception("Create Campaign: "+e.Message);
        }
        List<CampaignMember> thesemembers = newcampaign.Members.Where(mem => useremails.Contains(mem.Email?.Address) ).ToList();
        if(thesemembers.Count==0){
            throw new Exception("Create Campaign - member not found (logged in?)");
        }

        _campaignRepository.AddCampaign(newcampaign);
        await _campaignRepository.SaveChangesAsync();
        return new CampaignActionDTO{
            Campaign = toCampaignDTO(newcampaign, true, useremails),
            Message = null
        };
    }

}

