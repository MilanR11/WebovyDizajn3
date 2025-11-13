using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MakerslabInventory.Data;
using Microsoft.AspNetCore.Identity;
using OfficeOpenXml;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
// TENTO RIADOK M‘éE CH›BAç, PRIDAJ HO, ABY FUNGOVALO LicenseContext


// TENTO USING JE NOV›: Pre RoleManager a UserManager, ktorÈ pouûÌvaö niûöie
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Pridanie DbContext pre invent·r a Identity
builder.Services.AddDbContext<MakerslabInventoryContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MakerslabInventoryContext") ?? throw new InvalidOperationException("Connection string 'MakerslabInventoryContext' not found.")));

// Opraven· konfigur·cia Identity: Pouûitie AddIdentity na spr·vne povolenie rolÌ
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<MakerslabInventoryContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

// Pridanie ostatn˝ch sluûieb
// ...
// Pridanie sluûieb do kontajnera.
builder.Services.AddControllersWithViews();
// ... Ôalöie builder.Services.Add...

// ----------------------------------------------------
// KONE»N… NASTAVENIE PRE EPPLUS - Nach·dza sa na spr·vnom mieste
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
// ----------------------------------------------------

var app = builder.Build();

// K”D NA INICIALIZ¡CIU A PRIRADENIE ROLÕ
using (var scope = app.Services.CreateScope())
{
    // POZN¡MKA: RoleManager a UserManager s˙ teraz dostupnÈ vÔaka NOV…MU usingu
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // 1. Definovanie rolÌ
    string[] roleNames = { "Admin", "Ucitel", "Ziak" };
    foreach (var roleName in roleNames)
    {
        if (!roleManager.RoleExistsAsync(roleName).Result)
        {
            roleManager.CreateAsync(new IdentityRole(roleName)).Wait();
        }
    }

    // 2. Priradenie roly Admin prvÈmu pouûÌvateæovi
    string adminEmail = "milanrabcan@gmail.com";
    var adminUser = userManager.FindByEmailAsync(adminEmail).Result;

    if (adminUser != null && !userManager.IsInRoleAsync(adminUser, "Admin").Result)
    {
        userManager.AddToRoleAsync(adminUser, "Admin").Wait();
    }
}
// KONIEC K”DU PRE ROLY

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// MIDDLEWARE (Spr·vne poradie)
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages(); // Pre Identity str·nky (Register, Login)

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();