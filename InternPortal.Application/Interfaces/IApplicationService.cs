using InternPortal.Application.Dtos;
using InternPortal.Application.Common;
using InternPortal.Domain.Enums;

namespace InternPortal.Application.Interfaces;

public interface IApplicationService
{
    Task<ServiceResult<Guid>> SubmitAsync(Guid userId, ApplicationDto dto);

    //TODO: ServiceResult dönmeli
    Task<List<ApplicationDto>> GetByUserIdAsync(Guid userId);

    //TODO: ServiceResult dönmeli
    Task<ApplicationDto?> GetByIdAsync(Guid id);

    Task<ServiceResult<bool>> UpdateStatusAsync(Guid applicationId, ApplicationStatus newStatus);

    //TODO: ServiceResult dönmeli
    Task<List<ApplicationDto>> GetAllAsync(ApplicationFilterQuery filter, Guid currentUserId);

    Task<ServiceResult<bool>> UpdateAsync(Guid id, ApplicationUpdateDto dto);

    //TODO: ServiceResult dönmeli
    Task<List<ApplicationDto>> GetPendingManagerApprovalsAsync(Department department);
}