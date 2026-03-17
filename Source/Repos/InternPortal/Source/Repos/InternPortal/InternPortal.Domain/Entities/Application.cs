using InternPortal.Domain.Enums;

namespace InternPortal.Domain.Entities;

public class Application
{
    public Guid Id { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public int Grade { get; set; }
    public Department SelectedDepartment { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string CvUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
}