using InternPortal.Application.Common;
using InternPortal.Application.Dtos;

namespace InternPortal.Application.Interfaces;

public interface IUserService
{
    Task<ServiceResult> RegisterAsync(CreateUserDto dto);
    Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequest request);
    Task<List<UserResponseDto>> GetAllUsersAsync(GetUserFilterDto filter);
    Task<UserResponseDto?> UserByIdAsync(Guid userId);
    Task<List<UserResponseDto>> UsersByRoleIdAsync(Guid roleId);
    Task<List<AvailableMentorDto>> GetAvailableMentorsAsync(GetAvailableMentorsDto filter);
    Task<ServiceResult> UpdateUserByIdAsync(Guid userId, UpdateUserDto dto);
    Task<ServiceResult<bool>> UpdateUserRoleAsync(Guid userId, string roleName);
    Task<bool> UpdateMentorProfileAsync(Guid staffId, MentorProfileUpdateDto dto);
    Task<ServiceResult> AssignMentorAsync(Guid internId, Guid mentorId);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<List<UserResponseDto>> GetUsersByRoleNameAsync(string roleName);
    Task<IEnumerable<UserResponseDto>> GetMyInternsAsync(Guid staffId);
    Task<bool> SelectMentorAsync(Guid internId, Guid mentorId);

    Task<ServiceResult<UserResponseDto?>> GetUserByEmailAsync(string email);

    Task<ServiceResult> SavePasswordResetCodeAsync(Guid userId, string code, DateTime expiration);

    Task<ServiceResult> VerifyResetCodeAsync(string email, string code);

    Task<ServiceResult> CheckOldPasswordAsync(Guid userId, string newPassword);

    Task<ServiceResult> UpdatePasswordAsync(Guid userId, string newPassword);
}