using Lab10Negocios.Data;
using Lab10Negocios.Services;
using Lab10Negocios.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Lab10Negocios.Controllers;

public class PedidosController(NegociosContext db, PedidoService pedidos) : Controller
{
    // RF09: pedidos registrados, los más recientes primero
    public async Task<IActionResult> Index(string? idCliente)
    {
        var consulta = db.Pedidos.AsNoTracking();
        if (!string.IsNullOrEmpty(idCliente)) consulta = consulta.Where(p => p.IdCliente == idCliente);
        ViewBag.Clientes = new SelectList(await db.Clientes.OrderBy(c => c.NombreCia).ToListAsync(), "IdCliente", "NombreCia", idCliente);
        return View(await consulta.OrderByDescending(p => p.IdPedido).Take(20)
            .Select(p => new PedidoFilaVM(p.IdPedido, p.FechaPedido,
                p.IdClienteNavigation != null ? p.IdClienteNavigation.NombreCia : "(sin cliente)",
                p.Detallepedidos.Count,
                p.Detallepedidos.Sum(d => d.PrecioUnidad * d.Cantidad * (1 - (decimal)d.Descuento))))
            .ToListAsync());
    }

    // RF08 y RF09: cabecera, detalle y total
    public async Task<IActionResult> Details(int id)
    {
        var pedido = await pedidos.ObtenerAsync(id);
        return pedido is null ? NotFound() : View(pedido);
    }

    public async Task<IActionResult> Create()
    {
        await CargarListas();
        return View(new PedidoCrearVM { Lineas = [new LineaPedidoVM()] });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PedidoCrearVM vm)
    {
        // Filas vacías del formulario (sin producto ni cantidad) no cuentan como detalle ni se validan
        for (var i = 0; i < vm.Lineas.Count; i++)
            if (vm.Lineas[i].IdProducto is null && vm.Lineas[i].Cantidad is null)
                foreach (var clave in ModelState.Keys.Where(k => k.StartsWith($"Lineas[{i}].")).ToList()) ModelState.Remove(clave);
        vm.Lineas = vm.Lineas.Where(l => l.IdProducto is not null || l.Cantidad is not null).ToList();
        if (vm.Lineas.Count == 0) ModelState.AddModelError("", "El pedido debe tener al menos un producto.");

        if (ModelState.IsValid)
        {
            var resultado = await pedidos.RegistrarAsync(vm);
            if (resultado.Exito)
            {
                TempData["Exito"] = $"Pedido N.º {resultado.IdPedido} registrado correctamente.";
                return RedirectToAction(nameof(Details), new { id = resultado.IdPedido });
            }
            foreach (var e in resultado.Errores) ModelState.AddModelError("", e);
        }

        // Si algo falla se conserva lo que el usuario ingresó
        if (vm.Lineas.Count == 0) vm.Lineas.Add(new LineaPedidoVM());
        await CargarListas();
        return View(vm);
    }

    private async Task CargarListas()
    {
        ViewBag.Clientes = new SelectList(await db.Clientes.Where(c => c.Estado == "A").OrderBy(c => c.NombreCia).ToListAsync(), "IdCliente", "NombreCia");
        ViewBag.Empleados = new SelectList(await db.Empleados.Where(e => e.Estado == "A").OrderBy(e => e.ApeEmpleado)
            .Select(e => new { e.IdEmpleado, Nombre = e.NomEmpleado + " " + e.ApeEmpleado }).ToListAsync(), "IdEmpleado", "Nombre");
        ViewBag.Productos = await db.Productos.OrderBy(p => p.NombreProducto)
            .Select(p => new ProductoOpcionVM(p.IdProducto, p.NombreProducto, p.PrecioUnidad, p.UnidadesEnExistencia)).ToListAsync();
    }
}

public record PedidoFilaVM(int IdPedido, DateTime FechaPedido, string Cliente, int Productos, decimal Total);
public record ProductoOpcionVM(int IdProducto, string Nombre, decimal Precio, short Stock);
