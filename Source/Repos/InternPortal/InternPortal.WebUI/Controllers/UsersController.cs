using InternPortal.Application.Dtos;
using InternPortal.Application.Interfaces; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {

        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(CreateUserDto createUserDto)
    {
        var result = await _userService.RegisterAsync(createUserDto);

        if (!result.IsSuccess)
        {

            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = "Registration successful" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        var result = await _userService.LoginAsync(loginRequest);

        if (!result.IsSuccess)
        {
         return Unauthorized(new { message = result.Message });
        }

         return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var success = await _userService.DeleteUserAsync(id);
        if (!success)
        {
            return NotFound(new { message = "User not found" });    
        }
        return Ok(new { message = "User deleted successfully" });

    }
}