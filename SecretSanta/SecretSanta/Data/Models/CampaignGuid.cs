using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecretSanta.Data.Models;

public class CampaignGuid {

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid? Id { get; set; }
    public int? CampaignId { get; set; }
    public Campaign? Campaign { get; set; }

}

