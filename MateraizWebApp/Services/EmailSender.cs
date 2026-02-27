using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using MateraizWebApp.Models;

namespace MateraizWebApp.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;

        public EmailSender(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Email, _settings.Password),
                EnableSsl = true
            };

            // 🎨 PLANTILLA PROFESIONAL HTML
            string plantilla = $@"
    <div style='background-color:#f4f6f8; padding:40px 0; font-family:Arial, sans-serif;'>
        <div style='max-width:600px; margin:auto; background:white; border-radius:15px; 
                    box-shadow:0 5px 20px rgba(0,0,0,0.08); overflow:hidden;'>

            <!-- HEADER -->
            <div style='background:#556B2F; padding:25px; text-align:center;'>
                <h1 style='color:white; margin:0; font-size:24px;'>
                    🌿 Materaíz
                </h1>
                <p style='color:#e6f0d4; margin:5px 0 0 0; font-size:14px;'>
                    Macetas Artesanales
                </p>
            </div>

            <!-- BODY -->
            <div style='padding:35px 30px; color:#333; font-size:16px; line-height:1.6;'>
                {htmlMessage}
            </div>

            <!-- FOOTER -->
            <div style='background:#f9f9f9; padding:20px; text-align:center; font-size:12px; color:#777;'>
                © 2026 Materaíz 🌿 <br/>
                Este correo fue enviado automáticamente, por favor no respondas a este mensaje.
            </div>

        </div>
    </div>";

            var mail = new MailMessage
            {
                From = new MailAddress(_settings.Email, "Materaíz 🌿"),
                Subject = subject,
                Body = plantilla,
                IsBodyHtml = true
            };

            mail.To.Add(email);

            await client.SendMailAsync(mail);
        }
    }
}