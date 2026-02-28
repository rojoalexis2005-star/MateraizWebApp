using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using MateraizWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace MateraizWebApp.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El nombre es obligatorio")]
            [Display(Name = "Nombre")]
            public string Nombre { get; set; }

            [Required(ErrorMessage = "El correo es obligatorio")]
            [EmailAddress]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; }

            [Required(ErrorMessage = "La contraseña es obligatoria")]
            [StringLength(100, ErrorMessage = "La contraseña debe tener al menos 6 caracteres", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    Nombre = Input.Nombre
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    // 🔥 Asegurar que el rol Cliente existe
                    if (!await _roleManager.RoleExistsAsync("Cliente"))
                        await _roleManager.CreateAsync(new IdentityRole("Cliente"));

                    // 🔥 Asignar rol Cliente automáticamente
                    await _userManager.AddToRoleAsync(user, "Cliente");

                    // Generar token de confirmación
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        null,
                        new { area = "Identity", userId = user.Id, code = code },
                        Request.Scheme);

                    // 🎨 Correo bonito
                    var mensaje = $@"
                        <h2 style='color:#556B2F;'>¡Bienvenido a Materaíz! 🌿</h2>
                        <p>Hola <strong>{Input.Nombre}</strong>, gracias por registrarte.</p>
                        <p>Para activar tu cuenta haz clic en el botón:</p>
                        <div style='text-align:center; margin:30px 0;'>
                            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'
                               style='background:#556B2F; color:white; padding:12px 25px;
                                      text-decoration:none; border-radius:8px; font-weight:bold;'>
                                Confirmar mi cuenta
                            </a>
                        </div>
                        <p style='font-size:14px; color:#777;'>
                            Si tú no realizaste este registro puedes ignorar este correo.
                        </p>";

                    await _emailSender.SendEmailAsync(
                        Input.Email,
                        "Confirmar tu cuenta - Materaíz 🌿",
                        mensaje);

                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}