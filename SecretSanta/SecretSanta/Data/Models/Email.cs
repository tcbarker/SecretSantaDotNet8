using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SecretSanta.Data.Models;

[Index(nameof(Address), IsUnique = true)]
public class Email {

    [Key]//Required
    public int? Id { get; set; }
    [MaxLength(80)]
    public required string Address { get; set; }

    public List<CampaignMember>? CampaignMembers { get; set; }

    public List<CampaignMember>? DisplayMembers { get; set; }
    public List<CampaignMember>? ChosenMembers { get; set; }
    public List<CampaignMember>? VerifiedByMembers { get; set; }

    public string? ApplicationUserId { get; set; }
}

