using InternPortal.Application.Common;
using InternPortal.Application.Dtos;

namespace InternPortal.Application.Interfaces;

public interface IUserService
{
 
    Task<ServiceResult> RegisterAsync(CreateUserDto dto);
    Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequest request);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<bool> DeleteUserAsync(Guid userId);
    Task<bool> UpdateUserAsync(Guid userId, UpdateUserDto dto);
    Task<ServiceResult<bool>> UpdateRoleAsync(Guid userId, string newRoleName);

    
    Task<List<AvailableMentorDto>> GetAvailableMentorsAsync(string? expertise);
    Task<ServiceResult> AssignMentorAsync(Guid internId, Guid mentorId);

    
    Task<bool> UpdateStaffProfileAsync(Guid staffId, StaffProfileUpdateDto dto);
}