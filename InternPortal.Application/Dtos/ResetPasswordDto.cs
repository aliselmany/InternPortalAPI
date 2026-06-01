using System;
using System.Collections.Generic;
using System.Text;

public class ResetPasswordDto
{
    public string Email { get; set; }
    public string Code { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; } 
}