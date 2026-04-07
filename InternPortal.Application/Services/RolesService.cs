using InternPortal.Application.Interfaces;
using InternPortal.Infrastructure.Persistence; 
using InternPortal.Domain.Entities;
using InternPortal.Application.Common;
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


    public async Task<ServiceResult<bool>> UpdateUserRoleAsync(Guid userId, string roleName)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return ServiceResult<bool>.Failure("User not found.");

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role == null) return ServiceResult<bool>.Failure("Role not found.");

        user.UserRoles.Clear();
        user.UserRoles.Add(new UserRoleMapping { UserId = userId, RoleId = role.Id });

        await _context.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
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
}