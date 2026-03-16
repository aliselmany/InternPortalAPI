using InternPortal.Application.Dtos;
using InternPortal.Application.Interfaces;
using InternPortal.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternPortal.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationsController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpPost("submit")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubmitApplication([FromForm] ApplicationDto dto, IFormFile cvFile)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized(new { message = "User identity not found." });

        var userId = Guid.Parse(userIdClaim.Value);

        
        var result = await _applicationService.SubmitAsync(userId, dto, cvFile);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = "Application submitted successfully.", id = result.Data });
    }

    [HttpGet("my-applications")]
    public async Task<IActionResult> GetMyApplications()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        var userId = Guid.Parse(userIdClaim.Value);
        var applications = await _applicationService.GetByUserIdAsync(userId);

        return Ok(applications);
    }
}