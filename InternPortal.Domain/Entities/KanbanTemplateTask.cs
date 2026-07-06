using System;

namespace InternPortal.Domain.Entities;

public class KanbanTemplateTask
{
    public Guid Id { get; set; }

    public Guid TemplateId { get; set; }
    public KanbanBoardTemplate Template { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    //TODO: Burası enum olacak. Enum folderına taşıyıp oradan çağır.
    //Statuses: Todo, InProgress, Under Review, Done
    public string Status { get; set; } = "ToDo";
}