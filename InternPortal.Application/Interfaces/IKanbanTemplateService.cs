using InternPortal.Application.Common;
using InternPortal.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternPortal.Application.Interfaces;

public interface IKanbanTemplateService
{
    
    Task<ServiceResult> CreateTemplateFromBoardAsync(Guid staffId, CreateTemplateRequestDto dto);

  
    Task<ServiceResult<List<KanbanTemplateResponseDto>>> GetMyTemplatesAsync(Guid staffId);


    Task<ServiceResult> ApplyTemplateToInternAsync(Guid staffId, ApplyTemplateRequestDto dto);

    Task<ServiceResult> DeleteTemplateAsync(Guid staffId, Guid templateId);
}