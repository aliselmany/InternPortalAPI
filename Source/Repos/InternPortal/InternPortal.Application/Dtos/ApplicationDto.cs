using InternPortal.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace InternPortal.Application.Dtos;

public class ApplicationDto : IValidatableObject
{
    [Required(ErrorMessage = "University name is required.")]
    public string University { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select your grade.")]
    public StudentGrade Grade { get; set; }

    [Required(ErrorMessage = "Please select a department.")]
    public Department Department { get; set; }

    [Required(ErrorMessage = "Please select internship type.")]
    public InternshipType InternshipType { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^5\d{9}$", ErrorMessage = "Phone number must be 10 digits and start with 5.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required.")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required.")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    public string? Description { get; set; }

    [Required(ErrorMessage = "Please upload your CV.")]
    public required IFormFile CvFile { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      
        if (EndDate <= StartDate)
        {
            yield return new ValidationResult(
                "End date must be later than start date.",
                new[] { nameof(EndDate) });
        }

        if (StartDate.Date < DateTime.UtcNow.Date)
        {
            yield return new ValidationResult(
                "Start date cannot be in the past.",
                new[] { nameof(StartDate) });
        }

        var totalDays = (EndDate - StartDate).TotalDays;
        if (totalDays < 10)
        {
            yield return new ValidationResult(
                "Internship duration must be at least 10 days.",
                new[] { nameof(EndDate) });
        }
    }
}