using System;

namespace InternPortal.Application.DTOs;

public class CommentResponseDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}