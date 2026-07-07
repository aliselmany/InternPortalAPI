using InternPortal.Application.Interfaces;
using InternPortal.Application.Dtos;
using InternPortal.Application.Common;
using InternPortal.Infrastructure.Persistence;
using InternPortal.Domain.Entities;
using InternPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using InternPortal.Application.Mappers;

namespace InternPortal.Application.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly AppDbContext _context;

        public ApplicationService(AppDbContext context)
        {
            _context = context;
        }

        private async Task<string> _CheckUserApplication(Guid userId)
        {
            var entity = await _context.Applications
               .Where(x => x.UserId == userId)
               .OrderByDescending(x => x.AppliedDate)
               .FirstOrDefaultAsync();

            if (entity != null)
            {
                if (entity.Status == ApplicationStatus.Beklemede || entity.Status == ApplicationStatus.HrOnayladı)
                {
                    return "Beklemede olan bir başvurunuz zaten var. Lütfen sonuçlanmasını bekleyin.";
                }

                if (entity.Status == ApplicationStatus.Onaylandı)
                {
                    if (DateTime.UtcNow <= entity.EndDate)
                    {
                        return $"Aktif stajınız {entity.EndDate:yyyy-MM-dd} tarihine kadar devam ediyor.";
                    }
                }
            }

            return string.Empty;
        }

        public async Task<ServiceResult<Guid>> SubmitAsync(Guid userId, ApplicationDto dto)
        {
            var errorMessage = await _CheckUserApplication(userId);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return ServiceResult<Guid>.Failure(errorMessage);
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return ServiceResult<Guid>.Failure("Kullanıcı bulunamadı.");
            }

            if (user.UserRoles == null || !user.UserRoles.Any())
            {
                var internRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Intern");
                if (internRole != null)
                {
                    _context.UserRoleMappings.Add(new UserRole
                    {
                        UserId = userId,
                        RoleId = internRole.Id
                    });
                }
            }

            var application = new Domain.Entities.Application
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
            throw new NotImplementedException();
        }

        public async Task<ServiceResult<bool>> UpdateStatusAsync(Guid applicationId, ApplicationStatus newStatus)
        {
            var application = await _context.Applications
                .Include(a => a.User).ThenInclude(u => u.UserRoles)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
            {
                return ServiceResult<bool>.Failure("Başvuru bulunamadı.");
            }

            application.Status = newStatus;
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }

        public async Task<List<ApplicationDto>> GetByUserIdAsync(Guid userId)
        {
            var applications = await _context.Applications
                .Include(x => x.User)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.AppliedDate)
                .ToListAsync();

            var dtos = applications.Select(ApplicationAutoMapper.ApplicationToApplicationDto).ToList();
            return dtos;
        }

        public async Task<List<ApplicationDto>> GetAllAsync(ApplicationFilterQuery filter, Guid currentUserId)
        {
            var query = _context.Applications
                .Include(x => x.User)
                .AsQueryable();

            var currentUser = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (currentUser != null)
            {
                var roles = currentUser.UserRoles.Select(ur => ur.Role.Name).ToList();

                if (!roles.Contains("HR") && roles.Contains("DepartmanAdmin"))
                {
                    if (currentUser.ManagedDepartment.HasValue)
                    {
                        query = query.Where(a => a.Department == currentUser.ManagedDepartment.Value);
                    }
                    else
                    {
                        return new List<ApplicationDto>();
                    }
                }
            }

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

            return applications.Select(ApplicationAutoMapper.ApplicationToApplicationDto).ToList();
        }

        public async Task<ApplicationDto?> GetByIdAsync(Guid id)
        {
            var application = await _context.Applications
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);
                
            if (application == null) {
                throw new InvalidOperationException("Application not found");
            }

            return ApplicationAutoMapper.ApplicationToApplicationDto(application);
        }

        //TODO: Buna gerek yok
        public async Task<List<ApplicationDto>> GetPendingManagerApprovalsAsync(Department department)
        {
            var applications = await _context.Applications
                .Include(x => x.User)
                .Where(x => x.Status == ApplicationStatus.HrOnayladı && x.Department == department)
                .OrderByDescending(x => x.AppliedDate)
                .ToListAsync();

            return applications.Select(ApplicationAutoMapper.ApplicationToApplicationDto).ToList();
        }
    }
}