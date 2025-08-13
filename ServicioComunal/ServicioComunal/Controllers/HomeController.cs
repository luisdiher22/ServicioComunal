using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioComunal.Data;
using ServicioComunal.Services;
using ServicioComunal.Models;
using OfficeOpenXml;

namespace ServicioComunal.Controllers
{
    public class ActualizarAsignacionesRequest
    {
        public int GrupoNumero { get; set; }
        public List<int> EstudiantesIds { get; set; } = new List<int>();
    }

    public class AsignarGrupoRequest
    {
        public int TutorId { get; set; }
        public int GrupoNumero { get; set; }
    }

    public class HomeController : Controller
    {
        private readonly DataSeederService _seeder;
        private readonly ServicioComunalDbContext _context;

        public HomeController(DataSeederService seeder, ServicioComunalDbContext context)
        {
            _seeder = seeder;
            _context = context;
        }

        public IActionResult Dashboard()
        {
            // TODO: Implementar dashboard real
            ViewBag.Message = "¡Bienvenido al Dashboard de Servicio Comunal!";
            return View();
        }

        public async Task<IActionResult> Estudiantes()
        {
            try
            {
                var estudiantes = await _context.Estudiantes
                    .Include(e => e.GruposEstudiantes)
                    .OrderBy(e => e.Apellidos)
                    .ThenBy(e => e.Nombre)
                    .ToListAsync();
                
                return View(estudiantes);
            }
            catch (Exception)
            {
                // Log error
                ViewBag.Error = "Error al cargar la lista de estudiantes";
                return View(new List<ServicioComunal.Models.Estudiante>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerEstudiante(int id)
        {
            try
            {
                var estudiante = await _context.Estudiantes
                    .Include(e => e.GruposEstudiantes)
                    .FirstOrDefaultAsync(e => e.Identificacion == id);

                if (estudiante == null)
                {
                    return Json(new { success = false, message = "Estudiante no encontrado" });
                }

                return Json(new { 
                    success = true, 
                    estudiante = new {
                        identificacion = estudiante.Identificacion,
                        nombre = estudiante.Nombre,
                        apellidos = estudiante.Apellidos,
                        clase = estudiante.Clase,
                        tieneGrupo = estudiante.GruposEstudiantes.Any()
                    }
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al obtener el estudiante" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearEstudiante([FromBody] ServicioComunal.Models.Estudiante estudiante)
        {
            try
            {
                // Validar si ya existe un estudiante con esa identificación
                var existeEstudiante = await _context.Estudiantes
                    .AnyAsync(e => e.Identificacion == estudiante.Identificacion);

                if (existeEstudiante)
                {
                    return Json(new { success = false, message = "Ya existe un estudiante con esa cédula" });
                }

                // Validar datos
                if (string.IsNullOrWhiteSpace(estudiante.Nombre) || 
                    string.IsNullOrWhiteSpace(estudiante.Apellidos) ||
                    string.IsNullOrWhiteSpace(estudiante.Clase))
                {
                    return Json(new { success = false, message = "Todos los campos son requeridos" });
                }

                _context.Estudiantes.Add(estudiante);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Estudiante creado exitosamente" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al crear el estudiante" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> ActualizarEstudiante([FromBody] ServicioComunal.Models.Estudiante estudiante)
        {
            try
            {
                var estudianteExistente = await _context.Estudiantes
                    .FirstOrDefaultAsync(e => e.Identificacion == estudiante.Identificacion);

                if (estudianteExistente == null)
                {
                    return Json(new { success = false, message = "Estudiante no encontrado" });
                }

                // Actualizar campos
                estudianteExistente.Nombre = estudiante.Nombre;
                estudianteExistente.Apellidos = estudiante.Apellidos;
                estudianteExistente.Clase = estudiante.Clase;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Estudiante actualizado exitosamente" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al actualizar el estudiante" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> EliminarEstudiante(int id)
        {
            try
            {
                var estudiante = await _context.Estudiantes
                    .Include(e => e.GruposEstudiantes)
                    .FirstOrDefaultAsync(e => e.Identificacion == id);

                if (estudiante == null)
                {
                    return Json(new { success = false, message = "Estudiante no encontrado" });
                }

                // Verificar si el estudiante tiene grupos asignados
                if (estudiante.GruposEstudiantes.Any())
                {
                    return Json(new { success = false, message = "No se puede eliminar un estudiante que pertenece a un grupo" });
                }

                _context.Estudiantes.Remove(estudiante);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Estudiante eliminado exitosamente" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al eliminar el estudiante" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportarEstudiantesExcel()
        {
            try
            {
                // Configurar licencia de EPPlus (modo no comercial)
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var estudiantes = await _context.Estudiantes
                    .Include(e => e.GruposEstudiantes)
                        .ThenInclude(ge => ge.Grupo)
                    .OrderBy(e => e.Apellidos)
                    .ThenBy(e => e.Nombre)
                    .ToListAsync();

                using (var package = new ExcelPackage())
                {
                    // Hoja principal de datos
                    var worksheet = package.Workbook.Worksheets.Add("Estudiantes");

                    // Encabezados
                    worksheet.Cells[1, 1].Value = "Cédula";
                    worksheet.Cells[1, 2].Value = "Nombre";
                    worksheet.Cells[1, 3].Value = "Apellidos";
                    worksheet.Cells[1, 4].Value = "Clase";
                    worksheet.Cells[1, 5].Value = "Estado Grupo";
                    worksheet.Cells[1, 6].Value = "Número de Grupo";

                    // Estilo para encabezados
                    using (var range = worksheet.Cells[1, 1, 1, 6])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);
                    }

                    // Datos
                    int row = 2;
                    foreach (var estudiante in estudiantes)
                    {
                        worksheet.Cells[row, 1].Value = estudiante.Identificacion;
                        worksheet.Cells[row, 2].Value = estudiante.Nombre;
                        worksheet.Cells[row, 3].Value = estudiante.Apellidos;
                        worksheet.Cells[row, 4].Value = estudiante.Clase;

                        var tieneGrupo = estudiante.GruposEstudiantes.Any();
                        worksheet.Cells[row, 5].Value = tieneGrupo ? "Con Grupo" : "Sin Grupo";
                        worksheet.Cells[row, 6].Value = tieneGrupo ? estudiante.GruposEstudiantes.First().GrupoNumero.ToString() : "";

                        // Estilo condicional para el estado
                        if (tieneGrupo)
                        {
                            worksheet.Cells[row, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            worksheet.Cells[row, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                        }
                        else
                        {
                            worksheet.Cells[row, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            worksheet.Cells[row, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
                        }

                        row++;
                    }

                    // Ajustar ancho de columnas
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Agregar bordes a toda la tabla
                    using (var range = worksheet.Cells[1, 1, row - 1, 6])
                    {
                        range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    }

                    // Hoja de estadísticas
                    var statsWorksheet = package.Workbook.Worksheets.Add("Estadísticas");
                    
                    // Título
                    statsWorksheet.Cells[1, 1].Value = "Reporte de Estudiantes - Servicio Comunal";
                    statsWorksheet.Cells[1, 1, 1, 2].Merge = true;
                    statsWorksheet.Cells[1, 1].Style.Font.Bold = true;
                    statsWorksheet.Cells[1, 1].Style.Font.Size = 16;

                    // Fecha de generación
                    statsWorksheet.Cells[2, 1].Value = "Fecha de generación:";
                    statsWorksheet.Cells[2, 2].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                    // Estadísticas generales
                    statsWorksheet.Cells[4, 1].Value = "ESTADÍSTICAS GENERALES";
                    statsWorksheet.Cells[4, 1].Style.Font.Bold = true;
                    
                    var totalEstudiantes = estudiantes.Count;
                    var conGrupo = estudiantes.Count(e => e.GruposEstudiantes.Any());
                    var sinGrupo = totalEstudiantes - conGrupo;
                    
                    statsWorksheet.Cells[5, 1].Value = "Total de estudiantes:";
                    statsWorksheet.Cells[5, 2].Value = totalEstudiantes;
                    
                    statsWorksheet.Cells[6, 1].Value = "Estudiantes con grupo:";
                    statsWorksheet.Cells[6, 2].Value = conGrupo;
                    
                    statsWorksheet.Cells[7, 1].Value = "Estudiantes sin grupo:";
                    statsWorksheet.Cells[7, 2].Value = sinGrupo;

                    // Estadísticas por clase
                    statsWorksheet.Cells[9, 1].Value = "ESTADÍSTICAS POR CLASE";
                    statsWorksheet.Cells[9, 1].Style.Font.Bold = true;

                    var estadisticasPorClase = estudiantes
                        .GroupBy(e => e.Clase)
                        .Select(g => new {
                            Clase = g.Key,
                            Total = g.Count(),
                            ConGrupo = g.Count(e => e.GruposEstudiantes.Any()),
                            SinGrupo = g.Count(e => !e.GruposEstudiantes.Any())
                        })
                        .OrderBy(x => x.Clase)
                        .ToList();

                    int statsRow = 10;
                    foreach (var stat in estadisticasPorClase)
                    {
                        statsWorksheet.Cells[statsRow, 1].Value = $"Clase {stat.Clase}:";
                        statsWorksheet.Cells[statsRow, 2].Value = $"Total: {stat.Total}, Con grupo: {stat.ConGrupo}, Sin grupo: {stat.SinGrupo}";
                        statsRow++;
                    }

                    // Ajustar columnas de la hoja de estadísticas
                    statsWorksheet.Cells[statsWorksheet.Dimension.Address].AutoFitColumns();

                    // Generar el archivo
                    var fileName = $"Estudiantes_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    var fileBytes = package.GetAsByteArray();

                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                // En caso de error, redirigir con mensaje
                TempData["Error"] = $"Error al exportar: {ex.Message}";
                return RedirectToAction("Estudiantes");
            }
        }

        public async Task<IActionResult> GestionGrupos()
        {
            try
            {
                var grupos = await _context.Grupos
                    .Include(g => g.GruposEstudiantes)
                        .ThenInclude(ge => ge.Estudiante)
                    .Include(g => g.GruposProfesores)
                        .ThenInclude(gp => gp.Profesor)
                    .OrderBy(g => g.Numero)
                    .ToListAsync();
                
                return View(grupos);
            }
            catch (Exception)
            {
                ViewBag.Error = "Error al cargar la lista de grupos";
                return View(new List<ServicioComunal.Models.Grupo>());
            }
        }

        public async Task<IActionResult> AsignarTutores()
        {
            try
            {
                var tutores = await _context.Profesores
                    .Include(p => p.GruposProfesores)
                        .ThenInclude(gp => gp.Grupo)
                    .OrderBy(p => p.Apellidos)
                    .ThenBy(p => p.Nombre)
                    .ToListAsync();
                
                return View(tutores);
            }
            catch (Exception)
            {
                ViewBag.Error = "Error al cargar la lista de tutores";
                return View(new List<ServicioComunal.Models.Profesor>());
            }
        }

        // ===== ACCIONES PARA GRUPOS =====

        [HttpPost]
        public async Task<IActionResult> CrearGrupo([FromBody] ServicioComunal.Models.Grupo grupo)
        {
            try
            {
                var existeGrupo = await _context.Grupos
                    .AnyAsync(g => g.Numero == grupo.Numero);

                if (existeGrupo)
                {
                    return Json(new { success = false, message = "Ya existe un grupo con ese número" });
                }

                if (grupo.Numero <= 0)
                {
                    return Json(new { success = false, message = "El número de grupo debe ser mayor a 0" });
                }

                _context.Grupos.Add(grupo);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Grupo creado exitosamente" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al crear el grupo" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerGrupo(int id)
        {
            try
            {
                var grupo = await _context.Grupos
                    .Include(g => g.GruposEstudiantes)
                        .ThenInclude(ge => ge.Estudiante)
                    .Include(g => g.GruposProfesores)
                        .ThenInclude(gp => gp.Profesor)
                    .FirstOrDefaultAsync(g => g.Numero == id);

                if (grupo == null)
                {
                    return Json(new { success = false, message = "Grupo no encontrado" });
                }

                var tutor = grupo.GruposProfesores.FirstOrDefault()?.Profesor;

                return Json(new { 
                    success = true, 
                    grupo = new {
                        numero = grupo.Numero,
                        estudiantes = grupo.GruposEstudiantes.Select(ge => new {
                            identificacion = ge.Estudiante.Identificacion,
                            nombre = ge.Estudiante.Nombre,
                            apellidos = ge.Estudiante.Apellidos,
                            clase = ge.Estudiante.Clase
                        }).ToList(),
                        tutor = tutor != null ? new {
                            identificacion = tutor.Identificacion,
                            nombre = tutor.Nombre,
                            apellidos = tutor.Apellidos,
                            rol = tutor.Rol
                        } : null
                    }
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al obtener el grupo" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> EliminarGrupo(int id)
        {
            try
            {
                var grupo = await _context.Grupos
                    .Include(g => g.GruposEstudiantes)
                    .FirstOrDefaultAsync(g => g.Numero == id);

                if (grupo == null)
                {
                    return Json(new { success = false, message = "Grupo no encontrado" });
                }

                // Eliminar relaciones primero
                _context.GruposEstudiantes.RemoveRange(grupo.GruposEstudiantes);
                _context.Grupos.Remove(grupo);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Grupo eliminado exitosamente" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al eliminar el grupo" });
            }
        }

        // ===== ACCIONES PARA TUTORES =====

        [HttpPost]
        public async Task<IActionResult> CrearTutor([FromBody] ServicioComunal.Models.Profesor tutor)
        {
            try
            {
                var existeTutor = await _context.Profesores
                    .AnyAsync(p => p.Identificacion == tutor.Identificacion);

                if (existeTutor)
                {
                    return Json(new { success = false, message = "Ya existe un tutor con esa cédula" });
                }

                if (string.IsNullOrWhiteSpace(tutor.Nombre) || 
                    string.IsNullOrWhiteSpace(tutor.Apellidos) ||
                    string.IsNullOrWhiteSpace(tutor.Rol))
                {
                    return Json(new { success = false, message = "Todos los campos son requeridos" });
                }

                _context.Profesores.Add(tutor);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Tutor creado exitosamente" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al crear el tutor" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTutor(int id)
        {
            try
            {
                var tutor = await _context.Profesores
                    .Include(p => p.GruposProfesores)
                    .FirstOrDefaultAsync(p => p.Identificacion == id);

                if (tutor == null)
                {
                    return Json(new { success = false, message = "Tutor no encontrado" });
                }

                return Json(new { 
                    success = true, 
                    tutor = new {
                        identificacion = tutor.Identificacion,
                        nombre = tutor.Nombre,
                        apellidos = tutor.Apellidos,
                        rol = tutor.Rol,
                        gruposAsignados = tutor.GruposProfesores.Count()
                    }
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al obtener el tutor" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> ActualizarTutor([FromBody] ServicioComunal.Models.Profesor tutor)
        {
            try
            {
                var tutorExistente = await _context.Profesores
                    .FirstOrDefaultAsync(p => p.Identificacion == tutor.Identificacion);

                if (tutorExistente == null)
                {
                    return Json(new { success = false, message = "Tutor no encontrado" });
                }

                tutorExistente.Nombre = tutor.Nombre;
                tutorExistente.Apellidos = tutor.Apellidos;
                tutorExistente.Rol = tutor.Rol;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Tutor actualizado exitosamente" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al actualizar el tutor" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> EliminarTutor(int id)
        {
            try
            {
                var tutor = await _context.Profesores
                    .Include(p => p.GruposProfesores)
                    .FirstOrDefaultAsync(p => p.Identificacion == id);

                if (tutor == null)
                {
                    return Json(new { success = false, message = "Tutor no encontrado" });
                }

                // Quitar asignaciones antes de eliminar
                _context.GruposProfesores.RemoveRange(tutor.GruposProfesores);
                _context.Profesores.Remove(tutor);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Tutor eliminado exitosamente" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al eliminar el tutor" });
            }
        }

        public async Task<IActionResult> Configuracion()
        {
            try
            {
                // Cargar datos para la configuración
                var formularios = await _context.Formularios
                    .Include(f => f.Profesor)
                    .OrderByDescending(f => f.FechaIngreso)
                    .ToListAsync();

                var entregas = await _context.Entregas
                    .Include(e => e.Grupo)
                    .Include(e => e.Profesor)
                    .OrderByDescending(e => e.FechaLimite)
                    .ToListAsync();

                var profesores = await _context.Profesores
                    .OrderBy(p => p.Apellidos)
                    .ThenBy(p => p.Nombre)
                    .ToListAsync();

                var grupos = await _context.Grupos
                    .Include(g => g.GruposEstudiantes)
                    .OrderBy(g => g.Numero)
                    .ToListAsync();

                // Configuración actual (valores por defecto o de base de datos)
                ViewBag.MaxIntegrantesPorGrupo = 5; // Valor por defecto, puedes cambiarlo según tus necesidades
                ViewBag.Formularios = formularios;
                ViewBag.Entregas = entregas;
                ViewBag.Profesores = profesores;
                ViewBag.Grupos = grupos;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al cargar la configuración: {ex.Message}";
                return View();
            }
        }

        public async Task<IActionResult> SeedData()
        {
            try
            {
                await _seeder.SeedDataAsync();
                ViewBag.Message = "Datos de prueba insertados exitosamente!";
                ViewBag.Success = true;
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error al insertar datos: {ex.Message}";
                ViewBag.Success = false;
            }
            return View("Dashboard");
        }

        public async Task<IActionResult> ReseedUsuarios()
        {
            try
            {
                await _seeder.ReseedUsuariosAsync();
                ViewBag.Message = "Usuarios recreados exitosamente!";
                ViewBag.Success = true;
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error al recrear usuarios: {ex.Message}";
                ViewBag.Success = false;
            }
            return View("Dashboard");
        }

        public async Task<IActionResult> SeedUsuariosSafe()
        {
            try
            {
                await _seeder.SeedUsuariosSafeAsync();
                ViewBag.Message = "Usuarios creados de forma segura exitosamente!";
                ViewBag.Success = true;
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error al crear usuarios seguros: {ex.Message}";
                ViewBag.Success = false;
            }
            return View("Dashboard");
        }

        public async Task<IActionResult> VerificarUsuarios()
        {
            try
            {
                var usuarios = await _context.Usuarios.ToListAsync();
                var mensaje = $"Usuarios en BD: {usuarios.Count}\n\n";
                
                foreach (var user in usuarios)
                {
                    mensaje += $"- ID: {user.Identificacion}, Usuario: {user.NombreUsuario}, Rol: {user.Rol}, Activo: {user.Activo}\n";
                }
                
                ViewBag.Message = mensaje;
                ViewBag.Success = true;
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error verificando usuarios: {ex.Message}";
                ViewBag.Success = false;
            }
            return View("Dashboard");
        }

        // ===== ACCIONES ADICIONALES =====

        [HttpGet]
        public async Task<IActionResult> ObtenerEstudiantesSinGrupo()
        {
            try
            {
                var estudiantesSinGrupo = await _context.Estudiantes
                    .Where(e => !_context.GruposEstudiantes.Any(ge => ge.EstudianteIdentificacion == e.Identificacion))
                    .OrderBy(e => e.Apellidos)
                    .ThenBy(e => e.Nombre)
                    .Select(e => new {
                        identificacion = e.Identificacion,
                        nombre = e.Nombre,
                        apellidos = e.Apellidos,
                        clase = e.Clase
                    })
                    .ToListAsync();

                return Json(new { success = true, estudiantes = estudiantesSinGrupo });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al cargar estudiantes" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AsignarEstudianteAGrupo([FromBody] ServicioComunal.Models.Estudiante data)
        {
            try
            {
                var estudianteId = data.Identificacion;
                var grupoNumero = Convert.ToInt32(data.Clase); // Reutilizamos el campo clase para pasar el grupo

                var estudiante = await _context.Estudiantes
                    .FirstOrDefaultAsync(e => e.Identificacion == estudianteId);
                var grupo = await _context.Grupos
                    .FirstOrDefaultAsync(g => g.Numero == grupoNumero);

                if (estudiante == null || grupo == null)
                {
                    return Json(new { success = false, message = "Estudiante o grupo no encontrado" });
                }

                var yaAsignado = await _context.GruposEstudiantes
                    .AnyAsync(ge => ge.EstudianteIdentificacion == estudianteId);

                if (yaAsignado)
                {
                    return Json(new { success = false, message = "El estudiante ya está asignado a un grupo" });
                }

                var grupoEstudiante = new GrupoEstudiante
                {
                    EstudianteIdentificacion = estudianteId,
                    GrupoNumero = grupoNumero
                };

                _context.GruposEstudiantes.Add(grupoEstudiante);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Estudiante asignado exitosamente" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al asignar estudiante" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> RemoverEstudianteDeGrupo(int estudianteId, int grupoNumero)
        {
            try
            {
                var grupoEstudiante = await _context.GruposEstudiantes
                    .FirstOrDefaultAsync(ge => ge.EstudianteIdentificacion == estudianteId && 
                                             ge.GrupoNumero == grupoNumero);

                if (grupoEstudiante == null)
                {
                    return Json(new { success = false, message = "Asignación no encontrada" });
                }

                _context.GruposEstudiantes.Remove(grupoEstudiante);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Estudiante removido del grupo" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al remover estudiante" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AsignarTutorAGrupo([FromBody] ServicioComunal.Models.Profesor data)
        {
            try
            {
                var tutorId = data.Identificacion;
                var grupoNumero = Convert.ToInt32(data.Rol); // Reutilizamos el campo rol para pasar el grupo

                var tutor = await _context.Profesores
                    .FirstOrDefaultAsync(p => p.Identificacion == tutorId);
                var grupo = await _context.Grupos
                    .Include(g => g.GruposEstudiantes)
                    .Include(g => g.GruposProfesores)
                    .FirstOrDefaultAsync(g => g.Numero == grupoNumero);

                if (tutor == null || grupo == null)
                {
                    return Json(new { success = false, message = "Tutor o grupo no encontrado" });
                }

                if (!grupo.GruposEstudiantes.Any())
                {
                    return Json(new { success = false, message = "No se puede asignar tutor a un grupo sin estudiantes" });
                }

                // Verificar si ya existe una asignación
                var asignacionExistente = await _context.GruposProfesores
                    .FirstOrDefaultAsync(gp => gp.GrupoNumero == grupoNumero);

                if (asignacionExistente != null)
                {
                    // Actualizar asignación existente
                    asignacionExistente.ProfesorIdentificacion = tutorId;
                    asignacionExistente.FechaAsignacion = DateTime.Now;
                }
                else
                {
                    // Crear nueva asignación
                    var nuevaAsignacion = new GrupoProfesor
                    {
                        GrupoNumero = grupoNumero,
                        ProfesorIdentificacion = tutorId,
                        FechaAsignacion = DateTime.Now
                    };
                    _context.GruposProfesores.Add(nuevaAsignacion);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Tutor asignado exitosamente" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al asignar tutor" });
            }
        }



        [HttpPost]
        public async Task<IActionResult> AsignarGrupoATutor([FromBody] AsignarGrupoRequest request)
        {
            try
            {
                var tutor = await _context.Profesores
                    .FirstOrDefaultAsync(p => p.Identificacion == request.TutorId);
                var grupo = await _context.Grupos
                    .Include(g => g.GruposEstudiantes)
                    .FirstOrDefaultAsync(g => g.Numero == request.GrupoNumero);

                if (tutor == null || grupo == null)
                {
                    return Json(new { success = false, message = "Tutor o grupo no encontrado" });
                }

                if (!grupo.GruposEstudiantes.Any())
                {
                    return Json(new { success = false, message = "No se puede asignar tutor a un grupo sin estudiantes" });
                }

                // Verificar si ya existe la asignación
                var asignacionExistente = await _context.GruposProfesores
                    .FirstOrDefaultAsync(gp => gp.GrupoNumero == request.GrupoNumero);

                if (asignacionExistente != null)
                {
                    return Json(new { success = false, message = "Este grupo ya tiene un tutor asignado" });
                }

                // Crear nueva asignación
                var nuevaAsignacion = new GrupoProfesor
                {
                    GrupoNumero = request.GrupoNumero,
                    ProfesorIdentificacion = request.TutorId,
                    FechaAsignacion = DateTime.Now
                };

                _context.GruposProfesores.Add(nuevaAsignacion);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Grupo asignado al tutor exitosamente" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al asignar grupo al tutor" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> QuitarGrupoDeTutor([FromBody] AsignarGrupoRequest request)
        {
            try
            {
                var asignacion = await _context.GruposProfesores
                    .FirstOrDefaultAsync(gp => gp.GrupoNumero == request.GrupoNumero && 
                                             gp.ProfesorIdentificacion == request.TutorId);

                if (asignacion == null)
                {
                    return Json(new { success = false, message = "Asignación no encontrada" });
                }

                _context.GruposProfesores.Remove(asignacion);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Grupo quitado del tutor exitosamente" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al quitar grupo del tutor" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerGruposDisponibles()
        {
            try
            {
                var gruposDisponibles = await _context.Grupos
                    .Include(g => g.GruposEstudiantes)
                    .Include(g => g.GruposProfesores)
                    .Where(g => g.GruposEstudiantes.Any() && !g.GruposProfesores.Any())
                    .Select(g => new {
                        numero = g.Numero,
                        estudiantes = g.GruposEstudiantes.Count()
                    })
                    .OrderBy(g => g.numero)
                    .ToListAsync();

                return Json(new { success = true, grupos = gruposDisponibles });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al cargar grupos disponibles" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerGruposAsignadosATutor(int tutorId)
        {
            try
            {
                var gruposAsignados = await _context.GruposProfesores
                    .Include(gp => gp.Grupo)
                        .ThenInclude(g => g.GruposEstudiantes)
                    .Where(gp => gp.ProfesorIdentificacion == tutorId)
                    .Select(gp => new {
                        numero = gp.Grupo.Numero,
                        estudiantes = gp.Grupo.GruposEstudiantes.Count(),
                        fechaAsignacion = gp.FechaAsignacion
                    })
                    .OrderBy(g => g.numero)
                    .ToListAsync();

                return Json(new { success = true, grupos = gruposAsignados });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al cargar grupos asignados" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AsignacionAutomaticaTutores()
        {
            try
            {
                // Obtener grupos que tienen estudiantes pero no tienen tutor asignado
                var gruposSinTutor = await _context.Grupos
                    .Include(g => g.GruposEstudiantes)
                    .Include(g => g.GruposProfesores)
                    .Where(g => g.GruposEstudiantes.Any() && 
                               !g.GruposProfesores.Any())
                    .ToListAsync();

                var tutoresDisponibles = await _context.Profesores
                    .Include(p => p.GruposProfesores)
                    .OrderBy(p => p.GruposProfesores.Count())
                    .ToListAsync();

                if (!tutoresDisponibles.Any())
                {
                    return Json(new { success = false, message = "No hay tutores disponibles" });
                }

                int tutorIndex = 0;
                int asignaciones = 0;

                foreach (var grupo in gruposSinTutor)
                {
                    var tutor = tutoresDisponibles[tutorIndex % tutoresDisponibles.Count];
                    
                    var nuevaAsignacion = new GrupoProfesor
                    {
                        GrupoNumero = grupo.Numero,
                        ProfesorIdentificacion = tutor.Identificacion,
                        FechaAsignacion = DateTime.Now
                    };
                    
                    _context.GruposProfesores.Add(nuevaAsignacion);
                    asignaciones++;
                    tutorIndex++;
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Se asignaron tutores a {asignaciones} grupos automáticamente"
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error en la asignación automática" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerGruposSinTutor()
        {
            try
            {
                var gruposSinTutor = await _context.Grupos
                    .Include(g => g.GruposEstudiantes)
                    .Include(g => g.GruposProfesores)
                    .Where(g => g.GruposEstudiantes.Any() && 
                               !g.GruposProfesores.Any())
                    .Select(g => new {
                        numero = g.Numero,
                        cantidadEstudiantes = g.GruposEstudiantes.Count()
                    })
                    .ToListAsync();

                return Json(new { success = true, grupos = gruposSinTutor });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al cargar grupos sin tutor" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTutoresDisponibles()
        {
            try
            {
                var tutores = await _context.Profesores
                    .Include(p => p.GruposProfesores)
                    .Select(p => new {
                        identificacion = p.Identificacion,
                        nombre = p.Nombre,
                        apellidos = p.Apellidos,
                        rol = p.Rol,
                        gruposAsignados = p.GruposProfesores.Count()
                    })
                    .OrderBy(p => p.gruposAsignados)
                    .ThenBy(p => p.apellidos)
                    .ToListAsync();

                return Json(new { success = true, tutores = tutores });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error al cargar tutores" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarAsignacionesGrupo([FromBody] ActualizarAsignacionesRequest request)
        {
            try
            {
                // Verificar que el grupo existe
                var grupo = await _context.Grupos
                    .Include(g => g.GruposEstudiantes)
                    .FirstOrDefaultAsync(g => g.Numero == request.GrupoNumero);

                if (grupo == null)
                {
                    return Json(new { success = false, message = "Grupo no encontrado" });
                }

                // Remover todas las asignaciones existentes del grupo
                var asignacionesExistentes = grupo.GruposEstudiantes.ToList();
                _context.GruposEstudiantes.RemoveRange(asignacionesExistentes);

                // Crear nuevas asignaciones
                foreach (var estudianteId in request.EstudiantesIds)
                {
                    var estudiante = await _context.Estudiantes
                        .FirstOrDefaultAsync(e => e.Identificacion == estudianteId);

                    if (estudiante != null)
                    {
                        var nuevaAsignacion = new GrupoEstudiante
                        {
                            EstudianteIdentificacion = estudianteId,
                            GrupoNumero = request.GrupoNumero
                        };
                        _context.GruposEstudiantes.Add(nuevaAsignacion);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Se actualizaron las asignaciones del grupo {request.GrupoNumero} correctamente"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar asignaciones: " + ex.Message });
            }
        }

        public IActionResult Error()
        {
            return View();
        }

        // ===== ACCIONES PARA CONFIGURACIÓN =====

        [HttpPost]
        public async Task<IActionResult> CrearFormulario([FromBody] Formulario formulario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(formulario.Nombre) || 
                    string.IsNullOrWhiteSpace(formulario.Descripcion))
                {
                    return Json(new { success = false, message = "Nombre y descripción son requeridos" });
                }

                formulario.FechaIngreso = DateTime.Now;
                if (formulario.ArchivoRuta == null)
                    formulario.ArchivoRuta = "";

                _context.Formularios.Add(formulario);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Formulario creado exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al crear formulario: {ex.Message}" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> ActualizarFormulario([FromBody] Formulario formulario)
        {
            try
            {
                var formularioExistente = await _context.Formularios
                    .FirstOrDefaultAsync(f => f.Identificacion == formulario.Identificacion);

                if (formularioExistente == null)
                {
                    return Json(new { success = false, message = "Formulario no encontrado" });
                }

                formularioExistente.Nombre = formulario.Nombre;
                formularioExistente.Descripcion = formulario.Descripcion;
                formularioExistente.ArchivoRuta = formulario.ArchivoRuta ?? "";
                if (formulario.ProfesorIdentificacion > 0)
                    formularioExistente.ProfesorIdentificacion = formulario.ProfesorIdentificacion;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Formulario actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al actualizar formulario: {ex.Message}" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> EliminarFormulario(int id)
        {
            try
            {
                var formulario = await _context.Formularios
                    .FirstOrDefaultAsync(f => f.Identificacion == id);

                if (formulario == null)
                {
                    return Json(new { success = false, message = "Formulario no encontrado" });
                }

                _context.Formularios.Remove(formulario);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Formulario eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al eliminar formulario: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearEntrega([FromBody] Entrega entrega)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entrega.Nombre) || 
                    string.IsNullOrWhiteSpace(entrega.Descripcion))
                {
                    return Json(new { success = false, message = "Nombre y descripción son requeridos" });
                }

                if (entrega.ArchivoRuta == null)
                    entrega.ArchivoRuta = "";
                if (entrega.Retroalimentacion == null)
                    entrega.Retroalimentacion = "";

                _context.Entregas.Add(entrega);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Entrega creada exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al crear entrega: {ex.Message}" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> ActualizarEntrega([FromBody] Entrega entrega)
        {
            try
            {
                var entregaExistente = await _context.Entregas
                    .FirstOrDefaultAsync(e => e.Identificacion == entrega.Identificacion);

                if (entregaExistente == null)
                {
                    return Json(new { success = false, message = "Entrega no encontrada" });
                }

                entregaExistente.Nombre = entrega.Nombre;
                entregaExistente.Descripcion = entrega.Descripcion;
                entregaExistente.FechaLimite = entrega.FechaLimite;
                entregaExistente.ArchivoRuta = entrega.ArchivoRuta ?? "";
                entregaExistente.Retroalimentacion = entrega.Retroalimentacion ?? "";
                if (entrega.GrupoNumero > 0)
                    entregaExistente.GrupoNumero = entrega.GrupoNumero;
                if (entrega.ProfesorIdentificacion > 0)
                    entregaExistente.ProfesorIdentificacion = entrega.ProfesorIdentificacion;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Entrega actualizada exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al actualizar entrega: {ex.Message}" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> EliminarEntrega(int id)
        {
            try
            {
                var entrega = await _context.Entregas
                    .FirstOrDefaultAsync(e => e.Identificacion == id);

                if (entrega == null)
                {
                    return Json(new { success = false, message = "Entrega no encontrada" });
                }

                _context.Entregas.Remove(entrega);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Entrega eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al eliminar entrega: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerFormulario(int id)
        {
            try
            {
                var formulario = await _context.Formularios
                    .Include(f => f.Profesor)
                    .FirstOrDefaultAsync(f => f.Identificacion == id);

                if (formulario == null)
                {
                    return Json(new { success = false, message = "Formulario no encontrado" });
                }

                return Json(new { 
                    success = true, 
                    formulario = new {
                        identificacion = formulario.Identificacion,
                        nombre = formulario.Nombre,
                        descripcion = formulario.Descripcion,
                        archivoRuta = formulario.ArchivoRuta,
                        fechaIngreso = formulario.FechaIngreso,
                        profesorIdentificacion = formulario.ProfesorIdentificacion,
                        profesorNombre = formulario.Profesor != null ? 
                            $"{formulario.Profesor.Nombre} {formulario.Profesor.Apellidos}" : ""
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al obtener formulario: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerEntrega(int id)
        {
            try
            {
                var entrega = await _context.Entregas
                    .Include(e => e.Grupo)
                    .Include(e => e.Profesor)
                    .FirstOrDefaultAsync(e => e.Identificacion == id);

                if (entrega == null)
                {
                    return Json(new { success = false, message = "Entrega no encontrada" });
                }

                return Json(new { 
                    success = true, 
                    entrega = new {
                        identificacion = entrega.Identificacion,
                        nombre = entrega.Nombre,
                        descripcion = entrega.Descripcion,
                        archivoRuta = entrega.ArchivoRuta,
                        fechaLimite = entrega.FechaLimite,
                        retroalimentacion = entrega.Retroalimentacion,
                        fechaRetroalimentacion = entrega.FechaRetroalimentacion,
                        grupoNumero = entrega.GrupoNumero,
                        profesorIdentificacion = entrega.ProfesorIdentificacion,
                        profesorNombre = entrega.Profesor != null ? 
                            $"{entrega.Profesor.Nombre} {entrega.Profesor.Apellidos}" : ""
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al obtener entrega: {ex.Message}" });
            }
        }
    }
}
