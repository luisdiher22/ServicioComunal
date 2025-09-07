using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioComunal.Models
{
    [Table("NOTIFICACION")]
    public class Notificacion
    {
        [Key]
        [Column("Identificacion")]
        public int Identificacion { get; set; }

        [Required]
        [Column("Mensaje")]
        public string Mensaje { get; set; } = string.Empty;

        [Column("FechaHora")]
        public DateTime FechaHora { get; set; }

        [Column("Leido")]
        public bool Leido { get; set; }

        [Required]
        [Column("UsuarioDestino")]
        public int UsuarioDestino { get; set; }

        [Required]
        [Column("TipoNotificacion")]
        public string TipoNotificacion { get; set; } = string.Empty;

        [Column("EntregaId")]
        public int? EntregaId { get; set; }

        [Column("GrupoId")]
        public int? GrupoId { get; set; }

        [Column("UsuarioOrigen")]
        public int? UsuarioOrigen { get; set; }

        // Propiedades de navegación opcionales
        [ForeignKey("EntregaId")]
        public virtual Entrega? Entrega { get; set; }

        [ForeignKey("GrupoId")]
        public virtual Grupo? Grupo { get; set; }

        [ForeignKey("UsuarioDestino")]
        public virtual Usuario UsuarioDestinoNavigation { get; set; } = null!;

        [ForeignKey("UsuarioOrigen")]
        public virtual Usuario? UsuarioOrigenNavigation { get; set; }
    }

    // Enum para tipos de notificaciones
    public static class TipoNotificacion
    {
        public const string NuevaEntrega = "NUEVA_ENTREGA";
        public const string RetroalimentacionEntrega = "RETROALIMENTACION_ENTREGA";
        public const string EntregaRevisada = "ENTREGA_REVISADA";
        public const string SolicitudAceptada = "SOLICITUD_ACEPTADA";
        public const string NuevaSolicitudGrupo = "NUEVA_SOLICITUD_GRUPO";
        public const string RecordatorioEntrega = "RECORDATORIO_ENTREGA";
        public const string GrupoAsignado = "GRUPO_ASIGNADO";
        public const string EntregableRecibido = "ENTREGABLE_RECIBIDO";
    }
}
