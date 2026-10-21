using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PruebaCrud.Data;
using PruebaCrud.Models;

namespace PruebaCrud.Controllers
{
    public class EfmigrationsLockController : Controller
    {
        private readonly AppDbContext _context;

        public EfmigrationsLockController(AppDbContext context)
        {
            _context = context;
        }

        // GET: EfmigrationsLock
        public async Task<IActionResult> Index()
        {
            return View(await _context.EfmigrationsLocks.ToListAsync());
        }

        // GET: EfmigrationsLock/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var efmigrationsLock = await _context.EfmigrationsLocks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (efmigrationsLock == null)
            {
                return NotFound();
            }

            return View(efmigrationsLock);
        }

        // GET: EfmigrationsLock/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EfmigrationsLock/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Timestamp")] EfmigrationsLock efmigrationsLock)
        {
            if (ModelState.IsValid)
            {
                _context.Add(efmigrationsLock);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(efmigrationsLock);
        }

        // GET: EfmigrationsLock/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var efmigrationsLock = await _context.EfmigrationsLocks.FindAsync(id);
            if (efmigrationsLock == null)
            {
                return NotFound();
            }
            return View(efmigrationsLock);
        }

        // POST: EfmigrationsLock/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Timestamp")] EfmigrationsLock efmigrationsLock)
        {
            if (id != efmigrationsLock.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(efmigrationsLock);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EfmigrationsLockExists(efmigrationsLock.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(efmigrationsLock);
        }

        // GET: EfmigrationsLock/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var efmigrationsLock = await _context.EfmigrationsLocks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (efmigrationsLock == null)
            {
                return NotFound();
            }

            return View(efmigrationsLock);
        }

        // POST: EfmigrationsLock/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var efmigrationsLock = await _context.EfmigrationsLocks.FindAsync(id);
            if (efmigrationsLock != null)
            {
                _context.EfmigrationsLocks.Remove(efmigrationsLock);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EfmigrationsLockExists(int id)
        {
            return _context.EfmigrationsLocks.Any(e => e.Id == id);
        }
    }
}
