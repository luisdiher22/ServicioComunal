using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioComunal.Data;
using ServicioComunal.Models;
using ServicioComunal.Services;

namespace ServicioComunal.Controllers
{
    public class EstudianteController : Controller
    {
        private readonly ServicioComunalDbContext _context;
        private readonly UsuarioService _usuarioService;

        public EstudianteController(ServicioComunalDbContext context, UsuarioService usuarioService)
        {
            _context = context;
            _usuarioService = usuarioService;
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
                Console.WriteLine($"🔍 SOLICITAR INGRESO - Iniciando para grupo {grupoNumero}");

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

                Console.WriteLine($"🔍 SOLICITAR INGRESO - Usuario autenticado: {usuarioActual.NombreUsuario} (ID: {usuarioActual.Identificacion})");

                // Obtener el estudiante
                var estudiante = await _context.Estudiantes
                    .FirstOrDefaultAsync(e => e.Identificacion == usuarioActual.Identificacion);

                if (estudiante == null)
                {
                    return Json(new { success = false, message = "Estudiante no encontrado" });
                }

                Console.WriteLine($"🔍 SOLICITAR INGRESO - Estudiante encontrado: {estudiante.Nombre} {estudiante.Apellidos}");

                // Verificar que el estudiante no esté ya en un grupo
                var yaEnGrupo = await _context.GruposEstudiantes
                    .AnyAsync(ge => ge.EstudianteIdentificacion == estudiante.Identificacion);

                Console.WriteLine($"🔍 SOLICITAR INGRESO - ¿Ya en grupo?: {yaEnGrupo}");

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

                Console.WriteLine($"🔍 SOLICITAR INGRESO - Grupo {grupoNumero} encontrado con {grupo.GruposEstudiantes.Count} miembros");

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

                Console.WriteLine($"🔍 SOLICITAR INGRESO - ¿Solicitud existente?: {solicitudExistente}");

                if (solicitudExistente)
                {
                    Console.WriteLine("❌ SOLICITAR INGRESO - Ya existe una solicitud pendiente");
                    return Json(new { success = false, message = "Ya tienes una solicitud pendiente para este grupo" });
                }

                Console.WriteLine($"🔍 SOLICITAR INGRESO - Creando solicitud para el líder del grupo (ID: {grupo.LiderIdentificacion})");

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

                Console.WriteLine($"🔍 SOLICITAR INGRESO - Solicitud creada para el líder (ID: {grupo.LiderIdentificacion})");
                Console.WriteLine("🔍 SOLICITAR INGRESO - Guardando solicitud en la base de datos");
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ SOLICITAR INGRESO - Solicitud guardada exitosamente");

                return Json(new { success = true, message = "Solicitud enviada exitosamente" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SOLICITAR INGRESO - Error: {ex.Message}");
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

                Console.WriteLine($"🗑️ [Eliminar] Usuario: {usuario}, SolicitudId: {request.SolicitudId}");

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
                    Console.WriteLine($"❌ [Eliminar] Usuario {usuarioActual.Identificacion} no es el remitente {solicitud.EstudianteRemitenteId}");
                    return Json(new { success = false, message = "No tienes permiso para eliminar esta solicitud" });
                }

                // Solo permitir eliminar solicitudes pendientes
                if (solicitud.Estado != "PENDIENTE")
                {
                    Console.WriteLine($"❌ [Eliminar] Solicitud en estado: {solicitud.Estado}");
                    return Json(new { success = false, message = "Solo se pueden eliminar solicitudes pendientes" });
                }

                // Eliminar la solicitud
                _context.Solicitudes.Remove(solicitud);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ [Eliminar] Solicitud {request.SolicitudId} eliminada exitosamente");
                return Json(new { success = true, message = "Solicitud eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [Eliminar] Error: {ex.Message}");
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
        public async Task<IActionResult> TestSolicitarIngreso(int grupoNumero, string usuario)
        {
            try
            {
                Console.WriteLine($"🧪 [{DateTime.Now:HH:mm:ss.fff}] TEST SOLICITAR INGRESO - Grupo: {grupoNumero}, Usuario: {usuario}");

                // Establecer la sesión para simular el login
                HttpContext.Session.SetString("Usuario", usuario);
                Console.WriteLine($"🧪 [{DateTime.Now:HH:mm:ss.fff}] TEST - Sesión establecida para {usuario}");

                // Llamar al método real
                var resultado = await SolicitarIngreso(grupoNumero);
                var jsonResult = resultado as JsonResult;
                var value = jsonResult?.Value;

                Console.WriteLine($"🧪 [{DateTime.Now:HH:mm:ss.fff}] TEST RESULTADO: {System.Text.Json.JsonSerializer.Serialize(value)}");

                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ TEST ERROR: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
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
