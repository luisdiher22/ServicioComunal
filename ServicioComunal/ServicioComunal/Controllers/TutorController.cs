using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioComunal.Data;
using ServicioComunal.Models;
using ServicioComunal.Attributes;
using ServicioComunal.Services;
using System.Text.Json.Serialization;

namespace ServicioComunal.Controllers
{
    [RequireAuth("Profesor")]
    public class TutorController : Controller
    {
        private readonly ServicioComunalDbContext _context;
        private readonly NotificacionService _notificacionService;

        public TutorController(ServicioComunalDbContext context, NotificacionService notificacionService)
        {
            _context = context;
            _notificacionService = notificacionService;
        }

        // Método para verificar si el usuario actual es un tutor
        private async Task<bool> EsTutor(int identificacion)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Identificacion == identificacion);
            
            if (usuario?.Rol != "Profesor") return false;

            var profesor = await _context.Profesores
                .FirstOrDefaultAsync(p => p.Identificacion == identificacion);
            
            return profesor?.Rol == "Tutor";
        }

        // Método de prueba simple
        public IActionResult Test()
        {
            return Content("¡El controlador Tutor funciona correctamente!");
        }

        // Dashboard principal del tutor
        public async Task<IActionResult> Dashboard()
        {
            // Verificar si el usuario está autenticado
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuarioRol = HttpContext.Session.GetString("UsuarioRol");
            
            if (usuarioId == null || usuarioRol != "Profesor")
            {
                return RedirectToAction("Login", "Auth");
            }

            int tutorId = usuarioId.Value;
            
            // Verificar si es tutor (temporal: comentado para debugging)
            // if (!await EsTutor(tutorId))
            // {
            //     return RedirectToAction("Login", "Auth");
            // }

            Console.WriteLine($"Accediendo al Dashboard de Tutor - ID: {tutorId}...");

            // Obtener estadísticas del tutor
            var gruposAsignados = await _context.GruposProfesores
                .Where(gp => gp.ProfesorIdentificacion == tutorId)
                .Include(gp => gp.Grupo)
                .ThenInclude(g => g.GruposEstudiantes)
                .ThenInclude(ge => ge.Estudiante)
                .Include(gp => gp.Grupo)
                .ThenInclude(g => g.Entregas)
                .ToListAsync();

            // Estadísticas del dashboard
            var totalGrupos = gruposAsignados.Count;
            var revisionesPendientes = await _context.Entregas
                .Where(e => gruposAsignados.Select(ga => ga.GrupoNumero).Contains(e.GrupoNumero) 
                           && string.IsNullOrEmpty(e.Retroalimentacion)
                           && !string.IsNullOrEmpty(e.ArchivoRuta))
                .CountAsync();

            // Calcular progreso promedio (simplificado por ahora)
            var progresoPromedio = gruposAsignados.Any() ? 50 : 0; // Temporal

            // Entregas de esta semana
            var inicioSemana = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
            var finSemana = inicioSemana.AddDays(7);
            var entregasSemana = await _context.Entregas
                .Where(e => gruposAsignados.Select(ga => ga.GrupoNumero).Contains(e.GrupoNumero)
                           && e.FechaLimite >= inicioSemana && e.FechaLimite < finSemana)
                .CountAsync();

            ViewBag.TotalGrupos = totalGrupos;
            ViewBag.RevisionesPendientes = revisionesPendientes;
            ViewBag.ProgresoPromedio = progresoPromedio;
            ViewBag.EntregasSemana = entregasSemana;
            ViewBag.GruposAsignados = gruposAsignados;

            return View();
        }

        // Mis Grupos
        public async Task<IActionResult> MisGrupos()
        {
            var tutorId = HttpContext.Session.GetInt32("UsuarioId");

            var gruposAsignados = await _context.GruposProfesores
                .Where(gp => gp.ProfesorIdentificacion == tutorId)
                .Include(gp => gp.Grupo)
                .ThenInclude(g => g.GruposEstudiantes)
                .ThenInclude(ge => ge.Estudiante)
                .Include(gp => gp.Grupo)
                .ThenInclude(g => g.Entregas)
                .ToListAsync();

            return View(gruposAsignados);
        }

        // Revisiones
        public async Task<IActionResult> Revisiones(int? grupo = null)
        {
            // Verificar autenticación
            var tutorId = HttpContext.Session.GetInt32("UsuarioId");
            var usuarioRol = HttpContext.Session.GetString("UsuarioRol");
            
            if (tutorId == null || usuarioRol != "Profesor")
            {
                return RedirectToAction("Login", "Auth");
            }

            // Obtener grupos asignados al tutor
            var gruposAsignados = await _context.GruposProfesores
                .Where(gp => gp.ProfesorIdentificacion == tutorId)
                .Select(gp => gp.GrupoNumero)
                .ToListAsync();

            if (!gruposAsignados.Any())
            {
                ViewBag.Message = "No tienes grupos asignados.";
                return View(new List<Entrega>());
            }

            // Si se especifica un grupo, verificar que esté asignado al tutor
            if (grupo.HasValue && !gruposAsignados.Contains(grupo.Value))
            {
                ViewBag.Message = "No tienes acceso a este grupo.";
                return View(new List<Entrega>());
            }

            // Filtrar por grupo específico si se proporciona
            var gruposAFiltrar = grupo.HasValue ? new List<int> { grupo.Value } : gruposAsignados;

            // Obtener entregas de los grupos (filtradas o todas las asignadas)
            var entregas = await _context.Entregas
                .Where(e => gruposAFiltrar.Contains(e.GrupoNumero))
                .Include(e => e.Grupo)
                .ThenInclude(g => g.Lider) // Incluir información del líder del grupo
                .Include(e => e.Grupo.GruposEstudiantes)
                .ThenInclude(ge => ge.Estudiante)
                .Include(e => e.Formulario)
                .OrderBy(e => e.FechaLimite < DateTime.Now ? 0 : 1)  // Vencidas primero
                .ThenBy(e => e.FechaLimite)                          // Por fecha límite
                .ThenBy(e => string.IsNullOrEmpty(e.Retroalimentacion) && !string.IsNullOrEmpty(e.ArchivoRuta) ? 0 : 1)  // Listas para revisar primero
                .ThenBy(e => e.GrupoNumero)                          // Por grupo
                .ThenBy(e => e.Nombre)                               // Por nombre para consistencia
                .ToListAsync();

            // Pasar información del filtro a la vista
            ViewBag.GrupoFiltrado = grupo;
            ViewBag.GruposAsignados = gruposAsignados;

            // Categorizar entregas
            ViewBag.EntregasPendientes = entregas.Where(e => 
                !string.IsNullOrEmpty(e.ArchivoRuta) && 
                (string.IsNullOrEmpty(e.Retroalimentacion) || e.Retroalimentacion == "Pendiente de revisión")).ToList();
                
            ViewBag.EntregasRevisadas = entregas.Where(e => 
                !string.IsNullOrEmpty(e.ArchivoRuta) && 
                !string.IsNullOrEmpty(e.Retroalimentacion) && 
                e.Retroalimentacion != "Pendiente de revisión").ToList();
                
            ViewBag.EntregasSinSubir = entregas.Where(e => 
                string.IsNullOrEmpty(e.ArchivoRuta)).ToList();

            return View(entregas);
        }

        // Progreso
        public async Task<IActionResult> Progreso()
        {
            var tutorId = HttpContext.Session.GetInt32("UsuarioId");
            
            if (tutorId==null || !await EsTutor(tutorId.Value))
            {
                return RedirectToAction("Login", "Auth");
            }

            var gruposConProgreso = await _context.GruposProfesores
                .Where(gp => gp.ProfesorIdentificacion == tutorId)
                .Include(gp => gp.Grupo)
                .ThenInclude(g => g.GruposEstudiantes)
                .ThenInclude(ge => ge.Estudiante)
                .Include(gp => gp.Grupo)
                .ThenInclude(g => g.Entregas)
                .ToListAsync();

            return View(gruposConProgreso);
        }

        // API para aprobar entrega
        [HttpPost]
        public async Task<IActionResult> AprobarEntrega([FromBody] AprobarEntregaRequest request)
        {
            try
            {
                Console.WriteLine($"AprobarEntrega llamado con EntregaId: {request.EntregaId}");
                
                var entrega = await _context.Entregas
                    .Include(e => e.Grupo)
                    .ThenInclude(g => g.GruposEstudiantes)
                    .ThenInclude(ge => ge.Estudiante)
                    .FirstOrDefaultAsync(e => e.Identificacion == request.EntregaId);
                
                if (entrega != null)
                {
                    Console.WriteLine($"Entrega encontrada: {entrega.Identificacion} - {entrega.Nombre}");
                    
                    entrega.Retroalimentacion = $"APROBADO: {request.Comentarios ?? "Sin comentarios adicionales"}";
                    entrega.FechaRetroalimentacion = DateTime.Now;
                    await _context.SaveChangesAsync();

                    // Notificar a todos los estudiantes del grupo sobre la retroalimentación
                    var tutorId = HttpContext.Session.GetInt32("UsuarioId");
                    if (tutorId != null && entrega.Grupo?.GruposEstudiantes != null)
                    {
                        foreach (var grupoEstudiante in entrega.Grupo.GruposEstudiantes)
                        {
                            await _notificacionService.NotificarRetroalimentacionAsync(
                                grupoEstudiante.EstudianteIdentificacion,
                                entrega.Identificacion,
                                entrega.Nombre,
                                tutorId.Value
                            );

                            await _notificacionService.NotificarEntregaRevisadaAsync(
                                grupoEstudiante.EstudianteIdentificacion,
                                entrega.Identificacion,
                                entrega.Nombre,
                                "aprobada"
                            );
                        }
                    }
                    
                    return Json(new { success = true, message = "Entrega aprobada correctamente" });
                }
                
                Console.WriteLine($"Entrega no encontrada para ID: {request.EntregaId}");
                
                // Verificar si existen entregas y cuáles son sus IDs
                var todasLasEntregas = await _context.Entregas.Select(e => e.Identificacion).ToListAsync();
                Console.WriteLine($"IDs de entregas existentes: {string.Join(", ", todasLasEntregas)}");
                
                return Json(new { success = false, message = "Entrega no encontrada" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en AprobarEntrega: {ex.Message}");
                return Json(new { success = false, message = "Error al aprobar la entrega: " + ex.Message });
            }
        }

        // API para solicitar cambios
        [HttpPost]
        public async Task<IActionResult> SolicitarCambios([FromBody] SolicitarCambiosRequest request)
        {
            try
            {
                Console.WriteLine($"SolicitarCambios llamado con EntregaId: {request.EntregaId}");
                
                var entrega = await _context.Entregas
                    .Include(e => e.Grupo)
                    .ThenInclude(g => g.GruposEstudiantes)
                    .ThenInclude(ge => ge.Estudiante)
                    .FirstOrDefaultAsync(e => e.Identificacion == request.EntregaId);
                
                if (entrega != null)
                {
                    Console.WriteLine($"Entrega encontrada: {entrega.Identificacion} - {entrega.Nombre}");
                    
                    entrega.Retroalimentacion = $"CAMBIOS SOLICITADOS: {request.Comentarios}";
                    entrega.FechaRetroalimentacion = DateTime.Now;
                    await _context.SaveChangesAsync();

                    // Notificar a todos los estudiantes del grupo sobre la retroalimentación
                    var tutorId = HttpContext.Session.GetInt32("UsuarioId");
                    if (tutorId != null && entrega.Grupo?.GruposEstudiantes != null)
                    {
                        foreach (var grupoEstudiante in entrega.Grupo.GruposEstudiantes)
                        {
                            await _notificacionService.NotificarRetroalimentacionAsync(
                                grupoEstudiante.EstudianteIdentificacion,
                                entrega.Identificacion,
                                entrega.Nombre,
                                tutorId.Value
                            );

                            await _notificacionService.NotificarEntregaRevisadaAsync(
                                grupoEstudiante.EstudianteIdentificacion,
                                entrega.Identificacion,
                                entrega.Nombre,
                                "requiere cambios"
                            );
                        }
                    }
                    
                    return Json(new { success = true, message = "Cambios solicitados correctamente" });
                }
                
                Console.WriteLine($"Entrega no encontrada para ID: {request.EntregaId}");
                
                // Verificar si existen entregas y cuáles son sus IDs
                var todasLasEntregas = await _context.Entregas.Select(e => e.Identificacion).ToListAsync();
                Console.WriteLine($"IDs de entregas existentes: {string.Join(", ", todasLasEntregas)}");
                
                return Json(new { success = false, message = "Entrega no encontrada" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en SolicitarCambios: {ex.Message}");
                return Json(new { success = false, message = "Error al solicitar cambios: " + ex.Message });
            }
        }

        // API para descargar archivo
        public async Task<IActionResult> DescargarArchivo(int entregaId)
        {
            var tutorId = HttpContext.Session.GetInt32("UsuarioId");
            if (tutorId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Verificar que la entrega existe y el tutor tiene acceso
            var entrega = await _context.Entregas
                .Include(e => e.Grupo)
                .ThenInclude(g => g.GruposProfesores)
                .FirstOrDefaultAsync(e => e.Identificacion == entregaId);

            if (entrega == null || string.IsNullOrEmpty(entrega.ArchivoRuta))
            {
                return NotFound("Archivo no encontrado");
            }

            // Verificar que el tutor tiene acceso a este grupo
            var tieneAcceso = entrega.Grupo.GruposProfesores
                .Any(gp => gp.ProfesorIdentificacion == tutorId.Value);

            if (!tieneAcceso)
            {
                return Forbid();
            }

            // Construir la ruta completa del archivo
            var rutaCompleta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", entrega.ArchivoRuta.TrimStart('/'));

            if (!System.IO.File.Exists(rutaCompleta))
            {
                return NotFound("El archivo no existe en el servidor");
            }

            var extension = Path.GetExtension(rutaCompleta);
            var mimeType = GetMimeType(extension);
            var nombreDescarga = $"Entrega_{entrega.Nombre}_Grupo_{entrega.Grupo.Numero}{extension}";

            var fileBytes = await System.IO.File.ReadAllBytesAsync(rutaCompleta);
            return File(fileBytes, mimeType, nombreDescarga);
        }

        private string GetMimeType(string extension)
        {
            return extension.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                _ => "application/octet-stream"
            };
        }

        // Método para obtener detalles completos de un grupo
        [HttpGet]
        public async Task<IActionResult> ObtenerDetallesGrupo(int numeroGrupo)
        {
            try
            {
                // Verificar autenticación y rol
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (usuarioId == null || !await EsTutor(usuarioId.Value))
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                // Verificar que el tutor está asignado a este grupo
                var grupoProfesor = await _context.GruposProfesores
                    .FirstOrDefaultAsync(gp => gp.GrupoNumero == numeroGrupo && gp.ProfesorIdentificacion == usuarioId.Value);

                if (grupoProfesor == null)
                {
                    return Json(new { success = false, message = "No tienes acceso a este grupo" });
                }

                // Obtener información completa del grupo
                var grupo = await _context.Grupos
                    .Include(g => g.GruposEstudiantes)
                        .ThenInclude(ge => ge.Estudiante)
                    .Include(g => g.Lider)
                    .Include(g => g.Entregas)
                    .FirstOrDefaultAsync(g => g.Numero == numeroGrupo);

                if (grupo == null)
                {
                    return Json(new { success = false, message = "Grupo no encontrado" });
                }

                // Obtener fecha de asignación del tutor
                var fechaAsignacion = grupoProfesor.FechaAsignacion.ToString("dd/MM/yyyy");

                // Preparar datos del grupo
                var grupoInfo = new
                {
                    numero = grupo.Numero,
                    liderIdentificacion = grupo.LiderIdentificacion,
                    lider = grupo.Lider != null ? $"{grupo.Lider.Nombre} {grupo.Lider.Apellidos}" : null,
                    fechaAsignacion = fechaAsignacion,
                    estudiantes = grupo.GruposEstudiantes.Select(ge => new
                    {
                        identificacion = ge.Estudiante.Identificacion,
                        nombre = ge.Estudiante.Nombre,
                        apellidos = ge.Estudiante.Apellidos,
                        clase = ge.Estudiante.Clase
                    }).ToList(),
                    entregables = grupo.Entregas.Select(e => new
                    {
                        identificacion = e.Identificacion,
                        nombre = e.Nombre,
                        descripcion = e.Descripcion,
                        fechaLimite = e.FechaLimite.ToString("yyyy-MM-ddTHH:mm:ss"),
                        fechaEntrega = e.FechaEntrega?.ToString("yyyy-MM-ddTHH:mm:ss"),
                        archivoRuta = e.ArchivoRuta,
                        retroalimentacion = e.Retroalimentacion,
                        fechaRetroalimentacion = e.FechaRetroalimentacion?.ToString("yyyy-MM-ddTHH:mm:ss")
                    }).OrderBy(e => e.fechaLimite).ToList()
                };

                return Json(new { success = true, grupo = grupoInfo });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ObtenerDetallesGrupo: {ex.Message}");
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }
    }

    // Extensión para obtener el inicio de la semana
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }

    // Request models para las acciones del tutor
    public class SolicitarCambiosRequest
    {
        [JsonPropertyName("entregaId")]
        public int EntregaId { get; set; }
        
        [JsonPropertyName("comentarios")]
        public string Comentarios { get; set; } = string.Empty;
    }

    public class AprobarEntregaRequest
    {
        [JsonPropertyName("entregaId")]
        public int EntregaId { get; set; }
        
        [JsonPropertyName("comentarios")]
        public string Comentarios { get; set; } = string.Empty;
    }
}
