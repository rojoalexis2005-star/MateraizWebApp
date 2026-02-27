using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MateraizWebApp.Data;
using MateraizWebApp.Models;

namespace MateraizWebApp.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
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
            if (id == null)
                return NotFound();

            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null)
                return NotFound();

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

        // POST: CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (ModelState.IsValid)
            {
                if (producto.ImagenArchivo != null)
                {
                    string carpeta = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot/images/productos");

                    if (!Directory.Exists(carpeta))
                        Directory.CreateDirectory(carpeta);

                    string nombreArchivo = Guid.NewGuid().ToString() +
                                           Path.GetExtension(producto.ImagenArchivo.FileName);

                    string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await producto.ImagenArchivo.CopyToAsync(stream);
                    }

                    producto.ImagenUrl = "/images/productos/" + nombreArchivo;
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
            if (id == null)
                return NotFound();

            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound();

            return View(producto);
        }

        // POST: EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Producto producto)
        {
            if (id != producto.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var productoDb = await _context.Productos.FindAsync(id);

                    if (productoDb == null)
                        return NotFound();

                    productoDb.Nombre = producto.Nombre;
                    productoDb.Descripcion = producto.Descripcion;
                    productoDb.Precio = producto.Precio;
                    productoDb.Tamaño = producto.Tamaño;

                    if (producto.ImagenArchivo != null)
                    {
                        string carpeta = Path.Combine(Directory.GetCurrentDirectory(),
                            "wwwroot/images/productos");

                        if (!Directory.Exists(carpeta))
                            Directory.CreateDirectory(carpeta);

                        string nombreArchivo = Guid.NewGuid().ToString() +
                                               Path.GetExtension(producto.ImagenArchivo.FileName);

                        string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                        {
                            await producto.ImagenArchivo.CopyToAsync(stream);
                        }

                        productoDb.ImagenUrl = "/images/productos/" + nombreArchivo;
                    }

                    _context.Update(productoDb);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
                        return NotFound();
                    else
                        throw;
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
            if (id == null)
                return NotFound();

            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null)
                return NotFound();

            return View(producto);
        }

        // POST: DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto != null)
            {
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
