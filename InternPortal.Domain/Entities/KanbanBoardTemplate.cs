using System;
using System.Collections.Generic;

namespace InternPortal.Domain.Entities;

public class KanbanBoardTemplate
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid StaffId { get; set; }
    public User Staff { get; set; }

    public ICollection<KanbanTemplateTask> Tasks { get; set; } = new List<KanbanTemplateTask>();

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}