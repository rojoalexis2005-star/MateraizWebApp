using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MateraizWebApp.Models;

namespace MateraizWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 🔹 LISTA USUARIOS CON ROL
        public async Task<IActionResult> Index()
        {
            var usuarios = _userManager.Users.ToList();

            var lista = new List<UsuarioRolViewModel>();

            foreach (var user in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(user);

                lista.Add(new UsuarioRolViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Nombre = user.Nombre,
                    RolActual = roles.FirstOrDefault() ?? "Sin Rol"
                });
            }

            return View(lista);
        }

        [HttpPost]
        public async Task<IActionResult> CambiarRol(string userId, string nuevoRol)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            // Verificar que el rol exista
            if (!await _roleManager.RoleExistsAsync(nuevoRol))
            {
                TempData["Error"] = "El rol no existe.";
                return RedirectToAction("Index");
            }

            var rolesActuales = await _userManager.GetRolesAsync(user);

            // Remover todos los roles actuales
            if (rolesActuales.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesActuales);
                if (!removeResult.Succeeded)
                {
                    TempData["Error"] = "Error al remover rol.";
                    return RedirectToAction("Index");
                }
            }

            // Agregar nuevo rol
            var addResult = await _userManager.AddToRoleAsync(user, nuevoRol);

            if (!addResult.Succeeded)
            {
                TempData["Error"] = "Error al asignar rol.";
                return RedirectToAction("Index");
            }

            TempData["Success"] = "Rol actualizado correctamente.";

            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public async Task<IActionResult> EliminarUsuario(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            // 🔥 Evitar eliminar al último Admin
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin"))
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1)
                {
                    TempData["Error"] = "No puedes eliminar el último administrador.";
                    return RedirectToAction("Index");
                }
            }

            await _userManager.DeleteAsync(user);

            return RedirectToAction("Index");
        }
    }
}