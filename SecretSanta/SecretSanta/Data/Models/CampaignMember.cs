using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecretSanta.Data.Models;

[PrimaryKey(nameof(EmailId), nameof(CampaignId))]//compound key - EntityFrameworkCore
public class CampaignMember {

    public int? EmailId { get; set; }
    public required Email Email { get; set; }

    public int? CampaignId { get; set; }
    public Campaign? Campaign { get; set; }


    public bool? Accept { get; set; }
    public bool? Organiser { get; set; } = false;

    public bool Inactive { get; set; } = false;//todo - can be set by organiser to remove from draw. can be added back by user.

    public string? DisplayName { get; set; }//maybe allow organisers to set before accepted, but don't send to anyone except orgs?
    

    public int? DisplayEmailId { get; set; }
    public Email? DisplayEmail { get; set; }


    //could make these campaignmember refs?
    public int? ChosenEmailId { get; set; }//a link to the Email of the person who has been chosen for this person - as a reference, not to be sent - send their displayemail.? how?
    public Email? ChosenEmail { get; set; }

    public int? VerifiedByEmailId { get; set; }
    public Email? VerifiedByEmail { get; set; }

    public DateTime? Invitationsent { get; set; }


    //prefer/don't?

    //modified history?

}

