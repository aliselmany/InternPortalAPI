using InternPortal.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace InternPortal.Application.Services
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _configuration;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendTestEmail(string toEmail)
        {
            string host = _configuration["MailSettings:Host"] ?? "localhost";
            int port = int.Parse(_configuration["MailSettings:Port"] ?? "2525");

            using (SmtpClient client = new SmtpClient(host, port))
            {
                client.EnableSsl = false;
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("sistem@internportal.com", "Intern Portal");
                mailMessage.To.Add(toEmail);
                mailMessage.Subject = "Docker ve SMTP Testi Başarılı!";
                mailMessage.Body = "Harika! MailService smtp4dev ile kusursuz konuşuyor.";
                client.Send(mailMessage);
            }
        }

        public void SendPasswordResetEmail(string toEmail, string resetLink)
        {
            string host = _configuration["MailSettings:Host"] ?? "localhost";
            int port = int.Parse(_configuration["MailSettings:Port"] ?? "2525");

            using (SmtpClient client = new SmtpClient(host, port))
            {
                client.EnableSsl = false;

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("sistem@internportal.com", "trex Digital");
                mailMessage.To.Add(toEmail);
                mailMessage.Subject = "Şifre Sıfırlama Talebi - trex";

                mailMessage.IsBodyHtml = true;

                mailMessage.Body = $@"
                     <div style='font-family: Arial, sans-serif; background-color: #1a1a1a; color: #ffffff; padding: 30px; text-align: center; border-radius: 10px; max-width: 500px; margin: auto;'>
                         <h2 style='color: #4caf50;'>trex Digital Manufacturing</h2>
                         <h3>Şifre Sıfırlama Talebi</h3>
                         <p style='color: #cccccc;'>Hesabınızın şifresini sıfırlamak için bir talepte bulundunuz. Aşağıdaki butona tıklayarak yeni şifrenizi belirleyebilirsiniz:</p>
                         <br/>
                         <a href='{resetLink}' style='background-color: #ffffff; color: #1a1a1a; padding: 12px 25px; text-align: center; text-decoration: none; display: inline-block; font-weight: bold; border-radius: 5px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>Şifremi Sıfırla</a>
                         <br/><br/>
                         <p style='font-size: 11px; color: #888888;'>Eğer bu talebi siz yapmadıysanız, lütfen bu e-postayı dikkate almayınız.</p>
                    </div>";
                client.Send(mailMessage);
            }
        }

        
        public void SendPasswordResetCode(string toEmail, string resetCode)
        {
            string host = _configuration["MailSettings:Host"] ?? "localhost";
            int port = int.Parse(_configuration["MailSettings:Port"] ?? "2525");

            using (SmtpClient client = new SmtpClient(host, port))
            {
                client.EnableSsl = false;

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("sistem@internportal.com", "trex Digital");
                mailMessage.To.Add(toEmail);
                mailMessage.Subject = "Şifre Sıfırlama Onay Kodu - trex";
                mailMessage.IsBodyHtml = true;

                mailMessage.Body = $@"
                    <div style='font-family: Arial, sans-serif; background-color: #1a1a1a; color: #ffffff; padding: 30px; text-align: center; border-radius: 10px; max-width: 500px; margin: auto;'>
                        <h2 style='color: #4caf50;'>trex Digital Manufacturing</h2>
                        <h3>Şifre Sıfırlama Onay Kodu</h3>
                        <p style='color: #cccccc;'>Hesabınızın şifresini sıfırlamak için gereken 6 haneli onay kodunuz aşağıdadır. Bu kodu şifre sıfırlama ekranına giriniz:</p>
                        <br/>
                        <div style='background-color: #2a2a2a; color: #ffffff; font-size: 32px; font-weight: bold; letter-spacing: 8px; padding: 15px; display: inline-block; border-radius: 5px; border: 2px dashed #4caf50;'>
                            {resetCode}
                        </div>
                        <br/><br/>
                        <p style='font-size: 11px; color: #888888;'>Bu kod güvenlik nedeniyle kısa bir süre için geçerlidir. Talebi siz yapmadıysanız bu maili önemsemeyiniz.</p>
                    </div>";

                client.Send(mailMessage);
            }
        }
    }
}