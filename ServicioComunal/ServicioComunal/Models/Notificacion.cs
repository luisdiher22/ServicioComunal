using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioComunal.Models
{
    [Table("NOTIFICACION")]
    public class Notificacion
    {
        [Key]
        [Column("Identificacion")]
        public int Identificacion { get; set; }

        [Required]
        [Column("Mensaje")]
        public string Mensaje { get; set; } = string.Empty;

        [Column("FechaHora")]
        public DateTime FechaHora { get; set; }

        [Column("Leido")]
        public bool Leido { get; set; }

        [Column("GRUPO_Numero")]
        public int GrupoNumero { get; set; }

        [Column("PROFESOR_Identificacion")]
        public int ProfesorIdentificacion { get; set; }

        // Propiedades de navegación
        [ForeignKey("GrupoNumero")]
        public virtual Grupo Grupo { get; set; } = null!;

        [ForeignKey("ProfesorIdentificacion")]
        public virtual Profesor Profesor { get; set; } = null!;
    }
}
