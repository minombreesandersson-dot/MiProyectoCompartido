using Lab10Negocios.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Lab10Negocios.Controllers;

// RF02: productos con precio y stock, búsqueda por nombre y filtro por categoría
public class ProductosController(NegociosContext db) : Controller
{
    public async Task<IActionResult> Index(string? buscar, int? idCategoria)
    {
        var consulta = db.Productos.Include(p => p.IdCategoriaNavigation).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar)) consulta = consulta.Where(p => p.NombreProducto.Contains(buscar));
        if (idCategoria is not null) consulta = consulta.Where(p => p.IdCategoria == idCategoria);

        ViewBag.Buscar = buscar;
        ViewBag.Categorias = new SelectList(await db.Categorias.OrderBy(c => c.NombreCategoria).ToListAsync(),
            "IdCategoria", "NombreCategoria", idCategoria);
        return View(await consulta.OrderBy(p => p.NombreProducto).ToListAsync());
    }
}
