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
        /// Elimina tildes y caracteres especiales de una cadena
        /// </summary>
        /// <param name="texto">Texto a normalizar</param>
        /// <returns>Texto sin tildes ni caracteres especiales</returns>
        private static string EliminarTildes(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return texto;

            var normalizedString = texto.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        /// <summary>
        /// Genera un nombre de usuario único basado en el nombre y apellidos
        /// </summary>
        /// <param name="nombre">Nombre completo</param>
        /// <param name="apellidos">Apellidos</param>
        /// <returns>Nombre de usuario único</returns>
        private async Task<string> GenerarNombreUsuarioAsync(string nombre, string apellidos)
        {
            // Obtener primer nombre y apellidos
            string primerNombre = EliminarTildes(nombre.Split(' ')[0].ToLower());
            string[] apellidosArray = apellidos.Split(' ');
            string primerApellido = EliminarTildes(apellidosArray[0].ToLower());
            
            // Formato inicial: primer.apellido
            string nombreUsuario = $"{primerNombre}.{primerApellido}";
            
            // Verificar si ya existe
            var existeUsuario = await _context.Usuarios
                .AnyAsync(u => u.NombreUsuario == nombreUsuario);
            
            // Si existe, agregar segundo apellido
            if (existeUsuario && apellidosArray.Length > 1)
            {
                string segundoApellido = EliminarTildes(apellidosArray[1].ToLower());
                nombreUsuario = $"{primerNombre}.{primerApellido}.{segundoApellido}";
                
                // Verificar nuevamente
                existeUsuario = await _context.Usuarios
                    .AnyAsync(u => u.NombreUsuario == nombreUsuario);
            }
            
            // Si aún existe, agregar un número incremental
            if (existeUsuario)
            {
                int contador = 1;
                string nombreBase = nombreUsuario;
                do
                {
                    nombreUsuario = $"{nombreBase}.{contador}";
                    existeUsuario = await _context.Usuarios
                        .AnyAsync(u => u.NombreUsuario == nombreUsuario);
                    contador++;
                } while (existeUsuario);
            }
            
            return nombreUsuario;
        }

        /// <summary>
        /// Genera un nombre de usuario único basado en el nombre y apellidos del estudiante
        /// </summary>
        /// <param name="nombre">Nombre completo del estudiante</param>
        /// <param name="apellidos">Apellidos del estudiante</param>
        /// <returns>Nombre de usuario único</returns>
        private async Task<string> GenerarNombreUsuarioEstudianteAsync(string nombre, string apellidos)
        {
            return await GenerarNombreUsuarioAsync(nombre, apellidos);
        }

        /// <summary>
        /// Crea un usuario para un estudiante existente
        /// </summary>
        /// <param name="estudianteId">ID del estudiante</param>
        /// <param name="nombreUsuario">Nombre de usuario (opcional, si no se proporciona se genera automáticamente)</param>
        /// <returns>Información del usuario creado</returns>
        public async Task<(string nombreUsuario, string contraseñaInicial)> CrearUsuarioEstudianteAsync(int estudianteId, string? nombreUsuario = null)
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
                nombreUsuario = await GenerarNombreUsuarioEstudianteAsync(estudiante.Nombre, estudiante.Apellidos);
            }
            else
            {
                // Verificar que el nombre de usuario no está en uso
                var usuarioConMismoNombre = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

                if (usuarioConMismoNombre != null)
                    throw new InvalidOperationException("El nombre de usuario ya está en uso");
            }

            // Usar la cédula como contraseña inicial
            string contraseñaInicial = estudianteId.ToString();
            string contraseñaHash = PasswordHelper.HashPassword(contraseñaInicial);

            // Crear usuario
            var usuario = new Usuario
            {
                Identificacion = estudianteId,
                NombreUsuario = nombreUsuario,
                Contraseña = contraseñaHash,
                Rol = "Estudiante",
                FechaCreacion = DateTime.Now,
                Activo = true,
                RequiereCambioContraseña = true // Marcar que debe cambiar la contraseña
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return (nombreUsuario, contraseñaInicial);
        }

        /// <summary>
        /// Crea un usuario para un profesor existente
        /// </summary>
        /// <param name="profesorId">ID del profesor</param>
        /// <param name="nombreUsuario">Nombre de usuario (opcional, si no se proporciona se genera automáticamente)</param>
        /// <returns>Información del usuario creado</returns>
        public async Task<(string nombreUsuario, string contraseñaInicial)> CrearUsuarioProfesorAsync(int profesorId, string? nombreUsuario = null)
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
                nombreUsuario = await GenerarNombreUsuarioAsync(profesor.Nombre, profesor.Apellidos);
            }
            else
            {
                // Verificar que el nombre de usuario no está en uso
                var usuarioConMismoNombre = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

                if (usuarioConMismoNombre != null)
                    throw new InvalidOperationException("El nombre de usuario ya está en uso");
            }

            // Usar la cédula como contraseña inicial
            string contraseñaInicial = profesorId.ToString();
            string contraseñaHash = PasswordHelper.HashPassword(contraseñaInicial);

            // Crear usuario
            var usuario = new Usuario
            {
                Identificacion = profesorId,
                NombreUsuario = nombreUsuario,
                Contraseña = contraseñaHash,
                Rol = "Profesor",
                FechaCreacion = DateTime.Now,
                Activo = true,
                RequiereCambioContraseña = true // Los profesores también deben cambiar contraseña
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return (nombreUsuario, contraseñaInicial);
        }

        /// <summary>
        /// Crea un usuario administrador para un profesor existente
        /// </summary>
        /// <param name="profesorId">ID del profesor que será administrador</param>
        /// <param name="nombreUsuario">Nombre de usuario (opcional, si no se proporciona se genera automáticamente)</param>
        /// <returns>Información del usuario creado</returns>
        public async Task<(string nombreUsuario, string contraseñaInicial)> CrearUsuarioAdministradorAsync(int profesorId, string? nombreUsuario = null)
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
                nombreUsuario = await GenerarNombreUsuarioAsync(profesor.Nombre, profesor.Apellidos);
            }
            else
            {
                // Verificar que el nombre de usuario no está en uso
                var usuarioConMismoNombre = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

                if (usuarioConMismoNombre != null)
                    throw new InvalidOperationException("El nombre de usuario ya está en uso");
            }

            // Usar la cédula como contraseña inicial
            string contraseñaInicial = profesorId.ToString();
            string contraseñaHash = PasswordHelper.HashPassword(contraseñaInicial);

            // Crear usuario
            var usuario = new Usuario
            {
                Identificacion = profesorId,
                NombreUsuario = nombreUsuario,
                Contraseña = contraseñaHash,
                Rol = "Administrador",
                FechaCreacion = DateTime.Now,
                Activo = true,
                RequiereCambioContraseña = true // Los administradores también deben cambiar contraseña
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return (nombreUsuario, contraseñaInicial);
        }

        /// <summary>
        /// Crea usuarios masivamente para estudiantes importados
        /// </summary>
        /// <param name="estudianteIds">Lista de IDs de estudiantes</param>
        /// <returns>Diccionario con ID del estudiante y información del usuario creado</returns>
        public async Task<Dictionary<int, (string nombreUsuario, string contraseñaInicial)>> CrearUsuariosMasivoEstudiantesAsync(List<int> estudianteIds)
        {
            var resultado = new Dictionary<int, (string nombreUsuario, string contraseñaInicial)>();

            foreach (var estudianteId in estudianteIds)
            {
                try
                {
                    var infoUsuario = await CrearUsuarioEstudianteAsync(estudianteId);
                    resultado.Add(estudianteId, infoUsuario);
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
        /// Valida que una contraseña cumple con los requisitos de seguridad
        /// </summary>
        /// <param name="contraseña">Contraseña a validar</param>
        /// <returns>Tupla con resultado de validación y mensaje de error si aplica</returns>
        public static (bool esValida, string mensaje) ValidarContraseña(string contraseña)
        {
            if (string.IsNullOrWhiteSpace(contraseña))
                return (false, "La contraseña no puede estar vacía");
            
            if (contraseña.Length < 8)
                return (false, "La contraseña debe tener al menos 8 caracteres");
            
            if (!contraseña.Any(char.IsDigit))
                return (false, "La contraseña debe contener al menos un número");
            
            return (true, "");
        }

        /// <summary>
        /// Cambia la contraseña de un usuario y marca que ya no requiere cambio obligatorio
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario</param>
        /// <param name="contraseñaActual">Contraseña actual</param>
        /// <param name="nuevaContraseña">Nueva contraseña</param>
        /// <returns>True si se cambió exitosamente</returns>
        public async Task<(bool exito, string mensaje)> CambiarContraseñaConValidacionAsync(string nombreUsuario, string contraseñaActual, string nuevaContraseña)
        {
            // Validar nueva contraseña
            var (esValida, mensajeValidacion) = ValidarContraseña(nuevaContraseña);
            if (!esValida)
                return (false, mensajeValidacion);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

            if (usuario == null)
                return (false, "Usuario no encontrado");

            if (!PasswordHelper.VerifyPassword(contraseñaActual, usuario.Contraseña))
                return (false, "La contraseña actual es incorrecta");

            // Cambiar contraseña y marcar que ya no requiere cambio
            usuario.Contraseña = PasswordHelper.HashPassword(nuevaContraseña);
            usuario.RequiereCambioContraseña = false;
            
            await _context.SaveChangesAsync();
            return (true, "Contraseña cambiada exitosamente");
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
