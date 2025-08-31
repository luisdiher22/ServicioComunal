using System;
using System.Collections.Generic;

namespace ServicioComunal.TempModels;

public partial class Usuario
{
    public int Identificacion { get; set; }

    public string Usuario1 { get; set; } = null!;

    public string Contraseña { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public bool Activo { get; set; }

    public bool RequiereCambioContraseña { get; set; }
}
