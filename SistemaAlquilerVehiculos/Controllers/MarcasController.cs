using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAlquilerVehiculos.Data;
using SistemaAlquilerVehiculos.Models;

namespace SistemaAlquilerVehiculos.Controllers
{
    public class MarcasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MarcasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Listado de Marcas y sus Modelos
        public async Task<IActionResult> Index()
        {
            var marcas = await _context.Marcas.Include(m => m.Modelos).ToListAsync();
            return View(marcas);
        }

        // GET: Marcas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Marcas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Marca marca)
        {
            // Validar que no exista otra marca con el mismo nombre (sin distinguir mayúsculas/minúsculas)
            if (await _context.Marcas.AnyAsync(m => m.Nombre.ToLower() == marca.Nombre.ToLower()))
            {
                ModelState.AddModelError("Nombre", "Ya existe una marca registrada con este nombre.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(marca);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(marca);
        }
    }
}