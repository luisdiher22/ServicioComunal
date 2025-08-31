using System;
using System.Collections.Generic;

namespace ServicioComunal.TempModels;

public partial class Profesor
{
    public int Identificacion { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public virtual ICollection<Entrega> Entregas { get; set; } = new List<Entrega>();

    public virtual ICollection<Formulario> Formularios { get; set; } = new List<Formulario>();

    public virtual ICollection<GrupoProfesor> GrupoProfesors { get; set; } = new List<GrupoProfesor>();

    public virtual ICollection<Notificacion> Notificacions { get; set; } = new List<Notificacion>();
}
