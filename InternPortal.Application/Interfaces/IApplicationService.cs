using InternPortal.Application.Dtos;
using InternPortal.Application.Common;
using InternPortal.Domain.Enums;

namespace InternPortal.Application.Interfaces;

public interface IApplicationService
{
    Task<ServiceResult<Guid>> SubmitAsync(Guid userId, ApplicationDto dto);

    Task<List<ApplicationDto>> GetByUserIdAsync(Guid userId);

    Task<ApplicationDto?> GetByIdAsync(Guid id);

    Task<ServiceResult<bool>> UpdateStatusAsync(Guid applicationId, ApplicationStatus newStatus);

    Task<List<ApplicationDto>> GetAllAsync(ApplicationFilterQuery filter, Guid currentUserId);

    Task<ServiceResult<bool>> UpdateAsync(Guid id, ApplicationUpdateDto dto);

    Task<List<ApplicationDto>> GetPendingManagerApprovalsAsync(Department department);
}