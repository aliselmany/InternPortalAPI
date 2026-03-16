using InternPortal.Domain.Enums;

namespace InternPortal.Application.Dtos
{
    public class ApplicationDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime AppliedDate { get; set; }
        public Guid UserId { get; set; }

        public string SchoolName { get; set; } = string.Empty;
        public int Grade { get; set; }
        public string SelectedDepartment { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string CvUrl { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate {  get; set; }
        public string? Description { get; set; }

        public InternshipType InternshipType { get; set; }
    }
}