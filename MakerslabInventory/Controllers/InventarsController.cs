using MakerslabInventory.Data;
using MakerslabInventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MakerslabInventory.Controllers
{
    [Authorize(Roles = "Admin,Ucitel")]
    public class InventarsController : Controller
    {
        private readonly MakerslabInventoryContext _context;

        public InventarsController(MakerslabInventoryContext context)
        {
            _context = context;
        }

        // GET: Inventars
        // V súbore Controllers/InventarsController.cs

        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            // Uloženie aktuálneho triedenia a vyhľadávania pre zobrazenie v šablóne
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CategorySortParm"] = sortOrder == "Category" ? "category_desc" : "Category";
            ViewData["CurrentFilter"] = searchString;

            var inventar = from i in _context.Inventar
                           select i;

            // Aplikácia vyhľadávania (Search)
            if (!String.IsNullOrEmpty(searchString))
            {
                inventar = inventar.Where(s => s.Nazov.Contains(searchString)
                                           || s.Kategoria.Contains(searchString)
                                           || s.Lokalita.Contains(searchString)
                                           // Hľadanie v zapožičanom komu (ak nie je null)
                                           || (s.ZapozicaneKomu != null && s.ZapozicaneKomu.Contains(searchString)));
            }

            // Aplikácia triedenia (Sorting)
            switch (sortOrder)
            {
                case "name_desc":
                    inventar = inventar.OrderByDescending(s => s.Nazov);
                    break;
                case "Category":
                    inventar = inventar.OrderBy(s => s.Kategoria);
                    break;
                case "category_desc":
                    inventar = inventar.OrderByDescending(s => s.Kategoria);
                    break;
                default:
                    inventar = inventar.OrderBy(s => s.Nazov); // Predvolené triedenie podľa názvu (A-Z)
                    break;
            }

            return View(await inventar.ToListAsync());
        }

        // GET: Inventars/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Inventars/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nazov,Kategoria,Mnozstvo,Jednotka,Lokalita,MinMnozstvo,MaxMnozstvo,Stav,ZapozicaneKomu,DatumVypozicky")] Inventar inventar)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inventar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(inventar);
        }

        // GET: Inventars/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventar = await _context.Inventar.FindAsync(id);
            if (inventar == null)
            {
                return NotFound();
            }
            return View(inventar);
        }

        // POST: Inventars/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nazov,Kategoria,Mnozstvo,Jednotka,Lokalita,MinMnozstvo,MaxMnozstvo,Stav,ZapozicaneKomu,DatumVypozicky")] Inventar inventar)
        {
            if (id != inventar.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inventar);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InventarExists(inventar.Id))
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
            return View(inventar);
        }

        // GET: Inventars/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventar = await _context.Inventar
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inventar == null)
            {
                return NotFound();
            }

            return View(inventar);
        }

        // POST: Inventars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventar = await _context.Inventar.FindAsync(id);
            if (inventar != null)
            {
                _context.Inventar.Remove(inventar);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InventarExists(int id)
        {
            return _context.Inventar.Any(e => e.Id == id);
        }
    }
}
