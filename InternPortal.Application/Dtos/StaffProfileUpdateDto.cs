using System;
using System.Collections.Generic;
using System.Text;

namespace InternPortal.Application.Dtos;

public class StaffProfileUpdateDto
{
    public string? Expertise { get; set; }
    public string? Biography { get; set; }
    public int MaxInternCount { get; set; }
    public List<SocialAccountDto> SocialAccounts { get; set; } = new();
}