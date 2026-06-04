using InternPortal.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace InternPortal.Application.Dtos;

public class ApplicationDto : IValidatableObject
{
    [Required(ErrorMessage = "Lütfen eğitim seviyenizi seçiniz.")]
    public string EducationLevel { get; set; }
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lütfen okul adını giriniz.")]
    public string SchoolName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lütfen bölümünüzü veya alanınızı giriniz.")]
    public string DepartmentOfStudy { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lütfen sınıfınızı seçiniz.")]
    public string Grade { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lütfen staj departmanını seçiniz.")]
    public Department Department { get; set; }

    [Required(ErrorMessage = "Lütfen staj türünü seçiniz.")]
    public InternshipType InternshipType { get; set; }

    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [RegularExpression(@"^5\d{9}$", ErrorMessage = "Telefon numarası 5 ile başlamalı ve 10 haneli olmalıdır.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    public string? Description { get; set; }

    [Required(ErrorMessage = "Lütfen özgeçmiş (CV) dosyanızı yükleyiniz.")]
    public IFormFile CvFile { get; set; } = null!;

    public IFormFile? TranscriptFile { get; set; }

    public string? CvPath { get; set; }
    public string? TranscriptPath { get; set; }

    public ApplicationStatus Status { get; set; }

    public string? Reference { get; set; } = string.Empty;

    [RegularExpression(@"^5\d{9}$", ErrorMessage = "Referans telefonu 5 ile başlamalı ve 10 haneli olmalıdır.")]
    public string? ReferenceGsm { get; set; } = string.Empty;

    public string? ReferenceClosenessStatus { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate <= StartDate)
        {
            yield return new ValidationResult(
                "Bitiş tarihi başlangıç tarihinden sonra olmalıdır.",
                new[] { nameof(EndDate) });
        }

        if (StartDate.Date < DateTime.UtcNow.Date)
        {
            yield return new ValidationResult(
                "Başlangıç tarihi geçmiş bir tarih olamaz.",
                new[] { nameof(StartDate) });
        }

        var totalDays = (EndDate - StartDate).TotalDays;
        if (totalDays < 10)
        {
            yield return new ValidationResult(
                "Staj süresi en az 10 gün olmalıdır.",
                new[] { nameof(EndDate) });
        }
    }
}