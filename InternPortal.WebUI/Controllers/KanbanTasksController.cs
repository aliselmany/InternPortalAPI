using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternPortal.Domain.Entities;
using InternPortal.Application.Dtos;
using InternPortal.Infrastructure.Persistence;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Security.Claims;

namespace InternPortalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KanbanTasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KanbanTasksController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet("intern/{internId}")]
        public async Task<IActionResult> GetTasksByIntern(Guid internId)
        {
            var tasks = await _context.KanbanTasks
                .Where(t => t.InternId == internId)
                .OrderBy(t => t.Status)
                .ThenBy(t => t.OrderIndex)
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("{taskId}/comments")]
        public async Task<IActionResult> GetComments(int taskId)
        {
            var comments = await _context.KanbanComments
                .Where(c => c.TaskId == taskId && !c.IsDeleted) 
                .OrderBy(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.UserId, 
                    c.UserName,
                    c.Text,
                    CreatedAt = c.CreatedAt.ToString("HH:mm - dd.MM.yyyy")
                })
                .ToListAsync();

            return Ok(comments);
        }
         
        [HttpPut("move")]
        public async Task<IActionResult> MoveTask([FromBody] MoveKanbanTaskDto dto)
        {
            var task = await _context.KanbanTasks.FindAsync(dto.TaskId);
            if (task == null) return NotFound();

            task.Status = dto.NewStatus;
            task.OrderIndex = dto.NewOrderIndex;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateKanbanTaskDto dto)
        {
            var task = await _context.KanbanTasks.FindAsync(id);
            if (task == null) return NotFound();

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.DueDate = dto.DueDate;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}/description")]
        public async Task<IActionResult> UpdateTaskDescription(int id, [FromBody] TaskDescriptionUpdateModel model)
        {
            var task = await _context.KanbanTasks.FindAsync(id);
            if (task == null)
                return NotFound(new { message = "Görev bulunamadı!" });

            task.Description = model.Description;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Açıklama başarıyla güncellendi." });
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateKanbanTaskDto dto)
        {
            var lastOrder = await _context.KanbanTasks
                .Where(t => t.InternId == dto.InternId && t.Status == "ToDo")
                .MaxAsync(t => (int?)t.OrderIndex) ?? 0;

            var newTask = new KanbanTask
            {
                Title = dto.Title,
                InternId = dto.InternId,
                StaffId = dto.StaffId,
                Status = "ToDo",
                OrderIndex = lastOrder + 1,
                CreatedDate = DateTime.Now
            };

            _context.KanbanTasks.Add(newTask);
            await _context.SaveChangesAsync();

            return Ok(newTask);
        }

        [HttpPost("{taskId}/comments")]
        public async Task<IActionResult> AddComment(int taskId, [FromBody] CommentCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Text))
                return BadRequest(new { message = "Yorum boş olamaz." });

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized(new { message = "Kullanıcı kimliği doğrulanamadı." });

            string userName = "Kullanıcı";

            if (Guid.TryParse(userIdString, out Guid parsedUserId))
            {
                var user = await _context.Users.FindAsync(parsedUserId);
                if (user != null)
                {
                    userName = $"{user.Name} {user.Surname}";
                }
            }

            var comment = new KanbanComment
            {
                TaskId = taskId,
                UserId = userIdString,
                UserName = userName,
                Text = dto.Text,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            _context.KanbanComments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                comment.Id,
                comment.UserId,
                comment.UserName,
                comment.Text,
                CreatedAt = comment.CreatedAt.ToString("HH:mm")
            });
        }

        [HttpDelete("{id}")] 
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.KanbanTasks.FindAsync(id);
            if (task == null) return NotFound();

            
            task.IsDeleted = true;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comment = await _context.KanbanComments.FindAsync(commentId);

            if (comment == null || comment.IsDeleted)
                return NotFound(new { message = "Yorum bulunamadı." });

            if (comment.UserId != userId)
                return StatusCode(403, new { message = "Sadece kendi yorumunuzu silebilirsiniz." });

            comment.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Yorum başarıyla silindi." });
        }
    }

    public record TaskDescriptionUpdateModel(string Description);

    public class CommentCreateDto
    {
        public string Text { get; set; }
    }
}