using InternPortal.Application.Interfaces;
using InternPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternPortal.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] 
public class RolesController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IApplicationService _applicationService;

    public RolesController(IUserService userService, IApplicationService applicationService)
    {
        _userService = userService;
        _applicationService = applicationService;
    }

    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] string role) 
    {
        var result = await _userService.UpdateRoleAsync(id, role);
        if (!result.IsSuccess) return BadRequest(result.Message);

        return Ok(new { message = $"User role updated to {role}." });
    }
}