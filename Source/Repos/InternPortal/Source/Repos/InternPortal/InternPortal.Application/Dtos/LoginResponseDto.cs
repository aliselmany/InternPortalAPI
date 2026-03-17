using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace InternPortal.Application.Dtos
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = "Login Successful"; 

    }
}    
