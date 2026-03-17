using InternPortal.Domain.Entities;
using InternPortal.Application.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using InternPortal.Application.Common;

namespace InternPortal.Application.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult> RegisterAsync(CreateUserDto createUserDto);

        Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequest loginRequest);

        Task<IEnumerable<UserDto>> GetAllUsersAsync();
    }
}