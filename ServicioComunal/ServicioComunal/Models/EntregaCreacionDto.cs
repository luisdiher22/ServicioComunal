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
    }
}
