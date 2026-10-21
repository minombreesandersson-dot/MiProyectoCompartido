using System;
using System.Collections.Generic;

namespace Lab10Negocios.Models;

public partial class Pedido
{
    public int IdPedido { get; set; }

    public string? IdCliente { get; set; }

    public int IdEmpleado { get; set; }

    public DateTime FechaPedido { get; set; }

    public DateTime? FechaEntrega { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public string? Envio { get; set; }

    public decimal? Cargo { get; set; }

    public string? Destinatario { get; set; }

    public string? DireccionDestinatario { get; set; }

    public string? CiudadDestinatario { get; set; }

    public string? RegionDestinatario { get; set; }

    public string? CodPostalDestinatario { get; set; }

    public string? PaisDestinatario { get; set; }

    public virtual ICollection<Detallepedido> Detallepedidos { get; set; } = new List<Detallepedido>();

    public virtual Cliente? IdClienteNavigation { get; set; }

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;
}
