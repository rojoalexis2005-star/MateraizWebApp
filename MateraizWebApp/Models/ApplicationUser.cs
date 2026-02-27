using Microsoft.AspNetCore.Identity;

namespace MateraizWebApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Nombre { get; set; }
    }
}