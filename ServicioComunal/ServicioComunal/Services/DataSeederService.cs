using ServicioComunal.Data;
using ServicioComunal.Models;
using ServicioComunal.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ServicioComunal.Services
{
    /// <summary>
    /// Servicio para poblar la base de datos con datos de prueba
    /// </summary>
    public class DataSeederService
    {
        private readonly ServicioComunalDbContext _context;

        public DataSeederService(ServicioComunalDbContext context)
        {
            _context = context;
        }

        public ServicioComunalDbContext Context => _context;

        public async Task SeedDataAsync()
        {
            // Verificar si ya hay usuarios
            if (await _context.Usuarios.AnyAsync())
            {
                Console.WriteLine("La base de datos ya contiene usuarios.");
                return;
            }

            Console.WriteLine("🚀 Iniciando seeder de datos...");

            // PASO 1: Insertar profesores
            if (!await _context.Profesores.AnyAsync())
            {
                Console.WriteLine("📝 Insertando profesores...");
                var profesores = new List<Profesor>
                {
                    new Profesor { Identificacion = 123456789, Nombre = "María", Apellidos = "González Rodríguez", Rol = "Coordinadora" },
                    new Profesor { Identificacion = 987654321, Nombre = "Carlos", Apellidos = "Jiménez Vargas", Rol = "Profesor Guía" },
                    new Profesor { Identificacion = 456789123, Nombre = "Ana", Apellidos = "Mora Solís", Rol = "Profesor Supervisor" }
                };
                _context.Profesores.AddRange(profesores);
                await _context.SaveChangesAsync();
            }

            // PASO 2: Insertar estudiantes
            if (!await _context.Estudiantes.AnyAsync())
            {
                Console.WriteLine("👨‍🎓 Insertando estudiantes...");
                var estudiantes = new List<Estudiante>
                {
                    new Estudiante { Identificacion = 111222333, Nombre = "Luis", Apellidos = "Pérez Castro", Clase = "11-A" },
                    new Estudiante { Identificacion = 444555666, Nombre = "Sofia", Apellidos = "Ramírez León", Clase = "11-B" },
                    new Estudiante { Identificacion = 777888999, Nombre = "Diego", Apellidos = "Hernández Vega", Clase = "10-A" },
                    new Estudiante { Identificacion = 101112131, Nombre = "Camila", Apellidos = "Vargas Muñoz", Clase = "10-B" }
                };
                _context.Estudiantes.AddRange(estudiantes);
                await _context.SaveChangesAsync();
            }

            // PASO 3: Insertar grupos
            if (!await _context.Grupos.AnyAsync())
            {
                Console.WriteLine("👥 Insertando grupos...");
                var grupos = new List<Grupo>
                {
                    new Grupo { Numero = 1 },
                    new Grupo { Numero = 2 },
                    new Grupo { Numero = 3 }
                };
                _context.Grupos.AddRange(grupos);
                await _context.SaveChangesAsync();
            }

            // PASO 4: Crear usuarios (tabla independiente)
            Console.WriteLine("🔐 Creando usuarios...");
            string contraseñaComún = "password123";
            var usuarios = new List<Usuario>
            {
                new Usuario { Identificacion = 123456789, NombreUsuario = "admin", Contraseña = PasswordHelper.HashPassword(contraseñaComún), Rol = "Administrador", FechaCreacion = DateTime.Now, Activo = true },
                new Usuario { Identificacion = 987654321, NombreUsuario = "carlos.jimenez", Contraseña = PasswordHelper.HashPassword(contraseñaComún), Rol = "Profesor", FechaCreacion = DateTime.Now, Activo = true },
                new Usuario { Identificacion = 456789123, NombreUsuario = "ana.mora", Contraseña = PasswordHelper.HashPassword(contraseñaComún), Rol = "Profesor", FechaCreacion = DateTime.Now, Activo = true },
                new Usuario { Identificacion = 111222333, NombreUsuario = "luis.perez", Contraseña = PasswordHelper.HashPassword(contraseñaComún), Rol = "Estudiante", FechaCreacion = DateTime.Now, Activo = true },
                new Usuario { Identificacion = 444555666, NombreUsuario = "sofia.ramirez", Contraseña = PasswordHelper.HashPassword(contraseñaComún), Rol = "Estudiante", FechaCreacion = DateTime.Now, Activo = true },
                new Usuario { Identificacion = 777888999, NombreUsuario = "diego.hernandez", Contraseña = PasswordHelper.HashPassword(contraseñaComún), Rol = "Estudiante", FechaCreacion = DateTime.Now, Activo = true },
                new Usuario { Identificacion = 101112131, NombreUsuario = "camila.vargas", Contraseña = PasswordHelper.HashPassword(contraseñaComún), Rol = "Estudiante", FechaCreacion = DateTime.Now, Activo = true }
            };

            _context.Usuarios.AddRange(usuarios);
            await _context.SaveChangesAsync();

            var usuariosCreados = await _context.Usuarios.CountAsync();
            Console.WriteLine($"✅ {usuariosCreados} usuarios creados exitosamente!");
            Console.WriteLine("🔑 Usuarios creados con contraseña: password123");
            Console.WriteLine("   - admin, carlos.jimenez, ana.mora, luis.perez, sofia.ramirez, diego.hernandez, camila.vargas");
        }

        /// <summary>
        /// Método para crear usuarios sin errores de FK
        /// </summary>
        public async Task SeedUsuariosSafeAsync()
        {
            Console.WriteLine("🔐 Iniciando creación segura de usuarios...");
            
            // Verificar si ya hay usuarios
            if (await _context.Usuarios.AnyAsync())
            {
                Console.WriteLine("Ya existen usuarios en la base de datos.");
                return;
            }

            // Verificar que existan las entidades base
            var profesoresCount = await _context.Profesores.CountAsync();
            var estudiantesCount = await _context.Estudiantes.CountAsync();
            
            Console.WriteLine($"📊 Profesores en BD: {profesoresCount}");
            Console.WriteLine($"📊 Estudiantes en BD: {estudiantesCount}");

            if (profesoresCount == 0 && estudiantesCount == 0)
            {
                Console.WriteLine("⚠️ No hay profesores ni estudiantes. Ejecutando seeder completo...");
                await SeedDataAsync();
                return;
            }

            // Crear usuarios solo para las identificaciones que existen
            string contraseñaComún = "password123";
            var usuarios = new List<Usuario>();

            // Agregar usuarios para profesores existentes
            var profesores = await _context.Profesores.ToListAsync();
            foreach (var profesor in profesores)
            {
                usuarios.Add(new Usuario 
                { 
                    Identificacion = profesor.Identificacion, 
                    NombreUsuario = profesor.Identificacion == 123456789 ? "admin" : 
                                   profesor.Identificacion == 987654321 ? "carlos.jimenez" : "ana.mora",
                    Contraseña = PasswordHelper.HashPassword(contraseñaComún), 
                    Rol = profesor.Identificacion == 123456789 ? "Administrador" : "Profesor", 
                    FechaCreacion = DateTime.Now, 
                    Activo = true 
                });
            }

            // Agregar usuarios para estudiantes existentes
            var estudiantes = await _context.Estudiantes.ToListAsync();
            foreach (var estudiante in estudiantes)
            {
                string nombreUsuario = estudiante.Identificacion switch
                {
                    111222333 => "luis.perez",
                    444555666 => "sofia.ramirez", 
                    777888999 => "diego.hernandez",
                    101112131 => "camila.vargas",
                    _ => $"estudiante{estudiante.Identificacion}"
                };

                usuarios.Add(new Usuario 
                { 
                    Identificacion = estudiante.Identificacion, 
                    NombreUsuario = nombreUsuario,
                    Contraseña = PasswordHelper.HashPassword(contraseñaComún), 
                    Rol = "Estudiante", 
                    FechaCreacion = DateTime.Now, 
                    Activo = true 
                });
            }

            // Insertar usuarios de forma segura
            try
            {
                _context.Usuarios.AddRange(usuarios);
                await _context.SaveChangesAsync();
                
                var usuariosCreados = await _context.Usuarios.CountAsync();
                Console.WriteLine($"✅ {usuariosCreados} usuarios creados exitosamente!");
                
                Console.WriteLine("🔑 Usuarios creados con contraseña: password123");
                foreach (var usuario in usuarios)
                {
                    Console.WriteLine($"   - {usuario.NombreUsuario} ({usuario.Rol})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al crear usuarios: {ex.Message}");
                throw;
            }
        }
        public async Task ReseedUsuariosAsync()
        {
            Console.WriteLine("🧹 Limpiando usuarios existentes...");
            
            // Eliminar usuarios existentes
            var usuariosExistentes = await _context.Usuarios.ToListAsync();
            if (usuariosExistentes.Any())
            {
                _context.Usuarios.RemoveRange(usuariosExistentes);
                await _context.SaveChangesAsync();
                Console.WriteLine($"🗑️ Eliminados {usuariosExistentes.Count} usuarios existentes");
            }

            // Verificar que existan profesores y estudiantes
            var profesoresCount = await _context.Profesores.CountAsync();
            var estudiantesCount = await _context.Estudiantes.CountAsync();
            
            Console.WriteLine($"📊 Profesores en BD: {profesoresCount}");
            Console.WriteLine($"📊 Estudiantes en BD: {estudiantesCount}");

            if (profesoresCount == 0 || estudiantesCount == 0)
            {
                Console.WriteLine("⚠️ Faltan profesores o estudiantes. Ejecutando seeder completo...");
                await SeedDataAsync();
                return;
            }

            // Crear usuarios nuevamente
            Console.WriteLine("🔐 Creando usuarios nuevamente...");
            string contraseñaComún = "password123";
            var usuarios = new List<Usuario>
            {
                new Usuario 
                { 
                    Identificacion = 123456789, 
                    NombreUsuario = "admin", 
                    Contraseña = PasswordHelper.HashPassword(contraseñaComún), 
                    Rol = "Administrador", 
                    FechaCreacion = DateTime.Now, 
                    Activo = true 
                },
                new Usuario 
                { 
                    Identificacion = 987654321, 
                    NombreUsuario = "carlos.jimenez", 
                    Contraseña = PasswordHelper.HashPassword(contraseñaComún), 
                    Rol = "Profesor", 
                    FechaCreacion = DateTime.Now, 
                    Activo = true 
                },
                new Usuario 
                { 
                    Identificacion = 456789123, 
                    NombreUsuario = "ana.mora", 
                    Contraseña = PasswordHelper.HashPassword(contraseñaComún), 
                    Rol = "Profesor", 
                    FechaCreacion = DateTime.Now, 
                    Activo = true 
                },
                new Usuario 
                { 
                    Identificacion = 111222333, 
                    NombreUsuario = "luis.perez", 
                    Contraseña = PasswordHelper.HashPassword(contraseñaComún), 
                    Rol = "Estudiante", 
                    FechaCreacion = DateTime.Now, 
                    Activo = true 
                },
                new Usuario 
                { 
                    Identificacion = 444555666, 
                    NombreUsuario = "sofia.ramirez", 
                    Contraseña = PasswordHelper.HashPassword(contraseñaComún), 
                    Rol = "Estudiante", 
                    FechaCreacion = DateTime.Now, 
                    Activo = true 
                },
                new Usuario 
                { 
                    Identificacion = 777888999, 
                    NombreUsuario = "diego.hernandez", 
                    Contraseña = PasswordHelper.HashPassword(contraseñaComún), 
                    Rol = "Estudiante", 
                    FechaCreacion = DateTime.Now, 
                    Activo = true 
                },
                new Usuario 
                { 
                    Identificacion = 101112131, 
                    NombreUsuario = "camila.vargas", 
                    Contraseña = PasswordHelper.HashPassword(contraseñaComún), 
                    Rol = "Estudiante", 
                    FechaCreacion = DateTime.Now, 
                    Activo = true 
                }
            };
            
            _context.Usuarios.AddRange(usuarios);
            await _context.SaveChangesAsync();
            
            var usuariosCreados = await _context.Usuarios.CountAsync();
            Console.WriteLine($"✅ {usuariosCreados} usuarios recreados exitosamente!");
        }
    }
}