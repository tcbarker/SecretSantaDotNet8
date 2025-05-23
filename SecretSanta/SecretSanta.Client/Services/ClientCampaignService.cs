using System.Net.Http.Json;
using SecretSanta.Shared;
using SecretSanta.Shared.Models;
using SecretSanta.Shared.Interfaces;

namespace SecretSanta.Client.Services;

public class ClientCampaignService : ICampaignService {

    private readonly HttpClient _httpClient;//nuget Microsoft.AspNet.WebApi

    public ClientCampaignService(HttpClient httpClient){
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<CampaignDTO>> GetAllCampaignsAsync(){
        try {
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<CampaignDTO>>("/api/campaign");
            if(result!=null){
                return result;
            }
        }
        catch (HttpRequestException ex){
            Console.WriteLine(ex.Message);
        }
        return [];
    }

    public async Task<CampaignDTO> GetCampaignAsync(Guid guid){
        var result = await _httpClient.GetFromJsonAsync<CampaignDTO>("/api/campaign/"+guid.ToString());//throws HttpRequestException
        return result!;
    }

    public async Task<CampaignActionDTO> UpdateCampaignAsync(Guid guid, CampaignDTO updatedcampaigndto, string? action) {
        var result = await _httpClient.PutAsJsonAsync("/api/campaign/"+guid.ToString()+(action==null?"":"?action="+action),updatedcampaigndto);
        if(result.IsSuccessStatusCode){
            CampaignActionDTO? read = await result.Content.ReadFromJsonAsync<CampaignActionDTO>();
            if(read!=null){
                return read;//??
            }
        }
        throw new Exception("Update Campaign Fail: "+result.Content);
    }

    public async Task<CampaignActionDTO> CreateCampaignAsync(CampaignDTO newcampaigndto, string? action){
        var result = await _httpClient.PostAsJsonAsync("/api/campaign"+(action==null?"":"?action="+action), newcampaigndto);
        if(result.IsSuccessStatusCode){
            CampaignActionDTO? read = await result.Content.ReadFromJsonAsync<CampaignActionDTO>();
            if(read!=null){
                return read;//??
            }
        }
        throw new Exception("Create Campaign Fail: "+result.Content);
    }

}

