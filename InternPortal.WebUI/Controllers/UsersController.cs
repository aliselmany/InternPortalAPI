using InternPortal.Application.Common;
using InternPortal.Application.Dtos;
using InternPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions; 
namespace InternPortal.WebUI.Controllers;

[ApiController]
[Route("api/Users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMailService _mailService;

    private static readonly Regex PasswordPolicyRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");

    private const string PasswordPolicyErrorMessage = "Şifreniz kurallara uymuyor! Güvenliğiniz için şifreniz en az 8 karakter uzunluğunda olmalı; en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.";

    public UsersController(IUserService userService, IMailService mailService)
    {
        _userService = userService;
        _mailService = mailService;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyProfile()
    {
        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (currentUserIdClaim == null) return Unauthorized(new { message = "Yetkisiz erişim." });

        var userId = Guid.Parse(currentUserIdClaim.Value);
        var user = await _userService.UserByIdAsync(userId);

        if (user == null) return NotFound(new { message = "Kullanıcı bulunamadı." });

        return Ok(new
        {
            name = user.Name,
            surname = user.Surname,
            email = user.Email,
            phoneNumber = user.PhoneNumber
        });
    }

    [HttpPost("select-mentor")]
    [Authorize(Roles = "Intern")]
    public async Task<IActionResult> SelectMentor([FromBody] Guid mentorId)
    {
        var internIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (internIdClaim == null) return Unauthorized();

        var result = await _userService.SelectMentorAsync(Guid.Parse(internIdClaim.Value), mentorId);
        if (!result) return BadRequest(new { message = "Mentör seçimi başarısız oldu." });

        return Ok(new { message = "Mentör başarıyla atandı." });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto createUserDto)
    {
        if (string.IsNullOrEmpty(createUserDto.Password) || !PasswordPolicyRegex.IsMatch(createUserDto.Password))
        {
            return BadRequest(new { message = PasswordPolicyErrorMessage });
        }

        var result = await _userService.RegisterAsync(createUserDto);
        if (!result.IsSuccess) return BadRequest(new { message = result.Message });
        return Ok(new { message = "Registration successful" });
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto dto)
    {
        var result = await _userService.VerifyEmailAsync(dto.Email, dto.Code);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = "E-posta adresiniz başarıyla doğrulandı! Şimdi giriş yapabilirsiniz." });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var result = await _userService.LoginAsync(loginRequest);
        if (!result.IsSuccess) return Unauthorized(new { message = result.Message });
        return Ok(new { token = result.Data.Token, message = "Giriş başarılı" });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost("assign-mentor")]
    public async Task<IActionResult> AssignMentor([FromBody] AssignMentorRequest request)
    {
        var result = await _userService.AssignMentorAsync(request.InternId, request.MentorId);
        if (!result.IsSuccess) return BadRequest(new { message = result.Message });
        return Ok(new { message = "Mentor assigned successfully." });
    }

    [HttpGet("my-interns/{staffId}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetMyInterns(Guid staffId)
    {
        var interns = await _userService.GetMyInternsAsync(staffId);
        return Ok(interns);
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

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
        var userResult = await _userService.GetUserByEmailAsync(model.Email);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            return BadRequest(new { message = "Bu e-posta adresi sistemde kayıtlı değil." });
        }

        Random random = new Random();
        string resetCode = random.Next(100000, 999999).ToString();

        var saveResult = await _userService.SavePasswordResetCodeAsync(userResult.Data.Id, resetCode, DateTime.Now.AddMinutes(3));
        if (!saveResult.IsSuccess)
        {
            return BadRequest(new { message = "Onay kodu oluşturulurken bir teknik hata oluştu." });
        }

        _mailService.SendPasswordResetCode(model.Email, resetCode);

        return Ok(new { message = "6 haneli onay kodu e-posta adresinize gönderildi." });
    }

    [HttpPost("verify-reset-code")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeDto model)
    {
        var verifyResult = await _userService.VerifyResetCodeAsync(model.Email, model.Code);
        if (!verifyResult.IsSuccess)
        {
            return BadRequest(new { message = verifyResult.Message });
        }

        return Ok(new { message = "Onay kodu başarıyla doğrulandı." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        if (model.NewPassword != model.ConfirmPassword)
        {
            return BadRequest(new { message = "Girdiğiniz şifreler birbiriyle uyuşmuyor." });
        }

        if (string.IsNullOrEmpty(model.NewPassword) || !PasswordPolicyRegex.IsMatch(model.NewPassword))
        {
            return BadRequest(new { message = PasswordPolicyErrorMessage });
        }

        var verifyResult = await _userService.VerifyResetCodeAsync(model.Email, model.Code);
        if (!verifyResult.IsSuccess)
        {
            return BadRequest(new { message = "Doğrulama süresi dolmuş veya geçersiz işlem. Lütfen baştan başlayın." });
        }

        var userResult = await _userService.GetUserByEmailAsync(model.Email);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            return BadRequest(new { message = "Kullanıcı bulunamadı." });
        }

        var oldPasswordCheck = await _userService.CheckOldPasswordAsync(userResult.Data.Id, model.NewPassword);
        if (!oldPasswordCheck.IsSuccess)
        {
            return BadRequest(new { message = oldPasswordCheck.Message });
        }

        var updateResult = await _userService.UpdatePasswordAsync(userResult.Data.Id, model.NewPassword);
        if (!updateResult.IsSuccess)
        {
            return BadRequest(new { message = "Şifre güncellenirken bir hata oluştu." });
        }

        return Ok(new { message = "Şifreniz başarıyla güncellendi. Yeni şifrenizle giriş yapabilirsiniz." });
    }
}

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public class VerifyResetCodeDto
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class VerifyEmailRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}