using Microsoft.AspNetCore.Mvc;
using ServicioComunal.Services;
using ServicioComunal.Models;

namespace ServicioComunal.Controllers
{
    public class NotificacionController : Controller
    {
        private readonly NotificacionService _notificacionService;
        private readonly RecordatorioService _recordatorioService;

        public NotificacionController(NotificacionService notificacionService, RecordatorioService recordatorioService)
        {
            _notificacionService = notificacionService;
            _recordatorioService = recordatorioService;
        }

        /// <summary>
        /// Obtener notificaciones no leídas para la campana
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerNotificacionesNoLeidas()
        {
            try
            {
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (usuarioId == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var notificaciones = await _notificacionService.ObtenerNotificacionesNoLeidasAsync(usuarioId.Value);
                var cantidad = await _notificacionService.ContarNotificacionesNoLeidasAsync(usuarioId.Value);

                var notificacionesDto = notificaciones.Select(n => new
                {
                    id = n.Identificacion,
                    mensaje = n.Mensaje,
                    fechaHora = n.FechaHora.ToString("dd/MM/yyyy HH:mm"),
                    tipo = n.TipoNotificacion,
                    leido = n.Leido,
                    entregaId = n.EntregaId,
                    grupoId = n.GrupoId
                }).ToList();

                return Json(new
                {
                    success = true,
                    notificaciones = notificacionesDto,
                    cantidad = cantidad
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Obtener todas las notificaciones del usuario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerTodasLasNotificaciones(int pagina = 1)
        {
            try
            {
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (usuarioId == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var notificaciones = await _notificacionService.ObtenerNotificacionesUsuarioAsync(usuarioId.Value, pagina);

                var notificacionesDto = notificaciones.Select(n => new
                {
                    id = n.Identificacion,
                    mensaje = n.Mensaje,
                    fechaHora = n.FechaHora.ToString("dd/MM/yyyy HH:mm"),
                    tipo = n.TipoNotificacion,
                    leido = n.Leido,
                    entregaId = n.EntregaId,
                    grupoId = n.GrupoId
                }).ToList();

                return Json(new
                {
                    success = true,
                    notificaciones = notificacionesDto
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Marcar notificación como leída
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarcarComoLeida([FromBody] int notificacionId)
        {
            try
            {
                await _notificacionService.MarcarComoLeidaAsync(notificacionId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Marcar todas las notificaciones como leídas
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarcarTodasComoLeidas()
        {
            try
            {
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (usuarioId == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                await _notificacionService.MarcarTodasComoLeidasAsync(usuarioId.Value);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Obtener el conteo de notificaciones no leídas
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerConteoNoLeidas()
        {
            try
            {
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (usuarioId == null)
                {
                    return Json(new { success = false, cantidad = 0 });
                }

                var cantidad = await _notificacionService.ContarNotificacionesNoLeidasAsync(usuarioId.Value);
                return Json(new { success = true, cantidad = cantidad });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error obteniendo conteo de notificaciones: {ex.Message}");
                return Json(new { success = false, cantidad = 0 });
            }
        }

        /// <summary>
        /// Ejecutar recordatorios de entregas manualmente (solo para testing/admin)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> EjecutarRecordatorios()
        {
            try
            {
                await _recordatorioService.ProcesarRecordatoriosAsync();
                return Json(new { success = true, message = "Recordatorios procesados correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
