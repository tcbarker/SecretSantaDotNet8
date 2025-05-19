using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Shared.Models;

public class CampaignDTO {

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public string Name { get; set; } = string.Empty;
    public string WelcomeMessage { get; set; } = string.Empty;
    public CampaignMemberDTO[] Members { get; set; } = [];
    public bool JoinPublic { get; set; } = false;
    public bool RequireVerification { get; set; } = false;
    public bool OrganiserView {get; set; }
    public Guid? Guid { get; set; }

}

