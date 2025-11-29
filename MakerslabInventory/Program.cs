using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MakerslabInventory.Data;
using OfficeOpenXml;
using System.Diagnostics; // Potrebné pre Process.Start
using Microsoft.Extensions.Hosting; // Potrebné pre IHostApplicationLifetime

// Nastavenie pre System.Drawing (ak je relevantné pre váš systém)
System.AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);


var builder = WebApplication.CreateBuilder(args);

// Pridanie DbContext pre inventár a Identity
var connectionString = builder.Configuration.GetConnectionString("MakerslabInventoryContext");
builder.Services.AddDbContext<MakerslabInventoryContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Opravená konfigurácia Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<MakerslabInventoryContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

// Pridanie služieb do kontajnera.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// KONEČNÉ NASTAVENIE PRE EPPLUS
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var app = builder.Build();

// =========================================================================
// PRIDANÁ LOGIKA PRE AUTOMATICKÉ OTVORENIE PREHLIADAČA
// =========================================================================

// Získanie služby pre riadenie životného cyklu aplikácie
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

// Registrácia akcie, ktorá sa spustí po úspešnom naštartovaní hosta
lifetime.ApplicationStarted.Register(() =>
{
    // Zistenie URL, na ktorej aplikácia počúva (napr. http://localhost:5000)
    // Získame prvú URL zo zoznamu (ak ich je viac)
    var urls = app.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:5000";
    var url = urls.Split(';').First();

    try
    {
        // Spustenie predvoleného prehliadača s adresou aplikácie
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
    catch (Exception ex)
    {
        // Logovanie chyby, ak sa nepodarilo otvoriť prehliadač
        app.Logger.LogWarning(
            "Nepodarilo sa automaticky otvoriť prehliadač s URL {Url}: {Error}",
            url,
            ex.Message
        );
    }
});

// =========================================================================
// KONIEC LOGIKY PRE AUTOMATICKÉ OTVORENIE PREHLIADAČA
// =========================================================================


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

    if (adminUser == null)
    {
        var newAdmin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var result = userManager.CreateAsync(newAdmin, "Heslo123!").Result; // Nastavte si silné heslo
        if (result.Succeeded)
        {
            adminUser = newAdmin;
        }
    }

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