using System;
using System.Collections.Generic;

namespace Lab10Negocios.Models;

public partial class Empleado
{
    public int IdEmpleado { get; set; }

    public string ApeEmpleado { get; set; } = null!;

    public string NomEmpleado { get; set; } = null!;

    public string DirEmpleado { get; set; } = null!;

    public int IdDistrito { get; set; }

    public string? FonoEmpleado { get; set; }

    public int IdCargo { get; set; }

    public string? Estado { get; set; }

    public virtual Cargo IdCargoNavigation { get; set; } = null!;

    public virtual Distrito IdDistritoNavigation { get; set; } = null!;

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
