using BCrypt.Net;
using InternPortal.Application.Common;
using InternPortal.Application.Dtos;
using InternPortal.Application.Interfaces;
using InternPortal.Domain.Entities;
using InternPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InternPortal.Application.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public UserService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<List<UserResponseDto>> GetAllUsersAsync(GetUserFilterDto filter)
    {
        var query = _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsQueryable();

    
        if (filter.UserId.HasValue)
            query = query.Where(u => u.Id == filter.UserId.Value);

        if (filter.RoleId.HasValue)
            query = query.Where(u => u.UserRoles.Any(ur => ur.RoleId == filter.RoleId.Value));

        if (!string.IsNullOrEmpty(filter.Name))
            query = query.Where(u => u.Name.Contains(filter.Name));

        if (!string.IsNullOrEmpty(filter.Surname))
            query = query.Where(u => u.Surname.Contains(filter.Surname));

        return await query.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Name = u.Name,
            Surname = u.Surname,
            Email = u.Email,
            Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            Department = u.Department,
            Expertise = u.Expertise
        }).ToListAsync();
    }

    public async Task<ServiceResult> RegisterAsync(CreateUserDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return ServiceResult.Failure("Email address is already in use.");

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var user = new User { Name = dto.Name, Surname = dto.Surname, Email = dto.Email, Password = hashedPassword };

        var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (defaultRole != null) user.UserRoles.Add(new UserRoleMapping { RoleId = defaultRole.Id });

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            return ServiceResult<LoginResponseDto>.Failure("Invalid email or password.");

        return ServiceResult<LoginResponseDto>.Success(new LoginResponseDto { Token = GenerateJwtToken(user), Email = user.Email });
    }

    public async Task<ServiceResult<bool>> UpdateRoleAsync(Guid userId, string newRoleName)
    {
        var user = await _context.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return ServiceResult<bool>.Failure("User not found.");

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == newRoleName);
        if (role == null) return ServiceResult<bool>.Failure("Role not found.");

        user.UserRoles.Clear();
        user.UserRoles.Add(new UserRoleMapping { UserId = userId, RoleId = role.Id });
        await _context.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<List<AvailableMentorDto>> GetAvailableMentorsAsync(string? expertise)
    {
        var query = _context.Users
            .Include(u => u.SocialAccounts)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Interns)
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Staff"));

        if (!string.IsNullOrEmpty(expertise))
            query = query.Where(u => u.Expertise != null && u.Expertise.Contains(expertise));

        var mentors = await query.ToListAsync();

        return mentors.Select(u => new AvailableMentorDto
        {
            Id = u.Id,
            FullName = u.Name + " " + u.Surname,
            Expertise = u.Expertise,
            Biography = u.Biography,
            MaxCapacity = u.MaxInternCount,
            CurrentCount = u.Interns.Count,
            SocialAccounts = u.SocialAccounts.Select(s => new SocialAccountDto
            {
                PlatformName = s.PlatformName,
                ProfileUrl = s.ProfileUrl
            }).ToList()
        }).ToList();
    }

    public async Task<ServiceResult> AssignMentorAsync(Guid internId, Guid mentorId)
    {
        var mentor = await _context.Users.Include(u => u.Interns).Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == mentorId && u.UserRoles.Any(ur => ur.Role.Name == "Staff"));

        if (mentor == null) return ServiceResult.Failure("Mentor not found.");
        if (mentor.Interns.Count >= mentor.MaxInternCount) return ServiceResult.Failure("Capacity full.");

        var intern = await _context.Users.FindAsync(internId);
        if (intern == null || intern.MentorId != null) return ServiceResult.Failure("Invalid intern or already assigned.");

        intern.MentorId = mentorId;
        await _context.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<bool> UpdateStaffProfileAsync(Guid staffId, StaffProfileUpdateDto dto)
    {
        var staff = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.SocialAccounts)
            .FirstOrDefaultAsync(u => u.Id == staffId);

        if (staff == null) return false;

        if (staff.UserRoles.Any(ur => ur.Role.Name == "Intern"))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(dto.Expertise))
            staff.Expertise = dto.Expertise;

        if (!string.IsNullOrWhiteSpace(dto.Biography))
            staff.Biography = dto.Biography;

        if (dto.MaxInternCount.HasValue)
            staff.MaxInternCount = dto.MaxInternCount.Value;

        if (dto.SocialAccounts != null)
        {
 
            var existingAccounts = _context.UserSocialAccounts.Where(x => x.UserId == staffId);
            _context.UserSocialAccounts.RemoveRange(existingAccounts);

            foreach (var account in dto.SocialAccounts)
            {
                if (!string.IsNullOrWhiteSpace(account.PlatformName) && !string.IsNullOrWhiteSpace(account.ProfileUrl))
                {
                    _context.UserSocialAccounts.Add(new UserSocialAccount
                    {
                        Id = Guid.NewGuid(),
                        PlatformName = account.PlatformName,
                        ProfileUrl = account.ProfileUrl,
                        UserId = staffId
                    });
                }
            }
        }

        return await _context.SaveChangesAsync() > 0;
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };
        foreach (var mapping in user.UserRoles) claims.Add(new Claim(ClaimTypes.Role, mapping.Role.Name));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(3),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        user.Name = dto.Name;
        user.Surname = dto.Surname;
        user.Email = dto.Email;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ServiceResult> UpdateUserByIdAsync(Guid userId, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return ServiceResult.Failure("User not found.");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            user.Name = dto.Name;

        if (!string.IsNullOrWhiteSpace(dto.Surname))
            user.Surname = dto.Surname;

        if (!string.IsNullOrWhiteSpace(dto.Email))
            user.Email = dto.Email;

        if (!string.IsNullOrWhiteSpace(dto.Password))
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        await _context.SaveChangesAsync();
        return ServiceResult.Success();
    }
}