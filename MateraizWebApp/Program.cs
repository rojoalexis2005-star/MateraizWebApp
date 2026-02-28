using MateraizWebApp.Models;
using MateraizWebApp.Services;
using MateraizWebApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides; // 👈 Necesario para Render

var builder = WebApplication.CreateBuilder(args);

// ================================
// CONFIGURACIÓN DE PROXY (RENDER)
// ================================
// Esto ayuda a que la app entienda que está bajo HTTPS aunque Render use un proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ================================
// BASE DE DATOS
// ================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")));

// ================================
// IDENTITY + ROLES
// ================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ================================
// CONFIGURACIÓN DE COOKIES (PARA HTTPS)
// ================================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // 👈 Fuerza cookies seguras
    options.Cookie.SameSite = SameSiteMode.None; // 👈 Evita problemas de redirección
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// ================================
// EMAIL
// ================================
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// 👈 Activar los encabezados de proxy inmediatamente después del build
app.UseForwardedHeaders();

// ================================
// INICIALIZACIÓN DE ROLES (SCOPED)
// ================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await CrearRolesYAdmin(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al crear los roles.");
    }
}

// ================================
// MIDDLEWARE
// ================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

// ================================
// MÉTODO PARA CREAR ROLES Y ADMIN
// ================================
static async Task CrearRolesYAdmin(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    string adminEmail = "rojoalexis2005@gmail.com";
    string adminPassword = "Jonatan2005";

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    if (!await roleManager.RoleExistsAsync("Cliente"))
        await roleManager.CreateAsync(new IdentityRole("Cliente"));

    var user = await userManager.FindByEmailAsync(adminEmail);

    if (user == null)
    {
        user = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            Nombre = "Alexis"
        };

        var result = await userManager.CreateAsync(user, adminPassword);

        if (!result.Succeeded)
        {
            throw new Exception("Error creando usuario admin: " +
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    if (!await userManager.IsInRoleAsync(user, "Admin"))
        await userManager.AddToRoleAsync(user, "Admin");
}