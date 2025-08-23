using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioComunal.Services;

namespace ServicioComunal.Controllers
{
    public class AdminController : Controller
    {
        private readonly DataSeederService _dataSeeder;

        public AdminController(DataSeederService dataSeeder)
        {
            _dataSeeder = dataSeeder;
        }

        // Endpoint temporal para regenerar datos
        public async Task<IActionResult> RegenerarDatos()
        {
            try
            {
                await _dataSeeder.LimpiarYRegenerarAsync();
                return Json(new { 
                    success = true, 
                    message = "✅ Datos regenerados exitosamente. Ahora puedes probar con los usuarios tutores: patricia.rodriguez, miguel.sanchez, elena.castro (contraseña: password123)" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"❌ Error: {ex.Message}" 
                });
            }
        }

        // Ver estado de usuarios
        public async Task<IActionResult> VerUsuarios()
        {
            var context = _dataSeeder.Context;
            
            var usuarios = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                context.Usuarios.OrderBy(u => u.Rol).ThenBy(u => u.NombreUsuario)
            );
            
            var resultado = usuarios.Select(u => new {
                u.Identificacion,
                u.NombreUsuario,
                u.Rol,
                u.Activo,
                u.FechaCreacion
            }).ToList();
            
            return Json(new { success = true, usuarios = resultado });
        }
    }
}
