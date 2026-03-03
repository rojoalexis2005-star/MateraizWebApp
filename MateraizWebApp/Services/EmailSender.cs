using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MateraizWebApp.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

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
            // Usamos la API Key que ya tienes configurada en Render
            var client = new SendGridClient(_settings.Password);

            var from = new EmailAddress("5723110143@utrng.edu.mx", "Materaíz 🌿");
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);

            try
            {
                var response = await client.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ Correo enviado exitosamente vía API de SendGrid");
                }
                else
                {
                    Console.WriteLine($"❌ Error API SendGrid: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error crítico: {ex.Message}");
            }
        }
    }
}