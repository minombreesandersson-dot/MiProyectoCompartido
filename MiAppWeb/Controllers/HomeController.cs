using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MiAppWeb.Data;
using MiAppWeb.Models;

namespace MiAppWeb.Controllers;

public class HomeController : Controller
{
    private readonly AplicationDbContext _context;

    public HomeController(AplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        if (!_context.Productos.Any())
        {
            _context.Productos.Add(new Producto { Nombre = "Computadora Portátil", Precio = 850.00m });
            _context.Productos.Add(new Producto { Nombre = "Teclado Mecánico RGB", Precio = 75.50m });
            _context.SaveChanges();
        }

        var listaProductos = _context.Productos.ToList();
        return View(listaProductos);
    }
}
