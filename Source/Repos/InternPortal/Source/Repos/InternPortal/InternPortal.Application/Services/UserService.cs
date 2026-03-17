using BCrypt.Net;
using InternPortal.Application.Interfaces;
using InternPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using InternPortal.Domain.Enums;
using InternPortal.Infrastructure.Persistence;
using InternPortal.Application.Dtos;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using InternPortal.Application.Common;
using Microsoft.Extensions.Configuration;

namespace InternPortal.Application.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration; 

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
        })  .ToListAsync();
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
            Role = UserRole.Intern
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
            Email = user.Email,
            Message = "Login Successful"
        };

        return ServiceResult<LoginResponseDto>.Success(response);
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role.ToString())
    };

        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(3), 
            signingCredentials: creds
        );
           return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}