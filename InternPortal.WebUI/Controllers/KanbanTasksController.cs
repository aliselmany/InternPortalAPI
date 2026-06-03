using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternPortal.Domain.Entities;
using InternPortal.Application.Dtos;
using InternPortal.Infrastructure.Persistence;
using System.Threading.Tasks;
using System.Linq;
using System;

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.KanbanTasks.FindAsync(id);
            if (task == null) return NotFound();

            _context.KanbanTasks.Remove(task);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    public record TaskDescriptionUpdateModel(string Description);
}