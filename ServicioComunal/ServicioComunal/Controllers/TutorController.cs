using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioComunal.Data;
using ServicioComunal.Models;

namespace ServicioComunal.Controllers
{
    public class TutorController : Controller
    {
        private readonly ServicioComunalDbContext _context;

        public TutorController(ServicioComunalDbContext context)
        {
            _context = context;
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
                           && string.IsNullOrEmpty(e.Retroalimentacion))
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
        public async Task<IActionResult> Revisiones()
        {
            var tutorId = HttpContext.Session.GetInt32("UsuarioId");
            
            if (tutorId==null || !await EsTutor(tutorId.Value))
            {
                return RedirectToAction("Login", "Auth");
            }

            var gruposAsignados = await _context.GruposProfesores
                .Where(gp => gp.ProfesorIdentificacion == tutorId)
                .Select(gp => gp.GrupoNumero)
                .ToListAsync();

            var entregablesPendientes = await _context.Entregas
                .Where(e => gruposAsignados.Contains(e.GrupoNumero))
                .Include(e => e.Grupo)
                .ThenInclude(g => g.GruposEstudiantes)
                .ThenInclude(ge => ge.Estudiante)
                .OrderByDescending(e => e.FechaRetroalimentacion)
                .ToListAsync();

            return View(entregablesPendientes);
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
        public async Task<IActionResult> AprobarEntrega(int entregaId, string comentarios)
        {
            var entrega = await _context.Entregas.FindAsync(entregaId);
            if (entrega != null)
            {
                entrega.Retroalimentacion = comentarios ?? "Aprobado";
                entrega.FechaRetroalimentacion = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            
            return Json(new { success = true });
        }

        // API para solicitar cambios
        [HttpPost]
        public async Task<IActionResult> SolicitarCambios(int entregaId, string comentarios)
        {
            var entrega = await _context.Entregas.FindAsync(entregaId);
            if (entrega != null)
            {
                entrega.Retroalimentacion = $"CAMBIOS SOLICITADOS: {comentarios}";
                entrega.FechaRetroalimentacion = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            
            return Json(new { success = true });
        }

        // API para descargar archivo
        public async Task<IActionResult> DescargarArchivo(int entregaId)
        {
            var entrega = await _context.Entregas.FindAsync(entregaId);
            if (entrega == null || string.IsNullOrEmpty(entrega.ArchivoRuta))
            {
                return NotFound();
            }

            // Implementar descarga de archivo
            // Por ahora retornamos el nombre del archivo
            return Json(new { archivo = entrega.ArchivoRuta });
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
}
