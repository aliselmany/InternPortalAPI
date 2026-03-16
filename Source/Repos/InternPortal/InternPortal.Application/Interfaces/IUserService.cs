using InternPortal.Application.Common;
using InternPortal.Application.Dtos;

namespace InternPortal.Application.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult> RegisterAsync(CreateUserDto createUserDto);
        Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequest loginRequest);

        Task<IEnumerable<UserDto>> GetAllUsersAsync();

        Task<bool> DeleteUserAsync(Guid userId);
        Task<bool> UpdateUserAsync(Guid userId, UpdateUserDto dto);
    }
}