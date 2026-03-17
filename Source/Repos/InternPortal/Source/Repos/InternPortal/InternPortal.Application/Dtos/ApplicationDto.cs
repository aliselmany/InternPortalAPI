namespace InternPortal.Application.Dtos
{
    public class ApplicationDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime AppliedDate { get; set; }
        public Guid UserId { get; set; }
    }
}