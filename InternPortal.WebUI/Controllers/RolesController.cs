using InternPortal.Application.Dtos;
using InternPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternPortal.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
    private readonly IRolesService _rolesService;
    private readonly IApplicationService _applicationService;

    public RolesController(IRolesService rolesService, IApplicationService applicationService)
    {
        _rolesService = rolesService;
        _applicationService = applicationService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromQuery] string roleName)
    {
        
        var result = await _rolesService.UpdateUserRoleAsync(id, roleName);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = $"User role updated to {roleName} successfully." });
    }
}

public class UpdateRoleDto
{
    public string RoleName { get; set; } = string.Empty;
}