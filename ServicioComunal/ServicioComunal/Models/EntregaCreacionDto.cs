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

        // Nuevos campos para selección de grupos
        public bool EnviarATodosLosGrupos { get; set; } = true; // Por defecto enviar a todos
        
        public int? GrupoEspecifico { get; set; } // ID del grupo específico si no es para todos
    }
}
