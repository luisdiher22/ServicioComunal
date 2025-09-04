using System.ComponentModel.DataAnnotations;

namespace ServicioComunal.Models
{
    public class EntregaCreacionDto
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public DateTime FechaLimite { get; set; }

        public int? FormularioIdentificacion { get; set; }

        public int? TipoAnexo { get; set; } // 1, 2, 3, 5 para los anexos correspondientes
    }
}
