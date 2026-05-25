using System;
using System.Collections.Generic;
using System.Text;

namespace InternPortal.Application.Dtos
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public string? Department { get; set; }
        public string? Expertise { get; set; }
        public string? Biography { get; set; }
        public string? PhoneNumber { get; set; }
        public int MaxInternCount { get; set; }
        public int CurrentInternCount { get; set; }
        public Guid? MentorId { get; set; }
        public List<SocialAccountDto> SocialAccounts { get; set; } = new();
    }
}
