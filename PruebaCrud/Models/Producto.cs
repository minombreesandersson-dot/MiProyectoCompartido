using System;
using System.Collections.Generic;

namespace PruebaCrud.Models;

public partial class Producto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal Precio { get; set; }
}
