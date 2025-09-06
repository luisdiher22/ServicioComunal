using System.ComponentModel.DataAnnotations;

namespace ServicioComunal.Models
{
    public class AnexoFormularioDto
    {
        public int EntregaId { get; set; }
        public int TipoAnexo { get; set; }
        public Dictionary<string, object> Campos { get; set; } = new Dictionary<string, object>();
    }

    public class Anexo1FormularioDto
    {
        public int EntregaId { get; set; }
        public string? Seccion1 { get; set; }
        public string? Seccion2 { get; set; }
        public string? Seccion3 { get; set; }
        public string? Seccion4 { get; set; }
        public string? Estudiante1 { get; set; }
        public string? Estudiante2 { get; set; }
        public string? Estudiante3 { get; set; }
        public string? Estudiante4 { get; set; }
        public string? NombreProyecto { get; set; }
    }

    public class Anexo2FormularioDto
    {
        public int EntregaId { get; set; }
        public string? NombreProyecto { get; set; }
        public string? Tutor { get; set; }
        public string? Nombre1 { get; set; }
        public string? Nombre2 { get; set; }
        public string? Nombre3 { get; set; }
        public string? Nombre4 { get; set; }
        public string? Seccion1 { get; set; }
        public string? Seccion2 { get; set; }
        public string? Seccion3 { get; set; }
        public string? Seccion4 { get; set; }
        public bool MedioAmbiente { get; set; }
        public bool JovenesApoyandoJovenes { get; set; }
        public bool InformacionEstudiantil { get; set; }
        public bool Arte { get; set; }
        public bool EstilosVidaSaludables { get; set; }
        public bool Otros { get; set; }
        public string? OtrosEspecifique { get; set; }
        public string? BreveDescripcion { get; set; }
    }

    public class Anexo3FormularioDto
    {
        public int EntregaId { get; set; }
        public string? FirmaTutor { get; set; }
        public string? Tutor { get; set; }
        public string? NombreEstudiante1 { get; set; }
        public string? Seccion01 { get; set; }
        public string? Telefono { get; set; }
        public string? Telefono01 { get; set; }
        public string? NombreEstudiante2 { get; set; }
        public string? Seccion02 { get; set; }
        public string? Telefono02 { get; set; }
        public string? NombreEstudiante3 { get; set; }
        public string? Seccion03 { get; set; }
        public string? Telefono03 { get; set; }
        public string? NombreEstudiante4 { get; set; }
        public string? Seccion04 { get; set; }
        public string? Telefono04 { get; set; }
        public string? NombreDelProyecto { get; set; }
        public string? Provincia { get; set; }
        public string? Canton { get; set; }
        public string? Distrito { get; set; }
        public string? DireccionExacta { get; set; }
        public string? TipoProyecto { get; set; }
        public string? PoblacionMeta { get; set; }
        public string? ObjetivoGeneral1 { get; set; }
        public string? ObjetivoGeneral2 { get; set; }
        public string? ObjetivosEspecificos { get; set; }
        public string? ActividadEspecificaEjecucion1 { get; set; }
        public string? FechaAproximada1 { get; set; }
        public string? Horas1 { get; set; }
        public string? ActividadEspecificaEjecucion2 { get; set; }
        public string? ActividadEspecificaEjecucion3 { get; set; }
        public string? FechaAproximada2 { get; set; }
        public string? FechaAproximada3 { get; set; }
        public string? Horas2 { get; set; }
        public string? Horas3 { get; set; }
        public string? RecursosHumanos1 { get; set; }
        public string? RecursosHumanos2 { get; set; }
        public string? RecursosHumanos3 { get; set; }
        public string? RecursosMateriales1 { get; set; }
        public string? RecursosMateriales2 { get; set; }
        public string? RecursosMateriales3 { get; set; }
        public string? RecursosMateriales4 { get; set; }
        public string? RecursosEconomicos1 { get; set; }
        public string? RecursosEconomicos2 { get; set; }
        public string? RecursosEconomicos3 { get; set; }
        public string? RecursosEconomicos4 { get; set; }
    }

