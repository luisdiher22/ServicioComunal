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
}
