using InternPortal.Application.Common;
using System;
using System.Collections.Generic;
using System.Text;
using InternPortal.Application.Dtos;

namespace InternPortal.Application.Interfaces
{
    public interface IRolesService
    {
        Task<ServiceResult<IEnumerable<RoleResponse>>>GetAllRolesAsync();
        Task<ServiceResult<bool>> CreateRoleAsync(string roleName);
        Task<ServiceResult<bool>> UpdateUserRoleAsync(Guid userId, string roleName);
        Task<ServiceResult<bool>> UpdateUserNameAsync(Guid userId, string firstName, string lastName);
        Task<ServiceResult<bool>> DeleteRoleAsync(Guid roleId);
        string GenerateToken(InternPortal.Domain.Entities.User user);
    }
}