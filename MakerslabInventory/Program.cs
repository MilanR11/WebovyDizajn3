using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MakerslabInventory.Data;
using Microsoft.AspNetCore.Identity;
using OfficeOpenXml;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
// TENTO RIADOK MԎE CH�BA�, PRIDAJ HO, ABY FUNGOVALO LicenseContext


// TENTO USING JE NOV�: Pre RoleManager a UserManager, ktor� pou��va� ni��ie
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Pridanie DbContext pre invent�r a Identity
builder.Services.AddDbContext<MakerslabInventoryContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MakerslabInventoryContext") ?? throw new InvalidOperationException("Connection string 'MakerslabInventoryContext' not found.")));

// Opraven� konfigur�cia Identity: Pou�itie AddIdentity na spr�vne povolenie rol�
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<MakerslabInventoryContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

// Pridanie ostatn�ch slu�ieb
// ...
// Pridanie slu�ieb do kontajnera.
builder.Services.AddControllersWithViews();
// ... �al�ie builder.Services.Add...

// ----------------------------------------------------
// KONE�N� NASTAVENIE PRE EPPLUS - Nach�dza sa na spr�vnom mieste
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
// ----------------------------------------------------

var app = builder.Build();

// K�D NA INICIALIZ�CIU A PRIRADENIE ROL�
using (var scope = app.Services.CreateScope())
{
    // POZN�MKA: RoleManager a UserManager s� teraz dostupn� v�aka NOV�MU usingu
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // 1. Definovanie rol�
    string[] roleNames = { "Admin", "Ucitel", "Ziak" };
    foreach (var roleName in roleNames)
    {
        if (!roleManager.RoleExistsAsync(roleName).Result)
        {
            roleManager.CreateAsync(new IdentityRole(roleName)).Wait();
        }
    }

    // 2. Priradenie roly Admin prv�mu pou��vate�ovi
    string adminEmail = "milanrabcan@gmail.com";
    var adminUser = userManager.FindByEmailAsync(adminEmail).Result;

    if (adminUser != null && !userManager.IsInRoleAsync(adminUser, "Admin").Result)
    {
        userManager.AddToRoleAsync(adminUser, "Admin").Wait();
    }
}
// KONIEC K�DU PRE ROLY

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// MIDDLEWARE (Spr�vne poradie)
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages(); // Pre Identity stránky

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inventars}/{action=Index}/{id?}"); // ZMENENÉ Z 'Home' NA 'Inventars'

app.Run();