using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MateraizWebApp.Data;
using MateraizWebApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MateraizWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===============================
        // INICIO
        // ===============================
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

        public IActionResult Contacto()
        {
            return View();
        }

        // ===============================
        // GALERÍA (CORREGIDA)
        // ===============================
        public async Task<IActionResult> Galeria()
        {
            try
            {
                var productosConImagen = await _context.Productos
                    .Where(p => !string.IsNullOrEmpty(p.ImagenUrl))
                    .ToListAsync();

                // Evita null en la vista
                if (productosConImagen == null)
                {
                    productosConImagen = new List<Producto>();
                }

                return View(productosConImagen);
            }
            catch (Exception ex)
            {
                // ?? Mostrar error real (solo para debug)
                return Content("ERROR EN GALERÍA:\n\n" + ex.ToString());
            }
        }
    }
}