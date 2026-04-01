namespace InternPortal.Application.Dtos;

public class AvailableMentorDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Expertise { get; set; }
    public string? Biography { get; set; }
    public int MaxCapacity { get; set; }
    public int CurrentCount { get; set; }
    public int RemainingCapacity => MaxCapacity - CurrentCount;
    public List<SocialAccountDto> SocialAccounts { get; set; } = new();
}

public class SocialAccountDto
{
    public string PlatformName { get; set; } = string.Empty; 
    public string ProfileUrl { get; set; } = string.Empty;   
}