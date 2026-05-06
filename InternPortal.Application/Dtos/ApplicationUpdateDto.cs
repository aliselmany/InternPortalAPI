using InternPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace InternPortal.Application.Dtos;

public class ApplicationUpdateDto : IValidatableObject
{
    public string? EducationLevel { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? SchoolName { get; set; }
    public string? DepartmentOfStudy { get; set; }
    public string? Grade { get; set; }
    public Department? Department { get; set; }
    public InternshipType? InternshipType { get; set; }

    [RegularExpression(@"^5\d{9}$", ErrorMessage = "Telefon numarası 5 ile başlamalı ve 10 haneli olmalıdır.")]
    public string? PhoneNumber { get; set; }

    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    public string? Description { get; set; }
    public string? Reference { get; set; }

    [RegularExpression(@"^5\d{9}$", ErrorMessage = "Referans telefonu 5 ile başlamalı ve 10 haneli olmalıdır.")]
    public string? ReferenceGsm { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.HasValue && EndDate.HasValue)
        {
            if (EndDate.Value <= StartDate.Value)
            {
                yield return new ValidationResult(
                    "Bitiş tarihi başlangıç tarihinden sonra olmalıdır.",
                    new[] { nameof(EndDate) });
            }

            var totalDays = (EndDate.Value - StartDate.Value).TotalDays;
            if (totalDays < 10)
            {
                yield return new ValidationResult(
                    "Staj süresi en az 10 gün olmalıdır.",
                    new[] { nameof(EndDate) });
            }
        }
    }
}