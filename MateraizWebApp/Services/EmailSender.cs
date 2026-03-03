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
            // Usamos la API Key configurada en Render (EmailSettings__Password)
            var client = new SendGridClient(_settings.Password);

            // DEBE ser exactamente el verificado en tu imagen de SendGrid
            var from = new EmailAddress("5723110143@utrng.edu.mx", "Materaíz 🌿");
            var to = new EmailAddress(email);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);

            try
            {
                var response = await client.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ ¡ÉXITO! Correo enviado vía API.");
                }
                else
                {
                    // Esto nos dirá en los logs de Render el motivo exacto si falla
                    var errorBody = await response.Body.ReadAsStringAsync();
                    Console.WriteLine($"❌ Error API SendGrid: {response.StatusCode} - {errorBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error crítico: {ex.Message}");
            }
        }
    }
}