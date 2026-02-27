using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MateraizWebApp.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MateraizWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SobreProducto()
        {
            return View();
        }

        public IActionResult Beneficios()
        {
            return View();
        }

        // ?? GALERÍA DINÁMICA
        public async Task<IActionResult> Galeria()
        {
            var productosConImagen = await _context.Productos
                .Where(p => !string.IsNullOrEmpty(p.ImagenUrl))
                .ToListAsync();

            return View(productosConImagen);
        }

        public IActionResult Contacto()
        {
            return View();
        }
    }
}
