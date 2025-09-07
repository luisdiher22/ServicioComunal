using Microsoft.EntityFrameworkCore;
using ServicioComunal.Data;
using ServicioComunal.Models;

namespace ServicioComunal.Services
{
    public class RecordatorioService
    {
        private readonly ServicioComunalDbContext _context;
        private readonly NotificacionService _notificacionService;

        public RecordatorioService(ServicioComunalDbContext context, NotificacionService notificacionService)
        {
            _context = context;
            _notificacionService = notificacionService;
        }

        /// <summary>
        /// Procesar recordatorios de entregas que vencen en 24 horas
        /// Este método se debe ejecutar diariamente
        /// </summary>
        public async Task ProcesarRecordatoriosAsync()
        {
            try
            {
                // Calcular el rango de fechas (23-25 horas para dar margen)
                var ahora = DateTime.Now;
                var fechaInicio = ahora.AddHours(23);
                var fechaFin = ahora.AddHours(25);

                // Obtener entregas que vencen en las próximas 24 horas
                var entregasProximasAVencer = await _context.Entregas
                    .Include(e => e.Grupo)
                    .ThenInclude(g => g.GruposEstudiantes)
                    .ThenInclude(ge => ge.Estudiante)
                    .Where(e => e.FechaLimite >= fechaInicio && e.FechaLimite <= fechaFin)
                    .Where(e => string.IsNullOrEmpty(e.ArchivoRuta)) // Solo entregas sin enviar
                    .ToListAsync();

                Console.WriteLine($"📅 Procesando recordatorios: {entregasProximasAVencer.Count} entregas próximas a vencer");

                foreach (var entrega in entregasProximasAVencer)
                {
                    if (entrega.Grupo?.GruposEstudiantes != null)
                    {
                        foreach (var grupoEstudiante in entrega.Grupo.GruposEstudiantes)
                        {
                            // Verificar si ya se envió un recordatorio reciente (en las últimas 2 horas)
                            var recordatorioReciente = await _context.Notificaciones
                                .Where(n => n.UsuarioDestino == grupoEstudiante.EstudianteIdentificacion)
                                .Where(n => n.EntregaId == entrega.Identificacion)
                                .Where(n => n.TipoNotificacion == TipoNotificacion.RecordatorioEntrega)
                                .Where(n => n.FechaHora >= ahora.AddHours(-2))
                                .AnyAsync();

                            if (!recordatorioReciente)
                            {
                                await _notificacionService.NotificarRecordatorioEntregaAsync(
                                    grupoEstudiante.EstudianteIdentificacion,
                                    entrega.Identificacion,
                                    entrega.Nombre
                                );

                                Console.WriteLine($"🔔 Recordatorio enviado a estudiante {grupoEstudiante.EstudianteIdentificacion} para entrega '{entrega.Nombre}'");
                            }
                        }
                    }
                }

                Console.WriteLine("✅ Procesamiento de recordatorios completado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error procesando recordatorios: {ex.Message}");
            }
        }
    }
}
