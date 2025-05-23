using Microsoft.AspNetCore.Components;
using SecretSanta.Shared.Models;
using SecretSanta.Shared.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Security.Claims;
using System.Text.Json;
using System.Reflection;
using SecretSanta.Shared;

namespace SecretSanta.Client.Pages;

public class CampaignBase : ComponentBase
{
    [Parameter]
    public Guid? Guid { get; set; }

    [SupplyParameterFromForm]
    protected CampaignDTO? CampaignDTO { get; set; } = null;

    protected Dictionary<string,CampaignMemberDTO?> MemberOnServer = new Dictionary<string, CampaignMemberDTO?>();

    protected string[] Currentuseremails = Array.Empty<string>();

    protected string? Information;

    [Inject]
    public required ICampaignService CampaignService { get; set; }//constructor one in 9.0

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [Inject]
    public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    async Task<IEnumerable<Claim>> GetClaims(){
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        return authState.User.Claims;
    }

    protected override async Task OnInitializedAsync() {//@attribute [StreamRendering] needed on component
        //can i do this just on login? higher up in page hierarchy? pass to page? (can that be done, or is component needed?)
        string[]? deserialized = null;
        var json = (await GetClaims()).FirstOrDefault(claim => claim.Type==ClaimTypes.UserData)?.Value;
        if(json!=null){
            deserialized = JsonSerializer.Deserialize<string[]>(json);
        }
        if(deserialized!=null){
            Currentuseremails = deserialized;
        }
        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if(Guid.HasValue){
            try {
                CampaignDTO = await CampaignService.GetCampaignAsync(Guid.Value);
                CreateDictionary();
            } catch (Exception e) {
                Information = "not found/unathorised - get campaign by guid "+e.Message;
                Console.WriteLine(Information);
            }
        } else {
            if(Currentuseremails.Length>0){
                NewCampaign();
            } else {
                Information = "Log in to create new campaign.";
                Console.WriteLine(Information);
            }
        }
        await base.OnParametersSetAsync();
    }

    /*protected override async Task OnAfterRenderAsync(bool firstRender){

    }*/

    protected async Task HandleSubmit(){
        if(CampaignDTO==null){
            return;//is this ok? todo.
        }
        if(Guid.HasValue){
            try {
                var response = await CampaignService.UpdateCampaignAsync(Guid.Value,CampaignDTO, null);
                Information = response.Message;
                CampaignDTO = response.Campaign;
                CreateDictionary();
            } catch (Exception e) {
                Information = "update unauthorised "+e.Message;
                Console.WriteLine(Information);
            }
        } else {
            try {
                var response = await CampaignService.CreateCampaignAsync(CampaignDTO, null);
                NavigationManager.NavigateTo(NavigationManager.Uri+"/"+response.Campaign.Guid);
                //throwing it away?
            } catch (Exception e) {
                Information = "create is unauthorised "+e.Message;
                Console.WriteLine(Information);
            }
        }
    }

    void CreateDictionary(){
        MemberOnServer = new Dictionary<string, CampaignMemberDTO?>();
        if(CampaignDTO==null){
            return;
        }
        foreach(CampaignMemberDTO memdto in CampaignDTO.Members){
            if(memdto.Email==null){
                continue;
            }
            MemberOnServer.Add(memdto.Email, GetClone(memdto));
        }
    }

    protected bool IsUser(string email){
        return Currentuseremails.Contains(email);
    }

    protected void DeleteMember(CampaignMemberDTO member){
        if(CampaignDTO==null){
            throw new Exception("CampaignDTO==null");
        }
        //check dict? can't exist..
        List<CampaignMemberDTO> templist = new List<CampaignMemberDTO>(CampaignDTO.Members);
        if(templist.Remove(member)){
            CampaignDTO.Members = templist.ToArray();
        } else {
            Console.WriteLine("passed member not in??");
        }
    }

    protected bool AddMember(object fromthis){//int? index, string email = ""){
        if(CampaignDTO==null){
            throw new Exception("CampaignDTO==null");
        }
        string emailval =   (fromthis is int && Currentuseremails.Length>(int)fromthis)?Currentuseremails[(int)fromthis]:
                            (fromthis is string && fromthis!=null)? ((string)fromthis):
                            "";
        if(CampaignDTO.Members.FirstOrDefault( mem => mem.Email == emailval)!=null){
            return false;
        }
        List<CampaignMemberDTO> templist = new List<CampaignMemberDTO>(CampaignDTO.Members);
        templist.Add(new CampaignMemberDTO{
            Email = emailval
        });
        CampaignDTO.Members = templist.ToArray();
        return true;
    }

    void NewCampaign(int mailchoice = 0){
        CampaignDTO = new CampaignDTO{
            OrganiserView = true,
            Members = []
        };
        CreateDictionary();//clear
        AddMember(mailchoice);
        CampaignDTO.Members[0].Organiser=true;
    }



    protected static bool? GetNullableBool (object? args){
        if(args==null || string.IsNullOrEmpty(args.ToString())){
            return null;
        } else {
            bool.TryParse(args.ToString(), out var outval);
            return outval;
        }
    }

    public static T GetClone<T>(T toclone) where T : new(){//using System.Reflection;
        T thing = new T();
        PropertyInfo[] properties = typeof(T).GetProperties();
        foreach (PropertyInfo property in properties) {
            //Console.WriteLine(property.PropertyType);
            property.SetValue(thing, property.GetValue(toclone));
        }
        return thing;
    }

}

