using InternPortal.Application.Interfaces;
using InternPortal.Application.Dtos;
using InternPortal.Application.Common;
using InternPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using InternPortal.Domain.Entities;
using InternPortal.Domain.Enums;

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
       
        var hasExistingApplication = await _context.Applications
            .AnyAsync(x => x.UserId == userId && x.Status != ApplicationStatus.Rejected);

        if (hasExistingApplication)
        {
            return ServiceResult<Guid>.Failure("You already have an active application.");
        }

     
        var application = new InternPortal.Domain.Entities.Application
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SchoolName = dto.SchoolName,
            StudentGrade = dto.Grade, 
            SelectedDepartment = dto.SelectedDepartment,
            InternshipType = dto.InternshipType,
            PhoneNumber = dto.PhoneNumber,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Description = dto.Description,
            CvUrl = dto.CvFile != null ? dto.CvFile.FileName : string.Empty,
            AppliedDate = DateTime.UtcNow,
            Status = ApplicationStatus.Pending
        };

        _context.Applications.Add(application);
        await _context.SaveChangesAsync();

        return ServiceResult<Guid>.Success(application.Id);
    }

    public async Task<List<ApplicationDto>> GetByUserIdAsync(Guid userId)
    {
        var applications = await _context.Applications
            .Where(x => x.UserId == userId)
            .ToListAsync();

        return applications.Select(x => new ApplicationDto
        {
            SchoolName = x.SchoolName,
            Grade = x.StudentGrade,
            SelectedDepartment = x.SelectedDepartment,
            InternshipType = x.InternshipType,
            PhoneNumber = x.PhoneNumber,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            Description = x.Description,
            CvFile = null!
        }).ToList();
    }
}