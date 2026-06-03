using System;
using System.ComponentModel.DataAnnotations;

namespace InternPortal.Application.DTOs; 

public class CreateCommentDto
{
    [Required]
    public int TaskId { get; set; }

    [Required]
    public Guid UserId { get; set; } 

    [Required(ErrorMessage = "Yorum içeriği boş bırakılamaz.")]
    public string Content { get; set; } = string.Empty;
}