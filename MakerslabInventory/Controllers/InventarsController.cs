using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MakerslabInventory.Data;
using MakerslabInventory.Models;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml; // Nezabudni na tento using

namespace MakerslabInventory.Controllers

{
    [Authorize]
    public class InventarsController : Controller
    {
        private readonly MakerslabInventoryContext _context;

        public InventarsController(MakerslabInventoryContext context)
        {
            _context = context;
        }

        // TENTO KÓD NAHRADÍ CELÚ METÓDU INDEX
        public async Task<IActionResult> Index(string sortOrder, string searchString, StavInventara? inventoryState)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CategorySortParm"] = sortOrder == "Category" ? "category_desc" : "Category";
            ViewData["CurrentFilter"] = searchString;
            // NOVÉ: Uloženie aktuálneho filtra stavu
            ViewData["CurrentStateFilter"] = inventoryState;

            var inventar = from i in _context.Inventar
                           select i;

            // 1. APLIKÁCIA VYHĽADÁVANIA (Search)
            if (!String.IsNullOrEmpty(searchString))
            {
                inventar = inventar.Where(s => s.Nazov.Contains(searchString)
                                           || s.Kategoria.Contains(searchString)
                                           || s.Lokalita.Contains(searchString)
                                           || (s.ZapozicaneKomu != null && s.ZapozicaneKomu.Contains(searchString)));
            }

            // 2. NOVÉ: APLIKÁCIA FILTRA STAVU
            if (inventoryState.HasValue)
            {
                // Ak je vybraný stav iný ako 'NízkaZasoba'
                if (inventoryState.Value != StavInventara.NizkaZasoba)
                {
                    inventar = inventar.Where(i => i.Stav == inventoryState.Value);
                }
                else // Ak je vybraná 'Nízka zásoba' (musíme to prepočítať)
                {
                    inventar = inventar.Where(i => i.Mnozstvo <= i.MinMnozstvo);
                }
            }

            // 3. APLIKÁCIA TRIEDENIA (Sorting)
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
                    inventar = inventar.OrderBy(s => s.Nazov);
                    break;
            }

