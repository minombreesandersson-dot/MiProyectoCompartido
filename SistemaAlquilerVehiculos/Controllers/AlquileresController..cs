
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAlquilerVehiculos.Data;
using SistemaAlquilerVehiculos.Models;

namespace SistemaAlquilerVehiculos.Controllers
{
    public class AlquileresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlquileresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Alquileres
        public async Task<IActionResult> Index()
        {
            var alquileres = await _context.Alquileres
                .Include(a => a.Cliente)
                .Include(a => a.ModeloVehiculo)
                    .ThenInclude(m => m!.Marca)
                .ToListAsync();

            return View(alquileres);
        }

        // GET: Alquileres/Create
        [HttpGet]
        public IActionResult Create()
        {
            CargarListasDesplegables();
            return View();
        }

        // GET: Alquileres/GetModelosPorMarca
        [HttpGet]
        public async Task<JsonResult> GetModelosPorMarca(int marcaId)
        {
            var modelos = await _context.Modelos
                .Where(m => m.MarcaId == marcaId)
                .Select(m => new
                {
                    id = m.Id,
                    nombre = $"{m.Nombre} (Año {m.Anio}) - S/ {m.PrecioDia}/día",
                    precioDia = m.PrecioDia
                })
                .ToListAsync();

            return Json(modelos);
        }

        // POST: Alquileres/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int marcaId,
            Alquiler alquiler)
        {
            // Validar fechas
            if (alquiler.FechaFin < alquiler.FechaInicio)
            {
                ModelState.AddModelError(
                    nameof(alquiler.FechaFin),
                    "La fecha de fin no puede ser anterior a la fecha de inicio.");
            }

            // Validar descuento
            if (alquiler.DescuentoPorcentaje < 0 ||
                alquiler.DescuentoPorcentaje > 100)
            {
                ModelState.AddModelError(
                    nameof(alquiler.DescuentoPorcentaje),
                    "El descuento debe estar entre 0 y 100 %.");
            }

            // Verificar que el modelo exista
            var modelo = await _context.Modelos
                .FirstOrDefaultAsync(
                    m => m.Id == alquiler.ModeloVehiculoId);

            if (modelo == null)
            {
                ModelState.AddModelError(
                    nameof(alquiler.ModeloVehiculoId),
                    "El modelo seleccionado no existe.");
            }
            else if (modelo.MarcaId != marcaId)
            {
                ModelState.AddModelError(
                    nameof(alquiler.ModeloVehiculoId),
                    "El modelo seleccionado no pertenece a la marca indicada.");
            }

            // Verificar que el cliente exista
            bool clienteExiste = await _context.Clientes
                .AnyAsync(c => c.Id == alquiler.ClienteId);

            if (!clienteExiste)
            {
                ModelState.AddModelError(
                    nameof(alquiler.ClienteId),
                    "El cliente seleccionado no existe.");
            }

            if (ModelState.IsValid && modelo != null)
            {
                int dias =
                    (alquiler.FechaFin.Date -
                     alquiler.FechaInicio.Date).Days + 1;

                decimal subtotal = dias * modelo.PrecioDia;

                decimal importeDescuento =
                    subtotal *
                    (alquiler.DescuentoPorcentaje / 100m);

                decimal total = subtotal - importeDescuento;

                alquiler.Dias = dias;
                alquiler.PrecioDiaAplicado = modelo.PrecioDia;
                alquiler.CostoTotal = total;
                alquiler.Estado = "Registrado";

                _context.Alquileres.Add(alquiler);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Recargar listas si hay errores
            CargarListasDesplegables(
                marcaId,
                alquiler.ModeloVehiculoId,
                alquiler.ClienteId);

            return View(alquiler);
        }

        // GET: Alquileres/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alquiler = await _context.Alquileres
                .Include(a => a.Cliente)
                .Include(a => a.ModeloVehiculo)
                    .ThenInclude(m => m!.Marca)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alquiler == null)
            {
                return NotFound();
            }

            return View(alquiler);
        }

        private void CargarListasDesplegables(
            int? marcaId = null,
            int? modeloId = null,
            int? clienteId = null)
        {
            ViewBag.MarcaId = new SelectList(
                _context.Marcas,
                "Id",
                "Nombre",
                marcaId);

            ViewBag.ClienteId = new SelectList(
                _context.Clientes.Select(c => new
                {
                    Id = c.Id,
                    NombreCompleto =
                        c.Nombres + " " +
                        c.Apellidos + " (" +
                        c.Documento + ")"
                }),
                "Id",
                "NombreCompleto",
                clienteId);

            if (marcaId.HasValue)
            {
                ViewBag.ModeloVehiculoId = new SelectList(
                    _context.Modelos.Where(
                        m => m.MarcaId == marcaId.Value),
                    "Id",
                    "Nombre",
                    modeloId);
            }
            else
            {
                ViewBag.ModeloVehiculoId =
                    new SelectList(
                        Enumerable.Empty<SelectListItem>(),
                        "Value",
                        "Text");
            }
        }
    }
}
