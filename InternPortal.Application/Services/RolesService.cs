using InternPortal.Application.Interfaces;
using InternPortal.Infrastructure.Persistence;
using InternPortal.Domain.Entities;
using InternPortal.Application.Common;
using InternPortal.Application.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InternPortal.Application.Services;

public class RolesService : IRolesService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;

    public RolesService(IConfiguration config, AppDbContext context)
    {
        _config = config;
        _context = context;
    }

    public async Task<ServiceResult<IEnumerable<RoleResponse>>> GetAllRolesAsync()
    {
        var roles = await _context.Roles
            .Select(r => new RoleResponse
            {
                Id = r.Id,
                Name = r.Name
            })
            .ToListAsync();

        return ServiceResult<IEnumerable<RoleResponse>>.Success(roles);
    }

    public async Task<ServiceResult<bool>> CreateRoleAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return ServiceResult<bool>.Failure("Role name cannot be empty.");

        var exists = await _context.Roles.AnyAsync(r => r.Name == roleName);
        if (exists)
            return ServiceResult<bool>.Failure("Role already exists.");

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = roleName
        };

        await _context.Roles.AddAsync(role);
        var result = await _context.SaveChangesAsync() > 0;
            
        return result ? ServiceResult<bool>.Success(true) : ServiceResult<bool>.Failure("Failed to create role.");
    }

    public async Task<ServiceResult<bool>> UpdateUserRoleAsync(Guid userId, string roleName)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return ServiceResult<bool>.Failure("User not found.");

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role == null) return ServiceResult<bool>.Failure("Role not found.");

        user.UserRoles.Clear();
        user.UserRoles.Add(new UserRole { UserId = userId, RoleId = role.Id });

        await _context.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> DeleteRoleAsync(Guid roleId)
    {
        var role = await _context.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null) return ServiceResult<bool>.Failure("Role not found.");

        if (role.UserRoles != null && role.UserRoles.Any())
            return ServiceResult<bool>.Failure("Cannot delete role. It is currently assigned to users.");

        _context.Roles.Remove(role);
        var result = await _context.SaveChangesAsync() > 0;

        return result ? ServiceResult<bool>.Success(true) : ServiceResult<bool>.Failure("Failed to delete role.");
    }

    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        if (user.UserRoles != null)
        {
            foreach (var mapping in user.UserRoles)
            {
                if (mapping.Role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, mapping.Role.Name));
                }
            }
        }

        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing in configuration.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        if (!double.TryParse(_config["Jwt:DurationInMinutes"], out double duration))
        {
            duration = 60;
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(duration),
            SigningCredentials = creds,
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public async Task<ServiceResult<bool>> UpdateUserNameAsync(Guid userId, string firstName, string lastName)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
               return ServiceResult<bool>.Failure("User not found.");
        }

        user.Name = firstName;

        user.Surname = lastName;

        var result = await _context.SaveChangesAsync() > 0;

        return result
            ? ServiceResult<bool>.Success(true)
            : ServiceResult<bool>.Failure("Update failed or no changes detected.");
    }
}