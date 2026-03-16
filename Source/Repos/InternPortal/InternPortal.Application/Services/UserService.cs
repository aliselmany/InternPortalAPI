using BCrypt.Net;
using InternPortal.Application.Interfaces;
using InternPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using InternPortal.Infrastructure.Persistence;
using InternPortal.Application.Dtos;
using InternPortal.Application.Common;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

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

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Surname = u.Surname,
                Email = u.Email
            }).ToListAsync();
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

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != userId))
            return false;

        user.Name = dto.Name;
        user.Surname = dto.Surname;
        user.Email = dto.Email;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ServiceResult> RegisterAsync(CreateUserDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return ServiceResult.Failure("Email address is already in use.");

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Surname = dto.Surname,
            Email = dto.Email,
            Password = hashedPassword,
            Role = InternPortal.Domain.Enums.UserRole.Intern
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            return ServiceResult<LoginResponseDto>.Failure("Invalid email or password.");

        var token = GenerateJwtToken(user);

        var response = new LoginResponseDto
        {
            Token = token,
            Email = user.Email
          
        };

        return ServiceResult<LoginResponseDto>.Success(response);
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var jwtSection = _configuration.GetSection("Jwt");
        var keyString = jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(3),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}