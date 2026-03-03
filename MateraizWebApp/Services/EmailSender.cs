using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MateraizWebApp.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

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
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Materaíz 🌿", "5723110143@utrng.edu.mx")); // Tu correo verificado en SendGrid
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    // Conexión segura estándar para SendGrid
                    await client.ConnectAsync("smtp.sendgrid.net", 587, SecureSocketOptions.StartTls);

                    // Autenticación: El usuario siempre es "apikey" y el password es tu código SG
                    await client.AuthenticateAsync("apikey", _settings.Password);

                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                    Console.WriteLine("✅ Correo enviado con éxito a través de SendGrid");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Error crítico en EmailSender: " + ex.Message);
                }
            }
        }
    }
}