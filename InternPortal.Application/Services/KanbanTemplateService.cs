using InternPortal.Application.Common;
using InternPortal.Application.Dtos;
using InternPortal.Application.Interfaces;
using InternPortal.Domain.Entities;
using InternPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternPortal.Application.Services;

public class KanbanTemplateService : IKanbanTemplateService
{
    private readonly AppDbContext _context;

    public KanbanTemplateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult> CreateTemplateFromBoardAsync(Guid staffId, CreateTemplateRequestDto dto)
    {
        var tasks = await _context.KanbanTasks
            .Where(t => t.InternId == dto.SourceInternId && t.StaffId == staffId)
            .ToListAsync();

        if (!tasks.Any())
            return ServiceResult.Failure("Bu stajyerin panosunda taslak olarak kaydedilecek hiçbir görev bulunmuyor.");

        var template = new KanbanBoardTemplate
        {
            Name = dto.TemplateName,
            StaffId = staffId,
            CreatedAt = DateTime.Now
        };

        foreach (var task in tasks)
        {
            template.Tasks.Add(new KanbanTemplateTask
            {
                Title = task.Title,
              
                Description = task.Description ?? "",
                Status = task.Status
            });
        }

        _context.KanbanBoardTemplates.Add(template);
        await _context.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<List<KanbanTemplateResponseDto>>> GetMyTemplatesAsync(Guid staffId)
    {
        var templates = await _context.KanbanBoardTemplates
            .Include(t => t.Tasks)
            .Where(t => t.StaffId == staffId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new KanbanTemplateResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                CreatedAt = t.CreatedAt,
                TaskCount = t.Tasks.Count
            }).ToListAsync();

        return ServiceResult<List<KanbanTemplateResponseDto>>.Success(templates);
    }

    public async Task<ServiceResult> ApplyTemplateToInternAsync(Guid staffId, ApplyTemplateRequestDto dto)
    {
        var template = await _context.KanbanBoardTemplates
            .Include(t => t.Tasks)
            .FirstOrDefaultAsync(t => t.Id == dto.TemplateId && t.StaffId == staffId);

        if (template == null)
            return ServiceResult.Failure("Taslak bulunamadı.");

        var intern = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.TargetInternId && u.MentorId == staffId);
        if (intern == null)
            return ServiceResult.Failure("Hedef stajyer bulunamadı veya size ait değil.");

        var newTasks = template.Tasks.Select(tt => new KanbanTask
        {
            Title = tt.Title,
        
            Description = tt.Description ?? "",
            Status = tt.Status,
            InternId = dto.TargetInternId,
            StaffId = staffId
        }).ToList();

        _context.KanbanTasks.AddRange(newTasks);
        await _context.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteTemplateAsync(Guid staffId, Guid templateId)
    {
        var template = await _context.KanbanBoardTemplates
            .FirstOrDefaultAsync(t => t.Id == templateId && t.StaffId == staffId);

        if (template == null)
            return ServiceResult.Failure("Taslak bulunamadı veya silme yetkiniz yok.");

        _context.KanbanBoardTemplates.Remove(template);
        await _context.SaveChangesAsync();

        return ServiceResult.Success();
    }
}