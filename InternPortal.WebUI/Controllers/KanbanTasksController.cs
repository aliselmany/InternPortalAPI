using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternPortal.Domain.Entities;
using InternPortal.Application.Dtos;
using InternPortal.Infrastructure.Persistence;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using InternPortalAPI.Hubs;

namespace InternPortalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KanbanTasksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<KanbanHub> _hubContext;

        public KanbanTasksController(AppDbContext context, IHubContext<KanbanHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost("comments/add-to-task/{taskId}")]
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

            var task = await _context.KanbanTasks.FindAsync(taskId);
            if (task != null)
            {
                await LogActivityAsync(task.Id, "Yorum Eklendi", $"{userName} göreve yeni bir yorum yaptı.");
                await _hubContext.Clients.Group(task.InternId.ToString()).SendAsync("BoardUpdated");
                await _hubContext.Clients.Group(task.InternId.ToString()).SendAsync("ReceiveNotification", $"{userName} bir yorum yaptı: {dto.Text}");
            }

            return Ok(new
            {
                comment.Id,
                comment.UserId,
                comment.UserName,
                comment.Text,
                CreatedAt = comment.CreatedAt.ToString("HH:mm")
            });
        }

        [HttpPost("create")]
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

            await LogActivityAsync(newTask.Id, "Görev Oluşturuldu", "Görev panoya eklendi.");

            await _hubContext.Clients.Group(newTask.InternId.ToString()).SendAsync("BoardUpdated");
            await _hubContext.Clients.Group(newTask.InternId.ToString()).SendAsync("ReceiveNotification", $"Panoya yeni bir görev eklendi: {dto.Title}");

            return Ok(newTask);
        }

        [HttpGet("by-intern/{internId}")]
        public async Task<IActionResult> GetTasksByIntern(Guid internId)
        {
            var tasks = await _context.KanbanTasks
                .Where(t => t.InternId == internId)
                .OrderBy(t => t.Status)
                .ThenBy(t => t.OrderIndex)
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("comments/by-task/{taskId}")]
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

        [HttpGet("{taskId}/logs")]
        public async Task<IActionResult> GetTaskLogs(int taskId)
        {
            var logs = await _context.TaskActivityLogs
                .Where(l => l.TaskId == taskId)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new
                {
                    l.UserName,
                    l.ActionType,
                    l.Details,
                    CreatedAt = l.CreatedAt.ToString("dd.MM.yyyy HH:mm")
                })
                .ToListAsync();

            return Ok(logs);
        }

        [HttpPut("move")]
        public async Task<IActionResult> MoveTask([FromBody] MoveKanbanTaskDto dto)
        {
            var task = await _context.KanbanTasks.FindAsync(dto.TaskId);
            if (task == null) return NotFound();

            task.Status = dto.NewStatus;
            task.OrderIndex = dto.NewOrderIndex;

            await _context.SaveChangesAsync();

            await LogActivityAsync(task.Id, "Durum Değişti", $"Görev yeni bir sütuna taşındı: {dto.NewStatus}");

            await _hubContext.Clients.Group(task.InternId.ToString()).SendAsync("BoardUpdated");
            await _hubContext.Clients.Group(task.InternId.ToString()).SendAsync("ReceiveNotification", $"Bir görev taşındı: Yeni Durum -> {dto.NewStatus}");

            return Ok();
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateKanbanTaskDto dto)
        {
            var task = await _context.KanbanTasks.FindAsync(id);
            if (task == null) return NotFound();

            task.Title = dto.Title;
            task.Description = dto.Description;

            await _context.SaveChangesAsync();

            await LogActivityAsync(task.Id, "Görev Güncellendi", "Görevin başlık veya detay bilgileri güncellendi.");

            await _hubContext.Clients.Group(task.InternId.ToString()).SendAsync("BoardUpdated");

            return Ok();
        }

        [HttpPut("update-description/{id}")]
        public async Task<IActionResult> UpdateTaskDescription(int id, [FromBody] TaskDescriptionUpdateModel model)
        {
            var task = await _context.KanbanTasks.FindAsync(id);
            if (task == null)
                return NotFound(new { message = "Görev bulunamadı!" });

            task.Description = model.Description;
            await _context.SaveChangesAsync();

            await LogActivityAsync(task.Id, "Açıklama Güncellendi", "Görevin açıklaması değiştirildi.");

            await _hubContext.Clients.Group(task.InternId.ToString()).SendAsync("BoardUpdated");

            return Ok(new { message = "Açıklama başarıyla güncellendi." });
        }

        [HttpDelete("comments/delete/{commentId}")]
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

            var task = await _context.KanbanTasks.FindAsync(comment.TaskId);
            if (task != null)
            {
           
                await LogActivityAsync(task.Id, "Yorum Silindi", "Bir yorum silindi.");
                await _hubContext.Clients.Group(task.InternId.ToString()).SendAsync("BoardUpdated");
            }

            return Ok(new { message = "Yorum başarıyla silindi." });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.KanbanTasks.FindAsync(id);
            if (task == null) return NotFound();

            task.IsDeleted = true;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(task.InternId.ToString()).SendAsync("BoardUpdated");

            return Ok();
        }

       
        private async Task LogActivityAsync(int taskId, string actionType, string details)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = "Sistem/Bilinmeyen";

            if (Guid.TryParse(userId, out Guid parsedUserId))
            {
                var user = await _context.Users.FindAsync(parsedUserId);
                if (user != null) userName = $"{user.Name} {user.Surname}";
            }

            var log = new TaskActivityLog
            {
                TaskId = taskId,
                UserId = userId,
                UserName = userName,
                ActionType = actionType,
                Details = details,
                CreatedAt = DateTime.Now
            };

            _context.TaskActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }

    public record TaskDescriptionUpdateModel(string Description);

    public class CommentCreateDto
    {
        public string Text { get; set; }
    }
}