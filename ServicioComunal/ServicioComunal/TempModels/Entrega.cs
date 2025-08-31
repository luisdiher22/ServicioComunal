using System;
using System.Collections.Generic;

namespace ServicioComunal.TempModels;

public partial class Entrega
{
    public int Identificacion { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string ArchivoRuta { get; set; } = null!;

    public DateTime FechaLimite { get; set; }

    public string Retroalimentacion { get; set; } = null!;

    public DateTime FechaRetroalimentacion { get; set; }

    public int GrupoNumero { get; set; }

    public int? ProfesorIdentificacion { get; set; }

    public int? FormularioIdentificacion { get; set; }

    public virtual Formulario? FormularioIdentificacionNavigation { get; set; }

    public virtual Grupo GrupoNumeroNavigation { get; set; } = null!;

    public virtual Profesor? ProfesorIdentificacionNavigation { get; set; }
}
