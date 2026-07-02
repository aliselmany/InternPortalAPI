using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InternPortal.Domain.Enums; 

namespace InternPortal.Domain.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Surname is required.")]
    [MaxLength(50)]
    public string Surname { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string? Expertise { get; set; }
    public string? Biography { get; set; }
    public int MaxInternCount { get; set; } = 0;

    public string? University { get; set; }
    public string? Department { get; set; } 
    public string? PhoneNumber { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public Department? ManagedDepartment { get; set; }

    public Guid? MentorId { get; set; }

    [ForeignKey("MentorId")]
    public virtual User? Mentor { get; set; }

    [MaxLength(6)]
    public string? PasswordResetCode { get; set; }
    public DateTime? PasswordResetCodeExpiration { get; set; }

    public bool IsEmailVerified { get; set; } = false;

    [MaxLength(6)]
    public string? VerificationCode { get; set; }

    public DateTime? VerificationCodeExpiration { get; set; }

    public bool IsDeleted { get; set; } = false;

    public virtual ICollection<User> Interns { get; set; } = new List<User>();
    public virtual ICollection<UserSocialAccount> SocialAccounts { get; set; } = new List<UserSocialAccount>();
    public virtual ICollection<UserRoleMapping> UserRoles { get; set; } = new List<UserRoleMapping>();
    public virtual Application? Application { get; set; }
}