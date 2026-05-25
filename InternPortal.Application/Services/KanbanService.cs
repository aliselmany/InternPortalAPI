using InternPortal.Application.Dtos;
using InternPortal.Application.Interfaces;
using InternPortal.Domain.Entities;
using InternPortal.Infrastructure.Persistence; // AppDbContext yolun
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternPortal.Application.Services
{
    public class KanbanTaskService : IKanbanTaskService
    {
        private readonly AppDbContext _context;

        public KanbanTaskService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<KanbanTask>> GetTasksByInternAsync(Guid internId)
        {
            return await _context.KanbanTasks
                .Where(t => t.InternId == internId)
                .OrderBy(t => t.Status)
                .ThenBy(t => t.OrderIndex)
                .ToListAsync();
        }

        public async Task<KanbanTask> CreateTaskAsync(CreateKanbanTaskDto dto)
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

            return newTask;
        }

        public async Task<bool> MoveTaskAsync(MoveKanbanTaskDto dto)
        {
            var task = await _context.KanbanTasks.FindAsync(dto.TaskId);
            if (task == null) return false;

            task.Status = dto.NewStatus;
            task.OrderIndex = dto.NewOrderIndex;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateTaskAsync(int id, UpdateKanbanTaskDto dto)
        {
            var task = await _context.KanbanTasks.FindAsync(id);
            if (task == null) return false;

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.DueDate = dto.DueDate;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.KanbanTasks.FindAsync(id);
            if (task == null) return false;

            _context.KanbanTasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}