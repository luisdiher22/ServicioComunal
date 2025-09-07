using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioComunal.Data;
using ServicioComunal.Models;
using ServicioComunal.Services;
using ServicioComunal.Attributes;
using System.Text.Json;

namespace ServicioComunal.Controllers
{
    /// <summary>
    /// Controlador que maneja las funcionalidades específicas de los estudiantes.
    /// Incluye gestión de grupos, solicitudes de ingreso y dashboards.
    /// </summary>
    [RequireAuth("Estudiante")]
    public class EstudianteController : Controller
    {
        private readonly ServicioComunalDbContext _context;
        private readonly UsuarioService _usuarioService;
        private readonly IPdfFillerService _pdfFillerService;
        private readonly NotificacionService _notificacionService;
        private readonly ILogger<EstudianteController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EstudianteController(ServicioComunalDbContext context, UsuarioService usuarioService, IPdfFillerService pdfFillerService, NotificacionService notificacionService, ILogger<EstudianteController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _usuarioService = usuarioService;
            _pdfFillerService = pdfFillerService;
            _notificacionService = notificacionService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Dashboard()
        {
            var usuarioActual = _usuarioService.ObtenerUsuarioActual();
            if (usuarioActual == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Verificar si el usuario es un estudiante
            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e => e.Identificacion == usuarioActual.Identificacion);

            if (estudiante == null)
            {
                return Forbid();
            }

            // Verificar si el estudiante está en algún grupo
            var grupoEstudiante = await _context.GruposEstudiantes
                .Include(ge => ge.Grupo)
                .FirstOrDefaultAsync(ge => ge.EstudianteIdentificacion == estudiante.Identificacion);

            if (grupoEstudiante == null)
            {
                // Si no está en ningún grupo, redirigir a gestión de grupos
                return RedirectToAction("GestionGrupos");
            }

            // Si está en un grupo, mostrar el dashboard
            ViewBag.Estudiante = estudiante;
            ViewBag.Grupo = grupoEstudiante.Grupo;

            // Obtener entregas del grupo
            var entregas = await _context.Entregas
                .Include(e => e.Formulario)
                .Where(e => e.GrupoNumero == grupoEstudiante.GrupoNumero)
                .AsNoTracking()
                .OrderByDescending(e => e.FechaRetroalimentacion)
                .ToListAsync();

            ViewBag.Entregas = entregas;

            // Obtener compañeros de grupo
            var companeros = await _context.GruposEstudiantes
                .Include(ge => ge.Estudiante)
                .Where(ge => ge.GrupoNumero == grupoEstudiante.GrupoNumero && 
                            ge.EstudianteIdentificacion != estudiante.Identificacion)
                .Select(ge => ge.Estudiante)
                .ToListAsync();

            ViewBag.Companeros = companeros;

            // Obtener tutor asignado
            var tutor = await _context.GruposProfesores
                .Include(gp => gp.Profesor)
                .Where(gp => gp.GrupoNumero == grupoEstudiante.GrupoNumero)
                .Select(gp => gp.Profesor)
                .FirstOrDefaultAsync();

            ViewBag.Tutor = tutor;

            return View();
        }

        public async Task<IActionResult> GestionGrupos()
        {
            var usuarioActual = _usuarioService.ObtenerUsuarioActual();
            if (usuarioActual == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e => e.Identificacion == usuarioActual.Identificacion);

            if (estudiante == null)
            {
                return Forbid();
            }

            ViewBag.Estudiante = estudiante;

            // Obtener grupos existentes con sus integrantes
            var grupos = await _context.Grupos
                .Include(g => g.GruposEstudiantes)
                    .ThenInclude(ge => ge.Estudiante)
                .Include(g => g.GruposProfesores)
                    .ThenInclude(gp => gp.Profesor)
                .Include(g => g.Lider)
                .OrderBy(g => g.Numero)
                .ToListAsync();

            ViewBag.Grupos = grupos;

            // Obtener estudiantes sin grupo (excluyendo al estudiante actual)
            var estudiantesConGrupo = await _context.GruposEstudiantes
                .Select(ge => ge.EstudianteIdentificacion)
                .ToListAsync();

            var estudiantesSinGrupo = await _context.Estudiantes
                .Where(e => !estudiantesConGrupo.Contains(e.Identificacion) && 
                           e.Identificacion != estudiante.Identificacion)
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            ViewBag.EstudiantesSinGrupo = estudiantesSinGrupo;

            // Obtener solicitudes pendientes dirigidas al estudiante actual
            var solicitudesPendientes = await _context.Solicitudes
                .Include(s => s.EstudianteRemitente)
                .Include(s => s.Grupo)
                .Where(s => s.EstudianteDestinatarioId == estudiante.Identificacion && 
                           s.Estado == "PENDIENTE")
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();

            ViewBag.SolicitudesPendientes = solicitudesPendientes;

            // Verificar si el estudiante ya está en un grupo
            var grupoActual = await _context.GruposEstudiantes
                .Include(ge => ge.Grupo)
                .FirstOrDefaultAsync(ge => ge.EstudianteIdentificacion == estudiante.Identificacion);

            ViewBag.GrupoActual = grupoActual?.Grupo;

            return View();
        }

        public async Task<IActionResult> MisSolicitudes()
        {
            var usuarioActual = _usuarioService.ObtenerUsuarioActual();
            if (usuarioActual == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e => e.Identificacion == usuarioActual.Identificacion);

            if (estudiante == null)
            {
                return Forbid();
            }

            // Obtener solicitudes enviadas por el estudiante
            var solicitudesEnviadas = await _context.Solicitudes
                .Include(s => s.EstudianteDestinatario)
                .Include(s => s.Grupo)
                .Where(s => s.EstudianteRemitenteId == estudiante.Identificacion)
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();

            // Obtener solicitudes recibidas por el estudiante
            var solicitudesRecibidas = await _context.Solicitudes
                .Include(s => s.EstudianteRemitente)
                .Include(s => s.Grupo)
                .Where(s => s.EstudianteDestinatarioId == estudiante.Identificacion)
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();

            ViewBag.SolicitudesEnviadas = solicitudesEnviadas;
            ViewBag.SolicitudesRecibidas = solicitudesRecibidas;

            return View();
        }

        public async Task<IActionResult> MiPerfil()
        {
            var usuarioActual = _usuarioService.ObtenerUsuarioActual();
            if (usuarioActual == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e => e.Identificacion == usuarioActual.Identificacion);

            if (estudiante == null)
            {
                return Forbid();
            }

            return View(estudiante);
        }

        [HttpPost]
        public async Task<IActionResult> SolicitarIngreso(int grupoNumero)
        {
            try
            {
                var usuario = HttpContext.Session.GetString("Usuario");
                if (string.IsNullOrEmpty(usuario))
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                // Obtener el usuario actual
                var usuarioActual = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == usuario);
                if (usuarioActual == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // Obtener el estudiante
                var estudiante = await _context.Estudiantes
                    .FirstOrDefaultAsync(e => e.Identificacion == usuarioActual.Identificacion);

                if (estudiante == null)
                {
                    return Json(new { success = false, message = "Estudiante no encontrado" });
                }

                // Verificar que el estudiante no esté ya en un grupo
                var yaEnGrupo = await _context.GruposEstudiantes
                    .AnyAsync(ge => ge.EstudianteIdentificacion == estudiante.Identificacion);

                if (yaEnGrupo)
                {
                    return Json(new { success = false, message = "Ya perteneces a un grupo" });
                }

                // Verificar que el grupo existe y obtener al líder
                var grupo = await _context.Grupos
                    .Include(g => g.GruposEstudiantes)
                    .Include(g => g.Lider)
                    .FirstOrDefaultAsync(g => g.Numero == grupoNumero);

                if (grupo == null)
                {
                    return Json(new { success = false, message = "Grupo no encontrado" });
                }

                // Verificar que el grupo tenga líder
                if (grupo.LiderIdentificacion == null)
                {
                    return Json(new { success = false, message = "Este grupo no tiene líder asignado" });
                }

                // Verificar que no haya una solicitud pendiente ya
                var solicitudExistente = await _context.Solicitudes
                    .AnyAsync(s => s.EstudianteRemitenteId == estudiante.Identificacion &&
                                  s.GrupoNumero == grupoNumero &&
                                  s.Estado == "PENDIENTE");

                if (solicitudExistente)
                {
                    return Json(new { success = false, message = "Ya tienes una solicitud pendiente para este grupo" });
                }

                // Enviar solicitud solo al líder del grupo
                var solicitud = new Solicitud
                {
                    EstudianteRemitenteId = estudiante.Identificacion,
                    EstudianteDestinatarioId = grupo.LiderIdentificacion.Value,
                    GrupoNumero = grupoNumero,
                    Tipo = "SOLICITUD_INGRESO",
                    Estado = "PENDIENTE",
                    Mensaje = $"{estudiante.Nombre} {estudiante.Apellidos} solicita unirse al grupo {grupoNumero}."
                };

                _context.Solicitudes.Add(solicitud);
                await _context.SaveChangesAsync();

                // Notificar al líder del grupo sobre la nueva solicitud
                await _notificacionService.NotificarNuevaSolicitudGrupoAsync(
                    grupo.LiderIdentificacion.Value,
                    estudiante.Identificacion,
                    grupoNumero
                );

                return Json(new { success = true, message = "Solicitud enviada exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al enviar solicitud: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EliminarSolicitud([FromBody] EliminarSolicitudRequest request)
        {
            try
            {
                var usuario = HttpContext.Session.GetString("Usuario");
                if (string.IsNullOrEmpty(usuario))
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                // Obtener el usuario actual
                var usuarioActual = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == usuario);
                if (usuarioActual == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // Buscar la solicitud
                var solicitud = await _context.Solicitudes
                    .Include(s => s.EstudianteRemitente)
                    .Include(s => s.EstudianteDestinatario)
                    .FirstOrDefaultAsync(s => s.Id == request.SolicitudId);

                if (solicitud == null)
                {
                    return Json(new { success = false, message = "Solicitud no encontrada" });
                }

                // Verificar que la solicitud pertenece al usuario actual (remitente)
                if (solicitud.EstudianteRemitenteId != usuarioActual.Identificacion)
                {
                    return Json(new { success = false, message = "No tienes permiso para eliminar esta solicitud" });
                }

                // Solo permitir eliminar solicitudes pendientes
                if (solicitud.Estado != "PENDIENTE")
                {
                    return Json(new { success = false, message = "Solo se pueden eliminar solicitudes pendientes" });
                }

                // Eliminar la solicitud
                _context.Solicitudes.Remove(solicitud);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Solicitud eliminada exitosamente" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al eliminar la solicitud" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResponderSolicitud([FromBody] ResponderSolicitudRequest request)
        {
            var usuarioActual = _usuarioService.ObtenerUsuarioActual();
            if (usuarioActual == null)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            try
            {
                var solicitud = await _context.Solicitudes
                    .Include(s => s.EstudianteRemitente)
                    .Include(s => s.EstudianteDestinatario)
                    .Include(s => s.Grupo)
                    .FirstOrDefaultAsync(s => s.Id == request.SolicitudId);

                if (solicitud == null)
                {
                    return Json(new { success = false, message = "Solicitud no encontrada" });
                }

                // Verificar que el usuario actual es el destinatario
                if (solicitud.EstudianteDestinatarioId != usuarioActual.Identificacion)
                {
                    return Json(new { success = false, message = "No tienes permiso para responder esta solicitud" });
                }

                // Para solicitudes de ingreso, verificar que el usuario es el líder del grupo
                if (solicitud.Tipo == "SOLICITUD_INGRESO")
                {
                    var grupo = await _context.Grupos
                        .FirstOrDefaultAsync(g => g.Numero == solicitud.GrupoNumero);
                    
                    if (grupo == null || grupo.LiderIdentificacion != usuarioActual.Identificacion)
                    {
                        return Json(new { success = false, message = "Solo el líder del grupo puede responder solicitudes de ingreso" });
                    }
                }

                // Verificar que la solicitud está pendiente
                if (solicitud.Estado != "PENDIENTE")
                {
                    return Json(new { success = false, message = "Esta solicitud ya ha sido respondida" });
                }

                // Actualizar el estado de la solicitud
                solicitud.Estado = request.Aceptar ? "ACEPTADA" : "RECHAZADA";
                solicitud.FechaRespuesta = DateTime.Now;

                // Si se acepta la solicitud, agregar al estudiante al grupo
                if (request.Aceptar)
                {
                    // Verificar que el estudiante no esté ya en un grupo
                    var yaEnGrupo = await _context.GruposEstudiantes
                        .AnyAsync(ge => ge.EstudianteIdentificacion == solicitud.EstudianteRemitenteId);

                    if (yaEnGrupo)
                    {
                        return Json(new { success = false, message = "El estudiante ya pertenece a un grupo" });
                    }

                    // Agregar al grupo
                    var nuevoMiembro = new GrupoEstudiante
                    {
                        GrupoNumero = solicitud.GrupoNumero!.Value,
                        EstudianteIdentificacion = solicitud.EstudianteRemitenteId
                    };

                    _context.GruposEstudiantes.Add(nuevoMiembro);

                    // Notificar al estudiante que su solicitud fue aceptada
                    await _notificacionService.NotificarSolicitudAceptadaAsync(
                        solicitud.EstudianteRemitenteId,
                        solicitud.GrupoNumero!.Value
                    );

                    // Rechazar automáticamente todas las demás solicitudes pendientes del mismo estudiante
                    var otrasSolicitudes = await _context.Solicitudes
                        .Where(s => s.EstudianteRemitenteId == solicitud.EstudianteRemitenteId && 
                                   s.Estado == "PENDIENTE" && 
                                   s.Id != solicitud.Id)
                        .ToListAsync();

                    foreach (var otra in otrasSolicitudes)
                    {
                        otra.Estado = "RECHAZADA";
                        otra.FechaRespuesta = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();

                string mensaje = request.Aceptar ? 
                    "Solicitud aceptada. El estudiante ha sido agregado al grupo." : 
                    "Solicitud rechazada.";

                return Json(new { success = true, message = mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al procesar la solicitud: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearGrupo([FromBody] CrearGrupoRequest request)
        {
            try
            {
                var usuarioActual = _usuarioService.ObtenerUsuarioActual();
                if (usuarioActual == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var estudiante = await _context.Estudiantes
                    .FirstOrDefaultAsync(e => e.Identificacion == usuarioActual.Identificacion);

                if (estudiante == null)
                {
                    return Json(new { success = false, message = "Estudiante no encontrado" });
                }

                // Verificar que el estudiante no esté ya en un grupo
                var yaEnGrupo = await _context.GruposEstudiantes
                    .AnyAsync(ge => ge.EstudianteIdentificacion == estudiante.Identificacion);

                if (yaEnGrupo)
                {
                    return Json(new { success = false, message = "Ya perteneces a un grupo" });
                }

                // Verificar que los estudiantes seleccionados existan y no estén en grupo
                var estudiantesInvalidos = new List<int>();
                foreach (var estId in request.EstudiantesSeleccionados)
                {
                    var estudianteSeleccionado = await _context.Estudiantes
                        .FirstOrDefaultAsync(e => e.Identificacion == estId);
                    
                    if (estudianteSeleccionado == null)
                    {
                        estudiantesInvalidos.Add(estId);
                        continue;
                    }

                    var yaEnGrupoSeleccionado = await _context.GruposEstudiantes
                        .AnyAsync(ge => ge.EstudianteIdentificacion == estId);

                    if (yaEnGrupoSeleccionado)
                    {
                        estudiantesInvalidos.Add(estId);
                    }
                }

                if (estudiantesInvalidos.Any())
                {
                    return Json(new { success = false, message = "Algunos estudiantes seleccionados ya pertenecen a un grupo o no existen" });
                }

                // Obtener el próximo número de grupo disponible
                var ultimoGrupo = await _context.Grupos
                    .OrderByDescending(g => g.Numero)
                    .FirstOrDefaultAsync();

                int nuevoNumeroGrupo = (ultimoGrupo?.Numero ?? 0) + 1;

                // Crear el nuevo grupo con el estudiante actual como líder
                var nuevoGrupo = new Grupo
                {
                    Numero = nuevoNumeroGrupo,
                    LiderIdentificacion = estudiante.Identificacion
                };

                _context.Grupos.Add(nuevoGrupo);

                // Agregar al creador como miembro del grupo
                var grupoEstudianteLider = new GrupoEstudiante
                {
                    GrupoNumero = nuevoNumeroGrupo,
                    EstudianteIdentificacion = estudiante.Identificacion
                };

                _context.GruposEstudiantes.Add(grupoEstudianteLider);

                // Crear invitaciones para los estudiantes seleccionados
                foreach (var estId in request.EstudiantesSeleccionados)
                {
                    var invitacion = new Solicitud
                    {
                        EstudianteRemitenteId = estudiante.Identificacion,
                        EstudianteDestinatarioId = estId,
                        GrupoNumero = nuevoNumeroGrupo,
                        Tipo = "INVITACION_GRUPO",
                        Estado = "PENDIENTE",
                        Mensaje = $"{estudiante.Nombre} {estudiante.Apellidos} te invita a unirte al Grupo {nuevoNumeroGrupo}."
                    };

                    _context.Solicitudes.Add(invitacion);
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Grupo {nuevoNumeroGrupo} creado exitosamente. Se han enviado las invitaciones."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al crear el grupo: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> InvitarEstudiante([FromBody] InvitarEstudianteRequest request)
        {
            try
            {
                var usuarioActual = _usuarioService.ObtenerUsuarioActual();
                if (usuarioActual == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var estudiante = await _context.Estudiantes
                    .FirstOrDefaultAsync(e => e.Identificacion == usuarioActual.Identificacion);

                if (estudiante == null)
                {
                    return Json(new { success = false, message = "Estudiante no encontrado" });
                }

                // Verificar que el estudiante actual es líder de un grupo
                var grupo = await _context.Grupos
                    .Include(g => g.GruposEstudiantes)
                    .FirstOrDefaultAsync(g => g.LiderIdentificacion == estudiante.Identificacion);

                if (grupo == null)
                {
                    return Json(new { success = false, message = "No eres líder de ningún grupo" });
                }

                // Verificar que el grupo no esté lleno (máximo 4 miembros)
                if (grupo.GruposEstudiantes.Count >= 4)
                {
                    return Json(new { success = false, message = "El grupo ya tiene el máximo de miembros (4)" });
                }

                // Verificar que el estudiante a invitar existe y no está en un grupo
                var estudianteInvitado = await _context.Estudiantes
                    .FirstOrDefaultAsync(e => e.Identificacion == request.EstudianteId);

                if (estudianteInvitado == null)
                {
                    return Json(new { success = false, message = "Estudiante no encontrado" });
                }

                var yaEnGrupo = await _context.GruposEstudiantes
                    .AnyAsync(ge => ge.EstudianteIdentificacion == request.EstudianteId);

                if (yaEnGrupo)
                {
                    return Json(new { success = false, message = "El estudiante ya pertenece a un grupo" });
                }

                // Verificar que no haya una invitación pendiente ya
                var invitacionExistente = await _context.Solicitudes
                    .AnyAsync(s => s.EstudianteRemitenteId == estudiante.Identificacion &&
                                  s.EstudianteDestinatarioId == request.EstudianteId &&
                                  s.Estado == "PENDIENTE" &&
                                  s.Tipo == "INVITACION_GRUPO");

                if (invitacionExistente)
                {
                    return Json(new { success = false, message = "Ya existe una invitación pendiente para este estudiante" });
                }

                // Crear la invitación
                var invitacion = new Solicitud
                {
                    EstudianteRemitenteId = estudiante.Identificacion,
                    EstudianteDestinatarioId = request.EstudianteId,
                    GrupoNumero = grupo.Numero,
                    Tipo = "INVITACION_GRUPO",
                    Estado = "PENDIENTE",
                    Mensaje = $"{estudiante.Nombre} {estudiante.Apellidos} te invita a unirte al Grupo {grupo.Numero}."
                };

                _context.Solicitudes.Add(invitacion);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Invitación enviada exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al enviar la invitación: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LimpiarGruposSinLider()
        {
            try
            {
                // Solo permitir esta acción a usuarios autenticados
                var usuarioActual = _usuarioService.ObtenerUsuarioActual();
                if (usuarioActual == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                // Buscar grupos sin líder
                var gruposSinLider = await _context.Grupos
                    .Where(g => g.LiderIdentificacion == null)
                    .Include(g => g.GruposEstudiantes)
                    .Include(g => g.Entregas)
                    .Include(g => g.GruposProfesores)
                    .Include(g => g.Notificaciones)
                    .ToListAsync();

                if (!gruposSinLider.Any())
                {
                    return Json(new { success = true, message = "No hay grupos sin líder para eliminar" });
                }

                int gruposEliminados = 0;
                foreach (var grupo in gruposSinLider)
                {
                    // Eliminar todas las relaciones primero
                    _context.GruposEstudiantes.RemoveRange(grupo.GruposEstudiantes);
                    _context.Entregas.RemoveRange(grupo.Entregas);
                    _context.GruposProfesores.RemoveRange(grupo.GruposProfesores);
                    _context.Notificaciones.RemoveRange(grupo.Notificaciones);

                    // Eliminar solicitudes relacionadas con el grupo
                    var solicitudesGrupo = await _context.Solicitudes
                        .Where(s => s.GrupoNumero == grupo.Numero)
                        .ToListAsync();
                    _context.Solicitudes.RemoveRange(solicitudesGrupo);

                    // Eliminar el grupo
                    _context.Grupos.Remove(grupo);
                    gruposEliminados++;
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Se eliminaron {gruposEliminados} grupos sin líder"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al limpiar grupos: " + ex.Message });
            }
        }

        public async Task<IActionResult> MiGrupo()
        {
            var usuarioActual = _usuarioService.ObtenerUsuarioActual();
            if (usuarioActual == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Verificar si el usuario es un estudiante
            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e => e.Identificacion == usuarioActual.Identificacion);

            if (estudiante == null)
            {
                return Forbid();
            }

            // Verificar si el estudiante está en algún grupo
            var grupoEstudiante = await _context.GruposEstudiantes
                .Include(ge => ge.Grupo)
                .FirstOrDefaultAsync(ge => ge.EstudianteIdentificacion == estudiante.Identificacion);

            if (grupoEstudiante == null)
            {
                // Si no está en ningún grupo, redirigir a gestión de grupos
                return RedirectToAction("GestionGrupos");
            }

            ViewBag.Estudiante = estudiante;
            ViewBag.Grupo = grupoEstudiante.Grupo;

            // Obtener todos los integrantes del grupo (incluyendo al estudiante actual)
            var integrantesGrupo = await _context.GruposEstudiantes
                .Include(ge => ge.Estudiante)
                .Where(ge => ge.GrupoNumero == grupoEstudiante.GrupoNumero)
                .Select(ge => ge.Estudiante)
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            ViewBag.IntegrantesGrupo = integrantesGrupo;

            // Obtener el líder del grupo
            var lider = integrantesGrupo.FirstOrDefault(e => e.Identificacion == grupoEstudiante.Grupo.LiderIdentificacion);
            ViewBag.Lider = lider;

            // Obtener tutor asignado
            var tutor = await _context.GruposProfesores
                .Include(gp => gp.Profesor)
                .Where(gp => gp.GrupoNumero == grupoEstudiante.GrupoNumero)
                .Select(gp => gp.Profesor)
                .FirstOrDefaultAsync();

            ViewBag.Tutor = tutor;

            return View();
        }

        public async Task<IActionResult> Entregas()
        {
            var usuarioActual = _usuarioService.ObtenerUsuarioActual();
            if (usuarioActual == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Verificar si el usuario es un estudiante
            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e => e.Identificacion == usuarioActual.Identificacion);

            if (estudiante == null)
            {
                return Forbid();
            }

            // Verificar si el estudiante está en algún grupo
            var grupoEstudiante = await _context.GruposEstudiantes
                .Include(ge => ge.Grupo)
                .FirstOrDefaultAsync(ge => ge.EstudianteIdentificacion == estudiante.Identificacion);

            if (grupoEstudiante == null)
            {
                // Si no está en ningún grupo, redirigir a gestión de grupos
                return RedirectToAction("GestionGrupos");
            }

            ViewBag.Estudiante = estudiante;
            ViewBag.Grupo = grupoEstudiante.Grupo;

            // Obtener entregas del grupo
            var entregas = await _context.Entregas
                .Include(e => e.Formulario)
                .Include(e => e.Profesor)
                .Where(e => e.GrupoNumero == grupoEstudiante.GrupoNumero)
                .AsNoTracking()
                .OrderByDescending(e => e.FechaLimite)
                .ToListAsync();

            ViewBag.Entregas = entregas;

            return View();
        }

        public async Task<IActionResult> DetalleEntrega(int id)
        {
            var usuarioActual = _usuarioService.ObtenerUsuarioActual();
            if (usuarioActual == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Verificar si el usuario es un estudiante
            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e => e.Identificacion == usuarioActual.Identificacion);

            if (estudiante == null)
            {
                return Forbid();
            }

            // Verificar si el estudiante está en algún grupo
            var grupoEstudiante = await _context.GruposEstudiantes
                .Include(ge => ge.Grupo)
                .FirstOrDefaultAsync(ge => ge.EstudianteIdentificacion == estudiante.Identificacion);

            if (grupoEstudiante == null)
            {
                return RedirectToAction("GestionGrupos");
            }

            // Obtener la entrega específica
            var entrega = await _context.Entregas
                .Include(e => e.Formulario)
                .Include(e => e.Profesor)
                .Include(e => e.Grupo)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Identificacion == id && e.GrupoNumero == grupoEstudiante.GrupoNumero);

            if (entrega == null)
            {
                return NotFound();
            }

            ViewBag.Estudiante = estudiante;
            ViewBag.Grupo = grupoEstudiante.Grupo;

            return View(entrega);
        }

        [HttpPost]
        public async Task<IActionResult> SubirEntrega(int entregaId, IFormFile archivo)
        {
            try
            {
                var usuarioActual = _usuarioService.ObtenerUsuarioActual();
                if (usuarioActual == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                // Verificar si el usuario es un estudiante
                var estudiante = await _context.Estudiantes
                    .FirstOrDefaultAsync(e => e.Identificacion == usuarioActual.Identificacion);

                if (estudiante == null)
                {
                    return Json(new { success = false, message = "Estudiante no encontrado" });
                }

                // Verificar si el estudiante está en algún grupo
                var grupoEstudiante = await _context.GruposEstudiantes
                    .FirstOrDefaultAsync(ge => ge.EstudianteIdentificacion == estudiante.Identificacion);

                if (grupoEstudiante == null)
                {
                    return Json(new { success = false, message = "No perteneces a ningún grupo" });
                }

                // Obtener la entrega
                var entrega = await _context.Entregas
                    .FirstOrDefaultAsync(e => e.Identificacion == entregaId && e.GrupoNumero == grupoEstudiante.GrupoNumero);

                if (entrega == null)
                {
                    return Json(new { success = false, message = "Entrega no encontrada" });
                }

                // Verificar que se haya subido un archivo
                if (archivo == null || archivo.Length == 0)
                {
                    return Json(new { success = false, message = "Debe seleccionar un archivo" });
                }

                // Crear el directorio de entregas si no existe
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "entregas");
                Directory.CreateDirectory(uploadsPath);

                // Generar nombre único para el archivo
                var extension = Path.GetExtension(archivo.FileName);
                var nombreArchivo = $"entrega_{entregaId}_grupo_{grupoEstudiante.GrupoNumero}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                var rutaArchivo = Path.Combine(uploadsPath, nombreArchivo);

                // Guardar el archivo
                using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                // Actualizar la ruta del archivo en la entrega
                entrega.ArchivoRuta = $"/uploads/entregas/{nombreArchivo}";
                entrega.FechaEntrega = DateTime.Now;
                
                // Si había cambios solicitados, limpiar la retroalimentación para permitir nueva revisión
                if (!string.IsNullOrEmpty(entrega.Retroalimentacion) && entrega.Retroalimentacion.StartsWith("CAMBIOS SOLICITADOS:"))
                {
                    entrega.Retroalimentacion = string.Empty;
                    entrega.FechaRetroalimentacion = null;
                }

                await _context.SaveChangesAsync();

                // Obtener el tutor asignado al grupo y notificarle sobre el entregable recibido
                var grupoProfesor = await _context.GruposProfesores
                    .FirstOrDefaultAsync(gp => gp.GrupoNumero == grupoEstudiante.GrupoNumero);

                if (grupoProfesor != null)
                {
                    await _notificacionService.NotificarEntregableRecibidoAsync(
                        grupoProfesor.ProfesorIdentificacion,
                        entrega.Identificacion,
                        entrega.Nombre,
                        grupoEstudiante.GrupoNumero
                    );
                }

                return Json(new { success = true, message = "Entrega subida exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al subir la entrega: " + ex.Message });
            }
        }

        public async Task<IActionResult> DescargarArchivo(int entregaId, string tipo)
        {
            var usuarioActual = _usuarioService.ObtenerUsuarioActual();
            if (usuarioActual == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Verificar si el usuario es un estudiante
            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e => e.Identificacion == usuarioActual.Identificacion);

            if (estudiante == null)
            {
                return Forbid();
            }

            // Verificar si el estudiante está en algún grupo
            var grupoEstudiante = await _context.GruposEstudiantes
                .FirstOrDefaultAsync(ge => ge.EstudianteIdentificacion == estudiante.Identificacion);

            if (grupoEstudiante == null)
            {
                return NotFound();
            }

            // Obtener la entrega
            var entrega = await _context.Entregas
                .Include(e => e.Formulario)
                .FirstOrDefaultAsync(e => e.Identificacion == entregaId && e.GrupoNumero == grupoEstudiante.GrupoNumero);

            if (entrega == null)
            {
                return NotFound();
            }

            string rutaArchivo = "";
            string nombreDescarga = "";

            if (tipo == "formulario" && entrega.Formulario != null)
            {
                rutaArchivo = entrega.Formulario.ArchivoRuta;
                nombreDescarga = $"Formulario_{entrega.Formulario.Nombre}";
            }
            else if (tipo == "entrega")
            {
                rutaArchivo = entrega.ArchivoRuta;
                nombreDescarga = $"Entrega_{entrega.Nombre}";
            }

            if (string.IsNullOrEmpty(rutaArchivo))
            {
                return NotFound("Archivo no encontrado");
            }

            // Construir la ruta completa del archivo
            var rutaCompleta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", rutaArchivo.TrimStart('/'));

            if (!System.IO.File.Exists(rutaCompleta))
            {
                return NotFound("El archivo no existe en el servidor");
            }

            var extension = Path.GetExtension(rutaCompleta);
            var mimeType = GetMimeType(extension);

            var fileBytes = await System.IO.File.ReadAllBytesAsync(rutaCompleta);
            return File(fileBytes, mimeType, $"{nombreDescarga}{extension}");
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

        // ===== GESTIÓN DE ENTREGAS =====

        /// <summary>
        /// Vista para mostrar las entregas del grupo del estudiante
        /// </summary>
        public async Task<IActionResult> MisEntregas()
        {
            // Verificar autenticación
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuarioRol = HttpContext.Session.GetString("UsuarioRol");
            
            if (usuarioId == null || usuarioRol != "Estudiante")
            {
                return RedirectToAction("Login", "Auth");
            }

            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e => e.Identificacion == usuarioId.Value);

            if (estudiante == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Obtener el grupo del estudiante
            var grupoEstudiante = await _context.GruposEstudiantes
                .Include(ge => ge.Grupo)
                .FirstOrDefaultAsync(ge => ge.EstudianteIdentificacion == estudiante.Identificacion);

            if (grupoEstudiante == null)
            {
                ViewBag.Error = "No estás asignado a ningún grupo.";
                return View(new List<Entrega>());
            }

            // Obtener entregas del grupo
            var entregas = await _context.Entregas
                .Include(e => e.Formulario)
                .Include(e => e.Profesor)
                .Where(e => e.GrupoNumero == grupoEstudiante.GrupoNumero)
                .OrderByDescending(e => e.FechaLimite)
                .ToListAsync();

            ViewBag.Estudiante = estudiante;
            ViewBag.Grupo = grupoEstudiante.Grupo;

            return View(entregas);
        }

        /// <summary>
        /// Subir archivo para una entrega específica
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SubirEntrega()
        {
            try
            {
                // Verificar autenticación
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                var usuarioRol = HttpContext.Session.GetString("UsuarioRol");
                
                if (usuarioId == null || usuarioRol != "Estudiante")
                {
                    return Json(new { success = false, message = "No tienes permisos para subir entregas" });
                }

                var entregaId = int.Parse(Request.Form["entregaId"].ToString());
                var archivo = Request.Form.Files.GetFile("archivo");

                if (archivo == null || archivo.Length == 0)
                {
                    return Json(new { success = false, message = "Debe seleccionar un archivo" });
                }

                // Verificar que la entrega existe y pertenece al grupo del estudiante
                var estudiante = await _context.Estudiantes
                    .FirstOrDefaultAsync(e => e.Identificacion == usuarioId.Value);

                if (estudiante == null)
                {
                    return Json(new { success = false, message = "Estudiante no encontrado" });
                }

                var grupoEstudiante = await _context.GruposEstudiantes
                    .FirstOrDefaultAsync(ge => ge.EstudianteIdentificacion == estudiante.Identificacion);

                if (grupoEstudiante == null)
                {
                    return Json(new { success = false, message = "No estás asignado a ningún grupo" });
                }

                var entrega = await _context.Entregas
                    .FirstOrDefaultAsync(e => e.Identificacion == entregaId && 
                                            e.GrupoNumero == grupoEstudiante.GrupoNumero);

                if (entrega == null)
                {
                    return Json(new { success = false, message = "Entrega no encontrada o no pertenece a tu grupo" });
                }

                // Verificar fecha límite
                if (DateTime.Now > entrega.FechaLimite)
                {
                    return Json(new { success = false, message = "La fecha límite para esta entrega ya pasó" });
                }

                // Validar tipo de archivo
                var extensionesPermitidas = new[] { ".pdf", ".docx", ".doc", ".txt", ".zip", ".rar" };
                var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
                
                if (!extensionesPermitidas.Contains(extension))
                {
                    return Json(new { success = false, message = "Solo se permiten archivos PDF, DOC, DOCX, TXT, ZIP o RAR" });
                }

                // Validar tamaño (20MB máximo)
                if (archivo.Length > 20 * 1024 * 1024)
                {
                    return Json(new { success = false, message = "El archivo no puede ser mayor a 20MB" });
                }

                // Crear directorio si no existe
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "entregas");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                // Generar nombre único para el archivo
                var nombreArchivo = $"entrega_{entregaId}_{grupoEstudiante.GrupoNumero}_{Guid.NewGuid()}{extension}";
                var rutaCompleta = Path.Combine(uploadsDir, nombreArchivo);

                // Eliminar archivo anterior si existe
                if (!string.IsNullOrEmpty(entrega.ArchivoRuta))
                {
                    var archivoAnterior = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", entrega.ArchivoRuta);
                    if (System.IO.File.Exists(archivoAnterior))
                    {
                        System.IO.File.Delete(archivoAnterior);
                    }
                }

                // Guardar archivo
                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                // Actualizar entrega
                entrega.ArchivoRuta = $"uploads/entregas/{nombreArchivo}";
                entrega.FechaEntrega = DateTime.Now;
                entrega.Retroalimentacion = "Pendiente de revisión";

                await _context.SaveChangesAsync();

                // Notificar al tutor que se ha recibido un entregable
                var tutoresDelGrupo = await _context.GruposProfesores
                    .Where(gp => gp.GrupoNumero == grupoEstudiante.GrupoNumero)
                    .Select(gp => gp.ProfesorIdentificacion)
                    .ToListAsync();

                foreach (var tutorId in tutoresDelGrupo)
                {
                    await _notificacionService.NotificarEntregableRecibidoAsync(
                        tutorId, 
                        entrega.Identificacion, 
                        entrega.Nombre, 
                        grupoEstudiante.GrupoNumero
                    );
                }

                return Json(new { 
                    success = true, 
                    message = "Entrega subida exitosamente",
                    fechaEntrega = entrega.FechaEntrega?.ToString("dd/MM/yyyy HH:mm")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al subir entrega: {ex.Message}" });
            }
        }

        /// <summary>
        /// Descargar archivo de entrega (para estudiantes del grupo)
        /// </summary>
        public async Task<IActionResult> DescargarEntrega(int entregaId)
        {
            try
            {
                // Verificar autenticación
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                var usuarioRol = HttpContext.Session.GetString("UsuarioRol");
                
                if (usuarioId == null || usuarioRol != "Estudiante")
                {
                    return Forbid();
                }

                var estudiante = await _context.Estudiantes
                    .FirstOrDefaultAsync(e => e.Identificacion == usuarioId.Value);

                if (estudiante == null)
                {
                    return NotFound("Estudiante no encontrado");
                }

                var grupoEstudiante = await _context.GruposEstudiantes
                    .FirstOrDefaultAsync(ge => ge.EstudianteIdentificacion == estudiante.Identificacion);

                if (grupoEstudiante == null)
                {
                    return Forbid("No estás asignado a ningún grupo");
                }

                var entrega = await _context.Entregas
                    .FirstOrDefaultAsync(e => e.Identificacion == entregaId && 
                                            e.GrupoNumero == grupoEstudiante.GrupoNumero);

                if (entrega == null || string.IsNullOrEmpty(entrega.ArchivoRuta))
                {
                    return NotFound("Archivo no encontrado");
                }

                var rutaCompleta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", entrega.ArchivoRuta);
                
                if (!System.IO.File.Exists(rutaCompleta))
                {
                    return NotFound("El archivo no existe en el servidor");
                }

                var bytes = await System.IO.File.ReadAllBytesAsync(rutaCompleta);
                var extension = Path.GetExtension(rutaCompleta);
                var contentType = GetMimeType(extension);
                var nombreArchivo = Path.GetFileName(rutaCompleta);

                return File(bytes, contentType, nombreArchivo);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al descargar el archivo: {ex.Message}");
            }
        }

        // ========== FUNCIONES DE ANEXOS ==========

        // GET/POST: /Estudiante/FormularioAnexo/{entregaId}
        [HttpGet, HttpPost]
        public async Task<IActionResult> FormularioAnexo(int entregaId)
        {
            _logger.LogWarning("=== FormularioAnexo {Method} ejecutado - EntregaId: {EntregaId} ===", Request.Method, entregaId);
            // Verificar autenticación de estudiante
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuarioRol = HttpContext.Session.GetString("UsuarioRol");
            
            if (usuarioId == null || usuarioRol != "Estudiante")
            {
                return RedirectToAction("Login", "Auth");
            }

            var entrega = await _context.Entregas
                .Include(e => e.Grupo)
                .ThenInclude(g => g.GruposEstudiantes)
                .ThenInclude(ge => ge.Estudiante)
                .FirstOrDefaultAsync(e => e.Identificacion == entregaId);

            if (entrega == null)
            {
                return NotFound("Entrega no encontrada");
            }

            // Verificar que el estudiante actual pertenece al grupo de esta entrega
            var estudianteActual = await _context.Estudiantes
                .FirstOrDefaultAsync(e => e.Identificacion == usuarioId);
                
            if (estudianteActual == null)
            {
                return Forbid("No tienes permisos para acceder a esta entrega");
            }

            var perteneceAlGrupo = entrega.Grupo.GruposEstudiantes
                .Any(ge => ge.EstudianteIdentificacion == estudianteActual.Identificacion);
                
            if (!perteneceAlGrupo)
            {
                return Forbid("No tienes permisos para acceder a esta entrega");
            }

            // Cargar datos existentes si ya fueron llenados
            object? datosExistentes = null;
            if (!string.IsNullOrEmpty(entrega.DatosFormularioJson))
            {
                datosExistentes = JsonSerializer.Deserialize<object>(entrega.DatosFormularioJson);
            }

            ViewBag.Entrega = entrega;
            ViewBag.DatosExistentes = datosExistentes;
            ViewBag.Estudiantes = entrega.Grupo.GruposEstudiantes.Select(ge => ge.Estudiante).ToList();

            return entrega.TipoAnexo switch
            {
                1 => View("FormularioAnexo1"),
                2 => View("FormularioAnexo2"),
                3 => View("FormularioAnexo3"),
                5 => View("FormularioAnexo5"),
                6 => View("FormularioInformeFinalTutor"),
                7 => View("FormularioCartaInstitucion"),
                8 => View("FormularioCartaConsentimiento"),
                _ => BadRequest("Tipo de anexo no válido")
            };
        }

        // POST: /Estudiante/GuardarAnexo1
        [HttpPost]
        public async Task<IActionResult> GuardarAnexo1(Anexo1FormularioDto formData)
        {
            _logger.LogWarning("=== INICIO GuardarAnexo1 - ACCIÓN EJECUTÁNDOSE ===");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var entregaId = 0; // Declarar fuera del try para usar en catch
            
            try
            {
                _logger.LogInformation("POST recibido para GuardarAnexo1");
                _logger.LogInformation("FormData recibido: {@FormData}", formData);
                
                // Obtener entregaId desde el form data
                entregaId = formData?.EntregaId ?? 0;
                if (entregaId == 0)
                {
                    // Intentar obtener desde Request.Form como fallback
                    if (Request.Form.ContainsKey("entregaId") && int.TryParse(Request.Form["entregaId"], out var parsedId))
                    {
                        entregaId = parsedId;
                        if (formData != null)
                        {
                            formData.EntregaId = entregaId;
                        }
                    }
                }
                
                _logger.LogInformation("EntregaId: {EntregaId}", entregaId);
                _logger.LogInformation("NombreProyecto: '{NombreProyecto}'", formData?.NombreProyecto);
                
                if (entregaId == 0)
                {
                    _logger.LogWarning("EntregaId no válido: {EntregaId}", entregaId);
                    TempData["Error"] = "ID de entrega no válido";
                    return RedirectToAction("FormularioAnexo", new { entregaId });
                }
                
                // Verificar autenticación
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                var usuarioRol = HttpContext.Session.GetString("UsuarioRol");
                
                if (usuarioId == null || usuarioRol != "Estudiante")
                {
                    _logger.LogWarning("Usuario no autenticado o rol incorrecto");
                    return RedirectToAction("Login", "Auth");
                }
                
                _logger.LogInformation("Iniciando guardado de Anexo 1 para entrega {EntregaId} - Datos: {@FormData}", entregaId, formData);
                
                // Validación básica
                if (formData == null)
                {
                    _logger.LogWarning("FormData es null para entrega {EntregaId}", entregaId);
                    TempData["Error"] = "No se recibieron datos del formulario";
                    return RedirectToAction("FormularioAnexo", new { entregaId });
                }
                
                if (string.IsNullOrWhiteSpace(formData.NombreProyecto))
                {
                    _logger.LogWarning("Nombre del proyecto es requerido para entrega {EntregaId}. Valor recibido: '{NombreProyecto}'", entregaId, formData.NombreProyecto);
                    TempData["Error"] = "El nombre del proyecto es requerido";
                    return RedirectToAction("FormularioAnexo", new { entregaId });
                }
                
                var entrega = await _context.Entregas.FindAsync(entregaId);
                if (entrega == null)
                {
                    _logger.LogWarning("Entrega {EntregaId} no encontrada", entregaId);
                    return NotFound("Entrega no encontrada");
                }

                _logger.LogInformation("Entrega encontrada, serializando datos del formulario - Tiempo transcurrido: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                
                // Guardar datos del formulario en JSON
                var jsonData = JsonSerializer.Serialize(formData);
                entrega.DatosFormularioJson = jsonData;

                _logger.LogInformation("Generando PDF para Anexo 1 - Tiempo transcurrido: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                
                // Generar PDF rellenado con manejo de errores específico
                string rutaPdf;
                try
                {
                    _logger.LogInformation("Llamando al PdfFillerService.SaveFilledPdfAsync...");
                    rutaPdf = await _pdfFillerService.SaveFilledPdfAsync(entregaId, 1, formData);
                    _logger.LogInformation("PdfFillerService completado exitosamente. Ruta: {RutaPdf}", rutaPdf);
                }
                catch (Exception pdfEx)
                {
                    _logger.LogError(pdfEx, "Error específico en la generación de PDF para entrega {EntregaId}", entregaId);
                    throw new InvalidOperationException($"Error generando PDF: {pdfEx.Message}", pdfEx);
                }
                
                entrega.ArchivoRuta = rutaPdf;
                entrega.FechaEntrega = DateTime.Now;

                _logger.LogInformation("PDF generado en ruta: {RutaPdf} - Tiempo transcurrido: {ElapsedMs}ms", rutaPdf, stopwatch.ElapsedMilliseconds);

                await _context.SaveChangesAsync();

                stopwatch.Stop();
                _logger.LogInformation("Anexo 1 guardado exitosamente para entrega {EntregaId} - Tiempo total: {ElapsedMs}ms", entregaId, stopwatch.ElapsedMilliseconds);
                
                TempData["Success"] = "Anexo 1 guardado exitosamente";
                return RedirectToAction("DetalleEntrega", new { id = entregaId });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error guardando Anexo 1 para entrega {EntregaId} - Tiempo transcurrido: {ElapsedMs}ms", entregaId, stopwatch.ElapsedMilliseconds);
                TempData["Error"] = "Error al guardar el anexo: " + ex.Message;
                return RedirectToAction("FormularioAnexo", new { entregaId });
            }
        }

        // POST: /Estudiante/GuardarAnexo2
        [HttpPost]
        public async Task<IActionResult> GuardarAnexo2(Anexo2FormularioDto formData)
        {
            _logger.LogWarning("=== INICIO GuardarAnexo2 - ACCIÓN EJECUTÁNDOSE ===");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var entregaId = 0;
            
            try
            {
                _logger.LogInformation("POST recibido para GuardarAnexo2");
                _logger.LogInformation("FormData recibido: {@FormData}", formData);
                
                // Obtener entregaId desde el form data
                entregaId = formData?.EntregaId ?? 0;
                if (entregaId == 0)
                {
                    // Intentar obtener desde Request.Form como fallback
                    if (Request.Form.ContainsKey("entregaId") && int.TryParse(Request.Form["entregaId"], out var parsedId))
                    {
                        entregaId = parsedId;
                        if (formData != null)
                        {
                            formData.EntregaId = entregaId;
                        }
                    }
                }
                
                _logger.LogInformation("EntregaId: {EntregaId}", entregaId);
                _logger.LogInformation("NombreProyecto: '{NombreProyecto}'", formData?.NombreProyecto);
                
                if (entregaId == 0)
                {
                    _logger.LogWarning("EntregaId no válido: {EntregaId}", entregaId);
                    TempData["Error"] = "ID de entrega no válido";
                    return RedirectToAction("FormularioAnexo", new { entregaId });
                }
                
                // Verificar autenticación
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                var usuarioRol = HttpContext.Session.GetString("UsuarioRol");
                
                if (usuarioId == null || usuarioRol != "Estudiante")
                {
                    _logger.LogWarning("Usuario no autenticado o rol incorrecto");
                    return RedirectToAction("Login", "Auth");
                }
                
                _logger.LogInformation("Iniciando guardado de Anexo 2 para entrega {EntregaId} - Datos: {@FormData}", entregaId, formData);
                
                // Validación básica
                if (formData == null)
                {
                    _logger.LogWarning("FormData es null para entrega {EntregaId}", entregaId);
                    TempData["Error"] = "No se recibieron datos del formulario";
                    return RedirectToAction("FormularioAnexo", new { entregaId });
                }
                
                if (string.IsNullOrWhiteSpace(formData.NombreProyecto))
                {
                    _logger.LogWarning("Nombre del proyecto es requerido para entrega {EntregaId}. Valor recibido: '{NombreProyecto}'", entregaId, formData.NombreProyecto);
                    TempData["Error"] = "El nombre del proyecto es requerido";
                    return RedirectToAction("FormularioAnexo", new { entregaId });
                }
                
                var entrega = await _context.Entregas.FindAsync(entregaId);
                if (entrega == null)
                {
                    _logger.LogWarning("Entrega {EntregaId} no encontrada", entregaId);
                    return NotFound("Entrega no encontrada");
                }

                _logger.LogInformation("Entrega encontrada, serializando datos del formulario - Tiempo transcurrido: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                
                // Guardar datos del formulario en JSON
                var jsonData = JsonSerializer.Serialize(formData);
                entrega.DatosFormularioJson = jsonData;

                _logger.LogInformation("Generando PDF para Anexo 2 - Tiempo transcurrido: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                
                // Generar PDF rellenado con manejo de errores específico
                string rutaPdf;
                try
                {
                    _logger.LogInformation("Llamando al PdfFillerService.SaveFilledPdfAsync...");
                    rutaPdf = await _pdfFillerService.SaveFilledPdfAsync(entregaId, 2, formData);
                    _logger.LogInformation("PdfFillerService completado exitosamente. Ruta: {RutaPdf}", rutaPdf);
                }
                catch (Exception pdfEx)
                {
                    _logger.LogError(pdfEx, "Error específico en la generación de PDF para entrega {EntregaId}", entregaId);
                    throw new InvalidOperationException($"Error generando PDF: {pdfEx.Message}", pdfEx);
                }
                
                entrega.ArchivoRuta = rutaPdf;
                entrega.FechaEntrega = DateTime.Now;

                _logger.LogInformation("PDF generado en ruta: {RutaPdf} - Tiempo transcurrido: {ElapsedMs}ms", rutaPdf, stopwatch.ElapsedMilliseconds);

                await _context.SaveChangesAsync();

                stopwatch.Stop();
                _logger.LogInformation("Anexo 2 guardado exitosamente para entrega {EntregaId} - Tiempo total: {ElapsedMs}ms", entregaId, stopwatch.ElapsedMilliseconds);
                
                TempData["Success"] = "Anexo 2 guardado exitosamente";
                return RedirectToAction("DetalleEntrega", new { id = entregaId });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error guardando Anexo 2 para entrega {EntregaId} - Tiempo transcurrido: {ElapsedMs}ms", entregaId, stopwatch.ElapsedMilliseconds);
                
                // Escribir error detallado a un archivo para diagnóstico
                try
                {
                    var errorLogPath = Path.Combine("C:\\temp", "anexo2_error.log");
                    Directory.CreateDirectory("C:\\temp"); // Crear directorio si no existe
                    var errorDetails = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Error en GuardarAnexo2:\n" +
                                     $"EntregaId: {entregaId}\n" +
                                     $"Exception: {ex.GetType().Name}\n" +
                                     $"Message: {ex.Message}\n" +
                                     $"StackTrace: {ex.StackTrace}\n" +
                                     $"FormData: {System.Text.Json.JsonSerializer.Serialize(formData)}\n" +
                                     $"================================\n\n";
                    await System.IO.File.AppendAllTextAsync(errorLogPath, errorDetails);
                }
                catch { /* Ignore logging errors */ }
                
                TempData["Error"] = $"Error al guardar el anexo: {ex.Message}";
                return RedirectToAction("FormularioAnexo", new { entregaId });
            }
        }

        // POST: /Estudiante/GuardarAnexo3
        [HttpPost]
        public async Task<IActionResult> GuardarAnexo3(Anexo3FormularioDto formData)
        {
            _logger.LogWarning("=== INICIO GuardarAnexo3 - ACCIÓN EJECUTÁNDOSE ===");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var entregaId = 0;
            
            try
            {
                _logger.LogInformation("POST recibido para GuardarAnexo3");
                _logger.LogInformation("FormData recibido: {@FormData}", formData);
                
                // Obtener entregaId desde el form data
                entregaId = formData?.EntregaId ?? 0;
                if (entregaId == 0)
                {
                    // Intentar obtener desde Request.Form como fallback
                    if (Request.Form.ContainsKey("entregaId") && int.TryParse(Request.Form["entregaId"], out var parsedId))
                    {
                        entregaId = parsedId;
                        if (formData != null)
                        {
                            formData.EntregaId = entregaId;
                        }
                    }
                }
                
                _logger.LogInformation("EntregaId: {EntregaId}", entregaId);
                _logger.LogInformation("NombreDelProyecto: '{NombreDelProyecto}'", formData?.NombreDelProyecto);
                
                if (entregaId == 0)
                {
                    _logger.LogWarning("EntregaId no válido: {EntregaId}", entregaId);
                    TempData["Error"] = "ID de entrega no válido";
                    return RedirectToAction("FormularioAnexo", new { entregaId });
                }
                
                // Verificar autenticación
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                var usuarioRol = HttpContext.Session.GetString("UsuarioRol");
                
                if (usuarioId == null || usuarioRol != "Estudiante")
                {
                    _logger.LogWarning("Usuario no autenticado o rol incorrecto");
                    return RedirectToAction("Login", "Auth");
                }
                
                _logger.LogInformation("Iniciando guardado de Anexo 3 para entrega {EntregaId} - Datos: {@FormData}", entregaId, formData);
                
                // Validación básica
                if (formData == null)
                {
                    _logger.LogWarning("FormData es null para entrega {EntregaId}", entregaId);
                    TempData["Error"] = "No se recibieron datos del formulario";
                    return RedirectToAction("FormularioAnexo", new { entregaId });
                }
                
                var entrega = await _context.Entregas.FindAsync(entregaId);
                if (entrega == null)
                {
                    _logger.LogWarning("Entrega {EntregaId} no encontrada", entregaId);
                    return NotFound("Entrega no encontrada");
                }

                _logger.LogInformation("Entrega encontrada, serializando datos del formulario - Tiempo transcurrido: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                
                // Guardar datos del formulario en JSON
                var jsonData = JsonSerializer.Serialize(formData);
                entrega.DatosFormularioJson = jsonData;

                _logger.LogInformation("Generando PDF para Anexo 3 - Tiempo transcurrido: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                
                // Generar PDF rellenado con manejo de errores específico
                string rutaPdf;
                try
                {
                    _logger.LogInformation("Llamando al PdfFillerService.SaveFilledPdfAsync...");
                    rutaPdf = await _pdfFillerService.SaveFilledPdfAsync(entregaId, 3, formData);
                    _logger.LogInformation("PdfFillerService completado exitosamente. Ruta: {RutaPdf}", rutaPdf);
                }
                catch (Exception pdfEx)
                {
                    _logger.LogError(pdfEx, "Error específico en la generación de PDF para entrega {EntregaId}", entregaId);
                    throw new InvalidOperationException($"Error generando PDF: {pdfEx.Message}", pdfEx);
                }
                
                entrega.ArchivoRuta = rutaPdf;
                entrega.FechaEntrega = DateTime.Now;

                _logger.LogInformation("PDF generado en ruta: {RutaPdf} - Tiempo transcurrido: {ElapsedMs}ms", rutaPdf, stopwatch.ElapsedMilliseconds);

                await _context.SaveChangesAsync();

                stopwatch.Stop();
                _logger.LogInformation("Anexo 3 guardado exitosamente para entrega {EntregaId} - Tiempo total: {ElapsedMs}ms", entregaId, stopwatch.ElapsedMilliseconds);
                
                TempData["Success"] = "Anexo 3 guardado exitosamente";
                return RedirectToAction("DetalleEntrega", new { id = entregaId });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error guardando Anexo 3 para entrega {EntregaId} - Tiempo transcurrido: {ElapsedMs}ms", entregaId, stopwatch.ElapsedMilliseconds);
                TempData["Error"] = "Error al guardar el anexo: " + ex.Message;
                return RedirectToAction("FormularioAnexo", new { entregaId });
            }
        }

        // POST: /Estudiante/GuardarAnexo5
        [HttpPost]
        public async Task<IActionResult> GuardarAnexo5(Anexo5FormularioDto formData)
        {
            _logger.LogWarning("=== INICIO GuardarAnexo5 - ACCIÓN EJECUTÁNDOSE ===");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var entregaId = 0;
            
            try
            {
                _logger.LogInformation("POST recibido para GuardarAnexo5");
                _logger.LogInformation("FormData recibido: {@FormData}", formData);
                
                // Obtener entregaId desde el form data
                entregaId = formData?.EntregaId ?? 0;
                if (entregaId == 0)
                {
                    // Intentar obtener desde Request.Form como fallback
                    if (Request.Form.ContainsKey("entregaId") && int.TryParse(Request.Form["entregaId"], out var parsedId))
                    {
                        entregaId = parsedId;
                        if (formData != null)
                        {
                            formData.EntregaId = entregaId;
                        }
                    }
                }
                
                _logger.LogInformation("EntregaId: {EntregaId}", entregaId);
                _logger.LogInformation("NombreProyecto: '{NombreProyecto}'", formData?.NombreProyecto);
                
                if (entregaId == 0)
                {
                    _logger.LogWarning("EntregaId no válido: {EntregaId}", entregaId);
                    TempData["Error"] = "ID de entrega no válido";
                    return RedirectToAction("FormularioAnexo", new { entregaId });
                }
                
                // Verificar autenticación
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                var usuarioRol = HttpContext.Session.GetString("UsuarioRol");
                
                if (usuarioId == null || usuarioRol != "Estudiante")
                {
                    _logger.LogWarning("Usuario no autenticado o rol incorrecto");
                    return RedirectToAction("Login", "Auth");
                }
                
                _logger.LogInformation("Iniciando guardado de Anexo 5 para entrega {EntregaId} - Datos: {@FormData}", entregaId, formData);
                
                // Validación básica
                if (formData == null)
                {
                    _logger.LogWarning("FormData es null para entrega {EntregaId}", entregaId);
                    TempData["Error"] = "No se recibieron datos del formulario";
                    return RedirectToAction("FormularioAnexo", new { entregaId });
                }
                
                var entrega = await _context.Entregas.FindAsync(entregaId);
                if (entrega == null)
                {
                    _logger.LogWarning("Entrega {EntregaId} no encontrada", entregaId);
                    return NotFound("Entrega no encontrada");
                }

                _logger.LogInformation("Entrega encontrada, serializando datos del formulario - Tiempo transcurrido: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                
                // Guardar datos del formulario en JSON
                var jsonData = JsonSerializer.Serialize(formData);
                entrega.DatosFormularioJson = jsonData;

                _logger.LogInformation("Generando PDF para Anexo 5 - Tiempo transcurrido: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                
                // Generar PDF rellenado con manejo de errores específico
                string rutaPdf;
                try
                {
                    _logger.LogInformation("Llamando al PdfFillerService.SaveFilledPdfAsync...");
                    rutaPdf = await _pdfFillerService.SaveFilledPdfAsync(entregaId, 5, formData);
                    _logger.LogInformation("PdfFillerService completado exitosamente. Ruta: {RutaPdf}", rutaPdf);
                }
                catch (Exception pdfEx)
                {
                    _logger.LogError(pdfEx, "Error específico en la generación de PDF para entrega {EntregaId}", entregaId);
                    throw new InvalidOperationException($"Error generando PDF: {pdfEx.Message}", pdfEx);
                }
                
                entrega.ArchivoRuta = rutaPdf;
                entrega.FechaEntrega = DateTime.Now;

                _logger.LogInformation("PDF generado en ruta: {RutaPdf} - Tiempo transcurrido: {ElapsedMs}ms", rutaPdf, stopwatch.ElapsedMilliseconds);

                await _context.SaveChangesAsync();

                stopwatch.Stop();
                _logger.LogInformation("Anexo 5 guardado exitosamente para entrega {EntregaId} - Tiempo total: {ElapsedMs}ms", entregaId, stopwatch.ElapsedMilliseconds);
                
                TempData["Success"] = "Anexo 5 guardado exitosamente";
                return RedirectToAction("DetalleEntrega", new { id = entregaId });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error guardando Anexo 5 para entrega {EntregaId} - Tiempo transcurrido: {ElapsedMs}ms", entregaId, stopwatch.ElapsedMilliseconds);
                TempData["Error"] = "Error al guardar el anexo: " + ex.Message;
                return RedirectToAction("FormularioAnexo", new { entregaId });
            }
        }

        // POST: /Estudiante/GuardarInformeFinalTutor
        [HttpPost]
        public async Task<IActionResult> GuardarInformeFinalTutor(InformeFinalTutorFormularioDto formData)
        {
            _logger.LogWarning("=== INICIO GuardarInformeFinalTutor - ACCIÓN EJECUTÁNDOSE ===");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var entregaId = 0;
            
            try
            {
                _logger.LogInformation("POST recibido para GuardarInformeFinalTutor");
                _logger.LogInformation("FormData recibido: {@FormData}", formData);
                
                entregaId = formData?.EntregaId ?? 0;
                if (entregaId == 0 && Request.Form.ContainsKey("entregaId") && int.TryParse(Request.Form["entregaId"], out var parsedId))
                {
                    entregaId = parsedId;
                    if (formData != null) formData.EntregaId = entregaId;
                }
                
                var entrega = await _context.Entregas.FirstOrDefaultAsync(e => e.Identificacion == entregaId);
                if (entrega == null)
                {
                    TempData["Error"] = "Entrega no encontrada";
                    return RedirectToAction("Index");
                }
                
                // Serializar y guardar datos del formulario
                var datosJson = JsonSerializer.Serialize(formData);
                entrega.DatosFormularioJson = datosJson;
                entrega.ArchivoRuta = $"temp_informe_final_tutor_{entregaId}.pdf";
                
                _logger.LogInformation("Datos del formulario serializados correctamente");
                
                // Generar PDF usando el servicio PdfFillerService
                var pdfBytes = await _pdfFillerService.FillPdfAsync(6, datosJson);
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    throw new Exception("No se pudo generar el PDF");
                }
                
                // Guardar PDF en wwwroot/uploads
                var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsPath);
                
                var fileName = $"informe_final_tutor_{entregaId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = Path.Combine(uploadsPath, fileName);
                
                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);
                entrega.ArchivoRuta = $"uploads/{fileName}";
                
                await _context.SaveChangesAsync();
                stopwatch.Stop();
                
                _logger.LogInformation("Informe Final Tutor guardado exitosamente para entrega {EntregaId} - Tiempo total: {ElapsedMs}ms", entregaId, stopwatch.ElapsedMilliseconds);
                
                TempData["Success"] = "Informe Final Tutor guardado exitosamente";
                return RedirectToAction("DetalleEntrega", new { id = entregaId });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error guardando Informe Final Tutor para entrega {EntregaId} - Tiempo transcurrido: {ElapsedMs}ms", entregaId, stopwatch.ElapsedMilliseconds);
                TempData["Error"] = "Error al guardar el informe: " + ex.Message;
                return RedirectToAction("FormularioAnexo", new { entregaId });
            }
        }

        // POST: /Estudiante/GuardarCartaInstitucion
        [HttpPost]
        public async Task<IActionResult> GuardarCartaInstitucion(CartaInstitucionFormularioDto formData)
        {
            _logger.LogWarning("=== INICIO GuardarCartaInstitucion - ACCIÓN EJECUTÁNDOSE ===");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var entregaId = 0;
            
            try
            {
                _logger.LogInformation("POST recibido para GuardarCartaInstitucion");
                _logger.LogInformation("FormData recibido: {@FormData}", formData);
                
                entregaId = formData?.EntregaId ?? 0;
                if (entregaId == 0 && Request.Form.ContainsKey("entregaId") && int.TryParse(Request.Form["entregaId"], out var parsedId))
                {
                    entregaId = parsedId;
                    if (formData != null) formData.EntregaId = entregaId;
                }
                
                var entrega = await _context.Entregas.FirstOrDefaultAsync(e => e.Identificacion == entregaId);
                if (entrega == null)
                {
                    TempData["Error"] = "Entrega no encontrada";
                    return RedirectToAction("Index");
                }
                
                // Serializar y guardar datos del formulario
                var datosJson = JsonSerializer.Serialize(formData);
                entrega.DatosFormularioJson = datosJson;
                entrega.ArchivoRuta = $"temp_carta_institucion_{entregaId}.pdf";
                
                _logger.LogInformation("Datos del formulario serializados correctamente");
                
                // Generar PDF usando el servicio PdfFillerService
                var pdfBytes = await _pdfFillerService.FillPdfAsync(7, datosJson);
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    throw new Exception("No se pudo generar el PDF");
                }
                
                // Guardar PDF en wwwroot/uploads
                var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsPath);
                
                var fileName = $"carta_institucion_{entregaId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = Path.Combine(uploadsPath, fileName);
                
                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);
                entrega.ArchivoRuta = $"uploads/{fileName}";
                
                await _context.SaveChangesAsync();
                stopwatch.Stop();
                
                _logger.LogInformation("Carta Institución guardada exitosamente para entrega {EntregaId} - Tiempo total: {ElapsedMs}ms", entregaId, stopwatch.ElapsedMilliseconds);
                
                TempData["Success"] = "Carta para Institución guardada exitosamente";
                return RedirectToAction("DetalleEntrega", new { id = entregaId });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error guardando Carta Institución para entrega {EntregaId} - Tiempo transcurrido: {ElapsedMs}ms", entregaId, stopwatch.ElapsedMilliseconds);
                TempData["Error"] = "Error al guardar la carta: " + ex.Message;
                return RedirectToAction("FormularioAnexo", new { entregaId });
            }
        }

        // POST: /Estudiante/GuardarCartaConsentimiento
        [HttpPost]
        public async Task<IActionResult> GuardarCartaConsentimiento(CartaConsentimientoFormularioDto formData, IFormFile? imagenCedulaFrente, IFormFile? imagenCedulaAtras)
        {
            _logger.LogWarning("=== INICIO GuardarCartaConsentimiento - ACCIÓN EJECUTÁNDOSE ===");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var entregaId = 0;
            
            try
            {
                _logger.LogInformation("POST recibido para GuardarCartaConsentimiento");
                _logger.LogInformation("FormData recibido: {@FormData}", formData);
                
                entregaId = formData?.EntregaId ?? 0;
                if (entregaId == 0 && Request.Form.ContainsKey("entregaId") && int.TryParse(Request.Form["entregaId"], out var parsedId))
                {
                    entregaId = parsedId;
                    if (formData != null) formData.EntregaId = entregaId;
                }
                
                var entrega = await _context.Entregas.FirstOrDefaultAsync(e => e.Identificacion == entregaId);
                if (entrega == null)
                {
                    TempData["Error"] = "Entrega no encontrada";
                    return RedirectToAction("Index");
                }
                
                // Procesar imágenes de cédula si se enviaron
                if (imagenCedulaFrente != null && imagenCedulaFrente.Length > 0)
                {
                    var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cedulas");
                    Directory.CreateDirectory(uploadsPath);
                    
                    var fileName = $"cedula_frente_{entregaId}_{DateTime.Now:yyyyMMdd_HHmmss}.{Path.GetExtension(imagenCedulaFrente.FileName)}";
                    var filePath = Path.Combine(uploadsPath, fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagenCedulaFrente.CopyToAsync(stream);
                    }
                    if (formData != null)
                        formData.ImagenCedulaFrente = $"uploads/cedulas/{fileName}";
                }
                
                if (imagenCedulaAtras != null && imagenCedulaAtras.Length > 0)
                {
                    var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cedulas");
                    Directory.CreateDirectory(uploadsPath);
                    
                    var fileName = $"cedula_atras_{entregaId}_{DateTime.Now:yyyyMMdd_HHmmss}.{Path.GetExtension(imagenCedulaAtras.FileName)}";
                    var filePath = Path.Combine(uploadsPath, fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagenCedulaAtras.CopyToAsync(stream);
                    }
                    if (formData != null)
                        formData.ImagenCedulaAtras = $"uploads/cedulas/{fileName}";
                }
                
                // Serializar y guardar datos del formulario
                var datosJson = JsonSerializer.Serialize(formData);
                entrega.DatosFormularioJson = datosJson;
                entrega.ArchivoRuta = $"temp_carta_consentimiento_{entregaId}.pdf";
                
                _logger.LogInformation("Datos del formulario serializados correctamente");
                
                // Generar PDF usando el servicio PdfFillerService
                var pdfBytes = await _pdfFillerService.FillPdfAsync(8, datosJson);
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    throw new Exception("No se pudo generar el PDF");
                }
                
                // Guardar PDF en wwwroot/uploads
                var uploadsPath2 = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsPath2);
                
                var fileName2 = $"carta_consentimiento_{entregaId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath2 = Path.Combine(uploadsPath2, fileName2);
                
                await System.IO.File.WriteAllBytesAsync(filePath2, pdfBytes);
                entrega.ArchivoRuta = $"uploads/{fileName2}";
                
                await _context.SaveChangesAsync();
                stopwatch.Stop();
                
                _logger.LogInformation("Carta Consentimiento guardada exitosamente para entrega {EntregaId} - Tiempo total: {ElapsedMs}ms", entregaId, stopwatch.ElapsedMilliseconds);
                
                TempData["Success"] = "Carta de Consentimiento guardada exitosamente";
                return RedirectToAction("DetalleEntrega", new { id = entregaId });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error guardando Carta Consentimiento para entrega {EntregaId} - Tiempo transcurrido: {ElapsedMs}ms", entregaId, stopwatch.ElapsedMilliseconds);
                TempData["Error"] = "Error al guardar la carta: " + ex.Message;
                return RedirectToAction("FormularioAnexo", new { entregaId });
            }
        }

        // MÉTODO TEMPORAL PARA AGREGAR DATOS DE PRUEBA
        [HttpGet]
        public async Task<IActionResult> CrearDatosPrueba()
        {
            try
            {
                // Verificar si ya existen entregas de anexos
                var anexosExistentes = await _context.Entregas.Where(e => e.TipoAnexo > 0).CountAsync();
                if (anexosExistentes > 0)
                {
                    return Json(new { success = true, message = $"Ya existen {anexosExistentes} entregas de anexos" });
                }

                // Crear entregas de anexos para el grupo 1
                var nuevasEntregas = new List<Entrega>
                {
                    new Entrega
                    {
                        Identificacion = 2008,
                        Nombre = "Anexo #1 - Solicitud de Inscripción",
                        Descripcion = "Formulario de solicitud para inscribirse al proyecto de servicio comunal",
                        ArchivoRuta = "",
                        FechaLimite = DateTime.Now.AddDays(15),
                        Retroalimentacion = "Pendiente",
                        GrupoNumero = 1,
                        ProfesorIdentificacion = 987654321,
                        TipoAnexo = 1
                    },
                    new Entrega
                    {
                        Identificacion = 2009,
                        Nombre = "Anexo #2 - Áreas de Interés",
                        Descripcion = "Formulario para indicar áreas de interés del proyecto",
                        ArchivoRuta = "",
                        FechaLimite = DateTime.Now.AddDays(20),
                        Retroalimentacion = "Pendiente",
                        GrupoNumero = 1,
                        ProfesorIdentificacion = 987654321,
                        TipoAnexo = 2
                    },
                    new Entrega
                    {
                        Identificacion = 2010,
                        Nombre = "Anexo #3 - Plan de Trabajo",
                        Descripcion = "Plan detallado del proyecto a realizar",
                        ArchivoRuta = "",
                        FechaLimite = DateTime.Now.AddDays(25),
                        Retroalimentacion = "Pendiente",
                        GrupoNumero = 1,
                        ProfesorIdentificacion = 987654321,
                        TipoAnexo = 3
                    },
                    new Entrega
                    {
                        Identificacion = 2011,
                        Nombre = "Anexo #5 - Evaluación Final",
                        Descripcion = "Evaluación final del proyecto completado",
                        ArchivoRuta = "",
                        FechaLimite = DateTime.Now.AddDays(60),
                        Retroalimentacion = "Pendiente",
                        GrupoNumero = 1,
                        ProfesorIdentificacion = 987654321,
                        TipoAnexo = 5
                    }
                };

                _context.Entregas.AddRange(nuevasEntregas);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Datos de prueba creados exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando datos de prueba");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CambiarContraseña([FromBody] CambiarContraseñaEstudianteRequest request)
        {
            try
            {
                var usuarioActual = _usuarioService.ObtenerUsuarioActual();
                if (usuarioActual == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var resultado = await _usuarioService.CambiarContraseñaConValidacionAsync(
                    usuarioActual.NombreUsuario, 
                    request.ContraseñaActual, 
                    request.NuevaContraseña);

                return Json(new { success = resultado.exito, message = resultado.mensaje });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña del estudiante");
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CambiarLider([FromBody] CambiarLiderRequest request)
        {
            try
            {
                var usuarioActual = _usuarioService.ObtenerUsuarioActual();
                if (usuarioActual == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var estudiante = await _context.Estudiantes
                    .FirstOrDefaultAsync(e => e.Identificacion == usuarioActual.Identificacion);

                if (estudiante == null)
                {
                    return Json(new { success = false, message = "Estudiante no encontrado" });
                }

                // Verificar que el estudiante actual es líder del grupo
                var grupo = await _context.Grupos
                    .Include(g => g.Lider)
                    .FirstOrDefaultAsync(g => g.Numero == request.GrupoNumero);

                if (grupo == null)
                {
                    return Json(new { success = false, message = "Grupo no encontrado" });
                }

                if (grupo.LiderIdentificacion != estudiante.Identificacion)
                {
                    return Json(new { success = false, message = "Solo el líder actual puede cambiar el liderazgo" });
                }

                // Verificar que el nuevo líder es miembro del grupo
                var nuevoLider = await _context.GruposEstudiantes
                    .Include(ge => ge.Estudiante)
                    .FirstOrDefaultAsync(ge => ge.GrupoNumero == request.GrupoNumero && 
                                              ge.EstudianteIdentificacion == request.NuevoLiderIdentificacion);

                if (nuevoLider == null)
                {
                    return Json(new { success = false, message = "El nuevo líder debe ser miembro del grupo" });
                }

                // Cambiar el líder
                grupo.LiderIdentificacion = request.NuevoLiderIdentificacion;
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Liderazgo transferido exitosamente a {nuevoLider.Estudiante.Nombre} {nuevoLider.Estudiante.Apellidos}" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar líder del grupo");
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }
    }

    public class CambiarLiderRequest
    {
        public int GrupoNumero { get; set; }
        public int NuevoLiderIdentificacion { get; set; }
    }

    public class CambiarContraseñaEstudianteRequest
    {
        public string ContraseñaActual { get; set; } = string.Empty;
        public string NuevaContraseña { get; set; } = string.Empty;
    }

    public class ResponderSolicitudRequest
    {
        public int SolicitudId { get; set; }
        public bool Aceptar { get; set; }
    }

    public class EliminarSolicitudRequest
    {
        public int SolicitudId { get; set; }
    }

    public class CrearGrupoRequest
    {
        public List<int> EstudiantesSeleccionados { get; set; } = new List<int>();
    }

    public class InvitarEstudianteRequest
    {
        public int EstudianteId { get; set; }
    }
}
