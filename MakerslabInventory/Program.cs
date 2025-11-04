using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MakerslabInventory.Data;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Pridanie DbContext pre inventár a Identity
builder.Services.AddDbContext<MakerslabInventoryContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MakerslabInventoryContext") ?? throw new InvalidOperationException("Connection string 'MakerslabInventoryContext' not found.")));

// Opravená konfigurácia Identity: Použitie AddIdentity na správne povolenie rolí
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<MakerslabInventoryContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

// Pridanie ostatných služieb
builder.Services.AddControllersWithViews();

var app = builder.Build();

// KÓD NA INICIALIZÁCIU A PRIRADENIE ROLÍ
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // 1. Definovanie rolí
    string[] roleNames = { "Admin", "Ucitel", "Ziak" };
    foreach (var roleName in roleNames)
    {
        if (!roleManager.RoleExistsAsync(roleName).Result)
        {
            roleManager.CreateAsync(new IdentityRole(roleName)).Wait();
        }
    }

    // 2. Priradenie roly Admin prvému používate¾ovi
    string adminEmail = "milanrabcan@gmail.com";
    var adminUser = userManager.FindByEmailAsync(adminEmail).Result;

    if (adminUser != null && !userManager.IsInRoleAsync(adminUser, "Admin").Result)
    {
        userManager.AddToRoleAsync(adminUser, "Admin").Wait();
    }
}
// KONIEC KÓDU PRE ROLY

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

app.MapRazorPages(); // Pre Identity stránky (Register, Login)

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();