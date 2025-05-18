using Microsoft.AspNetCore.Components;
using SecretSanta.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Components.Account.Pages.Manage;

public class LinkEmailBase : ComponentBase
{
    [Inject]
    public required IUserService UserService { get; set; }

    [Inject]
    public required IEmailSendService EmailSendService { get; set; }

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [SupplyParameterFromForm]
    protected InputModel Input { get; set; } = new InputModel();//must be initialised or EditForm Model won't work.

    protected string? message = null;
    protected string? accepturl = null;

    protected override async Task OnInitializedAsync() {
        
        await base.OnInitializedAsync();
    }

    /*protected override async Task OnAfterRenderAsync(bool firstRender){

    }*/

    protected async Task HandleSubmit(){
        try {
            accepturl = NavigationManager.BaseUri+"linkusingjwt/"+(await UserService.createJWTAddNewEMailAsync(Input.NewEmail) );
            await EmailSendService.SendEmail(Input.NewEmail,"Linking your email","Follow "+accepturl+" to link your email");//"with?"+UserService.
            message = "Email sent!";
        } catch (Exception e) {
            message = e.Message;
        }
    }

    protected sealed class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email to link")]
        public string NewEmail { get; set; } = "";
    }

}

