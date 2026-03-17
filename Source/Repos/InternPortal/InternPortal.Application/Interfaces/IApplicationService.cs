using InternPortal.Application.Dtos;
using InternPortal.Application.Common;

namespace InternPortal.Application.Interfaces;

public interface IApplicationService
{    
    Task<ServiceResult<Guid>> SubmitAsync(Guid userId, ApplicationDto dto);

    Task<List<ApplicationDto>> GetByUserIdAsync(Guid userId);
}