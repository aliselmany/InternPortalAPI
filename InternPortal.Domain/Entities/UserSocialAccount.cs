using System;
using System.Collections.Generic;
using System.Text;

namespace InternPortal.Domain.Entities;

public class UserSocialAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PlatformName { get; set; } = string.Empty;
    public string ProfileUrl { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public bool IsDeleted { get; set; } = false;
}