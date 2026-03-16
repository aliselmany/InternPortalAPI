using InternPortal.Application.Dtos;
using InternPortal.Application.Common; 
using Microsoft.AspNetCore.Http;

namespace InternPortal.Application.Interfaces;

public interface IApplicationService
{
    Task<ServiceResult<Guid>> SubmitAsync(Guid userId, ApplicationDto dto, IFormFile cvFile);
    Task<List<ApplicationDto>> GetByUserIdAsync(Guid userId);
} 