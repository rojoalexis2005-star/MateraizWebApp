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
            message.From.Add(new MailboxAddress("Materaíz 🌿", _settings.Email));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;

            // Plantilla profesional de Materaíz
            string plantilla = $@"
            <div style='background-color:#f4f6f8; padding:40px 0; font-family:Arial, sans-serif;'>
                <div style='max-width:600px; margin:auto; background:white; border-radius:15px; box-shadow:0 5px 20px rgba(0,0,0,0.08); overflow:hidden;'>
                    <div style='background:#556B2F; padding:25px; text-align:center;'>
                        <h1 style='color:white; margin:0; font-size:24px;'>🌿 Materaíz</h1>
                        <p style='color:#e6f0d4; margin:5px 0 0 0; font-size:14px;'>Macetas Artesanales</p>
                    </div>
                    <div style='padding:35px 30px; color:#333; font-size:16px; line-height:1.6;'>
                        {htmlMessage}
                    </div>
                    <div style='background:#f9f9f9; padding:20px; text-align:center; font-size:12px; color:#777;'>
                        © 2026 Materaíz 🌿 <br/>
                        Este correo fue enviado automáticamente, por favor no respondas.
                    </div>
                </div>
            </div>";

            var bodyBuilder = new BodyBuilder { HtmlBody = plantilla };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    // Conexión usando el puerto 465 (SslOnConnect)
                    await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.SslOnConnect);

                    await client.AuthenticateAsync(_settings.Email, _settings.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    // Esto imprimirá el error real en los Logs de Render si el envío falla
                    Console.WriteLine("***********************************");
                    Console.WriteLine("ERROR ENVIANDO CORREO: " + ex.Message);
                    Console.WriteLine("***********************************");
                    throw; // Re-lanzamos para que Identity sepa que hubo un problema
                }
            }
        }
    }
}