using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioComunal.Models
{
    [Table("SOLICITUD")]
    public class Solicitud
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("EstudianteRemitenteId")]
        public int EstudianteRemitenteId { get; set; }

        [Required]
        [Column("EstudianteDestinatarioId")]
        public int EstudianteDestinatarioId { get; set; }

        [Column("GrupoNumero")]
        public int? GrupoNumero { get; set; }

        [Required]
        [Column("Tipo")]
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty; // "INVITACION_GRUPO" o "SOLICITUD_INGRESO"

        [Required]
        [Column("Estado")]
        [StringLength(20)]
        public string Estado { get; set; } = "PENDIENTE"; // "PENDIENTE", "ACEPTADA", "RECHAZADA"

        [Column("Mensaje")]
        [StringLength(500)]
        public string? Mensaje { get; set; }

        [Required]
        [Column("FechaCreacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Column("FechaRespuesta")]
        public DateTime? FechaRespuesta { get; set; }

        // Propiedades de navegación
        [ForeignKey("EstudianteRemitenteId")]
        public virtual Estudiante EstudianteRemitente { get; set; } = null!;

        [ForeignKey("EstudianteDestinatarioId")]
        public virtual Estudiante EstudianteDestinatario { get; set; } = null!;

        [ForeignKey("GrupoNumero")]
        public virtual Grupo? Grupo { get; set; }
    }
}
