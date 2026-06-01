using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace InternPortal.Application.Interfaces
{
    public interface IMailService
    {
        void SendTestEmail(string toEmail);
        void SendPasswordResetEmail(string toEmail, string resetLink);
        void SendPasswordResetCode(string toEmail, string resetCode);
    }
}