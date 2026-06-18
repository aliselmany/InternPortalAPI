using InternPortal.Application.Dtos;
using InternPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InternPortal.WebUI.Controllers; 

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff")] 
public class KanbanTemplatesController : ControllerBase
{
    private readonly IKanbanTemplateService _templateService;

    public KanbanTemplatesController(IKanbanTemplateService templateService)
    {
        _templateService = templateService;
    }

  
    [HttpPost("from-board")]
    public async Task<IActionResult> CreateFromBoard([FromBody] CreateTemplateRequestDto dto)
    {
        var staffId = GetCurrentUserId();
        if (staffId == Guid.Empty) return Unauthorized(new { message = "Yetkisiz erişim." });

        var result = await _templateService.CreateTemplateFromBoardAsync(staffId, dto);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = "Pano başarıyla taslak olarak kaydedildi." });
    }

  
    [HttpGet("my-templates")]
    public async Task<IActionResult> GetMyTemplates()
    {
        var staffId = GetCurrentUserId();
        if (staffId == Guid.Empty) return Unauthorized(new { message = "Yetkisiz erişim." });

        var result = await _templateService.GetMyTemplatesAsync(staffId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

  
    [HttpPost("apply")]
    public async Task<IActionResult> ApplyTemplate([FromBody] ApplyTemplateRequestDto dto)
    {
        var staffId = GetCurrentUserId();
        if (staffId == Guid.Empty) return Unauthorized(new { message = "Yetkisiz erişim." });

        var result = await _templateService.ApplyTemplateToInternAsync(staffId, dto);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = "Taslaktaki tüm görevler stajyerin panosuna başarıyla uygulandı." });
    }

   
    [HttpDelete("{templateId}")]
    public async Task<IActionResult> DeleteTemplate(Guid templateId)
    {
        var staffId = GetCurrentUserId();
        if (staffId == Guid.Empty) return Unauthorized(new { message = "Yetkisiz erişim." });

        var result = await _templateService.DeleteTemplateAsync(staffId, templateId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = "Taslak başarıyla silindi." });
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out Guid id) ? id : Guid.Empty;
    }
}