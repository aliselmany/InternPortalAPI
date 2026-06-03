using InternPortal.Application.DTOs;
using InternPortal.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InternPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TaskCommentsController : ControllerBase
{
    private readonly ITaskCommentService _commentService;

    public TaskCommentsController(ITaskCommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("task/{taskId}")]
    public async Task<IActionResult> GetCommentsByTaskId(int taskId)
    {
        var comments = await _commentService.GetCommentsByTaskIdAsync(taskId);
        return Ok(comments);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddComment([FromBody] CreateCommentDto commentDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _commentService.AddCommentAsync(commentDto);
        return Ok(result);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteComment(int id, [FromQuery] Guid userId)
    {

        var success = await _commentService.DeleteCommentAsync(id, userId);

        if (!success)
            return BadRequest(new { message = "Yorum silinemedi. Yorum bulunamadı ya da bunu silmeye yetkiniz yok!" });

        return Ok(new { message = "Yorum başarıyla silindi." });
    }
}