    public class Anexo5FormularioDto
    {
        public int EntregaId { get; set; }
        public string? Tutor { get; set; }
        public string? NombreProyecto { get; set; }
        public string? NombreEstudiante1 { get; set; }
        public string? NombreEstudiante2 { get; set; }
        public string? NombreEstudiante3 { get; set; }
        public string? NombreEstudiante4 { get; set; }
        public string? NombreEstudiante5 { get; set; }
        public string? Seccion1 { get; set; }
        public string? Seccion2 { get; set; }
        public string? Seccion3 { get; set; }
        public string? Seccion4 { get; set; }
        public string? Seccion5 { get; set; }
        public string? FirmaTutor { get; set; }
        public string? ResumenResultadosObjetivosMetas { get; set; }
        public string? ProblemasLimitacionesEncontradas { get; set; }
        public string? RecursosFacilidadesEncontradas { get; set; }
        public string? Sugerencias { get; set; }
        public string? Observaciones { get; set; }
    }

    // Informe Final Tutor Interactiva (Tipo 6) - Campos del PDF real
    public class InformeFinalTutorFormularioDto
    {
        public int EntregaId { get; set; }
        
        // Campos reales del PDF
        public string? SeccionRow1 { get; set; }
        public string? NombreTutor { get; set; }
        public string? CedulaTutor { get; set; }
        public string? NumeroProyecto { get; set; }
        public string? NombreProyecto { get; set; }
        public string? NombreEstudiante { get; set; }
        public string? CedulaEstudiante { get; set; }
        
        // Checkboxes de evaluación
        public bool ChecboxSatisfactoria { get; set; }
        public bool ChecboxRegular { get; set; }
        public bool CheckboxDeficiente { get; set; }
        public bool CheckboxInsuficiente { get; set; }
        
        // Firma del tutor (base64)
        public string? FirmaTutor { get; set; }
    }

    // Carta para Ingresar a la Institución Interactiva (Tipo 7) - Campos del PDF real
    public class CartaInstitucionFormularioDto
    {
        public int EntregaId { get; set; }
        
        // Campos de fecha
        public string? DiaFecha { get; set; }
        public string? MesFecha { get; set; }
        public string? YearFecha { get; set; }
        
        // Información del profesor y proyecto
        public string? NombreProfesor { get; set; }
        public string? TituloProyecto { get; set; }
        public string? Dias { get; set; }
        public string? Fechas { get; set; }
        public string? AreaDesignada { get; set; }
        
        // Estudiantes (hasta 5)
        public string? NombreEstudiante1 { get; set; }
        public string? NombreEstudiante2 { get; set; }
        public string? NombreEstudiante3 { get; set; }
        public string? NombreEstudiante4 { get; set; }
        public string? NombreEstudiante5 { get; set; }
        
        // Secciones
        public string? Seccion1 { get; set; }
        public string? Seccion2 { get; set; }
        public string? Seccion3 { get; set; }
        public string? Seccion4 { get; set; }
        public string? Seccion5 { get; set; }
        
        // Firma del tutor (base64)
        public string? FirmaTutor { get; set; }
    }

    // Carta de Consentimiento Encargado Legal (Tipo 8) - Campos del PDF real
    public class CartaConsentimientoFormularioDto
    {
        public int EntregaId { get; set; }
        
        // Campos reales del PDF
        public string? NombreTutor { get; set; }
        public string? CedulaTutor { get; set; }
        public string? CedulaEstudiante { get; set; }
        public string? TituloServicioComunal { get; set; }
        public string? NombreDocente { get; set; }
        public string? UbicacionServicio { get; set; }
        
        // Imágenes de cédula (para compatibilidad con el controlador)
        public string? ImagenCedulaFrente { get; set; }
        public string? ImagenCedulaAtras { get; set; }
        public string? CedulaFrontalImagenPath { get; set; }
        public string? CedulaTraseraImagenPath { get; set; }
    }
}
