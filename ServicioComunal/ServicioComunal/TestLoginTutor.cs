using Microsoft.EntityFrameworkCore;
using ServicioComunal.Data;
using ServicioComunal.Models;
using ServicioComunal.Utilities;

namespace ServicioComunal
{
    public class TestLoginTutor
    {
        public static async Task TestUsuariosTutores(ServicioComunalDbContext context)
        {
            Console.WriteLine("=== VERIFICANDO USUARIOS TUTORES ===");
            
            // Verificar todos los usuarios con rol Profesor
            var usuariosProfesores = await context.Usuarios
                .Where(u => u.Rol == "Profesor")
                .ToListAsync();
                
            Console.WriteLine($"Usuarios con rol 'Profesor' encontrados: {usuariosProfesores.Count}");
            
            foreach (var usuario in usuariosProfesores)
            {
                Console.WriteLine($"- ID: {usuario.Identificacion}, Usuario: {usuario.NombreUsuario}, Activo: {usuario.Activo}");
                
                // Verificar si existe el profesor correspondiente
                var profesor = await context.Profesores
                    .FirstOrDefaultAsync(p => p.Identificacion == usuario.Identificacion);
                    
                if (profesor != null)
                {
                    Console.WriteLine($"  -> Profesor: {profesor.Nombre} {profesor.Apellidos}, Rol: {profesor.Rol}");
                }
                else
                {
                    Console.WriteLine($"  -> NO SE ENCONTRÓ REGISTRO EN TABLA PROFESOR");
                }
            }
            
            Console.WriteLine("\n=== VERIFICANDO CONTRASEÑAS ===");
            
            // Probar contraseñas conocidas
            string[] passwordsToTest = { "password123", "12345", "123456" };
            
            foreach (var usuario in usuariosProfesores)
            {
                Console.WriteLine($"\nProbando contraseñas para {usuario.NombreUsuario}:");
                
                foreach (var password in passwordsToTest)
                {
                    bool isValid = PasswordHelper.VerifyPassword(password, usuario.Contraseña);
                    Console.WriteLine($"  - '{password}': {(isValid ? "✓ VÁLIDA" : "✗ Inválida")}");
                }
            }
            
            Console.WriteLine("\n=== INFORMACIÓN DE HASH ALMACENADO ===");
            
            foreach (var usuario in usuariosProfesores)
            {
                Console.WriteLine($"\nUsuario: {usuario.NombreUsuario}");
                Console.WriteLine($"Hash almacenado: {usuario.Contraseña}");
                Console.WriteLine($"Longitud del hash: {usuario.Contraseña.Length}");
                
                if (usuario.Contraseña.Contains('$'))
                {
                    var parts = usuario.Contraseña.Split('$');
                    Console.WriteLine($"  - Hash: {parts[0]}");
                    Console.WriteLine($"  - Salt: {parts[1]}");
                }
            }
        }
        
        public static async Task CrearUsuarioTutorPrueba(ServicioComunalDbContext context)
        {
            Console.WriteLine("\n=== CREANDO USUARIO TUTOR DE PRUEBA ===");
            
            int tutorId = 999888777;
            string password = "tutor123";
            
            // Verificar si ya existe
            var existeUsuario = await context.Usuarios.FindAsync(tutorId);
            var existeProfesor = await context.Profesores.FindAsync(tutorId);
            
            if (existeUsuario != null)
            {
                Console.WriteLine("El usuario de prueba ya existe. Eliminando...");
                context.Usuarios.Remove(existeUsuario);
            }
            
            if (existeProfesor != null)
            {
                Console.WriteLine("El profesor de prueba ya existe. Eliminando...");
                context.Profesores.Remove(existeProfesor);
            }
            
            await context.SaveChangesAsync();
            
            // Crear profesor
            var nuevoProfesor = new Profesor
            {
                Identificacion = tutorId,
                Nombre = "Tutor",
                Apellidos = "De Prueba",
                Rol = "Tutor"
            };
            
            context.Profesores.Add(nuevoProfesor);
            
            // Crear usuario con contraseña hasheada
            string hashedPassword = PasswordHelper.HashPassword(password);
            
            var nuevoUsuario = new Usuario
            {
                Identificacion = tutorId,
                NombreUsuario = "tutor.prueba",
                Contraseña = hashedPassword,
                Rol = "Profesor",
                FechaCreacion = DateTime.Now,
                Activo = true
            };
            
            context.Usuarios.Add(nuevoUsuario);
            
            await context.SaveChangesAsync();
            
            Console.WriteLine($"✓ Usuario creado:");
            Console.WriteLine($"  - ID: {tutorId}");
            Console.WriteLine($"  - Usuario: tutor.prueba");
            Console.WriteLine($"  - Contraseña: {password}");
            Console.WriteLine($"  - Hash: {hashedPassword}");
            
            // Verificar que funciona
            bool loginTest = PasswordHelper.VerifyPassword(password, hashedPassword);
            Console.WriteLine($"  - Verificación: {(loginTest ? "✓ ÉXITO" : "✗ FALLO")}");
        }
    }
}
