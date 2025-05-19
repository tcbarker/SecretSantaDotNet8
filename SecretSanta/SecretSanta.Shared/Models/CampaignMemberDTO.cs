using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Shared.Models;

public class CampaignMemberDTO {
    public string? Email { get; set; }//null if sent to normal user
    public bool? Organiser { get; set; } = false;
    public string? DisplayEmail { get; set; }
    public string? DisplayName { get; set; }
    public bool? Accept { get; set; } = null;

}

