using System;
using System.Collections.Generic;

namespace Lab10Negocios.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    public string NombreProducto { get; set; } = null!;

    public int IdProveedor { get; set; }

    public int IdCategoria { get; set; }

    public string? Umedida { get; set; }

    public decimal PrecioUnidad { get; set; }

    public short UnidadesEnExistencia { get; set; }

    public virtual ICollection<Detallepedido> Detallepedidos { get; set; } = new List<Detallepedido>();

    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;
}
