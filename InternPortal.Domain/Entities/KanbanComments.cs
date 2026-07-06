using InternPortal.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class KanbanComment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TaskId { get; set; }

    [Required]
    public string UserId { get; set; } 

    [Required]
    //TODO: UserName'ı User entity'sinden alabiliriz.
    public string UserName { get; set; } 

    [Required]
    public string Text { get; set; } 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("TaskId")]
    public virtual KanbanTask KanbanTask { get; set; }

    public bool IsDeleted { get; set; } = false;

}