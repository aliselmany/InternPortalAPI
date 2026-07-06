using InternPortal.Application.Dtos;
using InternPortal.Application.Interfaces;
using InternPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternPortal.WebUI.Controllers
{
    [Authorize]
    public class ApplicationsController : Controller
    {
        private readonly IApplicationService _applicationService;
        private readonly IWebHostEnvironment _env;

        public ApplicationsController(IApplicationService applicationService, IWebHostEnvironment env)
        {
            _applicationService = applicationService;
            _env = env;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Submit()
        {
            return Redirect("https://localhost:7050/Applications/Submit");
        }

        [HttpGet]
        public async Task<IActionResult> MyApplication()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Redirect("https://localhost:7050/Account/Login");

            return Redirect("https://localhost:7050/Applications/MyApplication");
        }

        [HttpPost("Applications/api/submit")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitApplication([FromForm] ApplicationDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı." });

            var userId = Guid.Parse(userIdClaim.Value);

            try
            {
                if (dto.CvFile != null) dto.CvPath = await SaveFileAsync(dto.CvFile, "cvs");
                if (dto.TranscriptFile != null) dto.TranscriptPath = await SaveFileAsync(dto.TranscriptFile, "transcripts");

                var result = await _applicationService.SubmitAsync(userId, dto);
                if (!result.IsSuccess) return BadRequest(new { message = result.Message });

                return Ok(new { message = "Başvuru başarıyla kaydedildi.", id = result.Data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu tarafında bir hata oluştu", error = ex.Message });
            }
        }

        [HttpGet("Applications/api/my-applications")]
        public async Task<IActionResult> GetMyApplications()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            var result = await _applicationService.GetByUserIdAsync(userId);
            return Ok(result);
        }

        [Authorize(Roles = "DepartmanAdmin,HR")]
        [HttpGet("Applications/api/all")]
        public async Task<IActionResult> GetAllApplications([FromQuery] ApplicationFilterQuery filter)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var currentUserId = Guid.Parse(userIdClaim.Value);

            var result = await _applicationService.GetAllAsync(filter, currentUserId);
            return Ok(result);
        }

        [HttpGet("Applications/api/{id}")]
        public async Task<IActionResult> GetApplicationById(Guid id)
        {
            var result = await _applicationService.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Başvuru bulunamadı." });
            return Ok(result);
        }

        [Authorize(Roles = "DepartmanAdmin,HR")]
        [HttpPut("Applications/update/api/{id}")]
        public async Task<IActionResult> UpdateApplication(Guid id, [FromBody] ApplicationUpdateDto updateData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _applicationService.UpdateAsync(id, updateData);

                if (result.IsSuccess)
                    return Ok(new { message = "Başvuru bilgileri başarıyla güncellendi." });

                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Güncelleme sırasında bir hata oluştu.", error = ex.Message });
            }
        }

        [Authorize(Roles = "DepartmanAdmin,HR")]
        [HttpPut("Applications/api/{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] ApplicationStatus status)
        {
            var result = await _applicationService.UpdateStatusAsync(id, status);
            if (result.IsSuccess) return Ok(new { message = "Durum güncellendi." });
            return BadRequest(new { message = result.Message });
        }

        private async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            var folderPath = Path.Combine(_env.WebRootPath, "uploads", subFolder);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return $"/uploads/{subFolder}/{fileName}";
        }
    }
}