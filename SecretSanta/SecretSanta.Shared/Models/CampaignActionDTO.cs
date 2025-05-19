using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Shared.Models;

public class CampaignActionDTO {
    //if these aren't properties (with get/set), or aren't public, JsonSerializer will not serialize this.
    public required CampaignDTO Campaign { get; set; }
    public string? Message { get; set; }//message about action error
}

