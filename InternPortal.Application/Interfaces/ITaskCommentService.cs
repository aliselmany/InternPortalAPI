using System;
using System.Collections.Generic;
using System.Text;
using InternPortal.Application.DTOs;

namespace InternPortal.Application.Interfaces;

public interface ITaskCommentService
{
    Task<CommentResponseDto> AddCommentAsync(CreateCommentDto commentDto);
    Task<List<CommentResponseDto>> GetCommentsByTaskIdAsync(int taskId);
    Task<bool> DeleteCommentAsync(int commentId, Guid userId);
}