using InternPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace InternPortal.Domain.Entities
{
    public class Application
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string University { get; set; } = string.Empty;

        [Required]
        public StudentGrade StudentGrade { get; set; }

        [Required]
        public Department Department { get; set; }

        [Required]
        public InternshipType InternshipType { get; set; }

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string CvUrl { get; set; } = string.Empty;

        [Required]
        public string TranscriptFile { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; } 

        [Required]
        public DateTime EndDate { get; set; } 

        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Beklemede;

        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public string Reference { get; set; } = string.Empty;
        public string ReferenceGsm { get; set; } = string.Empty;
        public string ReferenceClosenessStatus { get; set; } = string.Empty;
    }
}