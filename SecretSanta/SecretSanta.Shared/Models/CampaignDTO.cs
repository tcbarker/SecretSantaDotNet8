using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Shared.Models;

public class CampaignDTO {

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Campaign Name is required")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Welcome Message is required"), MaxLength(100, ErrorMessage = "Welcome message 100 characters max.")]
    public string WelcomeMessage { get; set; } = string.Empty;

    [DuplicateEmailsAttribute(ErrorMessage = "Cannot have duplicate/invalid emails.")]
    public CampaignMemberDTO[] Members { get; set; } = [];
    public bool JoinPublic { get; set; } = false;
    public bool RequireVerification { get; set; } = false;
    public bool OrganiserView {get; set; }
    public Guid? Guid { get; set; }

}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, 
                AllowMultiple = false)]
public class DuplicateEmailsAttribute: ValidationAttribute {
    public override bool IsValid(object? value) {
        CampaignMemberDTO[]? members = value as CampaignMemberDTO[];
        if(members==null){
            return false;
        }
        HashSet<string> storedmails = new HashSet<string>();
        foreach(CampaignMemberDTO mem in members){
            if(mem.Email!=null){//can have duplicate nulls for joining public.
                string thisemail = mem.Email.ToUpper();
                if(storedmails.Contains(thisemail)){
                    return false;
                }
                storedmails.Add(thisemail);
                if(!EmailHelper.IsValid(mem.Email)){
                    return false;
                }
            }
        }
        return true;
    }
}

