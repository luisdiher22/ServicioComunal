/// <summary>
/// Punto de entrada principal de la aplicación del Sistema de Servicio Comunal.
/// Configura los servicios, middleware y la base de datos.
/// </summary>

using Microsoft.EntityFrameworkCore;
using ServicioComunal.Data;
using ServicioComunal.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuración de la base de datos con Entity Framework
builder.Services.AddDbContext<ServicioComunalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuración de servicios MVC
builder.Services.AddControllersWithViews();

// Configuración de sesiones para autenticación
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiración de sesión
    options.Cookie.HttpOnly = true; // Seguridad de cookies
    options.Cookie.IsEssential = true;
});

// Registro de servicios personalizados
builder.Services.AddScoped<DataSeederService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<IPdfFillerService, PdfFillerService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configuración del pipeline de procesamiento de solicitudes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // HTTP Strict Transport Security
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Middleware de sesión para autenticación
app.UseSession();
app.UseAuthorization();

// Configuración de rutas por defecto (inicia en Login)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

// Inicialización de datos semilla al iniciar la aplicación
using (var scope = app.Services.CreateScope())
{
    var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeederService>();
    await dataSeeder.SeedDataAsync();
}

app.Run();
