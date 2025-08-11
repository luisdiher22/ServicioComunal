using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioComunal.Models
{
    [Table("GRUPO_PROFESOR")]
    public class GrupoProfesor
    {
        [Key, Column("GRUPO_Numero", Order = 0)]
        public int GrupoNumero { get; set; }

        [Key, Column("PROFESOR_Identificacion", Order = 1)]
        public int ProfesorIdentificacion { get; set; }

        [Column("FechaAsignacion")]
        public DateTime FechaAsignacion { get; set; } = DateTime.Now;

        // Propiedades de navegación
        [ForeignKey("GrupoNumero")]
        public virtual Grupo Grupo { get; set; } = null!;

        [ForeignKey("ProfesorIdentificacion")]
        public virtual Profesor Profesor { get; set; } = null!;
    }
}
