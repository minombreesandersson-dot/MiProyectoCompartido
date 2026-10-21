using System;
using System.Collections.Generic;

namespace Lab10Negocios.Models;

public partial class Proveedor
{
    public int IdProveedor { get; set; }

    public string NombreCia { get; set; } = null!;

    public string NombreContacto { get; set; } = null!;

    public string CargoContacto { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public int Idpais { get; set; }

    public string Telefono { get; set; } = null!;

    public string Fax { get; set; } = null!;

    public virtual Pais IdpaisNavigation { get; set; } = null!;

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
