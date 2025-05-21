using SecretSanta.Interfaces;
using SecretSanta.Data.Models;
using SecretSanta.Shared.Interfaces;
using SecretSanta.Shared.Models;

namespace SecretSanta.Services;

public class CampaignService : ICampaignService {
    private readonly ICampaignRepository _campaignRepository;
    private readonly IEmailRepository _emailRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<CampaignService> _logger;

    public CampaignService(ICampaignRepository campaignRepository, IEmailRepository emailRepository, IUserRepository userRepository, ILogger<CampaignService> logger){
        _campaignRepository = campaignRepository;
        _emailRepository = emailRepository;
        _userRepository = userRepository;
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

    void ValidateDisplayEmail(CampaignMember member, string[] useremails){
        if(member.DisplayEmail==null){
            if(member.Organiser==true){//could force joined members to do here, too?
                member.DisplayEmail = member.Email;//force
            }
        } else {
            if(!useremails.Contains(member.DisplayEmail.Address)){//todo
                member.DisplayEmail = member.Email;//force it
            }
        }
    }


    enum UserStatus {
        Absent,
        Present,
        Organiser
    }

    UserStatus userStatusInCampaign(List<CampaignMember> themembers, string[] useremails){
        UserStatus present = UserStatus.Absent;
        foreach(CampaignMember member in themembers.Where(mem => useremails.Contains(mem.Email?.Address) ).ToList() ){
            if(member.Organiser==true){
                return UserStatus.Organiser;
            }
            present = UserStatus.Present;
        }
        return present;
    }

    CampaignDTO getCampaignDTO(Campaign campaign, string[] useremails){
        UserStatus status = userStatusInCampaign(campaign.Members, useremails);
        if(status==UserStatus.Absent && campaign.JoinPublic==false){
            throw new Exception("Get campaign DTO - User not in, and it isn't public.");
        }
        return toCampaignDTO(campaign, status==UserStatus.Organiser, useremails, status==UserStatus.Absent);
    }

    public struct UserInfo {
        public string[] emails;
        public CampaignDTO[] campaigns;
    }

    public async Task<UserInfo> getCurrentUserInfoAsync(bool includecampaigns = false){
        UserInfo toreturn = new UserInfo{
            emails = [],
            campaigns = []
        };
        try {
            ApplicationUser appuser = await _userRepository.getApplicationUserAsync();
            toreturn.emails = appuser.Emails.Select(em => em.Address).ToArray();
            if(includecampaigns){
                List<Campaign> CampaignList = new List<Campaign>();//Distinct type?
                foreach( Email email in appuser.Emails){
                    if(email.CampaignMembers==null){
                        throw new Exception("email.CampaignMembers==null");
                    }
                    foreach(CampaignMember campaignMember in email.CampaignMembers){
                        if(campaignMember.Campaign==null){
                            throw new Exception("campaignMember.Campaign==null");
                        }
                        CampaignList.Add(campaignMember.Campaign);
                    }
                }
                //todo - maybe filter?
                toreturn.campaigns = CampaignList.Distinct().Select( campaign => getCampaignDTO(campaign, toreturn.emails) ).ToArray();
            }
        } catch (Exception e) {
            _logger.LogTrace("Exception: "+e.Message);
        }
        return toreturn;
    }

    public async Task<IEnumerable<CampaignDTO>> GetAllCampaignsAsync(){
        return (await getCurrentUserInfoAsync(true)).campaigns;
    }

    public async Task<CampaignDTO> GetCampaignAsync(Guid guid){
        try {
            return getCampaignDTO(await _campaignRepository.getCampaignByGuidAsync(guid),
                (await getCurrentUserInfoAsync()).emails
            );
        } catch (Exception e) {
            throw new Exception("Get - Exception: "+e.Message);
        }
    }

    public async Task<CampaignActionDTO> UpdateCampaignAsync(Guid guid, CampaignDTO updatedcampaigndto, string? action) {
        Campaign fromdb = await _campaignRepository.getCampaignByGuidAsync(guid);

        string[] useremails = (await getCurrentUserInfoAsync()).emails;

        List<CampaignMemberDTO> thesemembersdto = updatedcampaigndto.Members.Where(mem => useremails.Contains(mem.Email) ).ToList();
        if(thesemembersdto.Count==0){
            throw new Exception("Update - not logged in");
        }
        List<CampaignMember> thesemembers = fromdb.Members.Where(mem => useremails.Contains(mem.Email?.Address) ).ToList();
        
        //verified field in user - todo...?

        bool organiser = false;

        if(thesemembers.Count==0){
            if(fromdb.JoinPublic){
                foreach(CampaignMemberDTO newmemberdto in thesemembersdto){
                    CampaignMember newmember = (await fromCampaignMemberDTO(newmemberdto,false,useremails));
                    ValidateDisplayEmail(newmember,useremails);
                    fromdb.Members.Add(newmember);
                }
            } else {
                throw new Exception("Update - user not in this non-public campaign");
            }
        } else {
            foreach(CampaignMemberDTO updatedthismemberdto in thesemembersdto){
                CampaignMember? foundthismember = thesemembers.Find(mem => {return mem.Email?.Address == updatedthismemberdto.Email;});
                //and we don't care about members in the db that aren't in the dto update?
                if(foundthismember==null){
                    continue;//organiser can add them below...
                }

                if(foundthismember.Organiser==null){
                    foundthismember.Organiser = updatedthismemberdto.Organiser;
                }

                if(foundthismember.Accept!=true){
                    foundthismember.Accept = updatedthismemberdto.Accept;
                }

                if(foundthismember.Accept==true || foundthismember.Organiser==true){
                    foundthismember.DisplayName = updatedthismemberdto.DisplayName;
                    if(updatedthismemberdto.DisplayEmail!=null){
                        foundthismember.DisplayEmail = await _emailRepository.getEmailAsync(updatedthismemberdto.DisplayEmail);
                    }
                }
                ValidateDisplayEmail(foundthismember,useremails);

                if(foundthismember.Organiser==true){
                    organiser = true;
                }

            }
        }

        if(organiser){
            fromdb.Name = updatedcampaigndto.Name;
            fromdb.WelcomeMessage = updatedcampaigndto.WelcomeMessage;
            fromdb.JoinPublic = updatedcampaigndto.JoinPublic;
            fromdb.RequireVerification = updatedcampaigndto.RequireVerification;
            
            foreach (CampaignMemberDTO updatedmemberdto in updatedcampaigndto.Members){
                CampaignMember? memberindb = fromdb.Members.Find(//find or FirstOrDefault???
                    checkmember=> {return checkmember.Email?.Address==updatedmemberdto.Email;}
                );

                if(memberindb==null){
                    try {
                        fromdb.Members.Add(await fromCampaignMemberDTO(updatedmemberdto,true, useremails) );
                    } catch(Exception e) {
                        throw new Exception("Update Campaign: "+e.Message);
                    }
                } else {
                    //memberindb - check for differences we're allowed to update.
                    //not ourselves? issues? check.
                    if(memberindb.Organiser==false){
                        memberindb.Organiser = updatedmemberdto.Organiser;
                    }
                    //if(updatedmember.Inactive == true)? todo?
                }
            }
        }
        await _campaignRepository.SaveChangesAsync();
        return new CampaignActionDTO {
            Campaign = toCampaignDTO(fromdb,organiser,useremails),
            Message = null
        };
    }

    public async Task<CampaignActionDTO> CreateCampaignAsync(CampaignDTO newcampaigndto, string? action = null){
        string[] useremails = (await getCurrentUserInfoAsync()).emails;
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

        bool organiserfound = false;
        foreach (CampaignMember member in thesemembers){
            if(member.Organiser==true){
                organiserfound = true;
            }
            ValidateDisplayEmail(member,useremails);
        }

        if(!organiserfound){
            thesemembers[0].Organiser = true;//force
            ValidateDisplayEmail(thesemembers[0],useremails);
        }


        _campaignRepository.AddCampaign(newcampaign);
        await _campaignRepository.SaveChangesAsync();
        return new CampaignActionDTO{
            Campaign = toCampaignDTO(newcampaign, true, useremails),
            Message = null
        };
    }

}

