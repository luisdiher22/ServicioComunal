using ServicioComunal.Data;
using ServicioComunal.Models;
using ServicioComunal.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ServicioComunal.Services
{
    public class DataSeederService
    {
        private readonly ServicioComunalDbContext _context;

        public DataSeederService(ServicioComunalDbContext context)
        {
            _context = context;
        }

        // Función para eliminar tildes y caracteres especiales
        private string EliminarTildes(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return texto;

            var normalizedString = texto.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
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

            // Crear usuarios
            Console.WriteLine("🔐 Creando usuarios...");
            
            // Crear usuario administrador usando el nuevo sistema
            Console.WriteLine("👨‍💼 Creando usuario administrador con nuevo sistema...");
            try
            {
                var adminProfesor = await _context.Profesores.FirstOrDefaultAsync(p => p.Identificacion == 123456789);
                if (adminProfesor != null)
                {
                    string nombreUsuarioAdmin = EliminarTildes($"{adminProfesor.Nombre.Split(' ')[0]}.{adminProfesor.Apellidos.Split(' ')[0]}".ToLower());
                    
                    var usuarioAdmin = new Usuario 
                    { 
                        Identificacion = 123456789, 
                        NombreUsuario = nombreUsuarioAdmin, 
                        Contraseña = PasswordHelper.HashPassword("123456789"), // Usar cédula como contraseña inicial
                        Rol = "Administrador", 
                        FechaCreacion = DateTime.Now, 
                        Activo = true, 
                        RequiereCambioContraseña = true 
                    };
                    
                    _context.Usuarios.Add(usuarioAdmin);
                    Console.WriteLine($"   ✅ Usuario admin creado: {nombreUsuarioAdmin} (contraseña: 123456789)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error creando usuario admin: {ex.Message}");
            }

            // Crear usuarios para profesores usando el nuevo sistema
            Console.WriteLine("👨‍🏫 Creando usuarios para profesores con nuevo sistema...");
            var profesoresParaUsuario = await _context.Profesores
                .Where(p => p.Identificacion != 123456789) // Excluir al admin
                .ToListAsync();
                
            foreach (var profesor in profesoresParaUsuario)
            {
                try
                {
                    string primerNombre = EliminarTildes(profesor.Nombre.Split(' ')[0].ToLower());
                    string[] apellidosArray = profesor.Apellidos.Split(' ');
                    string primerApellido = EliminarTildes(apellidosArray[0].ToLower());
                    
                    string nombreUsuario = $"{primerNombre}.{primerApellido}";
                    
                    // Verificar si ya existe
                    var existeUsuario = await _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario);
                    
                    // Si existe, agregar segundo apellido
                    if (existeUsuario && apellidosArray.Length > 1)
                    {
                        string segundoApellido = EliminarTildes(apellidosArray[1].ToLower());
                        nombreUsuario = $"{primerNombre}.{primerApellido}.{segundoApellido}";
                    }
                    
                    // Crear usuario con cédula como contraseña inicial
                    string contraseñaInicial = profesor.Identificacion.ToString();
                    var usuarioProfesor = new Usuario
                    {
                        Identificacion = profesor.Identificacion,
                        NombreUsuario = nombreUsuario,
                        Contraseña = PasswordHelper.HashPassword(contraseñaInicial),
                        Rol = "Profesor",
                        FechaCreacion = DateTime.Now,
                        Activo = true,
                        RequiereCambioContraseña = true // Profesores también deben cambiar contraseña
                    };
                    
                    _context.Usuarios.Add(usuarioProfesor);
                    Console.WriteLine($"   ✅ Usuario profesor creado: {nombreUsuario} (contraseña: {contraseñaInicial})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ❌ Error creando usuario para profesor {profesor.Nombre}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            
            // Crear usuarios para estudiantes usando el nuevo sistema
            Console.WriteLine("👨‍🎓 Creando usuarios para estudiantes con nuevo sistema...");
            var estudiantesExistentes = await _context.Estudiantes.ToListAsync();
            foreach (var estudiante in estudiantesExistentes)
            {
                try
                {
                    // Generar nombre de usuario automáticamente sin tildes
                    string primerNombre = EliminarTildes(estudiante.Nombre.Split(' ')[0].ToLower());
                    string[] apellidosArray = estudiante.Apellidos.Split(' ');
                    string primerApellido = EliminarTildes(apellidosArray[0].ToLower());
                    
                    string nombreUsuario = $"{primerNombre}.{primerApellido}";
                    
                    // Verificar si ya existe
                    var existeUsuario = await _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario);
                    
                    // Si existe, agregar segundo apellido
                    if (existeUsuario && apellidosArray.Length > 1)
                    {
                        string segundoApellido = EliminarTildes(apellidosArray[1].ToLower());
                        nombreUsuario = $"{primerNombre}.{primerApellido}.{segundoApellido}";
                    }
                    
                    // Crear usuario con cédula como contraseña inicial
                    string contraseñaInicial = estudiante.Identificacion.ToString();
                    var usuarioEstudiante = new Usuario
                    {
                        Identificacion = estudiante.Identificacion,
                        NombreUsuario = nombreUsuario,
                        Contraseña = PasswordHelper.HashPassword(contraseñaInicial),
                        Rol = "Estudiante",
                        FechaCreacion = DateTime.Now,
                        Activo = true,
                        RequiereCambioContraseña = true // Requerirán cambio en primer login
                    };
                    
                    _context.Usuarios.Add(usuarioEstudiante);
                    Console.WriteLine($"   ✅ Usuario creado: {nombreUsuario} (contraseña: {contraseñaInicial})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ❌ Error creando usuario para {estudiante.Nombre}: {ex.Message}");
                }
            }

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
            Console.WriteLine("🔑 Sistema de usuarios implementado:");
            Console.WriteLine("   👤 Administradores y Profesores: maria.gonzalez, patricia.rodriguez, miguel.sanchez, ana.mora, elena.castro, carlos.jimenez");
            Console.WriteLine("   👨‍🎓 Estudiantes: camila.vargas, luis.perez, sofia.ramirez, diego.hernandez");
            Console.WriteLine("   🔐 Contraseña inicial: número de cédula de cada usuario");
            Console.WriteLine("   🔄 Todos los usuarios deben cambiar contraseña en primer login");
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
