using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioComunal.Data;
using ServicioComunal.Services;
using ServicioComunal.Models;
using ServicioComunal.Utilities;
using ServicioComunal.Attributes;
using OfficeOpenXml;
using System.Text;
using System.Text.Json.Serialization;

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

    [RequireAuth("Administrador")]
    public class HomeController : Controller
    {
        private readonly DataSeederService _seeder;
        private readonly ServicioComunalDbContext _context;
        private readonly NotificacionService _notificacionService;

        public HomeController(DataSeederService seeder, ServicioComunalDbContext context, NotificacionService notificacionService)
        {
            _seeder = seeder;
            _context = context;
            _notificacionService = notificacionService;
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

        [HttpPost]
        public async Task<IActionResult> AgregarDocente([FromBody] ServicioComunal.Models.Profesor profesor)
        {
            try
            {
                // Verificar si ya existe un profesor con esa identificación
                var existeProfesor = await _context.Profesores
                    .AnyAsync(p => p.Identificacion == profesor.Identificacion);

                if (existeProfesor)
                {
                    return Json(new { success = false, message = "Ya existe un docente con esa cédula" });
                }

                // Verificar campos obligatorios
                if (string.IsNullOrWhiteSpace(profesor.Nombre) || 
                    string.IsNullOrWhiteSpace(profesor.Apellidos) ||
                    string.IsNullOrWhiteSpace(profesor.Rol))
                {
                    return Json(new { success = false, message = "Todos los campos son obligatorios" });
                }

                // Validar que el rol sea válido
                var rolesValidos = new[] { "Tutor", "Coordinador", "Supervisor", "Administrador" };
                if (!rolesValidos.Contains(profesor.Rol))
                {
                    return Json(new { success = false, message = "Rol no válido" });
                }

                // Si es administrador, también crear usuario
                if (profesor.Rol == "Administrador")
                {
                    var existeUsuario = await _context.Usuarios
                        .AnyAsync(u => u.Identificacion == profesor.Identificacion);

                    if (!existeUsuario)
                    {
                        // Generar nombre de usuario en formato primer_nombre.primer_apellido
                        string nombreUsuario = GenerarNombreUsuario(profesor.Nombre, profesor.Apellidos);
                        
                        var usuario = new ServicioComunal.Models.Usuario
                        {
                            Identificacion = profesor.Identificacion,
                            NombreUsuario = nombreUsuario,
                            Contraseña = profesor.Identificacion.ToString(), // Contraseña temporal = cédula
                            Rol = "Administrador",
                            RequiereCambioContraseña = true
                        };
                        _context.Usuarios.Add(usuario);
                    }
                }

                _context.Profesores.Add(profesor);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = profesor.Rol == "Administrador" ? 
                        "Docente creado exitosamente. Se ha creado un usuario administrador con contraseña temporal igual a su cédula." :
                        "Docente creado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al crear el docente: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportarProfesores(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                TempData["Error"] = "Por favor seleccione un archivo válido.";
                return RedirectToAction("AsignarTutores");
            }

            var extensionesPermitidas = new[] { ".xlsx", ".xls" };
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            
            if (!extensionesPermitidas.Contains(extension))
            {
                TempData["Error"] = "Solo se permiten archivos Excel (.xlsx, .xls).";
                return RedirectToAction("AsignarTutores");
            }

            try
            {
                var profesoresImportados = 0;
                var profesoresExistentes = 0;
                var errores = new List<string>();

                using (var stream = archivo.OpenReadStream())
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension?.Rows ?? 0;

                        if (rowCount < 2)
                        {
                            TempData["Error"] = "El archivo debe contener al menos una fila de datos además del encabezado.";
                            return RedirectToAction("AsignarTutores");
                        }

                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var apellidos = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                var nombre = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                                var cedulaTexto = worksheet.Cells[row, 3].Value?.ToString()?.Trim();

                                if (string.IsNullOrWhiteSpace(apellidos) || 
                                    string.IsNullOrWhiteSpace(nombre) || 
                                    string.IsNullOrWhiteSpace(cedulaTexto))
                                {
                                    errores.Add($"Fila {row}: Datos incompletos");
                                    continue;
                                }

                                if (!int.TryParse(cedulaTexto, out int cedula))
                                {
                                    errores.Add($"Fila {row}: Cédula inválida ({cedulaTexto})");
                                    continue;
                                }

                                // Verificar si ya existe
                                var existeProfesor = await _context.Profesores
                                    .AnyAsync(p => p.Identificacion == cedula);

                                if (existeProfesor)
                                {
                                    profesoresExistentes++;
                                    continue;
                                }

                                var profesor = new ServicioComunal.Models.Profesor
                                {
                                    Identificacion = cedula,
                                    Nombre = nombre,
                                    Apellidos = apellidos,
                                    Rol = "Tutor" // Rol por defecto para importación masiva
                                };

                                _context.Profesores.Add(profesor);
                                profesoresImportados++;
                            }
                            catch (Exception ex)
                            {
                                errores.Add($"Fila {row}: {ex.Message}");
                            }
                        }

                        await _context.SaveChangesAsync();
                    }
                }

                var mensaje = new StringBuilder();
                mensaje.AppendLine($"Importación completada:");
                mensaje.AppendLine($"- Profesores importados: {profesoresImportados}");
                if (profesoresExistentes > 0)
                    mensaje.AppendLine($"- Profesores ya existentes (omitidos): {profesoresExistentes}");
                if (errores.Any())
                {
                    mensaje.AppendLine($"- Errores encontrados: {errores.Count}");
                    mensaje.AppendLine("Errores:");
                    foreach (var error in errores.Take(5))
                    {
                        mensaje.AppendLine($"  • {error}");
                    }
                    if (errores.Count > 5)
                        mensaje.AppendLine($"  • ... y {errores.Count - 5} errores más");
                }

                if (profesoresImportados > 0)
                    TempData["Success"] = mensaje.ToString();
                else
                    TempData["Warning"] = mensaje.ToString();

                return RedirectToAction("AsignarTutores");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al importar archivo: {ex.Message}";
                return RedirectToAction("AsignarTutores");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportarEstudiantes(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return Json(new { success = false, message = "Por favor seleccione un archivo válido." });
            }

            var extensionesPermitidas = new[] { ".xlsx", ".xls", ".csv" };
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            
            if (!extensionesPermitidas.Contains(extension))
            {
                return Json(new { success = false, message = "Solo se permiten archivos Excel (.xlsx, .xls) o CSV (.csv)." });
            }

            try
            {
                var estudiantesImportados = 0;
                var estudiantesExistentes = 0;
                var usuariosCreados = 0;
                var errores = new List<string>();
                var advertencias = new List<string>();

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        List<EstudianteImportDto> estudiantesParaImportar;

                        if (extension == ".csv")
                        {
                            estudiantesParaImportar = await ProcesarArchivoCSV(archivo);
                        }
                        else
                        {
                            estudiantesParaImportar = ProcesarArchivoExcel(archivo);
                        }

                        // Validar datos antes de importar
                        var validacionResult = ValidarDatosImportacion(estudiantesParaImportar);
                        if (!validacionResult.EsValido)
                        {
                            return Json(new { success = false, message = validacionResult.Mensaje, errores = validacionResult.Errores });
                        }

                        // Obtener estudiantes y usuarios existentes para evitar consultas repetidas
                        var estudiantesExistentesDb = await _context.Estudiantes
                            .Select(e => e.Identificacion)
                            .ToListAsync();
                        
                        var usuariosExistentesDb = await _context.Usuarios
                            .Select(u => u.Identificacion)
                            .ToListAsync();

                        var nombresUsuarioExistentes = await _context.Usuarios
                            .Select(u => u.NombreUsuario.ToLower())
                            .ToListAsync();

                        foreach (var estudianteDto in estudiantesParaImportar)
                        {
                            try
                            {
                                // Verificar si ya existe el estudiante
                                if (estudiantesExistentesDb.Contains(estudianteDto.Identificacion))
                                {
                                    estudiantesExistentes++;
                                    advertencias.Add($"Estudiante {estudianteDto.Nombre} {estudianteDto.Apellidos} (ID: {estudianteDto.Identificacion}) ya existe");
                                    continue;
                                }

                                // Crear estudiante
                                var estudiante = new ServicioComunal.Models.Estudiante
                                {
                                    Identificacion = estudianteDto.Identificacion,
                                    Nombre = estudianteDto.Nombre,
                                    Apellidos = estudianteDto.Apellidos,
                                    Clase = estudianteDto.Clase
                                };

                                _context.Estudiantes.Add(estudiante);
                                estudiantesImportados++;

                                // Crear usuario si no existe
                                if (!usuariosExistentesDb.Contains(estudianteDto.Identificacion))
                                {
                                    string nombreUsuario = GenerarNombreUsuarioUnico(estudianteDto.Nombre, estudianteDto.Apellidos, nombresUsuarioExistentes);
                                    
                                    var usuario = new ServicioComunal.Models.Usuario
                                    {
                                        Identificacion = estudianteDto.Identificacion,
                                        NombreUsuario = nombreUsuario,
                                        Contraseña = PasswordHelper.HashPassword(estudianteDto.Identificacion.ToString()),
                                        Rol = "Estudiante",
                                        RequiereCambioContraseña = true,
                                        FechaCreacion = DateTime.Now,
                                        Activo = true
                                    };

                                    _context.Usuarios.Add(usuario);
                                    usuariosCreados++;
                                    nombresUsuarioExistentes.Add(nombreUsuario.ToLower());
                                }
                            }
                            catch (Exception ex)
                            {
                                errores.Add($"Error procesando estudiante {estudianteDto.Nombre} {estudianteDto.Apellidos}: {ex.Message}");
                            }
                        }

                        if (estudiantesImportados == 0)
                        {
                            return Json(new { success = false, message = "No se importó ningún estudiante nuevo." });
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        
                        var mensaje = $"Importación completada: {estudiantesImportados} estudiantes y {usuariosCreados} usuarios creados.";
                        if (estudiantesExistentes > 0)
                        {
                            mensaje += $" {estudiantesExistentes} estudiantes ya existían.";
                        }

                        return Json(new { 
                            success = true, 
                            message = mensaje,
                            estudiantesImportados,
                            usuariosCreados,
                            estudiantesExistentes,
                            errores = errores.Count > 0 ? errores : null,
                            advertencias = advertencias.Count > 0 ? advertencias : null
                        });
                    }
                    catch (Exception transactionEx)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine($"Error en transacción de importación: {transactionEx.Message}");
                        return Json(new { success = false, message = $"Error en la transacción: {transactionEx.Message}" });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general en importación: {ex.Message}");
                return Json(new { success = false, message = $"Error al importar archivo: {ex.Message}" });
            }
        }

        private List<EstudianteImportDto> ProcesarArchivoExcel(IFormFile archivo)
        {
            var estudiantes = new List<EstudianteImportDto>();
            
            using (var stream = archivo.OpenReadStream())
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension?.Rows ?? 0;

                    if (rowCount < 2)
                    {
                        throw new InvalidOperationException("El archivo debe contener al menos una fila de datos además del encabezado.");
                    }

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var apellidos = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                        var nombre = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                        var clase = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                        var cedulaTexto = worksheet.Cells[row, 4].Value?.ToString()?.Trim();

                        if (!string.IsNullOrWhiteSpace(apellidos) && 
                            !string.IsNullOrWhiteSpace(nombre) && 
                            !string.IsNullOrWhiteSpace(clase) &&
                            !string.IsNullOrWhiteSpace(cedulaTexto) &&
                            int.TryParse(cedulaTexto, out int cedula))
                        {
                            estudiantes.Add(new EstudianteImportDto
                            {
                                Identificacion = cedula,
                                Nombre = nombre,
                                Apellidos = apellidos,
                                Clase = clase,
                                FilaOrigen = row
                            });
                        }
                    }
                }
            }
            
            return estudiantes;
        }

        private async Task<List<EstudianteImportDto>> ProcesarArchivoCSV(IFormFile archivo)
        {
            var estudiantes = new List<EstudianteImportDto>();
            
            using (var reader = new StreamReader(archivo.OpenReadStream()))
            {
                var line = await reader.ReadLineAsync(); // Saltar encabezado
                int fila = 2;
                
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var campos = line.Split(',');
                    
                    if (campos.Length >= 4 &&
                        !string.IsNullOrWhiteSpace(campos[0]) &&
                        !string.IsNullOrWhiteSpace(campos[1]) &&
                        !string.IsNullOrWhiteSpace(campos[2]) &&
                        !string.IsNullOrWhiteSpace(campos[3]) &&
                        int.TryParse(campos[3].Trim(), out int cedula))
                    {
                        estudiantes.Add(new EstudianteImportDto
                        {
                            Identificacion = cedula,
                            Nombre = campos[1].Trim(),
                            Apellidos = campos[0].Trim(),
                            Clase = campos[2].Trim(),
                            FilaOrigen = fila
                        });
                    }
                    fila++;
                }
            }
            
            return estudiantes;
        }

        private (bool EsValido, string Mensaje, List<string> Errores) ValidarDatosImportacion(List<EstudianteImportDto> estudiantes)
        {
            var errores = new List<string>();
            
            if (!estudiantes.Any())
            {
                return (false, "No se encontraron datos válidos para importar.", errores);
            }

            // Verificar duplicados en el archivo
            var duplicados = estudiantes.GroupBy(e => e.Identificacion)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicados.Any())
            {
                errores.Add($"Identificaciones duplicadas en el archivo: {string.Join(", ", duplicados)}");
            }

            // Validar longitud de campos
            foreach (var estudiante in estudiantes)
            {
                if (estudiante.Nombre.Length > 50)
                    errores.Add($"Fila {estudiante.FilaOrigen}: Nombre demasiado largo (máximo 50 caracteres)");
                
                if (estudiante.Apellidos.Length > 50)
                    errores.Add($"Fila {estudiante.FilaOrigen}: Apellidos demasiado largos (máximo 50 caracteres)");
                
                if (estudiante.Clase.Length > 20)
                    errores.Add($"Fila {estudiante.FilaOrigen}: Clase demasiado larga (máximo 20 caracteres)");
                
                if (estudiante.Identificacion <= 0)
                    errores.Add($"Fila {estudiante.FilaOrigen}: Identificación debe ser un número positivo");
            }

            return (errores.Count == 0, errores.Count > 0 ? $"Se encontraron {errores.Count} errores de validación." : "Validación exitosa", errores);
        }

        private string GenerarNombreUsuarioUnico(string nombre, string apellidos, List<string> nombresExistentes)
        {
            // Generar nombre base
            var nombreBase = GenerarNombreUsuario(nombre, apellidos).ToLower();
            
            if (!nombresExistentes.Contains(nombreBase))
            {
                return nombreBase;
            }

            // Si existe, agregar número
            int contador = 1;
            string nombreConNumero;
            do
            {
                nombreConNumero = $"{nombreBase}{contador}";
                contador++;
            }
            while (nombresExistentes.Contains(nombreConNumero));

            return nombreConNumero;
        }

        public class EstudianteImportDto
        {
            public int Identificacion { get; set; }
            public required string Nombre { get; set; }
            public required string Apellidos { get; set; }
            public required string Clase { get; set; }
            public int FilaOrigen { get; set; }
        }

        [HttpGet]
        public IActionResult DescargarPlantillaEstudiantes()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Estudiantes");

                    // Encabezados
                    worksheet.Cells[1, 1].Value = "Apellidos";
                    worksheet.Cells[1, 2].Value = "Nombre";
                    worksheet.Cells[1, 3].Value = "Clase";
                    worksheet.Cells[1, 4].Value = "Número de identificación";

                    // Estilo para encabezados
                    using (var range = worksheet.Cells[1, 1, 1, 4])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                        range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    // Datos de ejemplo
                    worksheet.Cells[2, 1].Value = "Pérez González";
                    worksheet.Cells[2, 2].Value = "Juan Carlos";
                    worksheet.Cells[2, 3].Value = "11-A";
                    worksheet.Cells[2, 4].Value = "123456789";

                    worksheet.Cells[3, 1].Value = "Rodríguez Mora";
                    worksheet.Cells[3, 2].Value = "María Elena";
                    worksheet.Cells[3, 3].Value = "10-B";
                    worksheet.Cells[3, 4].Value = "987654321";

                    worksheet.Cells[4, 1].Value = "Jiménez Castro";
                    worksheet.Cells[4, 2].Value = "Luis Fernando";
                    worksheet.Cells[4, 3].Value = "11-C";
                    worksheet.Cells[4, 4].Value = "456789123";

                    // Ajustar ancho de columnas
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Generar el archivo
                    var fileName = "Plantilla_Estudiantes.xlsx";
                    var fileBytes = package.GetAsByteArray();
                    
                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al generar plantilla: {ex.Message}";
                return RedirectToAction("Estudiantes");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugUsuarios()
        {
            var usuarios = await _context.Usuarios.ToListAsync();
            var estudiantes = await _context.Estudiantes.ToListAsync();
            
            var info = new StringBuilder();
            info.AppendLine($"Total usuarios: {usuarios.Count}");
            info.AppendLine($"Total estudiantes: {estudiantes.Count}");
            info.AppendLine("\nUsuarios:");
            foreach (var usuario in usuarios)
            {
                info.AppendLine($"- {usuario.NombreUsuario} (ID: {usuario.Identificacion}, Rol: {usuario.Rol})");
            }
            info.AppendLine("\nEstudiantes:");
            foreach (var estudiante in estudiantes)
            {
                info.AppendLine($"- {estudiante.Nombre} {estudiante.Apellidos} (ID: {estudiante.Identificacion}, Clase: {estudiante.Clase})");
            }
            
            return Content(info.ToString(), "text/plain");
        }

        [HttpGet]
        public async Task<IActionResult> TestImportEstudiantes()
        {
            try
            {
                Console.WriteLine("=== INICIANDO TEST DE IMPORTACIÓN ===");
                
                // Crear un estudiante de prueba directamente
                var testStudentId = 999888777;
                
                // Verificar si ya existe
                var existeEstudiante = await _context.Estudiantes
                    .AnyAsync(e => e.Identificacion == testStudentId);
                
                var existeUsuario = await _context.Usuarios
                    .AnyAsync(u => u.Identificacion == testStudentId);
                
                Console.WriteLine($"Estudiante existe: {existeEstudiante}");
                Console.WriteLine($"Usuario existe: {existeUsuario}");
                
                if (!existeEstudiante)
                {
                    // Crear estudiante
                    var estudiante = new ServicioComunal.Models.Estudiante
                    {
                        Identificacion = testStudentId,
                        Nombre = "Test",
                        Apellidos = "Student",
                        Clase = "TEST"
                    };
                    
                    _context.Estudiantes.Add(estudiante);
                    Console.WriteLine("Estudiante agregado al contexto");
                    
                    // Crear usuario correspondiente
                    if (!existeUsuario)
                    {
                        string nombreUsuario = GenerarNombreUsuario("Test", "Student");
                        Console.WriteLine($"Username generado: {nombreUsuario}");
                        
                        var usuario = new ServicioComunal.Models.Usuario
                        {
                            Identificacion = testStudentId,
                            NombreUsuario = nombreUsuario,
                            Contraseña = PasswordHelper.HashPassword(testStudentId.ToString()),
                            Rol = "Estudiante",
                            RequiereCambioContraseña = true,
                            FechaCreacion = DateTime.Now,
                            Activo = true
                        };
                        
                        _context.Usuarios.Add(usuario);
                        Console.WriteLine("Usuario agregado al contexto");
                    }
                    
                    // Guardar cambios
                    await _context.SaveChangesAsync();
                    Console.WriteLine("SaveChangesAsync completado");
                    
                    // Verificar que se guardaron
                    var estudianteGuardado = await _context.Estudiantes
                        .FirstOrDefaultAsync(e => e.Identificacion == testStudentId);
                    var usuarioGuardado = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.Identificacion == testStudentId);
                    
                    Console.WriteLine($"Estudiante guardado: {estudianteGuardado != null}");
                    Console.WriteLine($"Usuario guardado: {usuarioGuardado != null}");
                    
                    if (usuarioGuardado != null)
                    {
                        Console.WriteLine($"Usuario creado - Username: {usuarioGuardado.NombreUsuario}, Rol: {usuarioGuardado.Rol}");
                    }
                }
                
                var totalUsuarios = await _context.Usuarios.CountAsync();
                var totalEstudiantes = await _context.Estudiantes.CountAsync();
                
                var resultado = $"TEST COMPLETADO:\n" +
                              $"Total estudiantes: {totalEstudiantes}\n" +
                              $"Total usuarios: {totalUsuarios}\n" +
                              $"Estudiante test guardado: {await _context.Estudiantes.AnyAsync(e => e.Identificacion == testStudentId)}\n" +
                              $"Usuario test guardado: {await _context.Usuarios.AnyAsync(u => u.Identificacion == testStudentId)}";
                
                Console.WriteLine(resultado);
                return Content(resultado, "text/plain");
            }
            catch (Exception ex)
            {
                var error = $"ERROR EN TEST: {ex.Message}\nStackTrace: {ex.StackTrace}";
                Console.WriteLine(error);
                return Content(error, "text/plain");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CambiarRolProfesor([FromBody] CambiarRolRequest request)
        {
            try
            {
                Console.WriteLine($"Cambiando rol para ProfesorId: {request.ProfesorId} a {request.NuevoRol}");
                
                var profesor = await _context.Profesores
                    .FirstOrDefaultAsync(p => p.Identificacion == request.ProfesorId);

                if (profesor == null)
                {
                    Console.WriteLine($"Profesor no encontrado con ID: {request.ProfesorId}");
                    return Json(new { success = false, message = "Profesor no encontrado" });
                }

                var rolesValidos = new[] { "Tutor", "Administrador" };
                if (!rolesValidos.Contains(request.NuevoRol))
                {
                    return Json(new { success = false, message = "Rol no válido" });
                }

                var rolAnterior = profesor.Rol;
                Console.WriteLine($"Cambiando rol de {rolAnterior} a {request.NuevoRol} para {profesor.Nombre} {profesor.Apellidos}");
                
                profesor.Rol = request.NuevoRol;

                // Si se cambia a administrador, crear usuario si no existe o actualizar si existe
                if (request.NuevoRol == "Administrador")
                {
                    var usuarioExistente = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.Identificacion == profesor.Identificacion);

                    if (usuarioExistente == null)
                    {
                        // Generar nombre de usuario en formato primer_nombre.primer_apellido
                        string nombreUsuario = GenerarNombreUsuario(profesor.Nombre, profesor.Apellidos);
                        
                        var usuario = new ServicioComunal.Models.Usuario
                        {
                            Identificacion = profesor.Identificacion,
                            NombreUsuario = nombreUsuario,
                            Contraseña = PasswordHelper.HashPassword(profesor.Identificacion.ToString()),
                            Rol = "Administrador",
                            RequiereCambioContraseña = true
                        };
                        _context.Usuarios.Add(usuario);
                        Console.WriteLine($"Usuario administrador creado: {nombreUsuario} para {profesor.Nombre}");
                    }
                    else
                    {
                        usuarioExistente.Rol = "Administrador";
                        // Resetear contraseña con la cédula hasheada
                        usuarioExistente.Contraseña = PasswordHelper.HashPassword(profesor.Identificacion.ToString());
                        usuarioExistente.RequiereCambioContraseña = true;
                        
                        // También actualizar el nombre de usuario si está en formato de cédula
                        if (usuarioExistente.NombreUsuario == profesor.Identificacion.ToString())
                        {
                            usuarioExistente.NombreUsuario = GenerarNombreUsuario(profesor.Nombre, profesor.Apellidos);
                            Console.WriteLine($"Nombre de usuario actualizado a formato correcto: {usuarioExistente.NombreUsuario}");
                        }
                        Console.WriteLine($"Usuario existente actualizado a Administrador para {profesor.Nombre} - Contraseña reseteada");
                    }
                }
                // Si se quita el rol de administrador
                else if (rolAnterior == "Administrador")
                {
                    var usuario = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.Identificacion == profesor.Identificacion);
                    if (usuario != null)
                    {
                        // Si el nuevo rol permite tener usuario (como Profesor), actualizar rol
                        if (request.NuevoRol == "Profesor")
                        {
                            usuario.Rol = "Profesor";
                            Console.WriteLine($"Usuario actualizado de Administrador a Profesor para {profesor.Nombre}");
                        }
                        else
                        {
                            // Para otros roles, eliminar usuario
                            _context.Usuarios.Remove(usuario);
                            Console.WriteLine($"Usuario eliminado para {profesor.Nombre} (rol: {request.NuevoRol})");
                        }
                    }
                }
                // Si ya existe un usuario con rol diferente, actualizar
                else
                {
                    var usuario = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.Identificacion == profesor.Identificacion);
                    if (usuario != null)
                    {
                        usuario.Rol = request.NuevoRol;
                        Console.WriteLine($"Usuario existente actualizado de {usuario.Rol} a {request.NuevoRol} para {profesor.Nombre}");
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"Rol actualizado exitosamente en la base de datos");

                return Json(new { 
                    success = true, 
                    message = $"Rol cambiado exitosamente de {rolAnterior} a {request.NuevoRol}" +
                              (request.NuevoRol == "Administrador" ? ". Se ha creado acceso de administrador." : "")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cambiar el rol: " + ex.Message });
            }
        }

        public class CambiarRolRequest
        {
            [JsonPropertyName("profesorId")]
            public int ProfesorId { get; set; }
            
            [JsonPropertyName("nuevoRol")]
            public string NuevoRol { get; set; } = string.Empty;
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

        public async Task<IActionResult> Formularios()
        {
            try
            {
                // Versión simplificada para debug
                var formularios = new List<Formulario>();
                
                // Intentar cargar formularios de la base de datos
                try
                {
                    formularios = await _context.Formularios
                        .Include(f => f.Profesor)
                        .OrderByDescending(f => f.FechaIngreso)
                        .ToListAsync();
                }
                catch (Exception dbEx)
                {
                    ViewBag.Error = $"Error de base de datos: {dbEx.Message}";
                }

                ViewBag.Profesores = await _context.Profesores
                    .OrderBy(p => p.Apellidos)
                    .ThenBy(p => p.Nombre)
                    .ToListAsync();

                ViewBag.Formularios = formularios;

                return View(formularios);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error general: {ex.Message}";
                return View(new List<Formulario>());
            }
        }

        public async Task<IActionResult> Entregas()
        {
            try
            {
                var entregas = await _context.Entregas
                    .Include(e => e.Grupo)
                    .Include(e => e.Profesor)
                    .Include(e => e.Formulario)
                    .OrderByDescending(e => e.FechaLimite)
                    .ToListAsync();

                ViewBag.Grupos = await _context.Grupos
                    .Include(g => g.GruposEstudiantes)
                    .OrderBy(g => g.Numero)
                    .ToListAsync();

                ViewBag.Formularios = await _context.Formularios
                    .OrderBy(f => f.Nombre)
                    .ToListAsync();

                ViewBag.Profesores = await _context.Profesores
                    .OrderBy(p => p.Apellidos)
                    .ThenBy(p => p.Nombre)
                    .ToListAsync();

                return View(entregas);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al cargar entregas: {ex.Message}";
                return View(new List<Entrega>());
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
                // await _seeder.ReseedUsuariosAsync();
                await _seeder.LimpiarYRegenerarAsync();
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
                // await _seeder.SeedUsuariosSafeAsync();
                await _seeder.SeedDataAsync();
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
                var estudiantes = await _context.Estudiantes.ToListAsync();
                var gruposEstudiantes = await _context.GruposEstudiantes.ToListAsync();
                
                var mensaje = $"=== USUARIOS EN BD: {usuarios.Count} ===\n\n";
                
                foreach (var user in usuarios.Where(u => u.Rol == "Estudiante"))
                {
                    var estudiante = estudiantes.FirstOrDefault(e => e.Identificacion == user.Identificacion);
                    var tieneGrupo = gruposEstudiantes.Any(ge => ge.EstudianteIdentificacion == user.Identificacion);
                    
                    mensaje += $"ID: {user.Identificacion}\n";
                    mensaje += $"Usuario: {user.NombreUsuario}\n";
                    mensaje += $"Nombre: {estudiante?.Nombre} {estudiante?.Apellidos}\n";
                    mensaje += $"Activo: {user.Activo}\n";
                    mensaje += $"Tiene Grupo: {(tieneGrupo ? "SÍ" : "NO")}\n";
                    mensaje += $"Contraseña Hash: {user.Contraseña[..20]}...\n\n";
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

                // Notificar al tutor sobre la asignación del grupo
                await _notificacionService.NotificarGrupoAsignadoAsync(tutorId, grupoNumero);

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

                // Notificar al tutor sobre la asignación del grupo
                await _notificacionService.NotificarGrupoAsignadoAsync(request.TutorId, request.GrupoNumero);

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

        [HttpPost]
        public async Task<IActionResult> CrearFormularioConArchivo()
        {
            try
            {
                // Verificar autenticación
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                var usuarioRol = HttpContext.Session.GetString("UsuarioRol");
                
                if (usuarioId == null || usuarioRol != "Administrador")
                {
                    return Json(new { success = false, message = "No tienes permisos para crear formularios" });
                }

                var nombre = Request.Form["nombre"].ToString();
                var descripcion = Request.Form["descripcion"].ToString();
                var archivo = Request.Form.Files.GetFile("archivo");

                if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(descripcion))
                {
                    return Json(new { success = false, message = "Nombre y descripción son requeridos" });
                }

                string archivoRuta = "";

                // Procesar archivo si se subió uno
                if (archivo != null && archivo.Length > 0)
                {
                    // Validar tipo de archivo
                    var extensionesPermitidas = new[] { ".pdf", ".docx", ".doc" };
                    var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
                    
                    if (!extensionesPermitidas.Contains(extension))
                    {
                        return Json(new { success = false, message = "Solo se permiten archivos PDF, DOC o DOCX" });
                    }

                    // Validar tamaño (10MB máximo)
                    if (archivo.Length > 10 * 1024 * 1024)
                    {
                        return Json(new { success = false, message = "El archivo no puede ser mayor a 10MB" });
                    }

                    // Crear directorio si no existe
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "formularios");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    // Generar nombre único para el archivo
                    var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                    var rutaCompleta = Path.Combine(uploadsDir, nombreArchivo);

                    // Guardar archivo
                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await archivo.CopyToAsync(stream);
                    }

                    archivoRuta = $"uploads/formularios/{nombreArchivo}";
                }

                var formulario = new Formulario
                {
                    Nombre = nombre,
                    Descripcion = descripcion,
                    ArchivoRuta = archivoRuta,
                    FechaIngreso = DateTime.Now,
                    ProfesorIdentificacion = usuarioId.Value // Usuario autenticado
                };

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

        [HttpPost]
        public async Task<IActionResult> ActualizarFormularioConArchivo()
        {
            try
            {
                var idStr = Request.Form["identificacion"].ToString();
                var nombre = Request.Form["nombre"].ToString();
                var descripcion = Request.Form["descripcion"].ToString();
                var mantenerArchivo = Request.Form["mantenerArchivo"].ToString();
                var archivo = Request.Form.Files.GetFile("archivo");

                if (!int.TryParse(idStr, out int id))
                {
                    return Json(new { success = false, message = "ID de formulario inválido" });
                }

                var formularioExistente = await _context.Formularios
                    .FirstOrDefaultAsync(f => f.Identificacion == id);

                if (formularioExistente == null)
                {
                    return Json(new { success = false, message = "Formulario no encontrado" });
                }

                formularioExistente.Nombre = nombre;
                formularioExistente.Descripcion = descripcion;

                // Procesar archivo si se subió uno nuevo
                if (archivo != null && archivo.Length > 0)
                {
                    // Validar tipo de archivo
                    var extensionesPermitidas = new[] { ".pdf", ".docx", ".doc" };
                    var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
                    
                    if (!extensionesPermitidas.Contains(extension))
                    {
                        return Json(new { success = false, message = "Solo se permiten archivos PDF, DOC o DOCX" });
                    }

                    // Validar tamaño (10MB máximo)
                    if (archivo.Length > 10 * 1024 * 1024)
                    {
                        return Json(new { success = false, message = "El archivo no puede ser mayor a 10MB" });
                    }

                    // Eliminar archivo anterior si existe
                    if (!string.IsNullOrEmpty(formularioExistente.ArchivoRuta))
                    {
                        var archivoAnterior = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", formularioExistente.ArchivoRuta);
                        if (System.IO.File.Exists(archivoAnterior))
                        {
                            System.IO.File.Delete(archivoAnterior);
                        }
                    }

                    // Crear directorio si no existe
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "formularios");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    // Generar nombre único para el archivo
                    var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                    var rutaCompleta = Path.Combine(uploadsDir, nombreArchivo);

                    // Guardar archivo
                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await archivo.CopyToAsync(stream);
                    }

                    formularioExistente.ArchivoRuta = $"uploads/formularios/{nombreArchivo}";
                }
                else if (mantenerArchivo != "true")
                {
                    // Si no se mantiene el archivo y no se subió uno nuevo, eliminar el actual
                    if (!string.IsNullOrEmpty(formularioExistente.ArchivoRuta))
                    {
                        var archivoAnterior = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", formularioExistente.ArchivoRuta);
                        if (System.IO.File.Exists(archivoAnterior))
                        {
                            System.IO.File.Delete(archivoAnterior);
                        }
                        formularioExistente.ArchivoRuta = "";
                    }
                }

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

                // Eliminar archivo físico si existe
                if (!string.IsNullOrEmpty(formulario.ArchivoRuta))
                {
                    var rutaArchivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", formulario.ArchivoRuta);
                    if (System.IO.File.Exists(rutaArchivo))
                    {
                        System.IO.File.Delete(rutaArchivo);
                    }
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

        [HttpGet]
        public async Task<IActionResult> DescargarFormulario(int id)
        {
            try
            {
                var formulario = await _context.Formularios
                    .FirstOrDefaultAsync(f => f.Identificacion == id);

                if (formulario == null)
                {
                    return NotFound("Formulario no encontrado");
                }

                if (string.IsNullOrEmpty(formulario.ArchivoRuta))
                {
                    return NotFound("El formulario no tiene un archivo asociado");
                }

                var rutaArchivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", formulario.ArchivoRuta);
                
                if (!System.IO.File.Exists(rutaArchivo))
                {
                    return NotFound("El archivo no se encuentra en el servidor");
                }

                var bytes = await System.IO.File.ReadAllBytesAsync(rutaArchivo);
                var extension = Path.GetExtension(rutaArchivo).ToLowerInvariant();
                
                string contentType = extension switch
                {
                    ".pdf" => "application/pdf",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    ".doc" => "application/msword",
                    _ => "application/octet-stream"
                };

                var nombreArchivo = $"{formulario.Nombre}{extension}";
                
                return File(bytes, contentType, nombreArchivo);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al descargar el archivo: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearEntrega([FromBody] EntregaCreacionDto entregaDto)
        {
            try
            {
                // Verificar autenticación
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                var usuarioRol = HttpContext.Session.GetString("UsuarioRol");
                
                if (usuarioId == null || usuarioRol != "Administrador")
                {
                    return Json(new { success = false, message = "No tienes permisos para crear entregas" });
                }

                if (string.IsNullOrWhiteSpace(entregaDto.Nombre) || 
                    string.IsNullOrWhiteSpace(entregaDto.Descripcion))
                {
                    return Json(new { success = false, message = "Nombre y descripción son requeridos" });
                }

                // Verificar que el tipo de anexo es válido si se especificó uno
                if (entregaDto.TipoAnexo.HasValue)
                {
                    if (!new[] { 1, 2, 3, 5, 6, 7, 8 }.Contains(entregaDto.TipoAnexo.Value))
                    {
                        return Json(new { success = false, message = "El tipo de anexo seleccionado no es válido" });
                    }
                }

                // Determinar para qué grupos se creará la entrega
                List<Grupo> gruposDestino;
                
                if (entregaDto.EnviarATodosLosGrupos)
                {
                    // Obtener todos los grupos existentes
                    gruposDestino = await _context.Grupos
                        .Include(g => g.GruposProfesores)
                        .ThenInclude(gp => gp.Profesor)
                        .ToListAsync();
                }
                else
                {
                    // Validar que se especificó un grupo
                    if (!entregaDto.GrupoEspecifico.HasValue)
                    {
                        return Json(new { success = false, message = "Debe especificar un grupo cuando no se envía a todos los grupos" });
                    }
                    
                    // Obtener solo el grupo específico
                    var grupoEspecifico = await _context.Grupos
                        .Include(g => g.GruposProfesores)
                        .ThenInclude(gp => gp.Profesor)
                        .FirstOrDefaultAsync(g => g.Numero == entregaDto.GrupoEspecifico.Value);
                    
                    if (grupoEspecifico == null)
                    {
                        return Json(new { success = false, message = "El grupo especificado no existe" });
                    }
                    
                    gruposDestino = new List<Grupo> { grupoEspecifico };
                }

                if (!gruposDestino.Any())
                {
                    string mensajeError = entregaDto.EnviarATodosLosGrupos 
                        ? "No hay grupos disponibles para crear entregas" 
                        : "El grupo especificado no existe";
                    return Json(new { success = false, message = mensajeError });
                }

                // Crear una entrega por cada grupo destino
                var entregas = new List<Entrega>();
                
                foreach (var grupo in gruposDestino)
                {
                    var entrega = new Entrega
                    {
                        Nombre = entregaDto.Nombre,
                        Descripcion = entregaDto.Descripcion,
                        FechaLimite = entregaDto.FechaLimite,
                        GrupoNumero = grupo.Numero,
                        ProfesorIdentificacion = null, // Sin profesor asignado
                        FormularioIdentificacion = null, // Ya no se usan formularios
                        TipoAnexo = entregaDto.TipoAnexo ?? 0, // Asignar el tipo de anexo si se especificó
                        ArchivoRuta = "", // Se llenará cuando se adjunte archivo
                        Retroalimentacion = "", // Se llenará por el tutor
                        FechaRetroalimentacion = null
                    };
                    
                    entregas.Add(entrega);
                }

                _context.Entregas.AddRange(entregas);
                await _context.SaveChangesAsync();

                // Notificar a estudiantes sobre nueva entrega
                foreach (var entrega in entregas)
                {
                    await _notificacionService.NotificarNuevaEntregaAsync(
                        entrega.GrupoNumero, 
                        entrega.Identificacion, 
                        entrega.Nombre
                    );
                }

                string tipoDestinatario = entregaDto.EnviarATodosLosGrupos ? "todos los grupos" : $"el grupo {entregaDto.GrupoEspecifico}";
                string mensaje = $"Entrega creada exitosamente para {tipoDestinatario} ({entregas.Count} tarea{(entregas.Count > 1 ? "s" : "")} generada{(entregas.Count > 1 ? "s" : "")})";
                
                if (entregaDto.TipoAnexo.HasValue)
                {
                    string nombreAnexo = entregaDto.TipoAnexo.Value switch
                    {
                        1 => "Anexo #1 - Información Básica",
                        2 => "Anexo #2 - Propuesta de Proyecto", 
                        3 => "Anexo #3 - Plan de Trabajo",
                        5 => "Anexo #5 - Evaluación Final",
                        6 => "Anexo #6 - Informe Final Tutor",
                        7 => "Anexo #7 - Carta para Ingresar a la Institución",
                        8 => "Anexo #8 - Carta de Consentimiento Encargado Legal",
                        _ => $"Anexo #{entregaDto.TipoAnexo.Value}"
                    };
                    mensaje += $" con anexo asociado: {nombreAnexo}";
                }

                return Json(new { 
                    success = true, 
                    message = mensaje,
                    cantidadGrupos = entregas.Count
                });
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

                // Verificar que el formulario existe si se especificó uno
                if (entrega.FormularioIdentificacion.HasValue)
                {
                    var formularioExiste = await _context.Formularios
                        .AnyAsync(f => f.Identificacion == entrega.FormularioIdentificacion);
                    
                    if (!formularioExiste)
                    {
                        return Json(new { success = false, message = "El formulario seleccionado no existe" });
                    }
                }

                entregaExistente.Nombre = entrega.Nombre;
                entregaExistente.Descripcion = entrega.Descripcion;
                entregaExistente.FechaLimite = entrega.FechaLimite;
                entregaExistente.ArchivoRuta = entrega.ArchivoRuta ?? "";
                entregaExistente.Retroalimentacion = entrega.Retroalimentacion ?? "";
                entregaExistente.FormularioIdentificacion = entrega.FormularioIdentificacion;
                
                if (entrega.GrupoNumero > 0)
                    entregaExistente.GrupoNumero = entrega.GrupoNumero;

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
                        fechaIngreso = formulario.FechaIngreso
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
                    .Include(e => e.Formulario) // Incluir información del formulario
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
                        formularioIdentificacion = entrega.FormularioIdentificacion,
                        formularioNombre = entrega.Formulario?.Nombre // Incluir nombre del formulario
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al obtener entrega: {ex.Message}" });
            }
        }

        // ===== MÉTODO TEMPORAL PARA CONSULTAR ESTUDIANTES =====
        
        [HttpGet]
        public async Task<IActionResult> ConsultarEstudiantes()
        {
            try
            {
                // await _seeder.ConsultarEstadoEstudiantesAsync();
                Console.WriteLine("Método ConsultarEstadoEstudiantesAsync no disponible temporalmente");
                ViewBag.Message = "Consulta ejecutada. Revisa la consola para ver los resultados.";
                ViewBag.Success = true;
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error en consulta: {ex.Message}";
                ViewBag.Success = false;
            }
            return View("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> CrearEstudiantesSinGrupo()
        {
            try
            {
                // await _seeder.CrearEstudiantesSinGrupoAsync();
                Console.WriteLine("Método CrearEstudiantesSinGrupoAsync no disponible temporalmente");
                ViewBag.Message = "Estudiantes sin grupo creados. Revisa la consola para ver los resultados.";
                ViewBag.Success = true;
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error al crear estudiantes: {ex.Message}";
                ViewBag.Success = false;
            }
            return View("Dashboard");
        }

        /// <summary>
        /// Consultar estado detallado de estudiantes y grupos
        /// </summary>
        public async Task<IActionResult> ConsultarEstadoEstudiantes()
        {
            var dataSeeder = new DataSeederService(_context);
            // await dataSeeder.ConsultarEstadoEstudiantesAsync();
            Console.WriteLine("Método ConsultarEstadoEstudiantesAsync no disponible temporalmente");
            
            ViewBag.Message = "✅ Consulta completada. Revisa la consola para ver los detalles.";
            ViewBag.Success = true;
            return View("Dashboard");
        }

        /// <summary>
        /// Recrear todos los usuarios de prueba
        /// </summary>
        public async Task<IActionResult> RecrearUsuarios()
        {
            var dataSeeder = new DataSeederService(_context);
            // await dataSeeder.ReseedUsuariosAsync();
            await dataSeeder.LimpiarYRegenerarAsync();
            
            ViewBag.Message = "✅ Usuarios recreados exitosamente.";
            ViewBag.Success = true;
            return View("Dashboard");
        }

        /// <summary>
        /// Verificar solicitudes en la base de datos
        /// </summary>
        public async Task<IActionResult> VerificarSolicitudes()
        {
            var solicitudes = await _context.Solicitudes
                .Include(s => s.EstudianteRemitente)
                .Include(s => s.EstudianteDestinatario)
                .Include(s => s.Grupo)
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();

            var result = new StringBuilder();
            result.AppendLine("=== SOLICITUDES EN LA BASE DE DATOS ===\n");
            
            if (!solicitudes.Any())
            {
                result.AppendLine("❌ NO HAY SOLICITUDES EN LA BASE DE DATOS");
            }
            else
            {
                result.AppendLine($"✅ Total de solicitudes: {solicitudes.Count}\n");
                
                foreach (var solicitud in solicitudes)
                {
                    result.AppendLine($"🔸 ID: {solicitud.Id}");
                    result.AppendLine($"   Tipo: {solicitud.Tipo}");
                    result.AppendLine($"   Estado: {solicitud.Estado}");
                    result.AppendLine($"   Remitente: {solicitud.EstudianteRemitente?.Nombre} {solicitud.EstudianteRemitente?.Apellidos} (ID: {solicitud.EstudianteRemitenteId})");
                    result.AppendLine($"   Destinatario: {solicitud.EstudianteDestinatario?.Nombre} {solicitud.EstudianteDestinatario?.Apellidos} (ID: {solicitud.EstudianteDestinatarioId})");
                    result.AppendLine($"   Grupo: {solicitud.GrupoNumero}");
                    result.AppendLine($"   Mensaje: {solicitud.Mensaje}");
                    result.AppendLine($"   Fecha: {solicitud.FechaCreacion:dd/MM/yyyy HH:mm:ss}");
                    result.AppendLine();
                }
            }
            
            ViewBag.Resultado = result.ToString();
            return View("VerificarUsuarios");
        }

        /// <summary>
        /// Verificar información de grupos y sus miembros
        /// </summary>
        public async Task<IActionResult> VerificarGrupos()
        {
            var grupos = await _context.Grupos
                .Include(g => g.GruposEstudiantes)
                    .ThenInclude(ge => ge.Estudiante)
                .OrderBy(g => g.Numero)
                .ToListAsync();

            var result = new StringBuilder();
            result.AppendLine("=== GRUPOS Y SUS MIEMBROS ===\n");
            
            if (!grupos.Any())
            {
                result.AppendLine("❌ NO HAY GRUPOS EN LA BASE DE DATOS");
            }
            else
            {
                result.AppendLine($"✅ Total de grupos: {grupos.Count}\n");
                
                foreach (var grupo in grupos)
                {
                    result.AppendLine($"🏛️ GRUPO {grupo.Numero}");
                    result.AppendLine($"   Miembros: {grupo.GruposEstudiantes.Count}");
                    
                    if (grupo.GruposEstudiantes.Any())
                    {
                        result.AppendLine("   📋 Lista de estudiantes:");
                        foreach (var miembro in grupo.GruposEstudiantes)
                        {
                            result.AppendLine($"      👤 {miembro.Estudiante.Nombre} {miembro.Estudiante.Apellidos} (ID: {miembro.EstudianteIdentificacion})");
                        }
                    }
                    else
                    {
                        result.AppendLine("   ⚠️ Grupo vacío - sin estudiantes");
                    }
                    result.AppendLine();
                }
            }
            
            ViewBag.Resultado = result.ToString();
            return View("VerificarUsuarios");
        }

        [HttpPost]
        public async Task<IActionResult> EliminarEstudianteDeGrupo([FromBody] EliminarEstudianteGrupoRequest request)
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
        public async Task<IActionResult> CambiarLiderGrupo([FromBody] CambiarLiderGrupoRequest request)
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
        public async Task<IActionResult> ObtenerDetallesGrupoCompleto(int grupoNumero)
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

        [HttpPost]
        public async Task<IActionResult> LimpiarYReiniciarGrupos()
        {
            try
            {
                Console.WriteLine("🧹 Iniciando limpieza completa de grupos y solicitudes...");

                // 1. Eliminar todas las solicitudes existentes
                var todasLasSolicitudes = await _context.Solicitudes.ToListAsync();
                _context.Solicitudes.RemoveRange(todasLasSolicitudes);
                Console.WriteLine($"🗑️ Eliminando {todasLasSolicitudes.Count} solicitudes...");

                // 2. Eliminar todas las relaciones GrupoEstudiante
                var todasLasRelaciones = await _context.GruposEstudiantes.ToListAsync();
                _context.GruposEstudiantes.RemoveRange(todasLasRelaciones);
                Console.WriteLine($"🗑️ Eliminando {todasLasRelaciones.Count} relaciones grupo-estudiante...");

                // 3. Eliminar todas las entregas
                var todasLasEntregas = await _context.Entregas.ToListAsync();
                _context.Entregas.RemoveRange(todasLasEntregas);
                Console.WriteLine($"🗑️ Eliminando {todasLasEntregas.Count} entregas...");

                // 4. Eliminar todas las relaciones GrupoProfesor
                var todasLasRelacionesProfesores = await _context.GruposProfesores.ToListAsync();
                _context.GruposProfesores.RemoveRange(todasLasRelacionesProfesores);
                Console.WriteLine($"🗑️ Eliminando {todasLasRelacionesProfesores.Count} relaciones grupo-profesor...");

                // 5. Eliminar todas las notificaciones
                var todasLasNotificaciones = await _context.Notificaciones.ToListAsync();
                _context.Notificaciones.RemoveRange(todasLasNotificaciones);
                Console.WriteLine($"🗑️ Eliminando {todasLasNotificaciones.Count} notificaciones...");

                // 6. Eliminar todos los grupos
                var todosLosGrupos = await _context.Grupos.ToListAsync();
                _context.Grupos.RemoveRange(todosLosGrupos);
                Console.WriteLine($"🗑️ Eliminando {todosLosGrupos.Count} grupos...");

                // Guardar cambios de eliminación
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ Limpieza completada exitosamente");

                // 7. Crear grupos nuevos con líderes
                await CrearGruposConLideresInterno();

                return Json(new { 
                    success = true, 
                    message = "Limpieza completa realizada. Se han creado nuevos grupos con líderes asignados."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error durante la limpieza: {ex.Message}");
                return Json(new { success = false, message = "Error durante la limpieza: " + ex.Message });
            }
        }

        private async Task CrearGruposConLideresInterno()
        {
            Console.WriteLine("🏗️ Iniciando creación de nuevos grupos con líderes...");

            // Obtener todos los estudiantes
            var todosLosEstudiantes = await _context.Estudiantes
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            Console.WriteLine($"👥 Encontrados {todosLosEstudiantes.Count} estudiantes");

            if (!todosLosEstudiantes.Any())
            {
                Console.WriteLine("⚠️ No hay estudiantes para crear grupos");
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
                Console.WriteLine($"🏗️ Creando Grupo {numeroGrupo} con líder: {lider.Nombre} {lider.Apellidos}");

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

                    string rolDescripcion = estudiante.Identificacion == lider.Identificacion ? "👑 LÍDER" : "👤 Miembro";
                    Console.WriteLine($"   - {estudiante.Nombre} {estudiante.Apellidos} ({rolDescripcion})");
                }

                numeroGrupo++;
            }

            // Guardar todos los cambios
            await _context.SaveChangesAsync();
            Console.WriteLine($"✅ Se crearon {numeroGrupo - 1} grupos nuevos con líderes asignados");
        }

        // ===== GESTIÓN DE USUARIOS =====

        public async Task<IActionResult> Usuarios()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .OrderBy(u => u.Rol)
                    .ThenBy(u => u.NombreUsuario)
                    .ToListAsync();

                return View(usuarios);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al cargar usuarios: {ex.Message}";
                return View(new List<Usuario>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> RestablecerContraseña([FromBody] RestablecerContraseñaRequest request)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Identificacion == request.UsuarioId);

                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // Restablecer contraseña a la identificación del usuario
                string nuevaContraseña = usuario.Identificacion.ToString();
                usuario.Contraseña = ServicioComunal.Utilities.PasswordHelper.HashPassword(nuevaContraseña);
                
                // Marcar que requiere cambio de contraseña en el próximo login
                usuario.RequiereCambioContraseña = true;

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Contraseña restablecida. El usuario deberá usar su cédula ({nuevaContraseña}) para ingresar y se le pedirá cambiar la contraseña." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CambiarContraseñaUsuario([FromBody] CambiarContraseñaRequest request)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Identificacion == request.UsuarioId);

                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // Validar la nueva contraseña
                if (string.IsNullOrEmpty(request.NuevaContraseña) || request.NuevaContraseña.Length < 8)
                {
                    return Json(new { success = false, message = "La contraseña debe tener al menos 8 caracteres" });
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(request.NuevaContraseña, @".*\d.*"))
                {
                    return Json(new { success = false, message = "La contraseña debe contener al menos un número" });
                }

                // Hashear la nueva contraseña
                usuario.Contraseña = ServicioComunal.Utilities.PasswordHelper.HashPassword(request.NuevaContraseña);
                usuario.RequiereCambioContraseña = request.ForzarCambio;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Contraseña cambiada exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al cambiar contraseña: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstadoUsuario([FromBody] CambiarEstadoUsuarioRequest request)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Identificacion == request.UsuarioId);

                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                usuario.Activo = request.Activo;
                await _context.SaveChangesAsync();

                string mensaje = request.Activo ? "Usuario activado exitosamente" : "Usuario desactivado exitosamente";
                return Json(new { success = true, message = mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al cambiar estado: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarcarCambioContraseñaRealizado([FromBody] MarcarCambioContraseñaRequest request)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Identificacion == request.UsuarioId);

                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                usuario.RequiereCambioContraseña = false;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Usuario marcado como contraseña cambiada" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // Método auxiliar para generar nombre de usuario en formato primer_nombre.primer_apellido
        private string GenerarNombreUsuario(string nombre, string apellidos)
        {
            try
            {
                // Obtener primer nombre (antes del primer espacio)
                string primerNombre = nombre.Trim().Split(' ')[0].ToLower();
                
                // Obtener primer apellido (antes del primer espacio)
                string primerApellido = apellidos.Trim().Split(' ')[0].ToLower();
                
                // Remover acentos y caracteres especiales
                primerNombre = RemoverAcentos(primerNombre);
                primerApellido = RemoverAcentos(primerApellido);
                
                return $"{primerNombre}.{primerApellido}";
            }
            catch
            {
                // Si hay algún error, usar un formato básico
                return $"{nombre.ToLower()}.{apellidos.ToLower()}".Replace(" ", "");
            }
        }

        // Método auxiliar para remover acentos
        private string RemoverAcentos(string texto)
        {
            return texto
                .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                .Replace("ñ", "n")
                .Replace("ü", "u");
        }

        // MÉTODO TEMPORAL DE DIAGNÓSTICO - ELIMINAR DESPUÉS
        [HttpGet]
        public async Task<IActionResult> DiagnosticarContraseñas()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Where(u => u.NombreUsuario.Contains("elena") || u.Identificacion == 567890123)
                    .ToListAsync();

                var resultado = usuarios.Select(u => new {
                    Identificacion = u.Identificacion,
                    NombreUsuario = u.NombreUsuario,
                    ContraseñaLength = u.Contraseña?.Length ?? 0,
                    ContraseñaInicia = u.Contraseña?.Substring(0, Math.Min(10, u.Contraseña?.Length ?? 0)) + "...",
                    TieneSignoDolar = u.Contraseña?.Contains("$") ?? false,
                    Rol = u.Rol,
                    RequiereCambio = u.RequiereCambioContraseña,
                    // TEST: Verificar si la contraseña actual coincide con la cédula
                    CedulaComoTexto = u.Identificacion.ToString(),
                    VerificacionDirecta = u.Contraseña == u.Identificacion.ToString(),
                    VerificacionHash = PasswordHelper.VerifyPassword(u.Identificacion.ToString(), u.Contraseña ?? "")
                }).ToList();

                return Json(resultado);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // MÉTODO TEMPORAL PARA SINCRONIZAR ROLES - ELIMINAR DESPUÉS
        [HttpGet]
        public async Task<IActionResult> SincronizarRoles()
        {
            try
            {
                var sincronizados = 0;
                var problemas = new List<string>();
                
                // Obtener todos los usuarios con sus profesores correspondientes
                var usuariosConProfesores = await _context.Usuarios
                    .Join(_context.Profesores, 
                          u => u.Identificacion, 
                          p => p.Identificacion, 
                          (u, p) => new { Usuario = u, Profesor = p })
                    .ToListAsync();

                foreach (var item in usuariosConProfesores)
                {
                    // Si el usuario es Administrador pero el profesor no tiene el rol correcto
                    if (item.Usuario.Rol == "Administrador" && item.Profesor.Rol != "Administrador")
                    {
                        item.Profesor.Rol = "Administrador";
                        sincronizados++;
                        problemas.Add($"✅ {item.Profesor.Nombre} {item.Profesor.Apellidos}: PROFESOR actualizado a Administrador");
                    }
                    // Si el profesor es Administrador pero no existe usuario o el usuario no es Administrador
                    else if (item.Profesor.Rol == "Administrador" && item.Usuario.Rol != "Administrador")
                    {
                        item.Usuario.Rol = "Administrador";
                        sincronizados++;
                        problemas.Add($"✅ {item.Profesor.Nombre} {item.Profesor.Apellidos}: USUARIO actualizado a Administrador");
                    }
                    // Si ambos son profesores/tutores, asegurar consistencia
                    else if ((item.Usuario.Rol == "Profesor" || item.Usuario.Rol == "Tutor") && 
                             (item.Profesor.Rol == "Profesor" || item.Profesor.Rol == "Tutor"))
                    {
                        // Normalizar a "Tutor" para profesores no-administradores
                        if (item.Profesor.Rol != "Tutor")
                        {
                            item.Profesor.Rol = "Tutor";
                            sincronizados++;
                            problemas.Add($"✅ {item.Profesor.Nombre} {item.Profesor.Apellidos}: PROFESOR normalizado a Tutor");
                        }
                        if (item.Usuario.Rol != "Profesor")
                        {
                            item.Usuario.Rol = "Profesor";
                            sincronizados++;
                            problemas.Add($"✅ {item.Profesor.Nombre} {item.Profesor.Apellidos}: USUARIO normalizado a Profesor");
                        }
                    }
                }

                // También verificar profesores administradores sin usuario
                var profesoresAdmin = await _context.Profesores
                    .Where(p => p.Rol == "Administrador")
                    .ToListAsync();

                foreach (var profesor in profesoresAdmin)
                {
                    var usuario = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.Identificacion == profesor.Identificacion);
                    
                    if (usuario == null)
                    {
                        // Crear usuario para administrador
                        string nombreUsuario = GenerarNombreUsuario(profesor.Nombre, profesor.Apellidos);
                        var nuevoUsuario = new ServicioComunal.Models.Usuario
                        {
                            Identificacion = profesor.Identificacion,
                            NombreUsuario = nombreUsuario,
                            Contraseña = PasswordHelper.HashPassword(profesor.Identificacion.ToString()),
                            Rol = "Administrador",
                            RequiereCambioContraseña = true,
                            FechaCreacion = DateTime.Now,
                            Activo = true
                        };
                        _context.Usuarios.Add(nuevoUsuario);
                        sincronizados++;
                        problemas.Add($"🆕 {profesor.Nombre} {profesor.Apellidos}: USUARIO creado para administrador");
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Roles sincronizados: {sincronizados}",
                    detalles = problemas
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    error = ex.Message 
                });
            }
        }

        // MÉTODO TEMPORAL PARA CORREGIR CONTRASEÑAS - ELIMINAR DESPUÉS
        [HttpGet]
        public async Task<IActionResult> CorregirContraseñas()
        {
            try
            {
                var usuarios = await _context.Usuarios.ToListAsync();
                int corregidos = 0;

                foreach (var usuario in usuarios)
                {
                    // Si la contraseña es igual a la cédula (texto plano) o no tiene el formato hash correcto
                    if (usuario.Contraseña == usuario.Identificacion.ToString() || 
                        string.IsNullOrEmpty(usuario.Contraseña) || 
                        !usuario.Contraseña.Contains("$"))
                    {
                        // Hashear la cédula como contraseña temporal
                        usuario.Contraseña = PasswordHelper.HashPassword(usuario.Identificacion.ToString());
                        usuario.RequiereCambioContraseña = true;
                        corregidos++;
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Contraseñas corregidas: {corregidos}",
                    totalUsuarios = usuarios.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    error = ex.Message 
                });
            }
        }

        // ===== NUEVAS FUNCIONES MOVIDAS DESDE ADMIN =====

        // API para obtener entregas con filtros
        [HttpGet]
        public async Task<IActionResult> ObtenerEntregasAdmin()
        {
            try
            {
                var entregas = await _context.Entregas
                    .Include(e => e.Grupo)
                        .ThenInclude(g => g.GruposEstudiantes)
                        .ThenInclude(ge => ge.Estudiante)
                    .Include(e => e.Profesor)
                    .Include(e => e.Formulario)
                    .OrderByDescending(e => e.FechaLimite)
                    .ToListAsync();

                var resultado = entregas.Select(e => new
                {
                    e.Identificacion,
                    e.Nombre,
                    e.Descripcion,
                    e.FechaLimite,
                    e.FechaEntrega,
                    e.TipoAnexo,
                    TieneAnexo = e.TipoAnexo > 0,
                    Estado = GetEstadoEntregaAdmin(e),
                    Grupo = new
                    {
                        Numero = e.Grupo.Numero,
                        CantidadEstudiantes = e.Grupo.GruposEstudiantes.Count,
                        Estudiantes = e.Grupo.GruposEstudiantes.Select(ge => $"{ge.Estudiante.Nombre} {ge.Estudiante.Apellidos}").ToList()
                    },
                    Profesor = e.Profesor != null ? $"{e.Profesor.Nombre} {e.Profesor.Apellidos}" : "Sin asignar",
                    Formulario = e.Formulario != null ? e.Formulario.Nombre : "Sin formulario",
                    TieneEntrega = !string.IsNullOrEmpty(e.ArchivoRuta)
                }).ToList();

                return Json(new { success = true, entregas = resultado });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cargar entregas: " + ex.Message });
            }
        }

        private string GetEstadoEntregaAdmin(Entrega entrega)
        {
            if (!string.IsNullOrEmpty(entrega.Retroalimentacion))
            {
                return entrega.Retroalimentacion.StartsWith("CAMBIOS SOLICITADOS:") ? "Requiere Cambios" : "Revisada";
            }
            else if (!string.IsNullOrEmpty(entrega.ArchivoRuta))
            {
                return "Enviada";
            }
            else if (entrega.FechaLimite < DateTime.Now)
            {
                return "Vencida";
            }
            else
            {
                return "Pendiente";
            }
        }

        // Crear nueva entrega con todos los grupos
        [HttpPost]
        public async Task<IActionResult> CrearEntregaAdmin([FromBody] CrearEntregaAdminRequest request)
        {
            try
            {
                var entrega = new Entrega
                {
                    Nombre = request.Nombre,
                    Descripcion = request.Descripcion,
                    FechaLimite = request.FechaLimite,
                    GrupoNumero = request.GrupoNumero,
                    ProfesorIdentificacion = request.ProfesorId > 0 ? request.ProfesorId : null,
                    FormularioIdentificacion = request.FormularioId > 0 ? request.FormularioId : null,
                    TipoAnexo = request.TipoAnexo,
                    ArchivoRuta = string.Empty,
                    Retroalimentacion = string.Empty
                };

                _context.Entregas.Add(entrega);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Entrega creada exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al crear entrega: " + ex.Message });
            }
        }

        // Eliminar entrega específica
        [HttpPost]
        public async Task<IActionResult> EliminarEntregaAdmin([FromBody] EliminarEntregaAdminRequest request)
        {
            try
            {
                var entrega = await _context.Entregas.FindAsync(request.EntregaId);
                if (entrega == null)
                {
                    return Json(new { success = false, message = "Entrega no encontrada" });
                }

                // Eliminar archivo si existe
                if (!string.IsNullOrEmpty(entrega.ArchivoRuta))
                {
                    var rutaCompleta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", entrega.ArchivoRuta);
                    if (System.IO.File.Exists(rutaCompleta))
                    {
                        System.IO.File.Delete(rutaCompleta);
                    }
                }

                _context.Entregas.Remove(entrega);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Entrega eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar entrega: " + ex.Message });
            }
        }

        // Obtener formularios disponibles
        [HttpGet]
        public IActionResult ObtenerAnexosDisponibles()
        {
            var formularios = new[]
            {
                new { Id = 1, Nombre = "Anexo #1 - Información Básica", Archivo = "Anexo #1 Interactivo.pdf" },
                new { Id = 2, Nombre = "Anexo #2 - Propuesta de Proyecto", Archivo = "Anexo #2 Interactivo.pdf" },
                new { Id = 3, Nombre = "Anexo #3 - Plan de Trabajo", Archivo = "Anexo #3 Interactivo.pdf" },
                new { Id = 5, Nombre = "Anexo #5 - Evaluación Final", Archivo = "Anexo #5 Interactivo.pdf" },
                new { Id = 6, Nombre = "Informe Final Tutor", Archivo = "Informe final tutor Interactiva.pdf" },
                new { Id = 7, Nombre = "Carta para Ingresar a la Institución", Archivo = "Carta para ingresar a la institucion interactiva.pdf" },
                new { Id = 8, Nombre = "Carta de Consentimiento Encargado Legal", Archivo = "Carta de consentimiento encargado legal Interactiva.pdf" }
            };

            var formulariosConInfo = formularios.Select(f => new
            {
                f.Id,
                f.Nombre,
                f.Archivo,
                Existe = System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "formularios", f.Archivo)),
                RutaDescarga = $"/uploads/formularios/{Uri.EscapeDataString(f.Archivo)}"
            }).ToList();

            return Json(new { success = true, anexos = formulariosConInfo });
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

        [HttpGet]
        public async Task<IActionResult> UsuariosSinContraseñaNueva()
        {
            try
            {
                var usuariosSinCambio = await _context.Usuarios
                    .Where(u => u.RequiereCambioContraseña == true)
                    .OrderBy(u => u.Rol)
                    .ThenBy(u => u.NombreUsuario)
                    .ToListAsync();

                var resultado = usuariosSinCambio.Select(u => new {
                    Identificacion = u.Identificacion,
                    NombreUsuario = u.NombreUsuario,
                    Rol = u.Rol,
                    FechaCreacion = u.FechaCreacion.ToString("dd/MM/yyyy"),
                    UltimoAcceso = u.UltimoAcceso?.ToString("dd/MM/yyyy HH:mm") ?? "Nunca",
                    ContraseñaInicial = u.Identificacion.ToString(), // La contraseña inicial es la cédula
                    Activo = u.Activo
                }).ToList();

                var resumen = new
                {
                    TotalUsuarios = await _context.Usuarios.CountAsync(),
                    UsuariosSinCambioContraseña = usuariosSinCambio.Count,
                    UsuariosConContraseñaNueva = await _context.Usuarios.CountAsync(u => u.RequiereCambioContraseña == false),
                    Usuarios = resultado
                };

                return Json(resumen);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = "Error al consultar usuarios: " + ex.Message 
                });
            }
        }

        // Endpoint temporal para regenerar datos
        public async Task<IActionResult> RegenerarDatos()
        {
            try
            {
                await _seeder.LimpiarYRegenerarAsync();
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
            var usuarios = await _context.Usuarios
                .OrderBy(u => u.Rol)
                .ThenBy(u => u.NombreUsuario)
                .ToListAsync();
            
            var resultado = usuarios.Select(u => new {
                u.Identificacion,
                u.NombreUsuario,
                u.Rol,
                u.Activo,
                u.FechaCreacion
            }).ToList();
            
            return Json(new { success = true, usuarios = resultado });
        }
    }

    // Clases para requests
    public class CambiarContraseñaRequest
    {
        public int UsuarioId { get; set; }
        public string NuevaContraseña { get; set; } = string.Empty;
        public bool ForzarCambio { get; set; }
    }

    public class RestablecerContraseñaRequest
    {
        public int UsuarioId { get; set; }
    }

    public class CambiarEstadoUsuarioRequest
    {
        public int UsuarioId { get; set; }
        public bool Activo { get; set; }
    }

    public class MarcarCambioContraseñaRequest
    {
        public int UsuarioId { get; set; }
    }

    public class EliminarEstudianteGrupoRequest
    {
        public int GrupoNumero { get; set; }
        public int EstudianteId { get; set; }
    }

    public class CambiarLiderGrupoRequest
    {
        public int GrupoNumero { get; set; }
        public int NuevoLiderId { get; set; }
    }

    public class CrearEntregaAdminRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaLimite { get; set; }
        public int GrupoNumero { get; set; }
        public int ProfesorId { get; set; }
        public int FormularioId { get; set; }
        public int TipoAnexo { get; set; }
    }

    public class EliminarEntregaAdminRequest
    {
        public int EntregaId { get; set; }
    }
}
