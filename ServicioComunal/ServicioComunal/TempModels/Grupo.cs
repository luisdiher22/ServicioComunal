using System;
using System.Collections.Generic;

namespace ServicioComunal.TempModels;

public partial class Grupo
{
    public int Numero { get; set; }

    public int? LiderIdentificacion { get; set; }

    public virtual ICollection<Entrega> Entregas { get; set; } = new List<Entrega>();

    public virtual ICollection<GrupoProfesor> GrupoProfesors { get; set; } = new List<GrupoProfesor>();

    public virtual Estudiante? LiderIdentificacionNavigation { get; set; }

    public virtual ICollection<Notificacion> Notificacions { get; set; } = new List<Notificacion>();

    public virtual ICollection<Solicitud> Solicituds { get; set; } = new List<Solicitud>();

    public virtual ICollection<Estudiante> EstudianteIdentificacions { get; set; } = new List<Estudiante>();
}
