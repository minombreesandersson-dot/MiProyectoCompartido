using System;
using System.Collections.Generic;

namespace Lab10Negocios.Models;

public partial class Distrito
{
    public int IdDistrito { get; set; }

    public string NomDistrito { get; set; } = null!;

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
