using System;
using System.Collections.Generic;

namespace ServicioComunal.TempModels;

public partial class Solicitud
{
    public int Id { get; set; }

    public int EstudianteRemitenteId { get; set; }

    public int EstudianteDestinatarioId { get; set; }

    public int? GrupoNumero { get; set; }

    public string Tipo { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public string? Mensaje { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaRespuesta { get; set; }

    public virtual Estudiante EstudianteDestinatario { get; set; } = null!;

    public virtual Estudiante EstudianteRemitente { get; set; } = null!;

    public virtual Grupo? GrupoNumeroNavigation { get; set; }
}
