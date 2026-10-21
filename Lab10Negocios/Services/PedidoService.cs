using Lab10Negocios.Data;
using Lab10Negocios.Models;
using Lab10Negocios.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Lab10Negocios.Services;

public record ResultadoPedido(int? IdPedido, List<string> Errores)
{
    public bool Exito => IdPedido is not null;
}

// Reglas de negocio del pedido (sección 6 del laboratorio). Todo se valida aquí, en el servidor.
public class PedidoService(NegociosContext db, ILogger<PedidoService> log)
{
    public async Task<ResultadoPedido> RegistrarAsync(PedidoCrearVM vm)
    {
        var errores = new List<string>();

        var cliente = await db.Clientes.FindAsync(vm.IdCliente);
        if (cliente is null || cliente.Estado != "A") errores.Add("El cliente seleccionado no existe o no está activo.");
        var empleado = await db.Empleados.FindAsync(vm.IdEmpleado);
        if (empleado is null || empleado.Estado != "A") errores.Add("El empleado seleccionado no existe o no está activo.");

        // Al menos un detalle; el mismo producto repetido en dos filas se suma en una sola línea
        var lineas = vm.Lineas.Where(l => l.IdProducto is not null)
            .GroupBy(l => l.IdProducto!.Value)
            .Select(g => (IdProducto: g.Key, Cantidad: g.Sum(l => l.Cantidad ?? 0)))
            .ToList();
        if (lineas.Count == 0) errores.Add("El pedido debe tener al menos un producto.");
        if (lineas.Any(l => l.Cantidad <= 0)) errores.Add("Cada cantidad debe ser un entero mayor que cero.");
        if (lineas.Any(l => l.Cantidad > short.MaxValue)) errores.Add($"La cantidad máxima por producto es {short.MaxValue}.");

        // Precio y stock salen de la base, nunca del navegador
        var ids = lineas.Select(l => l.IdProducto).ToList();
        var productos = await db.Productos.Where(p => ids.Contains(p.IdProducto)).ToDictionaryAsync(p => p.IdProducto);
        foreach (var l in lineas)
        {
            if (!productos.TryGetValue(l.IdProducto, out var p)) errores.Add($"El producto {l.IdProducto} no existe.");
            else if (l.Cantidad > p.UnidadesEnExistencia)
                errores.Add($"Stock insuficiente para \"{p.NombreProducto}\": pidió {l.Cantidad} y hay {p.UnidadesEnExistencia} unidades.");
        }
        if (errores.Count > 0) return new(null, errores);

        // Cabecera + detalles + descuento de stock: todo o nada
        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            foreach (var l in lineas)
            {
                // UPDATE condicional: si otro usuario se llevó el stock entre la validación y este momento,
                // no se actualiza ninguna fila y el pedido completo se cancela (pregunta de reflexión 23).
                var filas = await db.Productos
                    .Where(p => p.IdProducto == l.IdProducto && p.UnidadesEnExistencia >= l.Cantidad)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.UnidadesEnExistencia, p => (short)(p.UnidadesEnExistencia - l.Cantidad)));
                if (filas == 0)
                {
                    await tx.RollbackAsync();
                    return new(null, [$"Stock insuficiente para \"{productos[l.IdProducto].NombreProducto}\": otro pedido acaba de usar esas unidades."]);
                }
            }

            // IdPedido no es IDENTITY en Negocios: se calcula bloqueando la tabla hasta el commit
            var idPedido = await db.Database
                .SqlQuery<int>($"SELECT ISNULL(MAX(IdPedido), 0) + 1 AS Value FROM PEDIDO WITH (UPDLOCK, HOLDLOCK)")
                .SingleAsync();

            db.Pedidos.Add(new Pedido
            {
                IdPedido = idPedido,
                IdCliente = cliente!.IdCliente,
                IdEmpleado = empleado!.IdEmpleado,
                FechaPedido = DateTime.Now,
                Envio = "0",
                Destinatario = cliente.NombreCia,
                DireccionDestinatario = cliente.Direccion,
                Detallepedidos = lineas.Select(l => new Detallepedido
                {
                    IdProducto = l.IdProducto,
                    PrecioUnidad = productos[l.IdProducto].PrecioUnidad,
                    Cantidad = (short)l.Cantidad,
                    Descuento = 0
                }).ToList()
            });
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return new(idPedido, []);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            log.LogError(ex, "Error al registrar el pedido del cliente {Cliente}", vm.IdCliente);
            return new(null, ["No se pudo registrar el pedido. No se guardó ningún cambio; inténtelo de nuevo."]);
        }
    }

    public async Task<PedidoDetalleVM?> ObtenerAsync(int id) =>
        await db.Pedidos.Where(p => p.IdPedido == id)
            .Select(p => new PedidoDetalleVM(p.IdPedido, p.FechaPedido,
                p.IdClienteNavigation != null ? p.IdClienteNavigation.NombreCia : "(sin cliente)",
                p.IdEmpleadoNavigation.NomEmpleado + " " + p.IdEmpleadoNavigation.ApeEmpleado,
                p.Detallepedidos.Select(d => new LineaDetalleVM(d.IdProductoNavigation.NombreProducto, d.PrecioUnidad, d.Cantidad, d.Descuento)).ToList()))
            .SingleOrDefaultAsync();
}
