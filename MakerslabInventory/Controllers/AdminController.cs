using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MakerslabInventory.Models;

namespace MakerslabInventory.Controllers
{
    [Authorize(Roles = "Admin")] // Len Admin sem má prístup
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Zoznam všetkých používateľov a ich rolí
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var thisViewModel = new UserRolesViewModel();
                thisViewModel.UserId = user.Id;
                thisViewModel.Email = user.Email;
                thisViewModel.Roles = await _userManager.GetRolesAsync(user);
                userRolesViewModel.Add(thisViewModel);
            }

            return View(userRolesViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(role))
            {
                ModelState.AddModelError("role", "Rola je povinná.");
                return RedirectToAction(nameof(Index));
            }

            // Overíme, že rola existuje
            if (!await _roleManager.RoleExistsAsync(role))
            {
                ModelState.AddModelError("role", $"Rola '{role}' neexistuje.");
                return RedirectToAction(nameof(Index));
            }

            // Najprv odstránime všetky existujúce roly (aby nemal naraz Ziak aj Ucitel)
            // Ak chceš povoliť viac rolí naraz, tento krok vynechaj
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // Pridáme novú rolu
            await _userManager.AddToRoleAsync(user, role);

            return RedirectToAction(nameof(Index));
        }
    }
}