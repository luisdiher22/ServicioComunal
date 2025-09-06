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
                6 => Path.Combine(templatesPath, "Informe final tutor Interactiva.pdf"),
                7 => Path.Combine(templatesPath, "Carta para ingresar a la institucion interactiva.pdf"),
                8 => Path.Combine(templatesPath, "Carta de consentimiento encargado legal Interactiva.pdf"),
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
        // Anexo 2 - Mapeo de campos
        console.log('Procesando Anexo 2...');
        
        try {
            if (formData.nombreProyecto) {
                const field = form.getTextField('NOMBREPROYECTO');
                field.setText(formData.nombreProyecto);
                console.log('Campo NOMBREPROYECTO llenado con:', formData.nombreProyecto);
            }
        } catch (e) { console.log('Error en NOMBREPROYECTO:', e.message); }
        
        try {
            if (formData.tutor) {
                const field = form.getTextField('TUTOR');
                field.setText(formData.tutor);
                console.log('Campo TUTOR llenado con:', formData.tutor);
            }
        } catch (e) { console.log('Error en TUTOR:', e.message); }
        
        try {
            if (formData.nombre1) {
                const field = form.getTextField('NOMBRE1');
                field.setText(formData.nombre1);
                console.log('Campo NOMBRE1 llenado con:', formData.nombre1);
            }
        } catch (e) { console.log('Error en NOMBRE1:', e.message); }
        
        try {
            if (formData.nombre2) {
                const field = form.getTextField('NOMBRE2');
                field.setText(formData.nombre2);
                console.log('Campo NOMBRE2 llenado con:', formData.nombre2);
            }
        } catch (e) { console.log('Error en NOMBRE2:', e.message); }
        
        try {
            if (formData.nombre3) {
                const field = form.getTextField('NOMBRE3');
                field.setText(formData.nombre3);
                console.log('Campo NOMBRE3 llenado con:', formData.nombre3);
            }
        } catch (e) { console.log('Error en NOMBRE3:', e.message); }
        
        try {
            if (formData.nombre4) {
                const field = form.getTextField('NOMBRE4');
                field.setText(formData.nombre4);
                console.log('Campo NOMBRE4 llenado con:', formData.nombre4);
            }
        } catch (e) { console.log('Error en NOMBRE4:', e.message); }
        
        try {
            if (formData.seccion1) {
                const field = form.getTextField('SECCION1');
                field.setText(formData.seccion1);
                console.log('Campo SECCION1 llenado con:', formData.seccion1);
            }
        } catch (e) { console.log('Error en SECCION1:', e.message); }
        
        try {
            if (formData.seccion2) {
                const field = form.getTextField('SECCION2');
                field.setText(formData.seccion2);
                console.log('Campo SECCION2 llenado con:', formData.seccion2);
            }
        } catch (e) { console.log('Error en SECCION2:', e.message); }
        
        try {
            if (formData.seccion3) {
                const field = form.getTextField('SECCION3');
                field.setText(formData.seccion3);
                console.log('Campo SECCION3 llenado con:', formData.seccion3);
            }
        } catch (e) { console.log('Error en SECCION3:', e.message); }
        
        try {
            if (formData.seccion4) {
                const field = form.getTextField('SECCION4');
                field.setText(formData.seccion4);
                console.log('Campo SECCION4 llenado con:', formData.seccion4);
            }
        } catch (e) { console.log('Error en SECCION4:', e.message); }
        
        // Checkboxes - Corregidos para usar camelCase
        try {
            if (formData.medioAmbiente) {
                const field = form.getCheckBox('CBMEDIOAMBIENTE');
                field.check();
                console.log('Checkbox CBMEDIOAMBIENTE marcado');
            }
        } catch (e) { console.log('Error en CBMEDIOAMBIENTE:', e.message); }
        
        try {
            if (formData.jovenesApoyandoJovenes) {
                const field = form.getCheckBox('CBJOVENESAPOYANDOJOVENES');
                field.check();
                console.log('Checkbox CBJOVENESAPOYANDOJOVENES marcado');
            }
        } catch (e) { console.log('Error en CBJOVENESAPOYANDOJOVENES:', e.message); }
        
        try {
            if (formData.informacionEstudiantil) {
                const field = form.getCheckBox('CBINFORMACIONESTUDIANTIL');
                field.check();
                console.log('Checkbox CBINFORMACIONESTUDIANTIL marcado');
            }
        } catch (e) { console.log('Error en CBINFORMACIONESTUDIANTIL:', e.message); }
        
        try {
            if (formData.arte) {
                const field = form.getCheckBox('CBARTE');
                field.check();
                console.log('Checkbox CBARTE marcado');
            }
        } catch (e) { console.log('Error en CBARTE:', e.message); }
        
        try {
            if (formData.estilosVidaSaludables) {
                const field = form.getCheckBox('CBESTILOSDEVIDASALUDABLES');
                field.check();
                console.log('Checkbox CBESTILOSDEVIDASALUDABLES marcado');
            }
        } catch (e) { console.log('Error en CBESTILOSDEVIDASALUDABLES:', e.message); }
        
        try {
            if (formData.otros) {
                const field = form.getCheckBox('CBOTROS');
                field.check();
                console.log('Checkbox CBOTROS marcado');
            }
        } catch (e) { console.log('Error en CBOTROS:', e.message); }
        
        try {
            if (formData.otrosEspecifique) {
                const field = form.getTextField('OTROSESPECIFIQUE');
                field.setText(formData.otrosEspecifique);
                console.log('Campo OTROSESPECIFIQUE llenado con:', formData.otrosEspecifique);
            }
        } catch (e) { console.log('Error en OTROSESPECIFIQUE:', e.message); }
        
        try {
            if (formData.breveDescripcion) {
                const field = form.getTextField('BREVEDESCRIPCION');
                field.setText(formData.breveDescripcion);
                console.log('Campo BREVEDESCRIPCION llenado con:', formData.breveDescripcion);
            }
        } catch (e) { console.log('Error en BREVEDESCRIPCION:', e.message); }
        ",

                3 => @"
        // Anexo 3 - Mapeo de campos principales
        console.log('Procesando Anexo 3...');
        
        try {
            if (formData.firmaTutor) {
                const field = form.getTextField('FirmaTutor');
                field.setText(formData.firmaTutor);
                console.log('Campo FirmaTutor llenado con:', formData.firmaTutor);
            }
        } catch (e) { console.log('Error en FirmaTutor:', e.message); }
        
        try {
            if (formData.tutor) {
                const field = form.getTextField('Tutor');
                field.setText(formData.tutor);
                console.log('Campo Tutor llenado con:', formData.tutor);
            }
        } catch (e) { console.log('Error en Tutor:', e.message); }
        
        try {
            if (formData.nombreEstudiante1) {
                const field = form.getTextField('NombreEstudiante1');
                field.setText(formData.nombreEstudiante1);
                console.log('Campo NombreEstudiante1 llenado con:', formData.nombreEstudiante1);
            }
        } catch (e) { console.log('Error en NombreEstudiante1:', e.message); }
        
        try {
            if (formData.seccion01) {
                const field = form.getTextField('Seccion01');
                field.setText(formData.seccion01);
                console.log('Campo Seccion01 llenado con:', formData.seccion01);
            }
        } catch (e) { console.log('Error en Seccion01:', e.message); }
        
        try {
            if (formData.telefono) {
                const field = form.getTextField('Telefono');
                field.setText(formData.telefono);
                console.log('Campo Telefono llenado con:', formData.telefono);
            }
        } catch (e) { console.log('Error en Telefono:', e.message); }
        
        try {
            if (formData.nombreDelProyecto) {
                const field = form.getTextField('NombredelProyecto');
                field.setText(formData.nombreDelProyecto);
                console.log('Campo NombredelProyecto llenado con:', formData.nombreDelProyecto);
            }
        } catch (e) { console.log('Error en NombredelProyecto:', e.message); }
        
        try {
            if (formData.provincia) {
                const field = form.getTextField('Provincia');
                field.setText(formData.provincia);
                console.log('Campo Provincia llenado con:', formData.provincia);
            }
        } catch (e) { console.log('Error en Provincia:', e.message); }
        
        try {
            if (formData.canton) {
                const field = form.getTextField('Canton');
                field.setText(formData.canton);
                console.log('Campo Canton llenado con:', formData.canton);
            }
        } catch (e) { console.log('Error en Canton:', e.message); }
        
        try {
            if (formData.distrito) {
                const field = form.getTextField('Distrito');
                field.setText(formData.distrito);
                console.log('Campo Distrito llenado con:', formData.distrito);
            }
        } catch (e) { console.log('Error en Distrito:', e.message); }
        
        // Nota: Anexo 3 tiene muchos más campos, estos son los principales para empezar
        ",

                5 => @"
        // Anexo 5 - Mapeo de campos
        console.log('Procesando Anexo 5...');
        
        try {
            if (formData.tutor) {
                const field = form.getTextField('Tutor');
                field.setText(formData.tutor);
                console.log('Campo Tutor llenado con:', formData.tutor);
            }
        } catch (e) { console.log('Error en Tutor:', e.message); }
        
        try {
            if (formData.nombreProyecto) {
                const field = form.getTextField('NombreProyecto');
                field.setText(formData.nombreProyecto);
                console.log('Campo NombreProyecto llenado con:', formData.nombreProyecto);
            }
        } catch (e) { console.log('Error en NombreProyecto:', e.message); }
        
        try {
            if (formData.nombreEstudiante1) {
                const field = form.getTextField('NombreEstudiante1');
                field.setText(formData.nombreEstudiante1);
                console.log('Campo NombreEstudiante1 llenado con:', formData.nombreEstudiante1);
            }
        } catch (e) { console.log('Error en NombreEstudiante1:', e.message); }
        
        try {
            if (formData.nombreEstudiante2) {
                const field = form.getTextField('NombreEstudiante2');
                field.setText(formData.nombreEstudiante2);
                console.log('Campo NombreEstudiante2 llenado con:', formData.nombreEstudiante2);
            }
        } catch (e) { console.log('Error en NombreEstudiante2:', e.message); }
        
        try {
            if (formData.nombreEstudiante3) {
                const field = form.getTextField('NombreEstudiante3');
                field.setText(formData.nombreEstudiante3);
                console.log('Campo NombreEstudiante3 llenado con:', formData.nombreEstudiante3);
            }
        } catch (e) { console.log('Error en NombreEstudiante3:', e.message); }
        
        try {
            if (formData.nombreEstudiante4) {
                const field = form.getTextField('NombreEstudiante4');
                field.setText(formData.nombreEstudiante4);
                console.log('Campo NombreEstudiante4 llenado con:', formData.nombreEstudiante4);
            }
        } catch (e) { console.log('Error en NombreEstudiante4:', e.message); }
        
        try {
            if (formData.seccion1) {
                const field = form.getTextField('Seccion1');
                field.setText(formData.seccion1);
                console.log('Campo Seccion1 llenado con:', formData.seccion1);
            }
        } catch (e) { console.log('Error en Seccion1:', e.message); }
        
        try {
            if (formData.seccion2) {
                const field = form.getTextField('Seccion2');
                field.setText(formData.seccion2);
                console.log('Campo Seccion2 llenado con:', formData.seccion2);
            }
        } catch (e) { console.log('Error en Seccion2:', e.message); }
        
        try {
            if (formData.seccion3) {
                const field = form.getTextField('Seccion3');
                field.setText(formData.seccion3);
                console.log('Campo Seccion3 llenado con:', formData.seccion3);
            }
        } catch (e) { console.log('Error en Seccion3:', e.message); }
        
        try {
            if (formData.seccion4) {
                const field = form.getTextField('Seccion4');
                field.setText(formData.seccion4);
                console.log('Campo Seccion4 llenado con:', formData.seccion4);
            }
        } catch (e) { console.log('Error en Seccion4:', e.message); }
        
        try {
            if (formData.resumenResultadosObjetivosMetas) {
                const field = form.getTextField('ResumenResultadosObjetivosMetas');
                field.setText(formData.resumenResultadosObjetivosMetas);
                console.log('Campo ResumenResultadosObjetivosMetas llenado');
            }
        } catch (e) { console.log('Error en ResumenResultadosObjetivosMetas:', e.message); }
        
        try {
            if (formData.problemasLimitacionesEncontradas) {
                const field = form.getTextField('ProblemasLimitacionesEncontradas');
                field.setText(formData.problemasLimitacionesEncontradas);
                console.log('Campo ProblemasLimitacionesEncontradas llenado');
            }
        } catch (e) { console.log('Error en ProblemasLimitacionesEncontradas:', e.message); }
        
        try {
            if (formData.recursosFacilidadesEncontradas) {
                const field = form.getTextField('RecursosFacilidadesEncontradas');
                field.setText(formData.recursosFacilidadesEncontradas);
                console.log('Campo RecursosFacilidadesEncontradas llenado');
            }
        } catch (e) { console.log('Error en RecursosFacilidadesEncontradas:', e.message); }
        
        try {
            if (formData.sugerencias) {
                const field = form.getTextField('Sugerencias');
                field.setText(formData.sugerencias);
                console.log('Campo Sugerencias llenado con:', formData.sugerencias);
            }
        } catch (e) { console.log('Error en Sugerencias:', e.message); }
        
        try {
            if (formData.observaciones) {
                const field = form.getTextField('Observaciones');
                field.setText(formData.observaciones);
                console.log('Campo Observaciones llenado con:', formData.observaciones);
            }
        } catch (e) { console.log('Error en Observaciones:', e.message); }
        ",

                6 => @"
        // Informe Final Tutor - Mapeo de campos
        console.log('Procesando Informe Final Tutor...');
        
        try {
            if (formData.nombreProyecto) {
                const field = form.getTextField('NombreProyecto');
                field.setText(formData.nombreProyecto);
                console.log('Campo NombreProyecto llenado con:', formData.nombreProyecto);
            }
        } catch (e) { console.log('Error en NombreProyecto:', e.message); }
        
        try {
            if (formData.tutor) {
                const field = form.getTextField('Tutor');
                field.setText(formData.tutor);
                console.log('Campo Tutor llenado con:', formData.tutor);
            }
        } catch (e) { console.log('Error en Tutor:', e.message); }
        
        try {
            if (formData.firmaTutor) {
                const field = form.getTextField('FirmaTutor');
                field.setText(formData.firmaTutor);
                console.log('Campo FirmaTutor llenado con:', formData.firmaTutor);
            }
        } catch (e) { console.log('Error en FirmaTutor:', e.message); }
        
        try {
            if (formData.nombreEstudiante1) {
                const field = form.getTextField('NombreEstudiante1');
                field.setText(formData.nombreEstudiante1);
                console.log('Campo NombreEstudiante1 llenado con:', formData.nombreEstudiante1);
            }
        } catch (e) { console.log('Error en NombreEstudiante1:', e.message); }
        
        try {
            if (formData.nombreEstudiante2) {
                const field = form.getTextField('NombreEstudiante2');
                field.setText(formData.nombreEstudiante2);
                console.log('Campo NombreEstudiante2 llenado con:', formData.nombreEstudiante2);
            }
        } catch (e) { console.log('Error en NombreEstudiante2:', e.message); }
        
        try {
            if (formData.nombreEstudiante3) {
                const field = form.getTextField('NombreEstudiante3');
                field.setText(formData.nombreEstudiante3);
                console.log('Campo NombreEstudiante3 llenado con:', formData.nombreEstudiante3);
            }
        } catch (e) { console.log('Error en NombreEstudiante3:', e.message); }
        
        try {
            if (formData.nombreEstudiante4) {
                const field = form.getTextField('NombreEstudiante4');
                field.setText(formData.nombreEstudiante4);
                console.log('Campo NombreEstudiante4 llenado con:', formData.nombreEstudiante4);
            }
        } catch (e) { console.log('Error en NombreEstudiante4:', e.message); }
        
        try {
            if (formData.fechaInicio) {
                const field = form.getTextField('FechaInicio');
                field.setText(formData.fechaInicio);
                console.log('Campo FechaInicio llenado con:', formData.fechaInicio);
            }
        } catch (e) { console.log('Error en FechaInicio:', e.message); }
        
        try {
            if (formData.fechaFin) {
                const field = form.getTextField('FechaFin');
                field.setText(formData.fechaFin);
                console.log('Campo FechaFin llenado con:', formData.fechaFin);
            }
        } catch (e) { console.log('Error en FechaFin:', e.message); }
        
        try {
            if (formData.totalHoras) {
                const field = form.getTextField('TotalHoras');
                field.setText(formData.totalHoras);
                console.log('Campo TotalHoras llenado con:', formData.totalHoras);
            }
        } catch (e) { console.log('Error en TotalHoras:', e.message); }
        
        try {
            if (formData.descripcionActividades) {
                const field = form.getTextField('DescripcionActividades');
                field.setText(formData.descripcionActividades);
                console.log('Campo DescripcionActividades llenado');
            }
        } catch (e) { console.log('Error en DescripcionActividades:', e.message); }
        
        try {
            if (formData.logrosObtenidos) {
                const field = form.getTextField('LogrosObtenidos');
                field.setText(formData.logrosObtenidos);
                console.log('Campo LogrosObtenidos llenado');
            }
        } catch (e) { console.log('Error en LogrosObtenidos:', e.message); }
        
        try {
            if (formData.dificultadesEncontradas) {
                const field = form.getTextField('DificultadesEncontradas');
                field.setText(formData.dificultadesEncontradas);
                console.log('Campo DificultadesEncontradas llenado');
            }
        } catch (e) { console.log('Error en DificultadesEncontradas:', e.message); }
        
        try {
            if (formData.recomendaciones) {
                const field = form.getTextField('Recomendaciones');
                field.setText(formData.recomendaciones);
                console.log('Campo Recomendaciones llenado');
            }
        } catch (e) { console.log('Error en Recomendaciones:', e.message); }
        
        try {
            if (formData.calificacionDesempeno) {
                const field = form.getTextField('CalificacionDesempeno');
                field.setText(formData.calificacionDesempeno);
                console.log('Campo CalificacionDesempeno llenado');
            }
        } catch (e) { console.log('Error en CalificacionDesempeno:', e.message); }
        
        try {
            if (formData.observacionesGenerales) {
                const field = form.getTextField('ObservacionesGenerales');
                field.setText(formData.observacionesGenerales);
                console.log('Campo ObservacionesGenerales llenado');
            }
        } catch (e) { console.log('Error en ObservacionesGenerales:', e.message); }
        ",

                7 => @"
        // Carta Institución - Mapeo de campos
        console.log('Procesando Carta Institución...');
        
        try {
            if (formData.fechaElaboracion) {
                const field = form.getTextField('FechaElaboracion');
                field.setText(formData.fechaElaboracion);
                console.log('Campo FechaElaboracion llenado con:', formData.fechaElaboracion);
            }
        } catch (e) { console.log('Error en FechaElaboracion:', e.message); }
        
        try {
            if (formData.nombreInstitucion) {
                const field = form.getTextField('NombreInstitucion');
                field.setText(formData.nombreInstitucion);
                console.log('Campo NombreInstitucion llenado con:', formData.nombreInstitucion);
            }
        } catch (e) { console.log('Error en NombreInstitucion:', e.message); }
        
        try {
            if (formData.direccionInstitucion) {
                const field = form.getTextField('DireccionInstitucion');
                field.setText(formData.direccionInstitucion);
                console.log('Campo DireccionInstitucion llenado');
            }
        } catch (e) { console.log('Error en DireccionInstitucion:', e.message); }
        
        try {
            if (formData.personaContacto) {
                const field = form.getTextField('PersonaContacto');
                field.setText(formData.personaContacto);
                console.log('Campo PersonaContacto llenado con:', formData.personaContacto);
            }
        } catch (e) { console.log('Error en PersonaContacto:', e.message); }
        
        try {
            if (formData.cargoContacto) {
                const field = form.getTextField('CargoContacto');
                field.setText(formData.cargoContacto);
                console.log('Campo CargoContacto llenado con:', formData.cargoContacto);
            }
        } catch (e) { console.log('Error en CargoContacto:', e.message); }
        
        try {
            if (formData.nombreProyecto) {
                const field = form.getTextField('NombreProyecto');
                field.setText(formData.nombreProyecto);
                console.log('Campo NombreProyecto llenado con:', formData.nombreProyecto);
            }
        } catch (e) { console.log('Error en NombreProyecto:', e.message); }
        
        try {
            if (formData.descripcionProyecto) {
                const field = form.getTextField('DescripcionProyecto');
                field.setText(formData.descripcionProyecto);
                console.log('Campo DescripcionProyecto llenado');
            }
        } catch (e) { console.log('Error en DescripcionProyecto:', e.message); }
        
        try {
            if (formData.objetivosProyecto) {
                const field = form.getTextField('ObjetivosProyecto');
                field.setText(formData.objetivosProyecto);
                console.log('Campo ObjetivosProyecto llenado');
            }
        } catch (e) { console.log('Error en ObjetivosProyecto:', e.message); }
        
        try {
            if (formData.beneficiosInstitucion) {
                const field = form.getTextField('BeneficiosInstitucion');
                field.setText(formData.beneficiosInstitucion);
                console.log('Campo BeneficiosInstitucion llenado');
            }
        } catch (e) { console.log('Error en BeneficiosInstitucion:', e.message); }
        
        try {
            if (formData.tutor) {
                const field = form.getTextField('Tutor');
                field.setText(formData.tutor);
                console.log('Campo Tutor llenado con:', formData.tutor);
            }
        } catch (e) { console.log('Error en Tutor:', e.message); }
        
        try {
            if (formData.firmaTutor) {
                const field = form.getTextField('FirmaTutor');
                field.setText(formData.firmaTutor);
                console.log('Campo FirmaTutor llenado con:', formData.firmaTutor);
            }
        } catch (e) { console.log('Error en FirmaTutor:', e.message); }
        
        try {
            if (formData.contactoTutor) {
                const field = form.getTextField('ContactoTutor');
                field.setText(formData.contactoTutor);
                console.log('Campo ContactoTutor llenado con:', formData.contactoTutor);
            }
        } catch (e) { console.log('Error en ContactoTutor:', e.message); }
        
        try {
            if (formData.nombreEstudiante1) {
                const field = form.getTextField('NombreEstudiante1');
                field.setText(formData.nombreEstudiante1);
                console.log('Campo NombreEstudiante1 llenado con:', formData.nombreEstudiante1);
            }
        } catch (e) { console.log('Error en NombreEstudiante1:', e.message); }
        
        try {
            if (formData.nombreEstudiante2) {
                const field = form.getTextField('NombreEstudiante2');
                field.setText(formData.nombreEstudiante2);
                console.log('Campo NombreEstudiante2 llenado con:', formData.nombreEstudiante2);
            }
        } catch (e) { console.log('Error en NombreEstudiante2:', e.message); }
        
        try {
            if (formData.nombreEstudiante3) {
                const field = form.getTextField('NombreEstudiante3');
                field.setText(formData.nombreEstudiante3);
                console.log('Campo NombreEstudiante3 llenado con:', formData.nombreEstudiante3);
            }
        } catch (e) { console.log('Error en NombreEstudiante3:', e.message); }
        
        try {
            if (formData.nombreEstudiante4) {
                const field = form.getTextField('NombreEstudiante4');
                field.setText(formData.nombreEstudiante4);
                console.log('Campo NombreEstudiante4 llenado con:', formData.nombreEstudiante4);
            }
        } catch (e) { console.log('Error en NombreEstudiante4:', e.message); }
        
        try {
            if (formData.duracionEstimada) {
                const field = form.getTextField('DuracionEstimada');
                field.setText(formData.duracionEstimada);
                console.log('Campo DuracionEstimada llenado con:', formData.duracionEstimada);
            }
        } catch (e) { console.log('Error en DuracionEstimada:', e.message); }
        
        try {
            if (formData.horarioTentativo) {
                const field = form.getTextField('HorarioTentativo');
                field.setText(formData.horarioTentativo);
                console.log('Campo HorarioTentativo llenado con:', formData.horarioTentativo);
            }
        } catch (e) { console.log('Error en HorarioTentativo:', e.message); }
        ",

                8 => @"
        // Carta Consentimiento - Mapeo de campos
        console.log('Procesando Carta Consentimiento...');
        
        try {
            if (formData.fechaElaboracion) {
                const field = form.getTextField('FechaElaboracion');
                field.setText(formData.fechaElaboracion);
                console.log('Campo FechaElaboracion llenado con:', formData.fechaElaboracion);
            }
        } catch (e) { console.log('Error en FechaElaboracion:', e.message); }
        
        try {
            if (formData.nombreEncargado) {
                const field = form.getTextField('NombreEncargado');
                field.setText(formData.nombreEncargado);
                console.log('Campo NombreEncargado llenado con:', formData.nombreEncargado);
            }
        } catch (e) { console.log('Error en NombreEncargado:', e.message); }
        
        try {
            if (formData.cedulaEncargado) {
                const field = form.getTextField('CedulaEncargado');
                field.setText(formData.cedulaEncargado);
                console.log('Campo CedulaEncargado llenado con:', formData.cedulaEncargado);
            }
        } catch (e) { console.log('Error en CedulaEncargado:', e.message); }
        
        try {
            if (formData.relacionEstudiante) {
                const field = form.getTextField('RelacionEstudiante');
                field.setText(formData.relacionEstudiante);
                console.log('Campo RelacionEstudiante llenado con:', formData.relacionEstudiante);
            }
        } catch (e) { console.log('Error en RelacionEstudiante:', e.message); }
        
        try {
            if (formData.telefonoEncargado) {
                const field = form.getTextField('TelefonoEncargado');
                field.setText(formData.telefonoEncargado);
                console.log('Campo TelefonoEncargado llenado con:', formData.telefonoEncargado);
            }
        } catch (e) { console.log('Error en TelefonoEncargado:', e.message); }
        
        try {
            if (formData.emailEncargado) {
                const field = form.getTextField('EmailEncargado');
                field.setText(formData.emailEncargado);
                console.log('Campo EmailEncargado llenado con:', formData.emailEncargado);
            }
        } catch (e) { console.log('Error en EmailEncargado:', e.message); }
        
        try {
            if (formData.direccionEncargado) {
                const field = form.getTextField('DireccionEncargado');
                field.setText(formData.direccionEncargado);
                console.log('Campo DireccionEncargado llenado');
            }
        } catch (e) { console.log('Error en DireccionEncargado:', e.message); }
        
        try {
            if (formData.nombreEstudiante) {
                const field = form.getTextField('NombreEstudiante');
                field.setText(formData.nombreEstudiante);
                console.log('Campo NombreEstudiante llenado con:', formData.nombreEstudiante);
            }
        } catch (e) { console.log('Error en NombreEstudiante:', e.message); }
        
        try {
            if (formData.cedulaEstudiante) {
                const field = form.getTextField('CedulaEstudiante');
                field.setText(formData.cedulaEstudiante);
                console.log('Campo CedulaEstudiante llenado con:', formData.cedulaEstudiante);
            }
        } catch (e) { console.log('Error en CedulaEstudiante:', e.message); }
        
        try {
            if (formData.seccionEstudiante) {
                const field = form.getTextField('SeccionEstudiante');
                field.setText(formData.seccionEstudiante);
                console.log('Campo SeccionEstudiante llenado con:', formData.seccionEstudiante);
            }
        } catch (e) { console.log('Error en SeccionEstudiante:', e.message); }
        
        try {
            if (formData.nombreProyecto) {
                const field = form.getTextField('NombreProyecto');
                field.setText(formData.nombreProyecto);
                console.log('Campo NombreProyecto llenado con:', formData.nombreProyecto);
            }
        } catch (e) { console.log('Error en NombreProyecto:', e.message); }
        
        try {
            if (formData.descripcionProyecto) {
                const field = form.getTextField('DescripcionProyecto');
                field.setText(formData.descripcionProyecto);
                console.log('Campo DescripcionProyecto llenado');
            }
        } catch (e) { console.log('Error en DescripcionProyecto:', e.message); }
        
        try {
            if (formData.lugarEjecucion) {
                const field = form.getTextField('LugarEjecucion');
                field.setText(formData.lugarEjecucion);
                console.log('Campo LugarEjecucion llenado');
            }
        } catch (e) { console.log('Error en LugarEjecucion:', e.message); }
        
        try {
            if (formData.fechaInicio) {
                const field = form.getTextField('FechaInicio');
                field.setText(formData.fechaInicio);
                console.log('Campo FechaInicio llenado con:', formData.fechaInicio);
            }
        } catch (e) { console.log('Error en FechaInicio:', e.message); }
        
        try {
            if (formData.fechaFin) {
                const field = form.getTextField('FechaFin');
                field.setText(formData.fechaFin);
                console.log('Campo FechaFin llenado con:', formData.fechaFin);
            }
        } catch (e) { console.log('Error en FechaFin:', e.message); }
        
        try {
            if (formData.horarioActividades) {
                const field = form.getTextField('HorarioActividades');
                field.setText(formData.horarioActividades);
                console.log('Campo HorarioActividades llenado');
            }
        } catch (e) { console.log('Error en HorarioActividades:', e.message); }
        
        try {
            if (formData.nombreTutor) {
                const field = form.getTextField('NombreTutor');
                field.setText(formData.nombreTutor);
                console.log('Campo NombreTutor llenado con:', formData.nombreTutor);
            }
        } catch (e) { console.log('Error en NombreTutor:', e.message); }
        
        try {
            if (formData.contactoTutor) {
                const field = form.getTextField('ContactoTutor');
                field.setText(formData.contactoTutor);
                console.log('Campo ContactoTutor llenado con:', formData.contactoTutor);
            }
        } catch (e) { console.log('Error en ContactoTutor:', e.message); }
        
        try {
            if (formData.riesgosIdentificados) {
                const field = form.getTextField('RiesgosIdentificados');
                field.setText(formData.riesgosIdentificados);
                console.log('Campo RiesgosIdentificados llenado');
            }
        } catch (e) { console.log('Error en RiesgosIdentificados:', e.message); }
        
        try {
            if (formData.medidasSeguridad) {
                const field = form.getTextField('MedidasSeguridad');
                field.setText(formData.medidasSeguridad);
                console.log('Campo MedidasSeguridad llenado');
            }
        } catch (e) { console.log('Error en MedidasSeguridad:', e.message); }
        
        try {
            if (formData.autorizaParticipacion) {
                const field = form.getCheckBox('AutorizaParticipacion');
                field.check();
                console.log('Checkbox AutorizaParticipacion marcado');
            }
        } catch (e) { console.log('Error en AutorizaParticipacion:', e.message); }
        
        try {
            if (formData.autorizaEmergenciaMedica) {
                const field = form.getCheckBox('AutorizaEmergenciaMedica');
                field.check();
                console.log('Checkbox AutorizaEmergenciaMedica marcado');
            }
        } catch (e) { console.log('Error en AutorizaEmergenciaMedica:', e.message); }
        
        try {
            if (formData.autorizaTransporte) {
                const field = form.getCheckBox('AutorizaTransporte');
                field.check();
                console.log('Checkbox AutorizaTransporte marcado');
            }
        } catch (e) { console.log('Error en AutorizaTransporte:', e.message); }
        
        try {
            if (formData.firmaEncargado) {
                const field = form.getTextField('FirmaEncargado');
                field.setText(formData.firmaEncargado);
                console.log('Campo FirmaEncargado llenado con:', formData.firmaEncargado);
            }
        } catch (e) { console.log('Error en FirmaEncargado:', e.message); }
        
        // Nota: Las imágenes de cédula se manejan por separado en el PDF
        ",

                _ => throw new ArgumentException($"Tipo de anexo no válido: {tipoAnexo}")
            };
        }
    }
}
