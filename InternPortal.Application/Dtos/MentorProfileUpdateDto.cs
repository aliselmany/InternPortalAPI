using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace InternPortal.Application.Dtos;

public class MentorProfileUpdateDto
{
    public string? Expertise { get; set; }

    public string? Biography { get; set; }

    public int? MaxInternCount { get; set; }

    public List<SocialAccountDto> SocialAccounts { get; set; } = new();
}