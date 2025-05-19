using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Data.Models;

public class Campaign {

    //[Key]
    public int? Id { get; set; }
    public required DateTime CreatedDate { get; set; }
    [MinLength(10)]
    public required string Name { get; set; }
    public required string WelcomeMessage { get; set; }
    public required List<CampaignMember> Members { get; set; }
    public required bool JoinPublic { get; set; }
    public required bool RequireVerification { get; set; }
    public CampaignGuid? CampaignGuid { get; set; }

}

