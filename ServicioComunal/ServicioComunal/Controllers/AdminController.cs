using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioComunal.Services;
using ServicioComunal.Data;
using ServicioComunal.Models;

namespace ServicioComunal.Controllers
{
    public class AdminController : Controller
    {
        private readonly DataSeederService _dataSeeder;
        private readonly ServicioComunalDbContext _context;

        public AdminController(DataSeederService dataSeeder, ServicioComunalDbContext context)
        {
            _dataSeeder = dataSeeder;
            _context = context;
        }

        // Endpoint temporal para regenerar datos
        public async Task<IActionResult> RegenerarDatos()
        {
            try
            {
                await _dataSeeder.LimpiarYRegenerarAsync();
                return Json(new { 
                    success = true, 
                    message = "✅ Datos regenerados exitosamente. Ahora puedes probar con los usuarios tutores: patricia.rodriguez, miguel.sanchez, elena.castro (contraseña: password123)" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"❌ Error: {ex.Message}" 
                });
            }
        }

        // Ver estado de usuarios
        public async Task<IActionResult> VerUsuarios()
        {
            var context = _dataSeeder.Context;
            
            var usuarios = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                context.Usuarios.OrderBy(u => u.Rol).ThenBy(u => u.NombreUsuario)
            );
            
            var resultado = usuarios.Select(u => new {
                u.Identificacion,
                u.NombreUsuario,
                u.Rol,
                u.Activo,
                u.FechaCreacion
            }).ToList();
            
            return Json(new { success = true, usuarios = resultado });
        }