            return View(await inventar.ToListAsync());
        }
        

        // V súbore Controllers/InventarsController.cs

        [Authorize(Roles = "Admin,Ucitel")]
        public async Task<IActionResult> ExportToExcel()
        {
            // 1. Načítanie dát
            var inventar = await _context.Inventar.ToListAsync();
           

            using (var package = new ExcelPackage())
            {
                // 3. Vytvorenie pracovného listu
                var worksheet = package.Workbook.Worksheets.Add("Inventár");

                // 4. Pridanie hlavičiek (Headers)
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Názov";
                worksheet.Cells[1, 3].Value = "Kategória";
                worksheet.Cells[1, 4].Value = "Množstvo";
                worksheet.Cells[1, 5].Value = "Jednotka";
                worksheet.Cells[1, 6].Value = "Lokalita";
                worksheet.Cells[1, 7].Value = "Min. limit";
                worksheet.Cells[1, 8].Value = "Stav";
                worksheet.Cells[1, 9].Value = "Zapožičané komu";
                worksheet.Cells[1, 10].Value = "Dátum výpožičky";

                // 5. Naplnenie dát (Data)
                int row = 2;
                foreach (var item in inventar)
                {
                    worksheet.Cells[row, 1].Value = item.Id;
                    worksheet.Cells[row, 2].Value = item.Nazov;
                    worksheet.Cells[row, 3].Value = item.Kategoria;
                    worksheet.Cells[row, 4].Value = item.Mnozstvo;
                    worksheet.Cells[row, 5].Value = item.Jednotka;
                    worksheet.Cells[row, 6].Value = item.Lokalita;
                    worksheet.Cells[row, 7].Value = item.MinMnozstvo;
                    worksheet.Cells[row, 8].Value = item.Stav.ToString(); // Konverzia ENUM na text
                    worksheet.Cells[row, 9].Value = item.ZapozicaneKomu;
                    worksheet.Cells[row, 10].Value = item.DatumVypozicky?.ToString("yyyy-MM-dd"); // Formátovanie dátumu
                    row++;
                }

                // 6. Automatická šírka stĺpcov
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // 7. Vytvorenie a odoslanie súboru
                var content = package.GetAsByteArray();
                var fileName = $"Inventar_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                return File(
                    content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
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

        // ... existing code ...
        private bool InventarExists(int id)
        {
            return _context.Inventar.Any(e => e.Id == id);
        }

        // --- NOVÝ KÓD PRE VÝPOŽIČKY ZAČÍNA TU ---

        // GET: Inventars/Pozicat/5
        [Authorize(Roles = "Admin,Ucitel")]
        public async Task<IActionResult> Pozicat(int? id)
        {
            if (id == null) return NotFound();

            var inventar = await _context.Inventar.FindAsync(id);
            if (inventar == null) return NotFound();

            // Ak už je požičaný, vrátime užívateľa späť (alebo vyhodíme chybu)
            if (inventar.Stav == StavInventara.Vypožičaný)
            {
                return RedirectToAction(nameof(Index));
            }

            var vypozicka = new Vypozicka
            {
                InventarId = inventar.Id,
                Inventar = inventar // Aby sme vo View videli názov
            };

            return View(vypozicka);
        }

        // POST: Inventars/Pozicat
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Ucitel")]
        public async Task<IActionResult> Pozicat([Bind("InventarId,MenoPoziciavatela,Poznamka")] Vypozicka vypozicka)
        {
            // 1. Načítame inventár, ktorý chceme požičať
            var inventar = await _context.Inventar.FindAsync(vypozicka.InventarId);
            if (inventar == null) return NotFound();

            // Odstránime validáciu pre objekt Inventar, lebo ho neposielame z formulára celý
            ModelState.Remove("Inventar");

            if (ModelState.IsValid)
            {
                // 2. Nastavíme dátum a uložíme výpožičku
                vypozicka.DatumOd = DateTime.Now;
                _context.Vypozicka.Add(vypozicka);

                // 3. Zmeníme stav inventára na Vypožičaný
                inventar.Stav = StavInventara.Vypožičaný;
                inventar.ZapozicaneKomu = vypozicka.MenoPoziciavatela;
                inventar.DatumVypozicky = DateTime.Now;
                _context.Update(inventar);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // Ak nastala chyba, vrátime formulár (musíme znova načítať inventár pre zobrazenie názvu)
            vypozicka.Inventar = inventar;
            return View(vypozicka);
        }

        // POST: Inventars/Vratit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Ucitel")]
        public async Task<IActionResult> Vratit(int id)
        {
            var inventar = await _context.Inventar.FindAsync(id);
            if (inventar == null) return NotFound();

            // Nájdi otvorenú výpožičku pre tento predmet
            var aktivnaVypozicka = await _context.Vypozicka
                .FirstOrDefaultAsync(v => v.InventarId == id && v.DatumDo == null);

            if (aktivnaVypozicka != null)
            {
                // Uzavri výpožičku
                aktivnaVypozicka.DatumDo = DateTime.Now;
                _context.Update(aktivnaVypozicka);
            }

            // Resetuj stav inventára
            inventar.Stav = StavInventara.Dostupný;
            inventar.ZapozicaneKomu = null;
            inventar.DatumVypozicky = null;
            _context.Update(inventar);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Inventars/Historia/5
        public async Task<IActionResult> Historia(int id)
        {
            var inventar = await _context.Inventar.FindAsync(id);
            if (inventar == null) return NotFound();

            var historia = await _context.Vypozicka
                .Where(v => v.InventarId == id)
                .OrderByDescending(v => v.DatumOd)
                .ToListAsync();

            ViewData["NazovInventara"] = inventar.Nazov;
            return View(historia);
        }
        // --- KONIEC NOVÉHO KÓDU ---
    }
}