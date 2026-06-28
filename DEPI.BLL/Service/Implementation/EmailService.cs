using DEPI.BLL.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DEPI.BLL.Service.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpServer = _configuration["SMTP:Server"] ?? _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var portStr = _configuration["SMTP:Port"] ?? _configuration["EmailSettings:Port"] ?? "587";
                var fromEmail = _configuration["SMTP:SenderEmail"] ?? _configuration["SMTP:Username"] ?? _configuration["EmailSettings:SenderEmail"] ?? "pharmaworks.depi@gmail.com";
                var password = _configuration["SMTP:Password"] ?? _configuration["EmailSettings:SenderPassword"] ?? "";
                var enableSsl = bool.Parse(_configuration["SMTP:EnableSsl"] ?? _configuration["EmailSettings:EnableSsl"] ?? "true");

                var port = int.Parse(portStr);

                using (var client = new SmtpClient(smtpServer, port))
                {
                    client.EnableSsl = enableSsl;
                    client.UseDefaultCredentials = false;
                    
                    if (!string.IsNullOrEmpty(password))
                    {
                        client.Credentials = new NetworkCredential(fromEmail, password);
                    }

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail, "PharmaWorks System"),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    
                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    Console.WriteLine($"✓ Email sent successfully to {toEmail}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed to send email to {toEmail}: {ex.Message}");
            }
        }
    }
}
