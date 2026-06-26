using BCrypt.Net;
using InternPortal.Application.Common;
using InternPortal.Application.Dtos;
using InternPortal.Application.Interfaces;
using InternPortal.Domain.Entities;
using InternPortal.Domain.Enums;
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
    private readonly IMailService _emailService;

    public UserService(AppDbContext context, IConfiguration configuration, IMailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<ServiceResult> RegisterAsync(CreateUserDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return ServiceResult.Failure("Email address is already in use.");

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        string verificationCode = Random.Shared.Next(100000, 999999).ToString();

        var user = new User
        {
            Name = dto.Name,
            Surname = dto.Surname,
            Email = dto.Email,
            Password = hashedPassword,
            IsEmailVerified = false,
            VerificationCode = verificationCode,
            VerificationCodeExpiration = DateTime.Now.AddMinutes(3)
        };

        var internRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Intern");
        if (internRole != null)
            user.UserRoles.Add(new UserRoleMapping { RoleId = internRole.Id });

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        string mailBody = $@"
            <div style='font-family: Arial, sans-serif; text-align: center; padding: 20px;'>
                <h2>InternPortal'a Hoş Geldiniz!</h2>
                <p>Sayın <b>{user.Name} {user.Surname}</b>, kaydınızı tamamlamak için son bir adım kaldı.</p>
                <p>Lütfen aşağıdaki 6 haneli doğrulama kodunu ekrandaki alana giriniz:</p>
                <h1 style='color: #0d6efd; letter-spacing: 5px; background: #f8f9fa; padding: 10px; border-radius: 8px; display: inline-block;'>{verificationCode}</h1>
                <p style='color: #6c757d; font-size: 14px;'>Bu kodun geçerlilik süresi 3 dakikadır.</p>
            </div>
        ";

        try
        {
            _emailService.SendEmail(user.Email, "InternPortal - E-Posta Doğrulama Kodu", mailBody);
        }
        catch
        {
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            return ServiceResult<LoginResponseDto>.Failure("Yanlış parola yada e-mail");

        if (!user.IsEmailVerified)
            return ServiceResult<LoginResponseDto>.Failure("Hesabınız henüz onaylanmamış. Lütfen e-postanıza gönderilen kod ile onaylayınız.");

        return ServiceResult<LoginResponseDto>.Success(new LoginResponseDto { Token = GenerateJwtToken(user), Email = user.Email });
    }

    public async Task<ServiceResult> VerifyEmailAsync(string email, string code)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (user == null)
            return ServiceResult.Failure("Kullanıcı bulunamadı.");

        if (user.IsEmailVerified)
            return ServiceResult.Failure("Bu hesap zaten doğrulanmış.");

        if (string.IsNullOrEmpty(user.VerificationCode) || user.VerificationCode != code)
            return ServiceResult.Failure("Girdiğiniz 6 haneli doğrulama kodu hatalı.");

        if (user.VerificationCodeExpiration < DateTime.Now)
            return ServiceResult.Failure("Kodun geçerlilik süresi (3 dakika) dolmuş. Lütfen yeni kod talep edin.");

        user.IsEmailVerified = true;
        user.VerificationCode = null;
        user.VerificationCodeExpiration = null;

        await _context.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<UserResponseDto?> UserByIdAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.SocialAccounts)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            Department = user.Department,
            Expertise = user.Expertise,
            Biography = user.Biography,
            MaxInternCount = user.MaxInternCount,
            MentorId = user.MentorId,
    
            StartDate = await _context.Applications
                                  .Where(a => a.UserId == user.Id)
                                  .Select(a => (DateTime?)a.StartDate)
                                  .FirstOrDefaultAsync(),
            EndDate = await _context.Applications
                                  .Where(a => a.UserId == user.Id)
                                  .Select(a => (DateTime?)a.EndDate)
                                  .FirstOrDefaultAsync(),
            SocialAccounts = user.SocialAccounts.Select(s => new SocialAccountDto
            {
                PlatformName = s.PlatformName,
                ProfileUrl = s.ProfileUrl
            }).ToList()
        };
    }

    public async Task<IEnumerable<UserResponseDto>> GetMyInternsAsync(Guid staffId)
    {
        return await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => u.MentorId == staffId)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Surname = u.Surname,
                Email = u.Email,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                Department = u.Department,
                Expertise = u.Expertise,
                Biography = u.Biography,
                MaxInternCount = u.MaxInternCount,
                MentorId = u.MentorId,
            
                StartDate = _context.Applications
                                  .Where(a => a.UserId == u.Id)
                                  .Select(a => (DateTime?)a.StartDate)
                                  .FirstOrDefault(),
                EndDate = _context.Applications
                                  .Where(a => a.UserId == u.Id)
                                  .Select(a => (DateTime?)a.EndDate)
                                  .FirstOrDefault()
            }).ToListAsync();
    }

    public async Task<List<UserResponseDto>> GetAllUsersAsync(GetUserFilterDto filter)
    {
        var query = _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (filter.UserId.HasValue) query = query.Where(u => u.Id == filter.UserId.Value);
        if (filter.RoleId.HasValue) query = query.Where(u => u.UserRoles.Any(ur => ur.RoleId == filter.RoleId.Value));
        if (!string.IsNullOrEmpty(filter.Name)) query = query.Where(u => u.Name.Contains(filter.Name));
        if (!string.IsNullOrEmpty(filter.Surname)) query = query.Where(u => u.Surname.Contains(filter.Surname));

        return await query.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Name = u.Name,
            Surname = u.Surname,
            Email = u.Email,
            Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            Department = u.Department,
            Expertise = u.Expertise,
            Biography = u.Biography,
            MaxInternCount = u.MaxInternCount,
            MentorId = u.MentorId,
         
            StartDate = _context.Applications
                              .Where(a => a.UserId == u.Id)
                              .Select(a => (DateTime?)a.StartDate)
                              .FirstOrDefault(),
            EndDate = _context.Applications
                              .Where(a => a.UserId == u.Id)
                              .Select(a => (DateTime?)a.EndDate)
                              .FirstOrDefault()
        }).ToListAsync();
    }

    public async Task<List<AvailableMentorDto>> GetAvailableMentorsAsync(GetAvailableMentorsDto filter)
    {
        var query = _context.Users
            .Include(u => u.SocialAccounts)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Interns)
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Staff"))
            .AsQueryable();

        if (filter.UserId.HasValue) query = query.Where(u => u.Id == filter.UserId.Value);
        if (!string.IsNullOrWhiteSpace(filter.FullName))
        {
            var search = filter.FullName.ToLower();
            query = query.Where(u => (u.Name + " " + u.Surname).ToLower().Contains(search));
        }
        if (!string.IsNullOrWhiteSpace(filter.Expertise))
        {
            var search = filter.Expertise.ToLower();
            query = query.Where(u => u.Expertise != null && u.Expertise.ToLower().Contains(search));
        }

        return await query.Select(u => new AvailableMentorDto
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
        }).ToListAsync();
    }

    public async Task<bool> UpdateMentorProfileAsync(Guid staffId, MentorProfileUpdateDto dto)
    {
        var staff = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.SocialAccounts)
            .FirstOrDefaultAsync(u => u.Id == staffId);

        if (staff == null || staff.UserRoles.Any(ur => ur.Role.Name == "Intern")) return false;

        staff.Expertise = dto.Expertise ?? staff.Expertise;
        staff.Biography = dto.Biography ?? staff.Biography;
        if (dto.MaxInternCount.HasValue) staff.MaxInternCount = dto.MaxInternCount.Value;

        var existingAccounts = _context.UserSocialAccounts.Where(x => x.UserId == staffId);
        _context.UserSocialAccounts.RemoveRange(existingAccounts);

        if (dto.SocialAccounts != null)
        {
            foreach (var account in dto.SocialAccounts)
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
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> SelectMentorAsync(Guid internId, Guid mentorId)
    {
        var intern = await _context.Users.FindAsync(internId);
        if (intern == null) return false;
        intern.MentorId = mentorId;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<ServiceResult> AssignMentorAsync(Guid internId, Guid mentorId)
    {
        var mentor = await _context.Users.Include(u => u.Interns).Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == mentorId && u.UserRoles.Any(ur => ur.Role.Name == "Staff"));

        if (mentor == null) return ServiceResult.Failure("Mentor not found.");
        if (mentor.Interns.Count >= mentor.MaxInternCount) return ServiceResult.Failure("Capacity full.");

        var intern = await _context.Users.FindAsync(internId);

        if (intern == null) return ServiceResult.Failure("Invalid intern.");

        intern.MentorId = mentorId;
        await _context.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
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

    public async Task<List<UserResponseDto>> UsersByRoleIdAsync(Guid roleId)
    {
        return await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId))
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Surname = u.Surname,
                Email = u.Email,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                Department = u.Department,
                Expertise = u.Expertise,
                Biography = u.Biography,
                MaxInternCount = u.MaxInternCount,
                MentorId = u.MentorId,

                StartDate = _context.Applications
                                  .Where(a => a.UserId == u.Id)
                                  .Select(a => (DateTime?)a.StartDate)
                                  .FirstOrDefault(),
                EndDate = _context.Applications
                                  .Where(a => a.UserId == u.Id)
                                  .Select(a => (DateTime?)a.EndDate)
                                  .FirstOrDefault()
            }).ToListAsync();
    }

    public async Task<List<UserResponseDto>> GetUsersByRoleNameAsync(string roleName)
    {
        return await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name.ToLower() == roleName.ToLower()))
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Surname = u.Surname,
                Email = u.Email,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                Department = u.Department,
                Expertise = u.Expertise,
                Biography = u.Biography,
                MaxInternCount = u.MaxInternCount,
                MentorId = u.MentorId,
 
                StartDate = _context.Applications
                                  .Where(a => a.UserId == u.Id)
                                  .Select(a => (DateTime?)a.StartDate)
                                  .FirstOrDefault(),
                EndDate = _context.Applications
                                  .Where(a => a.UserId == u.Id)
                                  .Select(a => (DateTime?)a.EndDate)
                                  .FirstOrDefault()
            }).ToListAsync();
    }

    public async Task<ServiceResult> UpdateUserByIdAsync(Guid userId, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return ServiceResult.Failure("Kullanıcı bulunamadı.");

        if (!string.IsNullOrWhiteSpace(dto.Name)) user.Name = dto.Name;
        if (!string.IsNullOrWhiteSpace(dto.Surname)) user.Surname = dto.Surname;
        if (!string.IsNullOrWhiteSpace(dto.Email)) user.Email = dto.Email;
        if (!string.IsNullOrWhiteSpace(dto.Password)) user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        await _context.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult<bool>> UpdateUserRoleAsync(Guid userId, string roleName)
    {
        var user = await _context.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return ServiceResult<bool>.Failure("Kullanıcı bulunamadı.");

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role == null) return ServiceResult<bool>.Failure("Rol bulunamadı.");

        user.UserRoles.Clear();
        user.UserRoles.Add(new UserRoleMapping { UserId = userId, RoleId = role.Id });

        await _context.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<UserResponseDto?>> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (user == null)
            return ServiceResult<UserResponseDto?>.Failure("Bu e-posta adresine ait bir kullanıcı bulunamadı.");

        var dto = new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
        };

        return ServiceResult<UserResponseDto?>.Success(dto);
    }

    public async Task<ServiceResult> SavePasswordResetCodeAsync(Guid userId, string code, DateTime expiration)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return ServiceResult.Failure("Kullanıcı bulunamadı.");

        user.PasswordResetCode = code;
        user.PasswordResetCodeExpiration = expiration;

        await _context.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> VerifyResetCodeAsync(string email, string code)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        if (user == null)
            return ServiceResult.Failure("Kullanıcı bulunamadı.");

        if (string.IsNullOrEmpty(user.PasswordResetCode) || user.PasswordResetCode != code)
            return ServiceResult.Failure("Girdiğiniz onay kodu hatalı.");

        if (user.PasswordResetCodeExpiration < DateTime.Now)
            return ServiceResult.Failure("Onay kodunun geçerlilik süresi dolmuş.");

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> CheckOldPasswordAsync(Guid userId, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return ServiceResult.Failure("Kullanıcı bulunamadı.");

        if (BCrypt.Net.BCrypt.Verify(newPassword, user.Password))
            return ServiceResult.Failure("Yeni şifreniz eski şifrenizle aynı olamaz.");

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdatePasswordAsync(Guid userId, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return ServiceResult.Failure("Kullanıcı bulunamadı.");

        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

        user.PasswordResetCode = null;
        user.PasswordResetCodeExpiration = null;

        await _context.SaveChangesAsync();
        return ServiceResult.Success();
    }
}