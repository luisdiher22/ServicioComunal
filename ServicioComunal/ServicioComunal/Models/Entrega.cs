using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioComunal.Models
{
    [Table("ENTREGA")]
    public class Entrega
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

        [Column("FechaLimite")]
        public DateTime FechaLimite { get; set; }

        [Column("FechaEntrega")]
        public DateTime? FechaEntrega { get; set; }

        [Required]
        [Column("Retroalimentacion")]
        public string Retroalimentacion { get; set; } = string.Empty;

        [Column("FechaRetroalimentacion")]
        public DateTime? FechaRetroalimentacion { get; set; }

        [Column("GRUPO_Numero")]
        public int GrupoNumero { get; set; }

        [Column("PROFESOR_Identificacion")]
        public int? ProfesorIdentificacion { get; set; }

        [Column("FORMULARIO_Identificacion")]
        public int? FormularioIdentificacion { get; set; }

        [Column("TipoAnexo")]
        public int TipoAnexo { get; set; } // 1, 2, 3, 5 para los anexos correspondientes

        [Column("DatosFormularioJson")]
        public string? DatosFormularioJson { get; set; } // JSON con los datos llenados por los estudiantes

        // Propiedades de navegación
        [ForeignKey("GrupoNumero")]
        public virtual Grupo Grupo { get; set; } = null!;

        [ForeignKey("ProfesorIdentificacion")]
        public virtual Profesor? Profesor { get; set; }

        [ForeignKey("FormularioIdentificacion")]
        public virtual Formulario? Formulario { get; set; }
    }
}