        // Página de gestión de grupos
        public IActionResult GestionGrupos()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LimpiarYReiniciarGrupos()
        {
            try
            {
                // 1. Eliminar todas las solicitudes existentes
                var todasLasSolicitudes = await _context.Solicitudes.ToListAsync();
                _context.Solicitudes.RemoveRange(todasLasSolicitudes);

                // 2. Eliminar todas las relaciones GrupoEstudiante
                var todasLasRelaciones = await _context.GruposEstudiantes.ToListAsync();
                _context.GruposEstudiantes.RemoveRange(todasLasRelaciones);

                // 3. Eliminar todas las entregas
                var todasLasEntregas = await _context.Entregas.ToListAsync();
                _context.Entregas.RemoveRange(todasLasEntregas);

                // 4. Eliminar todas las relaciones GrupoProfesor
                var todasLasRelacionesProfesores = await _context.GruposProfesores.ToListAsync();
                _context.GruposProfesores.RemoveRange(todasLasRelacionesProfesores);

                // 5. Eliminar todas las notificaciones
                var todasLasNotificaciones = await _context.Notificaciones.ToListAsync();
                _context.Notificaciones.RemoveRange(todasLasNotificaciones);

                // 6. Eliminar todos los grupos
                var todosLosGrupos = await _context.Grupos.ToListAsync();
                _context.Grupos.RemoveRange(todosLosGrupos);

                // Guardar cambios de eliminación
                await _context.SaveChangesAsync();

                // 7. Crear grupos nuevos con líderes
                await CrearGruposConLideres();

                return Json(new { 
                    success = true, 
                    message = "Limpieza completa realizada. Se han creado nuevos grupos con líderes asignados."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error durante la limpieza: " + ex.Message });
            }
        }

        private async Task CrearGruposConLideres()
        {
            // Obtener todos los estudiantes
            var todosLosEstudiantes = await _context.Estudiantes
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            if (!todosLosEstudiantes.Any())
            {
                return;
            }

            int numeroGrupo = 1;
            var estudiantesUsados = new HashSet<int>();

            // Crear grupos de máximo 4 estudiantes cada uno
            for (int i = 0; i < todosLosEstudiantes.Count; i += 4)
            {
                var estudiantesParaGrupo = todosLosEstudiantes
                    .Skip(i)
                    .Take(4)
                    .Where(e => !estudiantesUsados.Contains(e.Identificacion))
                    .ToList();

                if (!estudiantesParaGrupo.Any())
                    break;

                // El primer estudiante del grupo será el líder
                var lider = estudiantesParaGrupo.First();

                // Crear el grupo
                var nuevoGrupo = new Grupo
                {
                    Numero = numeroGrupo,
                    LiderIdentificacion = lider.Identificacion
                };

                _context.Grupos.Add(nuevoGrupo);
                // Agregar todos los estudiantes al grupo
                foreach (var estudiante in estudiantesParaGrupo)
                {
                    var grupoEstudiante = new GrupoEstudiante
                    {
                        GrupoNumero = numeroGrupo,
                        EstudianteIdentificacion = estudiante.Identificacion
                    };

                    _context.GruposEstudiantes.Add(grupoEstudiante);
                    estudiantesUsados.Add(estudiante.Identificacion);
                }

                numeroGrupo++;
            }

            // Guardar todos los cambios
            await _context.SaveChangesAsync();
        }

        private async Task MostrarResumenGrupos()
        {
            Console.WriteLine("\n📊 RESUMEN DE GRUPOS CREADOS:");
            Console.WriteLine("================================");

            var grupos = await _context.Grupos
                .Include(g => g.GruposEstudiantes)
                    .ThenInclude(ge => ge.Estudiante)
                .Include(g => g.Lider)
                .OrderBy(g => g.Numero)
                .ToListAsync();

            foreach (var grupo in grupos)
            {
                Console.WriteLine($"\n🏆 GRUPO {grupo.Numero}");
                Console.WriteLine($"   👑 Líder: {grupo.Lider?.Nombre} {grupo.Lider?.Apellidos}");
                Console.WriteLine($"   👥 Miembros ({grupo.GruposEstudiantes.Count}/4):");
                
                foreach (var miembro in grupo.GruposEstudiantes)
                {
                    string icono = miembro.EstudianteIdentificacion == grupo.LiderIdentificacion ? "👑" : "👤";
                    Console.WriteLine($"      {icono} {miembro.Estudiante.Nombre} {miembro.Estudiante.Apellidos}");
                }
            }

            var estudiantesSinGrupo = await _context.Estudiantes
                .Where(e => !_context.GruposEstudiantes.Any(ge => ge.EstudianteIdentificacion == e.Identificacion))
                .ToListAsync();

            if (estudiantesSinGrupo.Any())
            {
                Console.WriteLine($"\n⚠️ ESTUDIANTES SIN GRUPO ({estudiantesSinGrupo.Count}):");
                foreach (var estudiante in estudiantesSinGrupo)
                {
                    Console.WriteLine($"   - {estudiante.Nombre} {estudiante.Apellidos}");
                }
            }

            Console.WriteLine("\n================================");
        }

        [HttpGet]
        public async Task<IActionResult> VerificarEstadoGrupos()
        {
            try
            {
                var grupos = await _context.Grupos
                    .Include(g => g.GruposEstudiantes)
                        .ThenInclude(ge => ge.Estudiante)
                    .Include(g => g.Lider)
                    .ToListAsync();

                var gruposSinLider = grupos.Where(g => g.LiderIdentificacion == null).ToList();
                var gruposConLider = grupos.Where(g => g.LiderIdentificacion != null).ToList();

                var solicitudes = await _context.Solicitudes.CountAsync();
                var totalEstudiantes = await _context.Estudiantes.CountAsync();
                var estudiantesEnGrupos = await _context.GruposEstudiantes.CountAsync();

                var resultado = new
                {
                    TotalGrupos = grupos.Count,
                    GruposSinLider = gruposSinLider.Count,
                    GruposConLider = gruposConLider.Count,
                    TotalSolicitudes = solicitudes,
                    TotalEstudiantes = totalEstudiantes,
                    EstudiantesEnGrupos = estudiantesEnGrupos,
                    EstudiantesSinGrupo = totalEstudiantes - estudiantesEnGrupos,
                    Grupos = grupos.Select(g => new
                    {
                        Numero = g.Numero,
                        TieneLider = g.LiderIdentificacion != null,
                        NombreLider = g.Lider != null ? $"{g.Lider.Nombre} {g.Lider.Apellidos}" : "Sin líder",
                        CantidadMiembros = g.GruposEstudiantes.Count,
                        Miembros = g.GruposEstudiantes.Select(ge => new
                        {
                            Nombre = $"{ge.Estudiante.Nombre} {ge.Estudiante.Apellidos}",
                            EsLider = ge.EstudianteIdentificacion == g.LiderIdentificacion
                        }).ToList()
                    }).ToList()
                };

                return Json(resultado);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EliminarEstudianteDeGrupo([FromBody] EliminarEstudianteRequest request)
        {
            try
            {
                // Buscar la relación grupo-estudiante
                var grupoEstudiante = await _context.GruposEstudiantes
                    .Include(ge => ge.Estudiante)
                    .Include(ge => ge.Grupo)
                        .ThenInclude(g => g.Lider)
                    .FirstOrDefaultAsync(ge => ge.GrupoNumero == request.GrupoNumero && 
                                              ge.EstudianteIdentificacion == request.EstudianteId);

                if (grupoEstudiante == null)
                {
                    return Json(new { success = false, message = "La relación estudiante-grupo no existe" });
                }

                var grupo = grupoEstudiante.Grupo;
                var estudiante = grupoEstudiante.Estudiante;

                // Si es el líder, verificar si hay otros miembros para reasignar liderazgo
                bool eraLider = grupo.LiderIdentificacion == request.EstudianteId;
                
                if (eraLider)
                {
                    // Buscar otros miembros del grupo
                    var otrosMiembros = await _context.GruposEstudiantes
                        .Where(ge => ge.GrupoNumero == request.GrupoNumero && 
                                    ge.EstudianteIdentificacion != request.EstudianteId)
                        .Include(ge => ge.Estudiante)
                        .ToListAsync();

                    if (otrosMiembros.Any())
                    {
                        // Asignar liderazgo al primer miembro restante
                        var nuevoLider = otrosMiembros.First();
                        grupo.LiderIdentificacion = nuevoLider.EstudianteIdentificacion;
                        
                        Console.WriteLine($"👑 Nuevo líder del grupo {request.GrupoNumero}: {nuevoLider.Estudiante.Nombre} {nuevoLider.Estudiante.Apellidos}");
                    }
                    else
                    {
                        // Si no hay más miembros, el grupo se queda sin líder
                        grupo.LiderIdentificacion = null;
                        Console.WriteLine($"⚠️ Grupo {request.GrupoNumero} se queda sin líder (no hay más miembros)");
                    }
                }

                // Eliminar la relación
                _context.GruposEstudiantes.Remove(grupoEstudiante);

                // Eliminar solicitudes relacionadas con este estudiante y grupo
                var solicitudesRelacionadas = await _context.Solicitudes
                    .Where(s => (s.EstudianteRemitenteId == request.EstudianteId || 
                                s.EstudianteDestinatarioId == request.EstudianteId) &&
                               s.GrupoNumero == request.GrupoNumero)
                    .ToListAsync();

                _context.Solicitudes.RemoveRange(solicitudesRelacionadas);

                await _context.SaveChangesAsync();

                string mensaje = eraLider ? 
                    $"Estudiante eliminado del grupo. El liderazgo fue reasignado automáticamente." : 
                    $"Estudiante eliminado del grupo exitosamente.";

                return Json(new { success = true, message = mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar estudiante: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CambiarLiderGrupo([FromBody] CambiarLiderRequest request)
        {
            try
            {
                // Verificar que el grupo existe
                var grupo = await _context.Grupos
                    .Include(g => g.Lider)
                    .FirstOrDefaultAsync(g => g.Numero == request.GrupoNumero);

                if (grupo == null)
                {
                    return Json(new { success = false, message = "Grupo no encontrado" });
                }

                // Verificar que el nuevo líder está en el grupo
                var nuevoLiderEnGrupo = await _context.GruposEstudiantes
                    .Include(ge => ge.Estudiante)
                    .FirstOrDefaultAsync(ge => ge.GrupoNumero == request.GrupoNumero && 
                                              ge.EstudianteIdentificacion == request.NuevoLiderId);

                if (nuevoLiderEnGrupo == null)
                {
                    return Json(new { success = false, message = "El estudiante seleccionado no pertenece al grupo" });
                }

                var liderAnterior = grupo.Lider;
                var nuevoLider = nuevoLiderEnGrupo.Estudiante;

                // Cambiar el líder
                grupo.LiderIdentificacion = request.NuevoLiderId;

                await _context.SaveChangesAsync();

                string mensaje = liderAnterior != null ? 
                    $"Liderazgo transferido de {liderAnterior.Nombre} {liderAnterior.Apellidos} a {nuevoLider.Nombre} {nuevoLider.Apellidos}" :
                    $"Nuevo líder asignado: {nuevoLider.Nombre} {nuevoLider.Apellidos}";

                Console.WriteLine($"👑 {mensaje}");

                return Json(new { success = true, message = mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cambiar líder: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDetallesGrupo(int grupoNumero)
        {
            try
            {
                var grupo = await _context.Grupos
                    .Include(g => g.GruposEstudiantes)
                        .ThenInclude(ge => ge.Estudiante)
                    .Include(g => g.Lider)
                    .Include(g => g.Entregas)
                    .Include(g => g.GruposProfesores)
                        .ThenInclude(gp => gp.Profesor)
                    .FirstOrDefaultAsync(g => g.Numero == grupoNumero);

                if (grupo == null)
                {
                    return Json(new { success = false, message = "Grupo no encontrado" });
                }

                var resultado = new
                {
                    success = true,
                    grupo = new
                    {
                        Numero = grupo.Numero,
                        LiderIdentificacion = grupo.LiderIdentificacion,
                        NombreLider = grupo.Lider != null ? $"{grupo.Lider.Nombre} {grupo.Lider.Apellidos}" : null,
                        CantidadMiembros = grupo.GruposEstudiantes.Count,
                        CantidadEntregas = grupo.Entregas.Count,
                        TieneTutor = grupo.GruposProfesores.Any(),
                        Miembros = grupo.GruposEstudiantes.Select(ge => new
                        {
                            EstudianteId = ge.EstudianteIdentificacion,
                            Nombre = ge.Estudiante.Nombre,
                            Apellidos = ge.Estudiante.Apellidos,
                            NombreCompleto = $"{ge.Estudiante.Nombre} {ge.Estudiante.Apellidos}",
                            Clase = ge.Estudiante.Clase,
                            EsLider = ge.EstudianteIdentificacion == grupo.LiderIdentificacion
                        }).OrderByDescending(m => m.EsLider).ThenBy(m => m.Nombre).ToList(),
                        Tutores = grupo.GruposProfesores.Select(gp => new
                        {
                            ProfesorId = gp.ProfesorIdentificacion,
                            Nombre = gp.Profesor.Nombre,
                            Apellidos = gp.Profesor.Apellidos,
                            NombreCompleto = $"{gp.Profesor.Nombre} {gp.Profesor.Apellidos}"
                        }).ToList()
                    }
                };

                return Json(resultado);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener detalles: " + ex.Message });
            }
        }
    }

    public class EliminarEstudianteRequest
    {
        public int GrupoNumero { get; set; }
        public int EstudianteId { get; set; }
    }

    public class CambiarLiderRequest
    {
        public int GrupoNumero { get; set; }
        public int NuevoLiderId { get; set; }
    }
}
