using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioComunal.Data;
using ServicioComunal.Utilities;

namespace ServicioComunal.Controllers
{
    /// <summary>
    /// Controlador encargado de la autenticación y gestión de sesiones de usuarios.
    /// Maneja el login, logout y cambio de contraseñas del sistema.
    /// </summary>
    public class AuthController : Controller
    {
        private readonly ServicioComunalDbContext _context;

        public AuthController(ServicioComunalDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Muestra la página de inicio de sesión.
        /// </summary>
        /// <returns>Vista de login</returns>
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Procesa el inicio de sesión del usuario.
        /// Valida las credenciales y establece la sesión correspondiente.
        /// </summary>
        /// <param name="usuario">Nombre de usuario o número de identificación</param>
        /// <param name="contraseña">Contraseña del usuario</param>
        /// <param name="recordarme">Indica si recordar la sesión (no implementado)</param>
        /// <returns>Redirección al dashboard correspondiente según el rol</returns>
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

                // Obtener el nombre real del usuario según su rol
                string nombreCompleto = user.NombreUsuario; // Fallback al nombre de usuario
                string cedula = user.Identificacion.ToString();

                if (user.Rol == "Estudiante")
                {
                    var estudiante = await _context.Estudiantes
                        .FirstOrDefaultAsync(e => e.Identificacion == user.Identificacion);
                    if (estudiante != null)
                    {
                        nombreCompleto = $"{estudiante.Nombre} {estudiante.Apellidos}";
                    }
                }
                else if (user.Rol == "Profesor" || user.Rol == "Administrador")
                {
                    var profesor = await _context.Profesores
                        .FirstOrDefaultAsync(p => p.Identificacion == user.Identificacion);
                    if (profesor != null)
                    {
                        nombreCompleto = $"{profesor.Nombre} {profesor.Apellidos}";
                    }
                }

                // Establecer sesión del usuario
                HttpContext.Session.SetInt32("UsuarioId", user.Identificacion);
                HttpContext.Session.SetInt32("UsuarioIdentificacion", user.Identificacion);
                HttpContext.Session.SetString("UsuarioNombre", user.NombreUsuario);
                HttpContext.Session.SetString("UsuarioNombreCompleto", nombreCompleto);
                HttpContext.Session.SetString("UsuarioCedula", cedula);
                HttpContext.Session.SetString("UsuarioRol", user.Rol);

                // Verificar si necesita cambiar contraseña
                if (user.RequiereCambioContraseña)
                {
                    return RedirectToAction("CambiarContraseña", "Auth");
                }

                // Redireccionar según el rol
                if (user.Rol == "Profesor")
                {
                    return RedirectToAction("Dashboard", "Tutor");
                }
                else if (user.Rol == "Administrador")
                {
                    return RedirectToAction("Dashboard", "Home");
                }
                else if (user.Rol == "Estudiante")
                {
                    return RedirectToAction("Dashboard", "Estudiante");
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

        /// <summary>
        /// Muestra la página de recuperación de contraseña.
        /// </summary>
        /// <returns>Vista de recuperación de contraseña</returns>
        public IActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// Muestra la vista de cambio obligatorio de contraseña
        /// </summary>
        public IActionResult CambiarContraseña()
        {
            // Verificar que el usuario esté logueado
            var nombreUsuario = HttpContext.Session.GetString("UsuarioNombre");
            if (string.IsNullOrEmpty(nombreUsuario))
            {
                return RedirectToAction("Login");
            }

            ViewBag.NombreUsuario = nombreUsuario;
            return View();
        }

        /// <summary>
        /// Procesa el cambio de contraseña obligatorio
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CambiarContraseña(string contraseñaActual, string nuevaContraseña, string confirmarContraseña)
        {
            var nombreUsuario = HttpContext.Session.GetString("UsuarioNombre");
            if (string.IsNullOrEmpty(nombreUsuario))
            {
                return RedirectToAction("Login");
            }

            ViewBag.NombreUsuario = nombreUsuario;

            // Validaciones básicas
            if (string.IsNullOrEmpty(contraseñaActual) || string.IsNullOrEmpty(nuevaContraseña) || string.IsNullOrEmpty(confirmarContraseña))
            {
                ViewBag.Error = "Todos los campos son obligatorios";
                return View();
            }

            if (nuevaContraseña != confirmarContraseña)
            {
                ViewBag.Error = "La nueva contraseña y su confirmación no coinciden";
                return View();
            }

            try
            {
                var usuarioService = HttpContext.RequestServices.GetRequiredService<Services.UsuarioService>();
                var (exito, mensaje) = await usuarioService.CambiarContraseñaConValidacionAsync(nombreUsuario, contraseñaActual, nuevaContraseña);

                if (exito)
                {
                    ViewBag.Success = mensaje;
                    
                    // Actualizar la información del usuario en la base de datos para reflejar que ya no requiere cambio
                    var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
                    if (user != null)
                    {
                        // Redireccionar según el rol después del cambio exitoso
                        await Task.Delay(2000); // Pequeña pausa para mostrar el mensaje de éxito
                        
                        if (user.Rol == "Profesor")
                        {
                            return RedirectToAction("Dashboard", "Tutor");
                        }
                        else if (user.Rol == "Administrador")
                        {
                            return RedirectToAction("Dashboard", "Home");
                        }
                        else if (user.Rol == "Estudiante")
                        {
                            return RedirectToAction("Dashboard", "Estudiante");
                        }
                    }
                    
                    return RedirectToAction("Dashboard", "Home");
                }
                else
                {
                    ViewBag.Error = mensaje;
                    return View();
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Error interno del servidor. Inténtalo más tarde.";
                return View();
            }
        }

        /// <summary>
        /// Cierra la sesión del usuario actual y lo redirige al login.
        /// </summary>
        /// <returns>Redirección a la página de login</returns>
        /// <summary>
        /// Cierra la sesión del usuario actual y redirige al login.
        /// Limpia completamente todos los datos de sesión.
        /// </summary>
        /// <returns>Redirección a la página de login</returns>
        public IActionResult Logout()
        {
            // Limpiar toda la sesión
            HttpContext.Session.Clear();
            
            // Opcional: También invalidar la sesión completamente
            HttpContext.Session.Remove("UsuarioIdentificacion");
            HttpContext.Session.Remove("UsuarioRol");
            HttpContext.Session.Remove("UsuarioNombreCompleto");
            HttpContext.Session.Remove("UsuarioNombre");
            
            // Limpiar cookies si existen
            foreach (var cookie in Request.Cookies.Keys)
            {
                if (cookie.StartsWith("ServicioComunal") || cookie.Contains("Session"))
                {
                    Response.Cookies.Delete(cookie);
                }
            }
            
            // Agregar headers para evitar cache
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            
            return RedirectToAction("Login");
        }
    }
}
