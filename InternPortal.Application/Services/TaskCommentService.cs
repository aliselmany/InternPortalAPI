using InternPortal.Application.DTOs;
using InternPortal.Application.Interfaces;
using InternPortal.Domain.Entities;
using InternPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InternPortal.Application.Services;

public class TaskCommentService : ITaskCommentService
{
    private readonly AppDbContext _context;

    public TaskCommentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CommentResponseDto> AddCommentAsync(CreateCommentDto commentDto)
    {
        var comment = new TaskComment
        {
            TaskId = commentDto.TaskId,
            UserId = commentDto.UserId,
            Content = commentDto.Content,
            CreatedDate = DateTime.Now
        };

        _context.TaskComments.Add(comment);
        await _context.SaveChangesAsync();
        var user = await _context.Users.FindAsync(commentDto.UserId);

        return new CommentResponseDto
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            UserId = comment.UserId,
            UserFullName = user != null ? $"{user.Name} {user.Surname}" : "Bilinmeyen Kullanıcı",
            Content = comment.Content,
            CreatedDate = comment.CreatedDate
        };
    }

    public async Task<List<CommentResponseDto>> GetCommentsByTaskIdAsync(int taskId)
    {
        
        return await _context.TaskComments
            .Where(tc => tc.TaskId == taskId)
            .Include(tc => tc.User) 
            .OrderBy(tc => tc.CreatedDate)
            .Select(tc => new CommentResponseDto
            {
                Id = tc.Id,
                TaskId = tc.TaskId,
                UserId = tc.UserId,
                UserFullName = $"{tc.User.Name} {tc.User.Surname}",
                Content = tc.Content,
                CreatedDate = tc.CreatedDate
            })
            .ToListAsync();
    }

    public async Task<bool> DeleteCommentAsync(int commentId, Guid userId)
    {
        var comment = await _context.TaskComments.FirstOrDefaultAsync(tc => tc.Id == commentId);

        if (comment == null) return false;
    
        if (comment.UserId != userId) return false;

        _context.TaskComments.Remove(comment);
        await _context.SaveChangesAsync();

        return true;
    }
}