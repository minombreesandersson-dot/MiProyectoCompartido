using System.ComponentModel.DataAnnotations;

namespace Lab10Negocios.ViewModels;

// Lo que envía el formulario de "Nuevo pedido". No se enlazan las entidades de EF directamente.
public class PedidoCrearVM
{
    [Required(ErrorMessage = "Seleccione un cliente.")]
    [Display(Name = "Cliente")]
    public string? IdCliente { get; set; }

    [Required(ErrorMessage = "Seleccione el empleado que registra el pedido.")]
    [Display(Name = "Empleado")]
    public int? IdEmpleado { get; set; }

    public List<LineaPedidoVM> Lineas { get; set; } = new();
}

public class LineaPedidoVM
{
    [Required(ErrorMessage = "Seleccione un producto.")]
    [Display(Name = "Producto")]
    public int? IdProducto { get; set; }

    [Required(ErrorMessage = "Ingrese la cantidad.")]
    [Range(1, short.MaxValue, ErrorMessage = "La cantidad debe ser un entero mayor que cero.")]
    public int? Cantidad { get; set; }

    // Solo para mostrar en pantalla: el servidor la ignora y usa el precio guardado en la base.
    public decimal? PrecioUnidad { get; set; }
}

// Para la confirmación y el detalle de un pedido (RF08 y RF09)
public record LineaDetalleVM(string Producto, decimal PrecioUnidad, short Cantidad, double Descuento)
{
    public decimal Subtotal => PrecioUnidad * Cantidad * (1 - (decimal)Descuento);
}

public record PedidoDetalleVM(int IdPedido, DateTime FechaPedido, string Cliente, string Empleado, List<LineaDetalleVM> Lineas)
{
    public decimal Total => Lineas.Sum(l => l.Subtotal);
}
