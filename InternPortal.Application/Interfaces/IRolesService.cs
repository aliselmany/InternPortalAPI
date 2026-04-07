using InternPortal.Application.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace InternPortal.Application.Interfaces
{
    public interface IRolesService
    {
        Task<ServiceResult<bool>> UpdateUserRoleAsync(Guid userId, string roleName);
        string GenerateToken(InternPortal.Domain.Entities.User user);
    }
}