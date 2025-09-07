using Microsoft.EntityFrameworkCore;
using ServicioComunal.Data;
using ServicioComunal.Models;

namespace ServicioComunal.Services
{
    public class NotificacionService
    {
        private readonly ServicioComunalDbContext _context;

        public NotificacionService(ServicioComunalDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Crear una nueva notificación
        /// </summary>
        public async Task CrearNotificacionAsync(int usuarioDestino, string mensaje, string tipo, 
            int? entregaId = null, int? grupoId = null, int? usuarioOrigen = null)
        {
            try
            {
                var notificacion = new Notificacion
                {
                    UsuarioDestino = usuarioDestino,
                    Mensaje = mensaje,
                    TipoNotificacion = tipo,
                    FechaHora = DateTime.Now,
                    Leido = false,
                    EntregaId = entregaId,
                    GrupoId = grupoId,
                    UsuarioOrigen = usuarioOrigen
                };

                _context.Notificaciones.Add(notificacion);
                await _context.SaveChangesAsync();

                // Aplicar límite de 25 notificaciones por usuario
                await LimitarNotificacionesUsuarioAsync(usuarioDestino);

                Console.WriteLine($"📢 Notificación creada para usuario {usuarioDestino}: {mensaje}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creando notificación: {ex.Message}");
            }
        }

        /// <summary>
        /// Limitar las notificaciones de un usuario a 25, eliminando las más antiguas
        /// </summary>
        private async Task LimitarNotificacionesUsuarioAsync(int usuarioId)
        {
            try
            {
                const int LIMITE_NOTIFICACIONES = 25;

                var totalNotificaciones = await _context.Notificaciones
                    .CountAsync(n => n.UsuarioDestino == usuarioId);

                if (totalNotificaciones > LIMITE_NOTIFICACIONES)
                {
                    var notificacionesAEliminar = await _context.Notificaciones
                        .Where(n => n.UsuarioDestino == usuarioId)
                        .OrderBy(n => n.FechaHora) // Las más antiguas primero
                        .Take(totalNotificaciones - LIMITE_NOTIFICACIONES)
                        .ToListAsync();

                    _context.Notificaciones.RemoveRange(notificacionesAEliminar);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"🗑️ Eliminadas {notificacionesAEliminar.Count} notificaciones antiguas del usuario {usuarioId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error limitando notificaciones: {ex.Message}");
            }
        }

        /// <summary>
        /// Notificar nueva entrega a estudiantes de un grupo
        /// </summary>
        public async Task NotificarNuevaEntregaAsync(int grupoNumero, int entregaId, string nombreEntrega)
        {
            try
            {
                var estudiantes = await _context.GruposEstudiantes
                    .Where(ge => ge.GrupoNumero == grupoNumero)
                    .Include(ge => ge.Estudiante)
                    .Select(ge => ge.EstudianteIdentificacion)
                    .ToListAsync();

                var mensaje = $"Nueva entrega asignada: {nombreEntrega}";

                foreach (var estudianteId in estudiantes)
                {
                    await CrearNotificacionAsync(
                        estudianteId, 
                        mensaje, 
                        TipoNotificacion.NuevaEntrega, 
                        entregaId, 
                        grupoNumero
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error notificando nueva entrega: {ex.Message}");
            }
        }

        /// <summary>
        /// Notificar retroalimentación de entrega
        /// </summary>
        public async Task NotificarRetroalimentacionAsync(int estudianteId, int entregaId, string nombreEntrega, int tutorId)
        {
            try
            {
                var mensaje = $"Has recibido retroalimentación para la entrega: {nombreEntrega}";
                
                await CrearNotificacionAsync(
                    estudianteId, 
                    mensaje, 
                    TipoNotificacion.RetroalimentacionEntrega, 
                    entregaId, 
                    null, 
                    tutorId
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error notificando retroalimentación: {ex.Message}");
            }
        }

        /// <summary>
        /// Notificar entrega revisada
        /// </summary>
        public async Task NotificarEntregaRevisadaAsync(int estudianteId, int entregaId, string nombreEntrega, string estado)
        {
            try
            {
                var mensaje = string.Empty;
                if (estado.ToLower() == "requiere cambios")
                {
                    mensaje = $"Tu entrega '{nombreEntrega}' {estado.ToLower()}";
                } else
                {
                    mensaje = $"Tu entrega '{nombreEntrega}' ha sido {estado.ToLower()}";
                }

                await CrearNotificacionAsync(
                    estudianteId, 
                    mensaje, 
                    TipoNotificacion.EntregaRevisada, 
                    entregaId
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error notificando entrega revisada: {ex.Message}");
            }
        }

        /// <summary>
        /// Notificar solicitud de grupo aceptada
        /// </summary>
        public async Task NotificarSolicitudAceptadaAsync(int estudianteId, int grupoNumero)
        {
            try
            {
                var mensaje = $"Tu solicitud para unirte al grupo {grupoNumero} ha sido aceptada";
                
                await CrearNotificacionAsync(
                    estudianteId, 
                    mensaje, 
                    TipoNotificacion.SolicitudAceptada, 
                    null, 
                    grupoNumero
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error notificando solicitud aceptada: {ex.Message}");
            }
        }

        /// <summary>
        /// Notificar nueva solicitud de grupo al líder
        /// </summary>
        public async Task NotificarNuevaSolicitudGrupoAsync(int liderId, int solicitanteId, int grupoNumero)
        {
            try
            {
                var solicitante = await _context.Estudiantes
                    .FirstOrDefaultAsync(e => e.Identificacion == solicitanteId);

                var mensaje = $"{solicitante?.Nombre} {solicitante?.Apellidos} ha solicitado unirse a tu grupo";
                
                await CrearNotificacionAsync(
                    liderId, 
                    mensaje, 
                    TipoNotificacion.NuevaSolicitudGrupo, 
                    null, 
                    grupoNumero, 
                    solicitanteId
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error notificando nueva solicitud: {ex.Message}");
            }
        }

        /// <summary>
        /// Notificar recordatorio de entrega (24 horas antes)
        /// </summary>
        public async Task NotificarRecordatorioEntregaAsync(int estudianteId, int entregaId, string nombreEntrega)
        {
            try
            {
                var mensaje = $"Recordatorio: La entrega '{nombreEntrega}' vence en 24 horas";
                
                await CrearNotificacionAsync(
                    estudianteId, 
                    mensaje, 
                    TipoNotificacion.RecordatorioEntrega, 
                    entregaId
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error notificando recordatorio: {ex.Message}");
            }
        }

        /// <summary>
        /// Notificar grupo asignado a tutor
        /// </summary>
        public async Task NotificarGrupoAsignadoAsync(int tutorId, int grupoNumero)
        {
            try
            {
                var mensaje = $"Se te ha asignado el grupo {grupoNumero} para tutoría";
                
                await CrearNotificacionAsync(
                    tutorId, 
                    mensaje, 
                    TipoNotificacion.GrupoAsignado, 
                    null, 
                    grupoNumero
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error notificando grupo asignado: {ex.Message}");
            }
        }

        /// <summary>
        /// Notificar entregable recibido a tutor
        /// </summary>
        public async Task NotificarEntregableRecibidoAsync(int tutorId, int entregaId, string nombreEntrega, int grupoNumero)
        {
            try
            {
                var mensaje = $"El grupo {grupoNumero} ha enviado la entrega: {nombreEntrega}";
                
                await CrearNotificacionAsync(
                    tutorId, 
                    mensaje, 
                    TipoNotificacion.EntregableRecibido, 
                    entregaId, 
                    grupoNumero
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error notificando entregable recibido: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtener notificaciones no leídas de un usuario
        /// </summary>
        public async Task<List<Notificacion>> ObtenerNotificacionesNoLeidasAsync(int usuarioId)
        {
            try
            {
                return await _context.Notificaciones
                    .Where(n => n.UsuarioDestino == usuarioId && !n.Leido)
                    .OrderByDescending(n => n.FechaHora)
                    .Include(n => n.Entrega)
                    .Include(n => n.Grupo)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error obteniendo notificaciones: {ex.Message}");
                return new List<Notificacion>();
            }
        }

        /// <summary>
        /// Obtener todas las notificaciones de un usuario (con paginación)
        /// </summary>
        public async Task<List<Notificacion>> ObtenerNotificacionesUsuarioAsync(int usuarioId, int pagina = 1, int cantidad = 20)
        {
            try
            {
                return await _context.Notificaciones
                    .Where(n => n.UsuarioDestino == usuarioId)
                    .OrderByDescending(n => n.FechaHora)
                    .Skip((pagina - 1) * cantidad)
                    .Take(cantidad)
                    .Include(n => n.Entrega)
                    .Include(n => n.Grupo)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error obteniendo notificaciones del usuario: {ex.Message}");
                return new List<Notificacion>();
            }
        }

        /// <summary>
        /// Marcar notificación como leída
        /// </summary>
        public async Task MarcarComoLeidaAsync(int notificacionId)
        {
            try
            {
                var notificacion = await _context.Notificaciones
                    .FirstOrDefaultAsync(n => n.Identificacion == notificacionId);

                if (notificacion != null)
                {
                    notificacion.Leido = true;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error marcando notificación como leída: {ex.Message}");
            }
        }

        /// <summary>
        /// Marcar todas las notificaciones de un usuario como leídas
        /// </summary>
        public async Task MarcarTodasComoLeidasAsync(int usuarioId)
        {
            try
            {
                var notificaciones = await _context.Notificaciones
                    .Where(n => n.UsuarioDestino == usuarioId && !n.Leido)
                    .ToListAsync();

                foreach (var notificacion in notificaciones)
                {
                    notificacion.Leido = true;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error marcando todas las notificaciones como leídas: {ex.Message}");
            }
        }

        /// <summary>
        /// Contar notificaciones no leídas
        /// </summary>
        public async Task<int> ContarNotificacionesNoLeidasAsync(int usuarioId)
        {
            try
            {
                return await _context.Notificaciones
                    .CountAsync(n => n.UsuarioDestino == usuarioId && !n.Leido);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error contando notificaciones: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Procesar recordatorios de entregas (para ejecutar diariamente)
        /// </summary>
        public async Task ProcesarRecordatoriosEntregasAsync()
        {
            try
            {
                var fechaLimite = DateTime.Now.AddHours(24);
                var fechaInicio = DateTime.Now.AddHours(23);

                // Obtener entregas que vencen en 24 horas
                var entregas = await _context.Entregas
                    .Where(e => e.FechaLimite >= fechaInicio && e.FechaLimite <= fechaLimite)
                    .Include(e => e.Grupo)
                    .ThenInclude(g => g.GruposEstudiantes)
                    .ThenInclude(ge => ge.Estudiante)
                    .ToListAsync();

                foreach (var entrega in entregas)
                {
                    if (entrega.Grupo?.GruposEstudiantes != null)
                    {
                        foreach (var grupoEstudiante in entrega.Grupo.GruposEstudiantes)
                        {
                            // Solo notificar si no han entregado
                            if (string.IsNullOrEmpty(entrega.ArchivoRuta))
                            {
                                await NotificarRecordatorioEntregaAsync(
                                    grupoEstudiante.EstudianteIdentificacion,
                                    entrega.Identificacion,
                                    entrega.Nombre
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error procesando recordatorios: {ex.Message}");
            }
        }
    }
}
