using System;
using System.Collections.Generic;

namespace Lab10Negocios.Models;

public partial class Pais
{
    public int Idpais { get; set; }

    public string NombrePais { get; set; } = null!;

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<Proveedor> Proveedores { get; set; } = new List<Proveedor>();
}
