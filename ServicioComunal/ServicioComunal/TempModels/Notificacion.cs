using System;
using System.Collections.Generic;

namespace ServicioComunal.TempModels;

public partial class Notificacion
{
    public int Identificacion { get; set; }

    public string Mensaje { get; set; } = null!;

    public DateTime FechaHora { get; set; }

    public bool Leido { get; set; }

    public int GrupoNumero { get; set; }

    public int ProfesorIdentificacion { get; set; }

    public virtual Grupo GrupoNumeroNavigation { get; set; } = null!;

    public virtual Profesor ProfesorIdentificacionNavigation { get; set; } = null!;
}
