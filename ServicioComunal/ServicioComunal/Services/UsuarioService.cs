using ServicioComunal.Data;
using ServicioComunal.Models;
using ServicioComunal.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ServicioComunal.Services
{
    /// <summary>
    /// Servicio para manejar la creación y gestión de usuarios
    /// </summary>
    public class UsuarioService
    {
        private readonly ServicioComunalDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsuarioService(ServicioComunalDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Obtiene el usuario actualmente autenticado desde la sesión
        /// </summary>
        /// <returns>Usuario actual o null si no está autenticado</returns>
        public Usuario? ObtenerUsuarioActual()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            var identificacion = session.GetInt32("UsuarioIdentificacion");
            if (!identificacion.HasValue) return null;

            return _context.Usuarios.FirstOrDefault(u => u.Identificacion == identificacion.Value);
        }

        /// <summary>
        /// Crea un usuario para un estudiante existente
        /// </summary>
        /// <param name="estudianteId">ID del estudiante</param>
        /// <param name="nombreUsuario">Nombre de usuario (opcional, si no se proporciona usa la cédula)</param>
        /// <returns>Contraseña temporal generada</returns>
        public async Task<string> CrearUsuarioEstudianteAsync(int estudianteId, string? nombreUsuario = null)
        {
            // Verificar que el estudiante existe
            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e => e.Identificacion == estudianteId);

            if (estudiante == null)
                throw new ArgumentException("Estudiante no encontrado");

            // Verificar que no existe ya un usuario para este estudiante
            var usuarioExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Identificacion == estudianteId);

            if (usuarioExistente != null)
                throw new InvalidOperationException("Ya existe un usuario para este estudiante");

            // Generar nombre de usuario si no se proporciona
            if (string.IsNullOrEmpty(nombreUsuario))
            {
                nombreUsuario = estudianteId.ToString();
            }

            // Verificar que el nombre de usuario no está en uso
            var usuarioConMismoNombre = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

            if (usuarioConMismoNombre != null)
                throw new InvalidOperationException("El nombre de usuario ya está en uso");

            // Generar contraseña temporal
            string contraseñaTemporal = PasswordHelper.GenerateTemporaryPassword();
            string contraseñaHash = PasswordHelper.HashPassword(contraseñaTemporal);

            // Crear usuario
            var usuario = new Usuario
            {
                Identificacion = estudianteId,
                NombreUsuario = nombreUsuario,
                Contraseña = contraseñaHash,
                Rol = "Estudiante",
                FechaCreacion = DateTime.Now,
                Activo = true
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return contraseñaTemporal;
        }

        /// <summary>
        /// Crea un usuario para un profesor existente
        /// </summary>
        /// <param name="profesorId">ID del profesor</param>
        /// <param name="nombreUsuario">Nombre de usuario (opcional, si no se proporciona usa la cédula)</param>
        /// <returns>Contraseña temporal generada</returns>
        public async Task<string> CrearUsuarioProfesorAsync(int profesorId, string? nombreUsuario = null)
        {
            // Verificar que el profesor existe
            var profesor = await _context.Profesores
                .FirstOrDefaultAsync(p => p.Identificacion == profesorId);

            if (profesor == null)
                throw new ArgumentException("Profesor no encontrado");

            // Verificar que no existe ya un usuario para este profesor
            var usuarioExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Identificacion == profesorId);

            if (usuarioExistente != null)
                throw new InvalidOperationException("Ya existe un usuario para este profesor");

            // Generar nombre de usuario si no se proporciona
            if (string.IsNullOrEmpty(nombreUsuario))
            {
                nombreUsuario = profesorId.ToString();
            }

            // Verificar que el nombre de usuario no está en uso
            var usuarioConMismoNombre = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

            if (usuarioConMismoNombre != null)
                throw new InvalidOperationException("El nombre de usuario ya está en uso");

            // Generar contraseña temporal
            string contraseñaTemporal = PasswordHelper.GenerateTemporaryPassword();
            string contraseñaHash = PasswordHelper.HashPassword(contraseñaTemporal);

            // Crear usuario
            var usuario = new Usuario
            {
                Identificacion = profesorId,
                NombreUsuario = nombreUsuario,
                Contraseña = contraseñaHash,
                Rol = "Profesor",
                FechaCreacion = DateTime.Now,
                Activo = true
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return contraseñaTemporal;
        }

        /// <summary>
        /// Crea usuarios masivamente para estudiantes importados
        /// </summary>
        /// <param name="estudianteIds">Lista de IDs de estudiantes</param>
        /// <returns>Diccionario con ID del estudiante y su contraseña temporal</returns>
        public async Task<Dictionary<int, string>> CrearUsuariosMasivoEstudiantesAsync(List<int> estudianteIds)
        {
            var resultado = new Dictionary<int, string>();

            foreach (var estudianteId in estudianteIds)
            {
                try
                {
                    var contraseñaTemporal = await CrearUsuarioEstudianteAsync(estudianteId);
                    resultado.Add(estudianteId, contraseñaTemporal);
                }
                catch (Exception ex)
                {
                    // Log del error, pero continuar con los demás
                    Console.WriteLine($"Error creando usuario para estudiante {estudianteId}: {ex.Message}");
                }
            }

            return resultado;
        }

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario</param>
        /// <param name="contraseñaActual">Contraseña actual</param>
        /// <param name="nuevaContraseña">Nueva contraseña</param>
        /// <returns>True si se cambió exitosamente</returns>
        public async Task<bool> CambiarContraseñaAsync(string nombreUsuario, string contraseñaActual, string nuevaContraseña)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

            if (usuario == null || !PasswordHelper.VerifyPassword(contraseñaActual, usuario.Contraseña))
                return false;

            usuario.Contraseña = PasswordHelper.HashPassword(nuevaContraseña);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
