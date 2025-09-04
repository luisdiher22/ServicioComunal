using ServicioComunal.Models;
using System.Text.Json;

namespace ServicioComunal.Services
{
    public interface IPdfFillerService
    {
        Task<byte[]> FillPdfAsync(int tipoAnexo, object formData);
        Task<string> SaveFilledPdfAsync(int entregaId, int tipoAnexo, object formData);
    }

    public class PdfFillerService : IPdfFillerService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PdfFillerService> _logger;

        public PdfFillerService(IWebHostEnvironment environment, ILogger<PdfFillerService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<byte[]> FillPdfAsync(int tipoAnexo, object formData)
        {
            try
            {
                var templatePath = GetTemplatePath(tipoAnexo);
                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException($"Template PDF no encontrado: {templatePath}");
                }

                _logger.LogInformation("Procesando formulario tipo {TipoAnexo} con datos: {FormData}", 
                    tipoAnexo, JsonSerializer.Serialize(formData));

                // Generar script de Node.js para llenar el PDF
                var formDataJson = JsonSerializer.Serialize(formData, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true 
                });
                
                var scriptContent = GenerateFillScript(tipoAnexo, templatePath, formDataJson);
                var tempScriptPath = Path.Combine(_environment.ContentRootPath, $"temp_fill_script_{Guid.NewGuid()}.mjs");
                var tempOutputPath = Path.Combine(_environment.ContentRootPath, $"temp_output_{Guid.NewGuid()}.pdf");
                
                _logger.LogInformation("Creando script temporal en: {ScriptPath}", tempScriptPath);
                _logger.LogInformation("PDF de salida será: {OutputPath}", tempOutputPath);
                
                // Modificar el script para incluir la ruta de salida correcta
                scriptContent = scriptContent.Replace("'./temp_output.pdf'", $"'{Path.GetFileName(tempOutputPath)}'");
                
                await File.WriteAllTextAsync(tempScriptPath, scriptContent);
                _logger.LogInformation("Script escrito exitosamente");
                
                try
                {
                    // Ejecutar Node.js desde el directorio del proyecto
                    var processInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "node",
                        Arguments = Path.GetFileName(tempScriptPath),
                        WorkingDirectory = _environment.ContentRootPath,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    
                    _logger.LogInformation("Ejecutando Node.js desde: {WorkingDir}", _environment.ContentRootPath);
                    _logger.LogInformation("Comando: node {Script}", Path.GetFileName(tempScriptPath));
                    
                    using var process = System.Diagnostics.Process.Start(processInfo);
                    if (process == null)
                    {
                        throw new InvalidOperationException("No se pudo iniciar el proceso de Node.js");
                    }
                    
                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();
                    
                    await process.WaitForExitAsync();
                    
                    _logger.LogInformation("Node.js output: {Output}", output);
                    if (!string.IsNullOrEmpty(error))
                    {
                        _logger.LogWarning("Node.js stderr: {Error}", error);
                    }
                    
                    if (process.ExitCode != 0)
                    {
                        _logger.LogError("Error ejecutando Node.js. Exit code: {ExitCode}", process.ExitCode);
                        throw new InvalidOperationException($"Error procesando PDF con Node.js. Exit code: {process.ExitCode}");
                    }
                    
                    _logger.LogInformation("Node.js ejecutado exitosamente");
                    
                    // Leer el PDF generado
                    if (!File.Exists(tempOutputPath))
                    {
                        _logger.LogError("PDF generado no encontrado en: {OutputPath}", tempOutputPath);
                        // Listar archivos en el directorio para debug
                        var files = Directory.GetFiles(_environment.ContentRootPath, "*.pdf");
                        _logger.LogInformation("PDFs encontrados en directorio: {Files}", string.Join(", ", files.Select(Path.GetFileName)));
                        throw new FileNotFoundException($"PDF generado no encontrado en: {tempOutputPath}");
                    }
                    
                    var pdfBytes = await File.ReadAllBytesAsync(tempOutputPath);
                    _logger.LogInformation("PDF llenado exitosamente, tamaño: {Size} bytes", pdfBytes.Length);
                    
                    return pdfBytes;
                }
                finally
                {
                    // Limpiar archivos temporales
                    try
                    {
                        if (File.Exists(tempScriptPath)) 
                        {
                            File.Delete(tempScriptPath);
                            _logger.LogInformation("Script temporal eliminado");
                        }
                        if (File.Exists(tempOutputPath)) 
                        {
                            File.Delete(tempOutputPath);
                            _logger.LogInformation("PDF temporal eliminado");
                        }
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogWarning(cleanupEx, "Error limpiando archivos temporales");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error llenando PDF tipo {TipoAnexo}", tipoAnexo);
                throw;
            }
        }

        public async Task<string> SaveFilledPdfAsync(int entregaId, int tipoAnexo, object formData)
        {
            try
            {
                _logger.LogInformation("=== INICIO SaveFilledPdfAsync - EntregaId: {EntregaId}, TipoAnexo: {TipoAnexo} ===", entregaId, tipoAnexo);
                
                var pdfBytes = await FillPdfAsync(tipoAnexo, formData);
                
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "entregas");
                Directory.CreateDirectory(uploadsPath);
                
                _logger.LogInformation("Directorio de uploads creado/verificado: {UploadsPath}", uploadsPath);
                
                var fileName = $"entrega_{entregaId}_anexo_{tipoAnexo}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = Path.Combine(uploadsPath, fileName);
                
                await File.WriteAllBytesAsync(filePath, pdfBytes);
                
                _logger.LogInformation("PDF guardado exitosamente en: {FilePath}", filePath);
                
                return Path.Combine("uploads", "entregas", fileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando PDF para entrega {EntregaId}, anexo tipo {TipoAnexo}", entregaId, tipoAnexo);
                throw;
            }
        }

        private string GetTemplatePath(int tipoAnexo)
        {
            var templatesPath = Path.Combine(_environment.WebRootPath, "uploads", "formularios");
            return tipoAnexo switch
            {
                1 => Path.Combine(templatesPath, "Anexo #1 Interactivo.pdf"),
                2 => Path.Combine(templatesPath, "Anexo #2 Interactivo.pdf"),
                3 => Path.Combine(templatesPath, "Anexo #3 Interactivo.pdf"),
                5 => Path.Combine(templatesPath, "Anexo #5 Interactivo.pdf"),
                _ => throw new ArgumentException($"Tipo de anexo no válido: {tipoAnexo}")
            };
        }

        private string GenerateFillScript(int tipoAnexo, string templatePath, string formDataJson)
        {
            // Convertir ruta absoluta a relativa desde el directorio del proyecto
            var relativePath = Path.GetRelativePath(_environment.ContentRootPath, templatePath);
            
            return $@"
import {{ PDFDocument }} from 'pdf-lib';
import fs from 'fs';

async function fillPDF() {{
    try {{
        console.log('=== INICIO LLENADO PDF ===');
        const formData = {formDataJson};
        console.log('Datos del formulario:', JSON.stringify(formData, null, 2));
        
        const templatePath = '{relativePath.Replace("\\", "/")}';
        console.log('Leyendo template desde:', templatePath);
        
        if (!fs.existsSync(templatePath)) {{
            throw new Error(`Template no encontrado: ${{templatePath}}`);
        }}
        
        const existingPdfBytes = fs.readFileSync(templatePath);
        console.log('Template leído exitosamente, tamaño:', existingPdfBytes.length, 'bytes');
        
        const pdfDoc = await PDFDocument.load(existingPdfBytes);
        console.log('PDF cargado exitosamente');
        
        const form = pdfDoc.getForm();
        console.log('Formulario obtenido');
        
        // Mostrar campos disponibles
        const fields = form.getFields();
        console.log('Campos disponibles:', fields.length);
        fields.forEach((field, index) => {{
            console.log(`  Campo ${{index}}: ${{field.getName()}} (${{field.constructor.name}})`);
        }});

        {GenerateFieldMappingCode(tipoAnexo)}

        console.log('Guardando PDF...');
        const pdfBytes = await pdfDoc.save();
        fs.writeFileSync('./temp_output.pdf', pdfBytes);
        console.log('PDF guardado exitosamente como temp_output.pdf');
        console.log('=== FIN LLENADO PDF ===');
    }} catch (error) {{
        console.error('Error en llenado de PDF:', error);
        process.exit(1);
    }}
}}

fillPDF();
";
        }

        private string GenerateFieldMappingCode(int tipoAnexo)
        {
            return tipoAnexo switch
            {
                1 => @"
        // Anexo 1 - Mapeo correcto de campos
        console.log('Datos recibidos:', JSON.stringify(formData, null, 2));
        
        try {
            if (formData.seccion1) {
                const field = form.getTextField('SECCIÓN1');
                field.setText(formData.seccion1);
                console.log('Campo SECCIÓN1 llenado con:', formData.seccion1);
            }
        } catch (e) { console.log('Error en SECCIÓN1:', e.message); }
        
        try {
            if (formData.seccion2) {
                const field = form.getTextField('SECCIÓN2');
                field.setText(formData.seccion2);
                console.log('Campo SECCIÓN2 llenado con:', formData.seccion2);
            }
        } catch (e) { console.log('Error en SECCIÓN2:', e.message); }
        
        try {
            if (formData.seccion3) {
                const field = form.getTextField('SECCIÓN3');
                field.setText(formData.seccion3);
                console.log('Campo SECCIÓN3 llenado con:', formData.seccion3);
            }
        } catch (e) { console.log('Error en SECCIÓN3:', e.message); }
        
        try {
            if (formData.seccion4) {
                const field = form.getTextField('SECCIÓN4');
                field.setText(formData.seccion4);
                console.log('Campo SECCIÓN4 llenado con:', formData.seccion4);
            }
        } catch (e) { console.log('Error en SECCIÓN4:', e.message); }
        
        try {
            if (formData.estudiante1) {
                const field = form.getTextField('ESTUDIANTE1');
                field.setText(formData.estudiante1);
                console.log('Campo ESTUDIANTE1 llenado con:', formData.estudiante1);
            }
        } catch (e) { console.log('Error en ESTUDIANTE1:', e.message); }
        
        try {
            if (formData.estudiante2) {
                const field = form.getTextField('ESTUDIANTE2');
                field.setText(formData.estudiante2);
                console.log('Campo ESTUDIANTE2 llenado con:', formData.estudiante2);
            }
        } catch (e) { console.log('Error en ESTUDIANTE2:', e.message); }
        
        try {
            if (formData.estudiante3) {
                const field = form.getTextField('ESTUDIANTE3');
                field.setText(formData.estudiante3);
                console.log('Campo ESTUDIANTE3 llenado con:', formData.estudiante3);
            }
        } catch (e) { console.log('Error en ESTUDIANTE3:', e.message); }
        
        try {
            if (formData.estudiante4) {
                const field = form.getTextField('ESTUDIANTE4');
                field.setText(formData.estudiante4);
                console.log('Campo ESTUDIANTE4 llenado con:', formData.estudiante4);
            }
        } catch (e) { console.log('Error en ESTUDIANTE4:', e.message); }
        
        try {
            if (formData.nombreProyecto) {
                const field = form.getTextField('NOMBREPROYECTO');
                field.setText(formData.nombreProyecto);
                console.log('Campo NOMBREPROYECTO llenado con:', formData.nombreProyecto);
            }
        } catch (e) { console.log('Error en NOMBREPROYECTO:', e.message); }
        ",

                2 => @"
        // Anexo 2
        if (formData.NombreProyecto) form.getTextField('NOMBREPROYECTO').setText(formData.NombreProyecto);
        if (formData.Tutor) form.getTextField('TUTOR').setText(formData.Tutor);
        if (formData.Nombre1) form.getTextField('NOMBRE1').setText(formData.Nombre1);
        if (formData.Nombre2) form.getTextField('NOMBRE2').setText(formData.Nombre2);
        if (formData.Nombre3) form.getTextField('NOMBRE3').setText(formData.Nombre3);
        if (formData.Nombre4) form.getTextField('NOMBRE4').setText(formData.Nombre4);
        if (formData.Seccion1) form.getTextField('SECCION1').setText(formData.Seccion1);
        if (formData.Seccion2) form.getTextField('SECCION2').setText(formData.Seccion2);
        if (formData.Seccion3) form.getTextField('SECCION3').setText(formData.Seccion3);
        if (formData.Seccion4) form.getTextField('SECCION4').setText(formData.Seccion4);
        if (formData.MedioAmbiente) form.getCheckBox('CBMEDIOAMBIENTE').check();
        if (formData.JovenesApoyandoJovenes) form.getCheckBox('CBJOVENESAPOYANDOJOVENES').check();
        if (formData.InformacionEstudiantil) form.getCheckBox('CBINFORMACIONESTUDIANTIL').check();
        if (formData.Arte) form.getCheckBox('CBARTE').check();
        if (formData.EstilosVidaSaludables) form.getCheckBox('CBESTILOSDEVIDASALUDABLES').check();
        if (formData.Otros) form.getCheckBox('CBOTROS').check();
        if (formData.OtrosEspecifique) form.getTextField('OTROSESPECIFIQUE').setText(formData.OtrosEspecifique);
        if (formData.BreveDescripcion) form.getTextField('BREVEDESCRIPCION').setText(formData.BreveDescripcion);
        ",

                3 => @"
        // Anexo 3 - Solo algunos campos principales para el ejemplo
        if (formData.FirmaTutor) form.getTextField('FirmaTutor').setText(formData.FirmaTutor);
        if (formData.Tutor) form.getTextField('Tutor').setText(formData.Tutor);
        if (formData.NombreEstudiante1) form.getTextField('NombreEstudiante1').setText(formData.NombreEstudiante1);
        if (formData.Seccion01) form.getTextField('Seccion01').setText(formData.Seccion01);
        if (formData.Telefono) form.getTextField('Telefono').setText(formData.Telefono);
        if (formData.NombreDelProyecto) form.getTextField('NombredelProyecto').setText(formData.NombreDelProyecto);
        if (formData.Provincia) form.getTextField('Provincia').setText(formData.Provincia);
        if (formData.Canton) form.getTextField('Canton').setText(formData.Canton);
        if (formData.Distrito) form.getTextField('Distrito').setText(formData.Distrito);
        // ... más campos según sea necesario
        ",

                5 => @"
        // Anexo 5
        if (formData.Tutor) form.getTextField('Tutor').setText(formData.Tutor);
        if (formData.NombreProyecto) form.getTextField('NombreProyecto').setText(formData.NombreProyecto);
        if (formData.NombreEstudiante1) form.getTextField('NombreEstudiante1').setText(formData.NombreEstudiante1);
        if (formData.NombreEstudiante2) form.getTextField('NombreEstudiante2').setText(formData.NombreEstudiante2);
        if (formData.NombreEstudiante3) form.getTextField('NombreEstudiante3').setText(formData.NombreEstudiante3);
        if (formData.NombreEstudiante4) form.getTextField('NombreEstudiante4').setText(formData.NombreEstudiante4);
        if (formData.Seccion1) form.getTextField('Seccion1').setText(formData.Seccion1);
        if (formData.Seccion2) form.getTextField('Seccion2').setText(formData.Seccion2);
        if (formData.Seccion3) form.getTextField('Seccion3').setText(formData.Seccion3);
        if (formData.Seccion4) form.getTextField('Seccion4').setText(formData.Seccion4);
        if (formData.ResumenResultadosObjetivosMetas) form.getTextField('ResumenResultadosObjetivosMetas').setText(formData.ResumenResultadosObjetivosMetas);
        if (formData.ProblemasLimitacionesEncontradas) form.getTextField('ProblemasLimitacionesEncontradas').setText(formData.ProblemasLimitacionesEncontradas);
        if (formData.RecursosFacilidadesEncontradas) form.getTextField('RecursosFacilidadesEncontradas').setText(formData.RecursosFacilidadesEncontradas);
        if (formData.Sugerencias) form.getTextField('Sugerencias').setText(formData.Sugerencias);
        if (formData.Observaciones) form.getTextField('Observaciones').setText(formData.Observaciones);
        ",

                _ => throw new ArgumentException($"Tipo de anexo no válido: {tipoAnexo}")
            };
        }
    }
}
