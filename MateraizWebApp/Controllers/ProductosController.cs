using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MateraizWebApp.Data;
using MateraizWebApp.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace MateraizWebApp.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary;

        // Inyectamos el contexto y la configuración de Cloudinary
        public ProductosController(ApplicationDbContext context, IOptions<CloudinarySettings> config)
        {
            _context = context;
            // Autenticación con Cloudinary usando tus credenciales de Render
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(acc);
        }

        // ================================
        // INDEX (Público - Catálogo)
        // ================================
        public async Task<IActionResult> Index()
        {
            return View(await _context.Productos.ToListAsync());
        }

        // ================================
        // DETAILS (Público)
        // ================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null) return NotFound();

            return View(producto);
        }

        // ================================
        // CREATE (Solo Admin)
        // ================================
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (ModelState.IsValid)
            {
                // 🔥 NUEVA LÓGICA: Subida a Cloudinary
                if (producto.ImagenArchivo != null && producto.ImagenArchivo.Length > 0)
                {
                    try
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(producto.ImagenArchivo.FileName, producto.ImagenArchivo.OpenReadStream()),
                            Folder = "materaiz_productos",
                            Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                        };

                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        producto.ImagenUrl = uploadResult.SecureUrl.ToString();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error Cloudinary: {ex.Message}");
                    }
                }

                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }

        // ================================
        // EDIT (Solo Admin)
        // ================================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Producto producto)
        {
            if (id != producto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var productoDb = await _context.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    if (productoDb == null) return NotFound();

                    // 🔥 NUEVA LÓGICA: Actualizar imagen en Cloudinary si se sube una nueva
                    if (producto.ImagenArchivo != null && producto.ImagenArchivo.Length > 0)
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(producto.ImagenArchivo.FileName, producto.ImagenArchivo.OpenReadStream()),
                            Folder = "materaiz_productos"
                        };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        producto.ImagenUrl = uploadResult.SecureUrl.ToString();
                    }
                    else
                    {
                        // Mantener la URL anterior si no se subió una nueva
                        producto.ImagenUrl = productoDb.ImagenUrl;
                    }

                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }

        // ================================
        // DELETE (Solo Admin)
        // ================================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos.FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null) return NotFound();
            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                // Opcional: Podrías eliminar la imagen de Cloudinary aquí usando el PublicId
                _context.Productos.Remove(producto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}