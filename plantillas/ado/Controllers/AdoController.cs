using Microsoft.AspNetCore.Mvc;
using __NOMBRE__.Data;

namespace __NOMBRE__.Controllers;

// Ejemplo de consulta con ADO.NET (sin EF): /Ado y /Ado?tabla=Productos
public class AdoController(AdoService ado) : Controller
{
    public IActionResult Index(string? tabla)
    {
        var tablas = ado.Tablas();
        ViewBag.Tablas = tablas;
        tabla ??= tablas.FirstOrDefault();
        // El nombre de la tabla no puede ir como parámetro SQL: se valida contra la lista real
        if (tabla is null || !tablas.Contains(tabla)) return View();

        ViewBag.Tabla = tabla;
        ViewBag.Total = ado.ContarFilas(tabla);
        return View(ado.Consultar($"SELECT * FROM \"{tabla}\" LIMIT 100"));
    }
}
