using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MakerslabInventory.Data;
using Microsoft.AspNetCore.Identity;
using OfficeOpenXml;


var builder = WebApplication.CreateBuilder(args);

// Pridanie DbContext pre invent�r a Identity
builder.Services.AddDbContext<MakerslabInventoryContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MakerslabInventoryContext") ?? throw new InvalidOperationException("Connection string 'MakerslabInventoryContext' not found.")));

// Opraven� konfigur�cia Identity: Pou�itie AddIdentity na spr�vne povolenie rol�
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<MakerslabInventoryContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

// Pridanie služieb do kontajnera.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ----------------------------------------------------
// KONEČNÉ NASTAVENIE PRE EPPLUS - Nachádza sa na správnom mieste
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
// ----------------------------------------------------

var app = builder.Build();

// Automatická migrácia databázy a inicializácia rolí
using (var scope = app.Services.CreateScope())
{
    // Auto-apply EF Core migrations to keep DB schema up to date
    var db = scope.ServiceProvider.GetRequiredService<MakerslabInventoryContext>();
    db.Database.Migrate();

    // Získanie služieb pre správu rolí a používateľov
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Definovanie rolí
    string[] roleNames = { "Admin", "Ucitel", "Ziak" };
    foreach (var roleName in roleNames)
    {
        if (!roleManager.RoleExistsAsync(roleName).Result)
        {
            roleManager.CreateAsync(new IdentityRole(roleName)).Wait();
        }
    }

    // Priradenie roly Admin vybranému používateľovi
    string adminEmail = "milanrabcan@gmail.com";
    var adminUser = userManager.FindByEmailAsync(adminEmail).Result;

    if (adminUser != null && !userManager.IsInRoleAsync(adminUser, "Admin").Result)
    {
        userManager.AddToRoleAsync(adminUser, "Admin").Wait();
    }
}
// Koniec inicializácie rolí

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// MIDDLEWARE (Správne poradie)
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages(); // Pre Identity stránky

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inventars}/{action=Index}/{id?}"); // ZMENENÉ Z 'Home' NA 'Inventars'

app.Run();