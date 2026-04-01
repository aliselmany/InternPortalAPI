using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace InternPortal.Application.Dtos;

public class StaffProfileUpdateDto
{
    [DefaultValue("")]
    public string? Expertise { get; set; }

    [DefaultValue("")]
    public string? Biography { get; set; }

    [DefaultValue("")]
    public int? MaxInternCount { get; set; }

    [DefaultValue("")]
    public List<SocialAccountDto> SocialAccounts { get; set; } = new();
}