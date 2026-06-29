using InternPortal.Application.Interfaces;
using InternPortal.Application.Dtos;
using InternPortal.Application.Common;
using InternPortal.Infrastructure.Persistence;
using InternPortal.Domain.Entities;
using InternPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InternPortal.Application.Services
{
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

                EducationLevel = dto.EducationLevel,

                SchoolName = dto.SchoolName,
                DepartmentOfStudy = dto.DepartmentOfStudy,

                Grade = dto.Grade,
                Department = dto.Department,
                InternshipType = dto.InternshipType,
                PhoneNumber = dto.PhoneNumber,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Description = dto.Description,
                CvUrl = dto.CvPath ?? string.Empty,

                TranscriptFile = dto.TranscriptPath,

                AppliedDate = DateTime.UtcNow,
                Status = ApplicationStatus.Beklemede,
                Reference = dto.Reference,
                ReferenceGsm = dto.ReferenceGsm,
                ReferenceClosenessStatus = dto.ReferenceClosenessStatus
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();
            return ServiceResult<Guid>.Success(application.Id);
        }

        public async Task<ServiceResult<bool>> UpdateAsync(Guid id, ApplicationUpdateDto dto)
        {
            var application = await _context.Applications.FindAsync(id);

            if (application == null)
            {
                return ServiceResult<bool>.Failure("Güncellenmek istenen başvuru bulunamadı.");
            }

            if (!string.IsNullOrEmpty(dto.PhoneNumber)) application.PhoneNumber = dto.PhoneNumber;

            if (!string.IsNullOrEmpty(dto.EducationLevel))
            {
                application.EducationLevel = dto.EducationLevel;
            }

            if (!string.IsNullOrEmpty(dto.SchoolName)) application.SchoolName = dto.SchoolName;
            if (!string.IsNullOrEmpty(dto.DepartmentOfStudy)) application.DepartmentOfStudy = dto.DepartmentOfStudy;
            if (!string.IsNullOrEmpty(dto.Grade)) application.Grade = dto.Grade;

            if (dto.Department.HasValue)
                application.Department = dto.Department.Value;

            if (dto.InternshipType.HasValue)
                application.InternshipType = dto.InternshipType.Value;

            if (dto.StartDate.HasValue)
                application.StartDate = dto.StartDate.Value;

            if (dto.EndDate.HasValue)
                application.EndDate = dto.EndDate.Value;

            if (dto.Description != null) application.Description = dto.Description;
            if (dto.Reference != null) application.Reference = dto.Reference;
            if (dto.ReferenceGsm != null) application.ReferenceGsm = dto.ReferenceGsm;

            _context.Applications.Update(application);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
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
                .Include(x => x.User)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.AppliedDate)
                .ToListAsync();

            return applications.Select(MapToDto).ToList();
        }

        public async Task<List<ApplicationDto>> GetAllAsync(ApplicationFilterQuery filter)
        {
            var query = _context.Applications
                .Include(x => x.User)
                .AsQueryable();

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Name))
                {
                    var searchName = filter.Name.Trim().ToLower();
                    query = query.Where(a => (a.User.Name + " " + a.User.Surname).ToLower().Contains(searchName));
                }

                if (!string.IsNullOrWhiteSpace(filter.SchoolName))
                {
                    var searchSchool = filter.SchoolName.Trim().ToLower();
                    query = query.Where(a => a.SchoolName.ToLower().Contains(searchSchool));
                }

                if (!string.IsNullOrWhiteSpace(filter.DepartmentOfStudy))
                {
                    var searchDept = filter.DepartmentOfStudy.Trim().ToLower();
                    query = query.Where(a => a.DepartmentOfStudy.ToLower().Contains(searchDept));
                }

                if (filter.InternshipType.HasValue)
                {
                    query = query.Where(a => (int)a.InternshipType == filter.InternshipType.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.Status))
                {

                    if (Enum.TryParse<ApplicationStatus>(filter.Status, true, out var parsedStatus))
                    {
                        query = query.Where(a => a.Status == parsedStatus);
                    }
                }
            }

            var applications = await query
                .OrderByDescending(x => x.AppliedDate)
                .ToListAsync();

            return applications.Select(MapToDto).ToList();
        }

        public async Task<ApplicationDto?> GetByIdAsync(Guid id)
        {
            var application = await _context.Applications
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            return application == null ? null : MapToDto(application);
        }

        private static ApplicationDto MapToDto(InternPortal.Domain.Entities.Application x)
        {
            return new ApplicationDto
            {
                Id = x.Id,
                UserId = x.UserId, 

                Name = x.User?.Name ?? "İsimsiz",
                Surname = x.User?.Surname ?? "Aday",

                EducationLevel = x.EducationLevel,

                SchoolName = x.SchoolName,
                DepartmentOfStudy = x.DepartmentOfStudy,
                Grade = x.Grade,
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
                TranscriptFile = null
            };
        }
    }
}