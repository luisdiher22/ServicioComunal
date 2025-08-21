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
                .OrderBy(g => g.Numero)
                .ToListAsync();

            ViewBag.Grupos = grupos;

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

                // Verificar que el grupo existe y obtener sus miembros
                var grupo = await _context.Grupos
                    .Include(g => g.GruposEstudiantes)
                    .FirstOrDefaultAsync(g => g.Numero == grupoNumero);

                if (grupo == null)
                {
                    return Json(new { success = false, message = "Grupo no encontrado" });
                }

                Console.WriteLine($"🔍 SOLICITAR INGRESO - Grupo {grupoNumero} encontrado con {grupo.GruposEstudiantes.Count} miembros");

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

                Console.WriteLine($"🔍 SOLICITAR INGRESO - Creando solicitudes para los {grupo.GruposEstudiantes.Count} miembros del grupo");

                // Enviar solicitud a todos los miembros del grupo
                int solicitudesCreadas = 0;
                foreach (var miembro in grupo.GruposEstudiantes)
                {
                    var solicitud = new Solicitud
                    {
                        EstudianteRemitenteId = estudiante.Identificacion,
                        EstudianteDestinatarioId = miembro.EstudianteIdentificacion,
                        GrupoNumero = grupoNumero,
                        Tipo = "SOLICITUD_INGRESO",
                        Estado = "PENDIENTE",
                        Mensaje = $"{estudiante.Nombre} {estudiante.Apellidos} solicita unirse al grupo {grupoNumero}."
                    };

                    _context.Solicitudes.Add(solicitud);
                    solicitudesCreadas++;
                    Console.WriteLine($"🔍 SOLICITAR INGRESO - Solicitud creada para destinatario ID: {miembro.EstudianteIdentificacion}");
                }

                Console.WriteLine($"🔍 SOLICITAR INGRESO - Guardando {solicitudesCreadas} solicitudes en la base de datos");
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ SOLICITAR INGRESO - Solicitudes guardadas exitosamente");

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
}
