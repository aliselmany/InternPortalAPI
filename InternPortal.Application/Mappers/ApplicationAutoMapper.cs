using InternPortal.Application.Dtos;
using InternPortal.Domain.Entities;

namespace InternPortal.Application.Mappers;

public class ApplicationAutoMapper
{
    public static ApplicationDto ApplicationToApplicationDto(Domain.Entities.Application x)
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
