using System;
using System.Collections.Generic;

namespace Lab10Negocios.Models;

public partial class Detallepedido
{
    public int IdPedido { get; set; }

    public int IdProducto { get; set; }

    public decimal PrecioUnidad { get; set; }

    public short Cantidad { get; set; }

    public double Descuento { get; set; }

    public virtual Pedido IdPedidoNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
