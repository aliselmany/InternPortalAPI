using InternPortal.Application.Interfaces;
using InternPortal.Application.Dtos;
using InternPortal.Application.Common;
using InternPortal.Infrastructure.Persistence;
using InternPortal.Domain.Entities;
using InternPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InternPortal.Application.Services;

public class ApplicationService : IApplicationService
{
    private readonly AppDbContext _context;

    public ApplicationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<Guid>> SubmitAsync(Guid userId, ApplicationDto dto)
    {
          var lastApplication = await _context.Applications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.AppliedDate)
            .FirstOrDefaultAsync();

        if (lastApplication != null)
        {
            if (lastApplication.Status == ApplicationStatus.Beklemede)
            {
                return ServiceResult<Guid>.Failure("Beklemede olan bir başvurunuz zaten var. Lütfen sonuçlanmasını bekleyin.");
            }

            if (lastApplication.Status == ApplicationStatus.Onaylandı)
            {
                if (DateTime.UtcNow <= lastApplication.EndDate)
                {
                    return ServiceResult<Guid>.Failure($"Aktif stajınız {lastApplication.EndDate:yyyy-MM-dd} tarihine kadar devam ediyor.");
                }
            }
        }
 
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return ServiceResult<Guid>.Failure("Kullanıcı bulunamadı.");

        if (user.UserRoles == null || !user.UserRoles.Any())
        {
            var internRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Intern");
            if (internRole != null)
            {
                _context.UserRoleMappings.Add(new UserRoleMapping
                {
                    UserId = userId,
                    RoleId = internRole.Id
                });
            }
        }

        var application = new InternPortal.Domain.Entities.Application
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            University = dto.University,
            StudentGrade = dto.Grade,
            Department = dto.Department,
            InternshipType = dto.InternshipType,
            PhoneNumber = dto.PhoneNumber,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Description = dto.Description,
            CvUrl = dto.CvPath ?? string.Empty,
            TranscriptFile = dto.TranscriptPath ?? string.Empty,
            AppliedDate = DateTime.UtcNow,
            Status = ApplicationStatus.Beklemede,
            Reference = dto.Reference,
            ReferenceGsm = dto.ReferenceGsm,
            ReferenceClosenessStatus = dto.ReferenceClosenessStatus
        };

        user.University = dto.University;
        user.Department = dto.Department.ToString();
        user.StartDate = dto.StartDate;
        user.EndDate = dto.EndDate;

        _context.Applications.Add(application);
        _context.Users.Update(user);

        await _context.SaveChangesAsync();
        return ServiceResult<Guid>.Success(application.Id);
    }

    public async Task<ServiceResult<bool>> UpdateStatusAsync(Guid applicationId, ApplicationStatus newStatus)
    {
        var application = await _context.Applications
            .Include(a => a.User)
                .ThenInclude(u => u.UserRoles)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application == null) return ServiceResult<bool>.Failure("Başvuru bulunamadı.");

        if (newStatus == ApplicationStatus.Onaylandı && application.User != null && !application.User.UserRoles.Any())
        {
            var internRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Intern");
            if (internRole != null)
            {
                _context.UserRoleMappings.Add(new UserRoleMapping
                {
                    UserId = application.UserId,
                    RoleId = internRole.Id
                });
            }
        }

        application.Status = newStatus;
        var saved = await _context.SaveChangesAsync() > 0;
        return ServiceResult<bool>.Success(saved);
    }

    public async Task<List<ApplicationDto>> GetByUserIdAsync(Guid userId)
    {
        var applications = await _context.Applications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.AppliedDate)
            .ToListAsync();

        return applications.Select(MapToDto).ToList();
    }

    public async Task<List<ApplicationDto>> GetAllAsync()
    {
        var applications = await _context.Applications
            .OrderByDescending(x => x.AppliedDate)
            .ToListAsync();

        return applications.Select(MapToDto).ToList();
    }

    public async Task<ApplicationDto?> GetByIdAsync(Guid id)
    {
        var application = await _context.Applications.FirstOrDefaultAsync(x => x.Id == id);
        return application == null ? null : MapToDto(application);
    }

    private static ApplicationDto MapToDto(InternPortal.Domain.Entities.Application x)
    {
        return new ApplicationDto
        {
            University = x.University,
            Grade = x.StudentGrade,
            Department = x.Department,
            InternshipType = x.InternshipType,
            PhoneNumber = x.PhoneNumber,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            Description = x.Description,
            CvPath = x.CvUrl,
            TranscriptPath = x.TranscriptFile,
            Reference = x.Reference,
            ReferenceGsm = x.ReferenceGsm,
            ReferenceClosenessStatus = x.ReferenceClosenessStatus,
            Status = x.Status, 
            CvFile = null!,   
            TranscriptFile = null!
        };
    }
}