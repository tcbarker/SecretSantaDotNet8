using Microsoft.AspNetCore.Mvc;
using SecretSanta.Shared.Interfaces;
using SecretSanta.Shared.Models;
using SecretSanta.Shared;

namespace SecretSanta.Controllers;

[Route("api/campaign")]
[ApiController]
public class CampaignController : ControllerBase {

    private readonly ICampaignService _campaignService;
    private readonly ILogger<CampaignController> _logger;

    public CampaignController(ICampaignService campaignService, ILogger<CampaignController> logger){
        _campaignService = campaignService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CampaignDTO>>> GetAllCampaignsAsync(){
        return Ok(await _campaignService.GetAllCampaignsAsync());
    }

    [HttpGet("{guid}")]
    public async Task<ActionResult<CampaignDTO>> GetCampaignAsync(Guid guid){
        try{
            return Ok(await _campaignService.GetCampaignAsync(guid) );
        } catch (Exception e) {
            _logger.LogTrace("Campaign controller Get: "+e.Message);
            return Unauthorized();
        }
    }

    [HttpPut("{guid}")]
    public async Task<ActionResult<CampaignActionDTO>> UpdateCampaignAsync(Guid guid, CampaignDTO updatedcampaigndto, string? action) {
        try {
            return Ok(await _campaignService.UpdateCampaignAsync(guid,updatedcampaigndto, action) );
        } catch (Exception e) {
            _logger.LogTrace("Campaign controller Update: "+e.Message);
            return Unauthorized();
        }
    }

    [HttpPost]
    public async Task<ActionResult<CampaignActionDTO>> CreateCampaignAsync(CampaignDTO newcampaigndto, string? action){
        try {
            return Ok(await _campaignService.CreateCampaignAsync(newcampaigndto, action) );
        } catch (Exception e) {
            _logger.LogTrace("Campaign controller Create: "+e.Message);
            return Unauthorized(e.Message);
        }
    }

}

