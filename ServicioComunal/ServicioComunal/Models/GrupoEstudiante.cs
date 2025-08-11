using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioComunal.Models
{
    [Table("GRUPO_ESTUDIANTE")]
    public class GrupoEstudiante
    {
        [Key, Column("ESTUDIANTE_Identificacion", Order = 0)]
        public int EstudianteIdentificacion { get; set; }

        [Key, Column("GRUPO_Numero", Order = 1)]
        public int GrupoNumero { get; set; }

        // Propiedades de navegación
        [ForeignKey("EstudianteIdentificacion")]
        public virtual Estudiante Estudiante { get; set; } = null!;

        [ForeignKey("GrupoNumero")]
        public virtual Grupo Grupo { get; set; } = null!;
    }
}
