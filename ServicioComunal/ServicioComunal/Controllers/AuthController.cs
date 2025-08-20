using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioComunal.Data;
using ServicioComunal.Utilities;

namespace ServicioComunal.Controllers
{
    public class AuthController : Controller
    {
        private readonly ServicioComunalDbContext _context;

        public AuthController(ServicioComunalDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string usuario, string contraseña, bool recordarme = false)
        {
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contraseña))
            {
                ViewBag.Error = "Por favor ingresa tu usuario y contraseña";
                return View();
            }

            try
            {
                // Buscar usuario en la base de datos
                var user = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.NombreUsuario == usuario || u.Identificacion.ToString() == usuario);

                if (user == null || !user.Activo)
                {
                    ViewBag.Error = "Usuario no encontrado o inactivo";
                    return View();
                }

                // Verificar contraseña
                if (!PasswordHelper.VerifyPassword(contraseña, user.Contraseña))
                {
                    ViewBag.Error = "Contraseña incorrecta";
                    return View();
                }

                // Actualizar último acceso
                user.UltimoAcceso = DateTime.Now;
                await _context.SaveChangesAsync();

                // Establecer sesión del usuario
                HttpContext.Session.SetInt32("UsuarioId", user.Identificacion);
                HttpContext.Session.SetString("UsuarioNombre", user.NombreUsuario);
                HttpContext.Session.SetString("UsuarioRol", user.Rol);

                // TEMPORAL: Forzar redirección a tutor para debugging
                Console.WriteLine($"Usuario autenticado: {user.NombreUsuario}, Rol: {user.Rol}");

                if (user.Rol == "Profesor")
                {
                    Console.WriteLine("Redirigiendo a Dashboard de Tutor...");
                    return RedirectToAction("Dashboard", "Tutor");
                }
                else if (user.Rol == "Administrador")
                {
                    Console.WriteLine("Redirigiendo a Dashboard de Administrador...");
                    return RedirectToAction("Dashboard", "Home");
                }
                else
                {
                    ViewBag.Error = "Acceso no autorizado para este rol.";
                    return View();
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Error interno del servidor. Inténtalo más tarde.";
                // TODO: Log the exception
                return View();
            }
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        public IActionResult Logout()
        {
            // Limpiar sesión
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
