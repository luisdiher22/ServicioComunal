using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioComunal.Models
{
    [Table("GRUPO")]
    public class Grupo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("Numero")]
        public int Numero { get; set; }

        [Column("LiderIdentificacion")]
        public int? LiderIdentificacion { get; set; }

        // Propiedades de navegación
        [ForeignKey("LiderIdentificacion")]
        public virtual Estudiante? Lider { get; set; }

        public virtual ICollection<Entrega> Entregas { get; set; } = new List<Entrega>();
        public virtual ICollection<GrupoEstudiante> GruposEstudiantes { get; set; } = new List<GrupoEstudiante>();
        public virtual ICollection<GrupoProfesor> GruposProfesores { get; set; } = new List<GrupoProfesor>();
        public virtual ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
    }
}
