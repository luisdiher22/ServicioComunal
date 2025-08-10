using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioComunal.Models
{
    [Table("USUARIO")]
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("Identificacion")]
        public int Identificacion { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("Contraseña")]
        public string Contraseña { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Column("Rol")]
        public string Rol { get; set; } = string.Empty; // "Estudiante", "Profesor", "Administrador"

        [Column("FechaCreacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Column("UltimoAcceso")]
        public DateTime? UltimoAcceso { get; set; }

        [Column("Activo")]
        public bool Activo { get; set; } = true;

        // Sin propiedades de navegación - tabla independiente
    }
}
