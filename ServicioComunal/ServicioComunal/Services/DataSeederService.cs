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

        /// <summary>
        /// Método para consultar el estado de estudiantes y grupos
        /// </summary>
        public async Task ConsultarEstadoEstudiantesAsync()
        {
            Console.WriteLine("🔍 VERIFICANDO ESTUDIANTES Y GRUPOS\n");

            // Obtener todos los estudiantes
            var estudiantes = await _context.Estudiantes.ToListAsync();
            var usuarios = await _context.Usuarios.Where(u => u.Rol == "Estudiante").ToListAsync();
            var gruposEstudiantes = await _context.GruposEstudiantes.ToListAsync();

            Console.WriteLine("📋 LISTADO DE ESTUDIANTES:");
            Console.WriteLine(new string('-', 100));
            Console.WriteLine($"{"ID",-12} {"NOMBRE",-20} {"APELLIDOS",-20} {"CLASE",-8} {"USUARIO",-15} {"GRUPO",-6}");
            Console.WriteLine(new string('-', 100));

            foreach (var est in estudiantes)
            {
                var usuario = usuarios.FirstOrDefault(u => u.Identificacion == est.Identificacion);
                var grupoAsignado = gruposEstudiantes.FirstOrDefault(ge => ge.EstudianteIdentificacion == est.Identificacion);
                
                var usuarioStr = usuario?.NombreUsuario ?? "NO TIENE";
                var grupoStr = grupoAsignado?.GrupoNumero.ToString() ?? "NINGUNO";
                
                Console.WriteLine($"{est.Identificacion,-12} {est.Nombre,-20} {est.Apellidos,-20} {est.Clase,-8} {usuarioStr,-15} {grupoStr,-6}");
            }

            Console.WriteLine(new string('-', 100));

            // Mostrar estadísticas
            var conGrupo = gruposEstudiantes.Count;
            var sinGrupo = estudiantes.Count - conGrupo;
            var conUsuario = usuarios.Count;

            Console.WriteLine("\n📊 ESTADÍSTICAS:");
            Console.WriteLine($"   Total estudiantes: {estudiantes.Count}");
            Console.WriteLine($"   Con usuario: {conUsuario}");
            Console.WriteLine($"   Con grupo: {conGrupo}");
            Console.WriteLine($"   Sin grupo: {sinGrupo}");

            // Mostrar estudiantes sin grupo que tienen usuario
            var estudiantesSinGrupo = estudiantes
                .Where(e => usuarios.Any(u => u.Identificacion == e.Identificacion) && 
                           !gruposEstudiantes.Any(ge => ge.EstudianteIdentificacion == e.Identificacion))
                .ToList();

            if (estudiantesSinGrupo.Any())
            {
                Console.WriteLine("\n🎯 ESTUDIANTES SIN GRUPO PARA PROBAR:");
                Console.WriteLine("   (Estos pueden usar la funcionalidad de Gestión de Grupos)");
                foreach (var est in estudiantesSinGrupo)
                {
                    var usuario = usuarios.First(u => u.Identificacion == est.Identificacion);
                    Console.WriteLine($"   👤 {est.Nombre} {est.Apellidos}");
                    Console.WriteLine($"      - Usuario: {usuario.NombreUsuario}");
                    Console.WriteLine($"      - Contraseña: password123");
                    Console.WriteLine($"      - ID: {est.Identificacion}");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("\n⚠️  TODOS LOS ESTUDIANTES YA TIENEN GRUPO ASIGNADO");
                Console.WriteLine("    Para probar la funcionalidad de gestión de grupos,");
                Console.WriteLine("    necesitas crear más estudiantes o quitar algunos de sus grupos.");
            }

            Console.WriteLine("\n✅ Consulta completada.");
        }

        /// <summary>
        /// Método para crear estudiantes adicionales sin grupo
        /// </summary>
        public async Task CrearEstudiantesSinGrupoAsync()
        {
            Console.WriteLine("👨‍🎓 Creando estudiantes adicionales sin grupo...");

            var nuevosEstudiantes = new List<Estudiante>
            {
                new Estudiante { Identificacion = 202301234, Nombre = "María José", Apellidos = "López Herrera", Clase = "11-A" },
                new Estudiante { Identificacion = 202301235, Nombre = "Carlos Eduardo", Apellidos = "Rojas Morales", Clase = "11-B" },
                new Estudiante { Identificacion = 202301236, Nombre = "Ana Lucía", Apellidos = "Fernández Quesada", Clase = "10-A" },
                new Estudiante { Identificacion = 202301237, Nombre = "José Miguel", Apellidos = "Salazar Castro", Clase = "10-B" },
                new Estudiante { Identificacion = 202301238, Nombre = "Paola Andrea", Apellidos = "Méndez Villalobos", Clase = "11-A" }
            };

            // Verificar cuáles no existen ya
            var estudiantesNuevos = new List<Estudiante>();
            foreach (var est in nuevosEstudiantes)
            {
                var existe = await _context.Estudiantes.AnyAsync(e => e.Identificacion == est.Identificacion);
                if (!existe)
                {
                    estudiantesNuevos.Add(est);
                }
            }

            if (estudiantesNuevos.Any())
            {
                _context.Estudiantes.AddRange(estudiantesNuevos);
                await _context.SaveChangesAsync();
                Console.WriteLine($"   ✅ Se crearon {estudiantesNuevos.Count} nuevos estudiantes");

                // Crear usuarios para estos estudiantes
                string contraseñaComún = "password123";
                var nuevosUsuarios = new List<Usuario>();

                foreach (var est in estudiantesNuevos)
                {
                    var nombreUsuario = est.Nombre.ToLower().Replace(" ", ".") + "." + est.Apellidos.Split(' ')[0].ToLower();
                    
                    var usuario = new Usuario
                    {
                        Identificacion = est.Identificacion,
                        NombreUsuario = nombreUsuario,
                        Contraseña = PasswordHelper.HashPassword(contraseñaComún),
                        Rol = "Estudiante",
                        FechaCreacion = DateTime.Now,
                        Activo = true
                    };
                    
                    nuevosUsuarios.Add(usuario);
                }

                _context.Usuarios.AddRange(nuevosUsuarios);
                await _context.SaveChangesAsync();

                Console.WriteLine($"   🔐 Se crearon {nuevosUsuarios.Count} usuarios para los nuevos estudiantes");
                Console.WriteLine("   📋 NUEVOS ESTUDIANTES DISPONIBLES PARA PROBAR:");
                foreach (var usuario in nuevosUsuarios)
                {
                    var estudiante = estudiantesNuevos.First(e => e.Identificacion == usuario.Identificacion);
                    Console.WriteLine($"      👤 {estudiante.Nombre} {estudiante.Apellidos}");
                    Console.WriteLine($"         - Usuario: {usuario.NombreUsuario}");
                    Console.WriteLine($"         - Contraseña: password123");
                    Console.WriteLine($"         - ID: {usuario.Identificacion}");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("   ℹ️ Todos los estudiantes ya existen");
            }
        }
    }
}