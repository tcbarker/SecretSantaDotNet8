using Microsoft.AspNetCore.Mvc;//ControllerBase
using SecretSanta.Data.Models;
using SecretSanta.Interfaces;

namespace SecretSanta.Controllers;

[Route("api/test")]
[ApiController]
public class TestController : ControllerBase {

    private readonly IUserService _userService;

    public TestController(IUserService userService){
        _userService = userService;
    }

    [HttpGet("addmail/{mail}")]//test - no validation that they have access to this address, not for production, remove. todo.
    public async Task<ActionResult<ApplicationUser>> testNoSecurityAddUserEMailsAsync(string mail){
        try {
            return Ok(await _userService.AddEmailToCurrentUserAsync(mail));
        } catch (Exception e){
            return Unauthorized(e.Message);
        }
    }

}

