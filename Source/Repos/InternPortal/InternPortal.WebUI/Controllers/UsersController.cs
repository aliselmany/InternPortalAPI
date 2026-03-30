using InternPortal.Application.Dtos;
using InternPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternPortal.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto createUserDto)
    {
        var result = await _userService.RegisterAsync(createUserDto);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = "Registration successful" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var result = await _userService.LoginAsync(loginRequest);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var success = await _userService.DeleteUserAsync(id);
        if (!success)
        {
            return NotFound(new { message = "User not found." });
        }
        return Ok(new { message = "User deleted successfully." });
    }


    [Authorize(Roles = "Staff,Admin")]
    [HttpPut("update-staff-profile")]
    public async Task<IActionResult> UpdateStaffProfile([FromBody] StaffProfileUpdateDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        var userId = Guid.Parse(userIdClaim.Value);
        var result = await _userService.UpdateStaffProfileAsync(userId, dto);

        if (!result)
            return BadRequest(new { message = "Profile could not be updated." });

        return Ok(new { message = "Profile updated successfully." });
    }

    [Authorize] 
    [HttpGet("available-mentors")]
    public async Task<IActionResult> GetAvailableMentors([FromQuery] string? expertise)
    {
        var mentors = await _userService.GetAvailableMentorsAsync(expertise);
        return Ok(mentors);
    }
        
    [Authorize(Roles = "Admin,User")]
    [HttpPost("assign-mentor")]
    public async Task<IActionResult> AssignMentor([FromBody] AssignMentorRequest request)
    {
        var result = await _userService.AssignMentorAsync(request.InternId, request.MentorId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = "Mentor assigned successfully." });
    }
}