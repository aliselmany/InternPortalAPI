using InternPortal.Application.Dtos;
using InternPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternPortal.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Intern")]
public class RolesController : ControllerBase
{
    private readonly IRolesService _rolesService;
    private readonly IUserService _userService;

    public RolesController(IRolesService rolesService, IUserService userService)
    {
        _rolesService = rolesService;
        _userService = userService;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllRoles()
    {
        var result = await _rolesService.GetAllRolesAsync();
        return Ok(result.Data);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
    {
        var result = await _rolesService.CreateRoleAsync(dto.RoleName);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = "Role created successfully." });
    }
    
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

    [HttpPut("users/{id}/update-name")]
    public async Task<IActionResult> UpdateUserName(Guid id, [FromBody] UpdateNameDto dto)
    {
        var result = await _rolesService.UpdateUserNameAsync(id, dto.FirstName, dto.LastName);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = "User name updated successfully." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var result = await _rolesService.DeleteRoleAsync(id);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = "Role deleted successfully." });
    }

    public class CreateRoleDto
    {
        public string RoleName { get; set; } = string.Empty;
    }

    public class UpdateRoleDto
    {
        public string RoleName { get; set; } = string.Empty;
    }
}