#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using MateraizWebApp.Models;   // 👈 IMPORTANTE

namespace MateraizWebApp.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code },
                protocol: Request.Scheme);
            var mensaje = $@"
    <h2 style='color:#8B0000;'>Restablecer contraseña 🔐</h2>

    <p>Recibimos una solicitud para cambiar tu contraseña.</p>

    <div style='text-align:center; margin:30px 0;'>
        <a href='{callbackUrl}'
           style='background:#8B0000; color:white; padding:12px 25px;
                  text-decoration:none; border-radius:8px; font-weight:bold;'>
            Restablecer contraseña
        </a>
    </div>

    <p style='font-size:14px; color:#777;'>
        Si no solicitaste este cambio, ignora este mensaje.
    </p>";

            await _emailSender.SendEmailAsync(
                Input.Email,
                "Restablecer contraseña - Materaíz 🌿",
                mensaje);
            await _emailSender.SendEmailAsync(
                Input.Email,
                "Restablecer contraseña - Materaíz 🌿",
                $"Para restablecer tu contraseña haz clic aquí: <br/><br/>" +
                $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Restablecer contraseña</a>");

            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}