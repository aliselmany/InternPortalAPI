using InternPortal.Application.Dtos;
using InternPortal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternPortal.Application.Interfaces
{
    public interface IKanbanTaskService
    {
        Task<IEnumerable<KanbanTask>> GetTasksByInternAsync(Guid internId);
        Task<KanbanTask> CreateTaskAsync(CreateKanbanTaskDto dto);
        Task<bool> MoveTaskAsync(MoveKanbanTaskDto dto);
        Task<bool> UpdateTaskAsync(int id, UpdateKanbanTaskDto dto);
        Task<bool> DeleteTaskAsync(int id);
        
    }
}