using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioComunal.Models
{
    [Table("PROFESOR")]
    public class Profesor
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
        [Column("Rol")]
        public string Rol { get; set; } = string.Empty;

        // Propiedades de navegación
        public virtual ICollection<Entrega> Entregas { get; set; } = new List<Entrega>();
        public virtual ICollection<Formulario> Formularios { get; set; } = new List<Formulario>();
        public virtual ICollection<GrupoEstudiante> GruposEstudiantes { get; set; } = new List<GrupoEstudiante>();
        public virtual ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
        // Sin referencia a Usuario - tabla independiente
    }
}
