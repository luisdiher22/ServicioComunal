using System;
using System.Collections.Generic;

namespace ServicioComunal.TempModels;

public partial class GrupoProfesor
{
    public int GrupoNumero { get; set; }

    public int ProfesorIdentificacion { get; set; }

    public DateTime FechaAsignacion { get; set; }

    public virtual Grupo GrupoNumeroNavigation { get; set; } = null!;

    public virtual Profesor ProfesorIdentificacionNavigation { get; set; } = null!;
}
