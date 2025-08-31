using System;
using System.Collections.Generic;

namespace ServicioComunal.TempModels;

public partial class Formulario
{
    public int Identificacion { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string ArchivoRuta { get; set; } = null!;

    public DateTime FechaIngreso { get; set; }

    public int ProfesorIdentificacion { get; set; }

    public virtual ICollection<Entrega> Entregas { get; set; } = new List<Entrega>();

    public virtual Profesor ProfesorIdentificacionNavigation { get; set; } = null!;
}
