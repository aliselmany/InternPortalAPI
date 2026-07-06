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
public class InternTransfersController : ControllerBase
{
    private readonly IInternTransferService _transferService;

    public InternTransfersController(IInternTransferService transferService)
    {
        _transferService = transferService;
    }
  
    [HttpPost("request-transfer")]
    [Authorize(Roles = "DepartmanAdmin,Staff")]
    public async Task<IActionResult> RequestTransfer([FromBody] CreateTransferRequestDto dto)
    {       
        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (currentUserIdClaim == null) return Unauthorized(new { message = "Yetkisiz erişim." });

        var fromStaffId = Guid.Parse(currentUserIdClaim.Value);

        var result = await _transferService.CreateTransferRequestAsync(fromStaffId, dto);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = "Stajyer devir talebiniz başarıyla oluşturuldu, Admin onayına gönderildi." });
    }

    [HttpPost("respond-transfer")]
    [Authorize(Roles = "DepartmanAdmin")]
    public async Task<IActionResult> RespondTransfer([FromBody] RespondTransferRequestDto dto)
    {
        var result = await _transferService.RespondToTransferRequestAsync(dto);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        string statusMessage = dto.IsApproved ? "onaylandı ve stajyer yeni mentörüne devredildi." : "reddedildi.";
        return Ok(new { message = $"Devir talebi başarıyla {statusMessage}" });
    }
    
    [HttpGet("pending-requests")]
    [Authorize(Roles = "DepartmanAdmin")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var result = await _transferService.GetPendingRequestsAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }
}