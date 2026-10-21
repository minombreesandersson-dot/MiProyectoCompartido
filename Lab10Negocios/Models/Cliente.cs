using System;
using System.Collections.Generic;

namespace Lab10Negocios.Models;

public partial class Cliente
{
    public string IdCliente { get; set; } = null!;

    public string NombreCia { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public int Idpais { get; set; }

    public string Telefono { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public virtual Pais IdpaisNavigation { get; set; } = null!;

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
