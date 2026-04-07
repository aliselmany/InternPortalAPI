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
            if (lastApplication.Status == ApplicationStatus.Pending)
            {
                return ServiceResult<Guid>.Failure("You already have a pending application. Please wait for the result.");
            }

            if (lastApplication.Status == ApplicationStatus.Approved)
            {
                if (DateTime.UtcNow <= lastApplication.EndDate)
                {
                    return ServiceResult<Guid>.Failure($"Your current internship is active until {lastApplication.EndDate:yyyy-MM-dd}. You can apply after this date.");
                }
            }
        }

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return ServiceResult<Guid>.Failure("User not found.");

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

        string savedFileName = string.Empty;
        if (dto.CvFile != null && dto.CvFile.Length > 0)
        {
            var extension = Path.GetExtension(dto.CvFile.FileName).ToLower();
            string[] allowedExtensions = { ".pdf", ".docx", ".doc" };

            if (!allowedExtensions.Contains(extension))
                return ServiceResult<Guid>.Failure("Only .pdf, .doc, and .docx files are allowed.");

            if (dto.CvFile.Length > 5 * 1024 * 1024)
                return ServiceResult<Guid>.Failure("File size cannot exceed 5MB.");

            savedFileName = $"{Guid.NewGuid()}{extension}";
            var uploadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "cvs");

            if (!Directory.Exists(uploadFolderPath))
                Directory.CreateDirectory(uploadFolderPath);

            var filePath = Path.Combine(uploadFolderPath, savedFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.CvFile.CopyToAsync(stream);
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
            CvUrl = savedFileName,
            AppliedDate = DateTime.UtcNow,
            Status = ApplicationStatus.Pending
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

        if (application == null)
        {
            return ServiceResult<bool>.Failure("Application not found.");
        }

        if (newStatus == ApplicationStatus.Approved && application.User != null && !application.User.UserRoles.Any())
        {
            var internRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Intern");
            if (internRole != null)
            {
                application.User.UserRoles.Add(new UserRoleMapping
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
            CvFile = null!
        };
    }
}