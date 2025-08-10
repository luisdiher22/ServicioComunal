using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioComunal.Models
{
    [Table("ESTUDIANTE")]
    public class Estudiante
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("Identificacion")]
        public int Identificacion { get; set; }

        [Required]
        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Column("Apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Required]
        [Column("Clase")]
        public string Clase { get; set; } = string.Empty;

        // Propiedades de navegación
        public virtual ICollection<GrupoEstudiante> GruposEstudiantes { get; set; } = new List<GrupoEstudiante>();
        // Sin referencia a Usuario - tabla independiente
    }
}
