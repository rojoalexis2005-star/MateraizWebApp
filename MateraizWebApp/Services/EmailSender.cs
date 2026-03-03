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
                    // SendGrid funciona mejor con StartTls en el puerto 587
                    await client.ConnectAsync("smtp.sendgrid.net", 587, SecureSocketOptions.StartTls);

                    // IMPORTANTE: El usuario siempre es la palabra "apikey"
                    await client.AuthenticateAsync("apikey", _settings.Password);

                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("***********************************");
                    Console.WriteLine("ERROR SENDGRID: " + ex.Message);
                    Console.WriteLine("***********************************");
                }
            }
        }
    }
}