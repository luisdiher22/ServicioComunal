using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioComunal.Models
{
    [Table("FORMULARIO")]
    public class Formulario
    {
        [Key]
        [Column("Identificacion")]
        public int Identificacion { get; set; }

        [Required]
        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Column("Descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [Column("ArchivoRuta")]
        public string ArchivoRuta { get; set; } = string.Empty;

        [Column("FechaIngreso")]
        public DateTime FechaIngreso { get; set; }

        [Column("PROFESOR_Identificacion")]
        public int ProfesorIdentificacion { get; set; }

        // Propiedades de navegación
        [ForeignKey("ProfesorIdentificacion")]
        public virtual Profesor Profesor { get; set; } = null!;
    }
}
