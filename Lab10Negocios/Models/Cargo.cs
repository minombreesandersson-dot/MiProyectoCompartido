using System;
using System.Collections.Generic;

namespace Lab10Negocios.Models;

public partial class Cargo
{
    public int IdCargo { get; set; }

    public string NomCargo { get; set; } = null!;

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
