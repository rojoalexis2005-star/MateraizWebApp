using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace MateraizWebApp.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }

        // 🔹 ESTA ES LA RUTA QUE SE GUARDA EN BD
        public string? ImagenUrl { get; set; }

        // 🔹 ESTE ES SOLO PARA RECIBIR EL ARCHIVO
        [NotMapped]
        public IFormFile? ImagenArchivo { get; set; }

        public string? Tamaño { get; set; }
    }
}
