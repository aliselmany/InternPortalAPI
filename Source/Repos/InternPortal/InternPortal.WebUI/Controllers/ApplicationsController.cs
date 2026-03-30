using InternPortal.Application.Dtos;
using InternPortal.Application.Interfaces;
using InternPortal.Application.Common;
using InternPortal.Domain.Enums;
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
    public async Task<IActionResult> SubmitApplication([FromForm] ApplicationDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized(new { message = "User identity not found." });

        var userId = Guid.Parse(userIdClaim.Value);

        var result = await _applicationService.SubmitAsync(userId, dto);

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

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllApplications()
    {
        var applications = await _applicationService.GetAllAsync();
        return Ok(applications);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetApplicationById(Guid id)
    {
        var application = await _applicationService.GetByIdAsync(id);

        if (application == null)
            return NotFound(new { message = "Application not found." });

        return Ok(application);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] ApplicationStatus status)
    {
        var result = await _applicationService.UpdateStatusAsync(id, status);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = $"Application status updated to {status}." });
    }
}