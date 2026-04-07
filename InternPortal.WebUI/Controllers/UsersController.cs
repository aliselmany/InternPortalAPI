using InternPortal.Application.Common;
using InternPortal.Application.Dtos;
using InternPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace InternPortal.WebUI.Controllers;



[ApiController]

[Route("api/Users")]
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

        if (!result.IsSuccess) return BadRequest(new { message = result.Message });

        return Ok(new { message = "Registration successful" });

    }



    [HttpPost("login")]

    [AllowAnonymous]

    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)

    {

        var result = await _userService.LoginAsync(loginRequest);

        if (!result.IsSuccess) return Unauthorized(new { message = result.Message });

        return Ok(result.Data);

    }



    [Authorize(Roles = "Admin,User")]

    [HttpPost("assign-mentor")]

    public async Task<IActionResult> AssignMentor([FromBody] AssignMentorRequest request)

    {

        var result = await _userService.AssignMentorAsync(request.InternId, request.MentorId);

        if (!result.IsSuccess) return BadRequest(new { message = result.Message });

        return Ok(new { message = "Mentor assigned successfully." });

    }



    [Authorize(Roles = "Admin")]

    [HttpGet("all-users")]

    public async Task<IActionResult> GetAllUsers([FromQuery] GetUserFilterDto filter)

    {

        var users = await _userService.GetAllUsersAsync(filter);

        return Ok(users);

    }



    [AllowAnonymous]

    [HttpGet("available-mentors")]

    public async Task<IActionResult> GetAvailableMentors([FromQuery] GetAvailableMentorsDto filter)

    {

        var mentors = await _userService.GetAvailableMentorsAsync(filter);

        return Ok(mentors);

    }




    [HttpGet("get-by-id/{id:guid}")]

    public async Task<IActionResult> GetUserById(Guid id)

    {

        var user = await _userService.UserByIdAsync(id);

        if (user == null) return NotFound(new { message = "User not found." });

        return Ok(user);

    }




    [HttpGet("get-by-role/{roleIdentifier}")]

    public async Task<IActionResult> GetUsersByRole(string roleIdentifier)

    {

        if (Guid.TryParse(roleIdentifier, out Guid roleId))

        {

            var users = await _userService.UsersByRoleIdAsync(roleId);

            return Ok(users);

        }



        var usersByName = await _userService.GetUsersByRoleNameAsync(roleIdentifier);

        return Ok(usersByName);

    }



    [Authorize(Roles = "Admin,Staff")]

    [HttpPut("update-mentor-profile")]

    public async Task<IActionResult> MentorStaffProfile([FromBody] MentorProfileUpdateDto dto, [FromQuery] Guid? targetUserId = null)

    {

        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;



        if (currentUserIdClaim == null) return Unauthorized();



        Guid idToUpdate = (currentUserRole == "Admin" && targetUserId.HasValue)

        ? targetUserId.Value

        : Guid.Parse(currentUserIdClaim.Value);



        var result = await _userService.UpdateMentorProfileAsync(idToUpdate, dto);

        if (!result) return BadRequest(new { message = "Profile update failed." });



        return Ok(new { message = "Profile updated successfully." });

    }


    [HttpDelete("delete/{id}")]

    [Authorize(Roles = "Admin")]

    public async Task<IActionResult> DeleteUser(Guid id)

    {

        var success = await _userService.DeleteUserAsync(id);

        if (!success) return NotFound(new { message = "User not found." });

        return Ok(new { message = "User deleted successfully." });

    }

}