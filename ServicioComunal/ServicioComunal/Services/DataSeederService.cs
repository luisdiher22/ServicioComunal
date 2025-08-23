using ServicioComunal.Data;
using ServicioComunal.Models;
using ServicioComunal.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ServicioComunal.Services
{
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
            if (await _context.Usuarios.AnyAsync())
            {
                Console.WriteLine("La base de datos ya contiene usuarios.");
                return;
            }

            Console.WriteLine("🚀 Iniciando seeder de datos...");

            // Insertar profesores incluyendo tutores
            if (!await _context.Profesores.AnyAsync())
            {
                Console.WriteLine("📝 Insertando profesores...");
                var profesores = new List<Profesor>
                {
                    new Profesor { Identificacion = 123456789, Nombre = "María", Apellidos = "González Rodríguez", Rol = "Coordinadora" },
                    new Profesor { Identificacion = 987654321, Nombre = "Carlos", Apellidos = "Jiménez Vargas", Rol = "Profesor Guía" },
                    new Profesor { Identificacion = 456789123, Nombre = "Ana", Apellidos = "Mora Solís", Rol = "Profesor Supervisor" },
                    new Profesor { Identificacion = 234567890, Nombre = "Patricia", Apellidos = "Rodríguez Campos", Rol = "Tutor" },
                    new Profesor { Identificacion = 345678901, Nombre = "Miguel", Apellidos = "Sánchez Herrera", Rol = "Tutor" },
                    new Profesor { Identificacion = 567890123, Nombre = "Elena", Apellidos = "Castro Mendoza", Rol = "Tutor" }
                };
                _context.Profesores.AddRange(profesores);
                await _context.SaveChangesAsync();
            }

            // Insertar estudiantes
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

            // Insertar grupos
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

            // Crear usuarios con contraseña password123
            Console.WriteLine("🔐 Creando usuarios...");
            string contraseñaComún = "password123";
            var usuarios = new List<Usuario>
            {
                new Usuario { Identificacion = 123456789, NombreUsuario = "admin", Contraseña = PasswordHelper.HashPassword(contraseñaComún), Rol = "Administrador", FechaCreacion = DateTime.Now, Activo = true },
                new Usuario { Identificacion = 987654321, NombreUsuario = "carlos.jimenez", Contraseña = PasswordHelper.HashPassword(contraseñaComún), Rol = "Profesor", FechaCreacion = DateTime.Now, Activo = true },
                new Usuario { Identificacion = 456789123, NombreUsuario = "ana.mora", Contraseña = PasswordHelper.HashPassword(contraseñaComún), Rol = "Profesor", FechaCreacion = DateTime.Now, Activo = true },
                new Usuario { Identificacion = 234567890, NombreUsuario = "patricia.rodriguez", Contraseña = PasswordHelper.HashPassword(contraseñaComún), Rol = "Profesor", FechaCreacion = DateTime.Now, Activo = true },
                new Usuario { Identificacion = 345678901, NombreUsuario = "miguel.sanchez", Contraseña = PasswordHelper.HashPassword(contraseñaComún), Rol = "Profesor", FechaCreacion = DateTime.Now, Activo = true },
                new Usuario { Identificacion = 567890123, NombreUsuario = "elena.castro", Contraseña = PasswordHelper.HashPassword(contraseñaComún), Rol = "Profesor", FechaCreacion = DateTime.Now, Activo = true },
                new Usuario { Identificacion = 111222333, NombreUsuario = "luis.perez", Contraseña = PasswordHelper.HashPassword(contraseñaComún), Rol = "Estudiante", FechaCreacion = DateTime.Now, Activo = true },
                new Usuario { Identificacion = 444555666, NombreUsuario = "sofia.ramirez", Contraseña = PasswordHelper.HashPassword(contraseñaComún), Rol = "Estudiante", FechaCreacion = DateTime.Now, Activo = true },
                new Usuario { Identificacion = 777888999, NombreUsuario = "diego.hernandez", Contraseña = PasswordHelper.HashPassword(contraseñaComún), Rol = "Estudiante", FechaCreacion = DateTime.Now, Activo = true },
                new Usuario { Identificacion = 101112131, NombreUsuario = "camila.vargas", Contraseña = PasswordHelper.HashPassword(contraseñaComún), Rol = "Estudiante", FechaCreacion = DateTime.Now, Activo = true }
            };

            _context.Usuarios.AddRange(usuarios);
            await _context.SaveChangesAsync();

            // Asignar grupos a tutores
            if (!await _context.GruposProfesores.AnyAsync())
            {
                Console.WriteLine("👥 Asignando grupos a tutores...");
                var asignaciones = new List<GrupoProfesor>
                {
                    new GrupoProfesor { GrupoNumero = 1, ProfesorIdentificacion = 234567890, FechaAsignacion = DateTime.Now },
                    new GrupoProfesor { GrupoNumero = 2, ProfesorIdentificacion = 345678901, FechaAsignacion = DateTime.Now },
                    new GrupoProfesor { GrupoNumero = 3, ProfesorIdentificacion = 567890123, FechaAsignacion = DateTime.Now }
                };
                _context.GruposProfesores.AddRange(asignaciones);
                await _context.SaveChangesAsync();
            }

            var usuariosCreados = await _context.Usuarios.CountAsync();
            Console.WriteLine($"✅ {usuariosCreados} usuarios creados exitosamente!");
            Console.WriteLine("🔑 Usuarios creados con contraseña: password123");
            Console.WriteLine("   🎓 TUTORES: patricia.rodriguez, miguel.sanchez, elena.castro");
            Console.WriteLine("   👨‍🎓 ESTUDIANTES: luis.perez, sofia.ramirez, diego.hernandez, camila.vargas");
        }

        public async Task LimpiarYRegenerarAsync()
        {
            Console.WriteLine("🧹 LIMPIANDO BASE DE DATOS...");
            
            var gruposProfesores = await _context.GruposProfesores.ToListAsync();
            var gruposEstudiantes = await _context.GruposEstudiantes.ToListAsync();
            var entregas = await _context.Entregas.ToListAsync();
            var notificaciones = await _context.Notificaciones.ToListAsync();
            var formularios = await _context.Formularios.ToListAsync();
            var usuarios = await _context.Usuarios.ToListAsync();
            var grupos = await _context.Grupos.ToListAsync();
            var estudiantes = await _context.Estudiantes.ToListAsync();
            var profesores = await _context.Profesores.ToListAsync();

            if (gruposProfesores.Any()) _context.GruposProfesores.RemoveRange(gruposProfesores);
            if (gruposEstudiantes.Any()) _context.GruposEstudiantes.RemoveRange(gruposEstudiantes);
            if (entregas.Any()) _context.Entregas.RemoveRange(entregas);
            if (notificaciones.Any()) _context.Notificaciones.RemoveRange(notificaciones);
            if (formularios.Any()) _context.Formularios.RemoveRange(formularios);
            if (usuarios.Any()) _context.Usuarios.RemoveRange(usuarios);
            if (grupos.Any()) _context.Grupos.RemoveRange(grupos);
            if (estudiantes.Any()) _context.Estudiantes.RemoveRange(estudiantes);
            if (profesores.Any()) _context.Profesores.RemoveRange(profesores);
            
            await _context.SaveChangesAsync();
            
            Console.WriteLine("🗑️ Datos eliminados. Regenerando...");
            await SeedDataAsync();
        }
    }
}
