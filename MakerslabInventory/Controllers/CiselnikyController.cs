using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MakerslabInventory.Data;
using MakerslabInventory.Models;
using Microsoft.AspNetCore.Authorization;

namespace MakerslabInventory.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CiselnikyController : Controller
    {
        private readonly MakerslabInventoryContext _context;

        public CiselnikyController(MakerslabInventoryContext context)
        {
            _context = context;
        }

        // Zoznam kategórií a jednotiek
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Kategoria.ToListAsync();
            var units = await _context.Jednotka.ToListAsync();

            ViewData["Categories"] = categories;
            ViewData["Units"] = units;

            return View();
        }

        // Pridanie kategórie
        public IActionResult CreateKategoria() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateKategoria([Bind("Id,Nazov")] Kategoria kategoria)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kategoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(kategoria);
        }

        // Pridanie jednotky
        public IActionResult CreateJednotka() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJednotka([Bind("Id,Nazov")] Jednotka jednotka)
        {
            if (ModelState.IsValid)
            {
                _context.Add(jednotka);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(jednotka);
        }

        // Zmazanie kategórie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteKategoria(int id)
        {
            var kategoria = await _context.Kategoria.FindAsync(id);
            if (kategoria != null)
            {
                _context.Kategoria.Remove(kategoria);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Zmazanie jednotky
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJednotka(int id)
        {
            var jednotka = await _context.Jednotka.FindAsync(id);
            if (jednotka != null)
            {
                _context.Jednotka.Remove(jednotka);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}