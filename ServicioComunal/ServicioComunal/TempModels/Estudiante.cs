using System;
using System.Collections.Generic;

namespace ServicioComunal.TempModels;

public partial class Estudiante
{
    public int Identificacion { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Clase { get; set; } = null!;

    public virtual ICollection<Grupo> Grupos { get; set; } = new List<Grupo>();

    public virtual ICollection<Solicitud> SolicitudEstudianteDestinatarios { get; set; } = new List<Solicitud>();

    public virtual ICollection<Solicitud> SolicitudEstudianteRemitentes { get; set; } = new List<Solicitud>();

    public virtual ICollection<Grupo> GrupoNumeros { get; set; } = new List<Grupo>();
}
