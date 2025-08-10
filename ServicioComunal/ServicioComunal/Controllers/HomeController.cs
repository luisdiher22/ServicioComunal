using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioComunal.Data;
using ServicioComunal.Services;

namespace ServicioComunal.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataSeederService _seeder;
        private readonly ServicioComunalDbContext _context;

        public HomeController(DataSeederService seeder, ServicioComunalDbContext context)
        {
            _seeder = seeder;
            _context = context;
        }

        public IActionResult Dashboard()
        {
            // TODO: Implementar dashboard real
            ViewBag.Message = "¡Bienvenido al Dashboard de Servicio Comunal!";
            return View();
        }

        public async Task<IActionResult> SeedData()
        {
            try
            {
                await _seeder.SeedDataAsync();
                ViewBag.Message = "Datos de prueba insertados exitosamente!";
                ViewBag.Success = true;
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error al insertar datos: {ex.Message}";
                ViewBag.Success = false;
            }
            return View("Dashboard");
        }

        public async Task<IActionResult> ReseedUsuarios()
        {
            try
            {
                await _seeder.ReseedUsuariosAsync();
                ViewBag.Message = "Usuarios recreados exitosamente!";
                ViewBag.Success = true;
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error al recrear usuarios: {ex.Message}";
                ViewBag.Success = false;
            }
            return View("Dashboard");
        }

        public async Task<IActionResult> SeedUsuariosSafe()
        {
            try
            {
                await _seeder.SeedUsuariosSafeAsync();
                ViewBag.Message = "Usuarios creados de forma segura exitosamente!";
                ViewBag.Success = true;
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error al crear usuarios seguros: {ex.Message}";
                ViewBag.Success = false;
            }
            return View("Dashboard");
        }

        public async Task<IActionResult> VerificarUsuarios()
        {
            try
            {
                var usuarios = await _context.Usuarios.ToListAsync();
                var mensaje = $"Usuarios en BD: {usuarios.Count}\n\n";
                
                foreach (var user in usuarios)
                {
                    mensaje += $"- ID: {user.Identificacion}, Usuario: {user.NombreUsuario}, Rol: {user.Rol}, Activo: {user.Activo}\n";
                }
                
                ViewBag.Message = mensaje;
                ViewBag.Success = true;
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error verificando usuarios: {ex.Message}";
                ViewBag.Success = false;
            }
            return View("Dashboard");
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
