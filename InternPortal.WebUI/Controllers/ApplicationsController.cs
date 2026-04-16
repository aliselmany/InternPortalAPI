using InternPortal.Application.Dtos;
using InternPortal.Application.Interfaces;
using InternPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternPortal.WebUI.Controllers
{
    [Authorize]
    // DİKKAT: Buradaki [Route] etiketini TAMAMEN SİLDİK. 
    // .NET'in kendi standart yönlendirmesine bırakıyoruz.
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

            // return View();
            return Content("çalışıyor");
        }

        [HttpGet]
        public async Task<IActionResult> MyApplication()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Account");

            var userId = Guid.Parse(userIdClaim.Value);
            var applications = await _applicationService.GetByUserIdAsync(userId);
            var myApp = applications?.FirstOrDefault();

            if (myApp == null) return RedirectToAction("Submit");

            return View(myApp);
        }

        

        [HttpPost("Applications/api/submit")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitApplication([FromForm] ApplicationDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized(new { message = "User identity not found." });

            var userId = Guid.Parse(userIdClaim.Value);

            try
            {
                if (dto.CvFile != null) dto.CvPath = await SaveFileAsync(dto.CvFile, "cvs");
                if (dto.TranscriptFile != null) dto.TranscriptPath = await SaveFileAsync(dto.TranscriptFile, "transcripts");

                var result = await _applicationService.SubmitAsync(userId, dto);
                if (!result.IsSuccess) return BadRequest(new { message = result.Message });

                return Ok(new { message = "Application submitted successfully.", id = result.Data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hata oluştu", error = ex.Message });
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

        [Authorize(Roles = "Admin")]
        [HttpGet("Applications/api/all")]
        public async Task<IActionResult> GetAllApplications()
        {
            var result = await _applicationService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("Applications/api/{id}")]
        public async Task<IActionResult> GetApplicationById(Guid id)
        {
            var result = await _applicationService.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Başvuru bulunamadı." });
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
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

            using (var stream = new FileStream(filePath, FileMode.Create)) { await file.CopyToAsync(stream); }
            return $"/uploads/{subFolder}/{fileName}";
        }
    }
}