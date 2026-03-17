using InternPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace InternPortal.Domain.Entities
{
    public class Application
    {
        public Guid Id { get; set; } = Guid.NewGuid(); 

        [Required]
        public string SchoolName { get; set; } = string.Empty;

        [Required]
        public StudentGrade StudentGrade { get; set; } 

        [Required]
        public Department SelectedDepartment { get; set; } 

        [Required]
        public InternshipType InternshipType { get; set; } 

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string CvUrl { get; set; } = string.Empty; 

        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public DateTime AppliedDate { get; set; } = DateTime.UtcNow; 

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending; 

        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}