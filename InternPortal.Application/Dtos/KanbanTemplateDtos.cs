using System;

namespace InternPortal.Application.Dtos;

public class CreateTemplateRequestDto
{
    public string TemplateName { get; set; } = string.Empty;
    public Guid SourceInternId { get; set; } 
}

public class KanbanTemplateResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int TaskCount { get; set; }
}

public class ApplyTemplateRequestDto
{
    public Guid TemplateId { get; set; } 
    public Guid TargetInternId { get; set; } 
}