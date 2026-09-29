/// <summary>
/// Punto de entrada principal de la aplicación del Sistema de Servicio Comunal.
/// Configura los servicios, middleware y la base de datos.
/// </summary>

using Microsoft.EntityFrameworkCore;
using ServicioComunal.Data;
using ServicioComunal.Services;

// Carga la cadena local desde .env solo si el proceso aún no recibió la variable.
const string connectionStringVariable = "ConnectionStrings__DefaultConnection";
if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(connectionStringVariable)))
{
    var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
    if (File.Exists(envFile))
    {
        foreach (var line in File.ReadLines(envFile))
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.Length == 0 || trimmedLine.StartsWith('#'))
                continue;

            var separator = trimmedLine.IndexOf('=');
            if (separator < 0 || !string.Equals(
                    trimmedLine[..separator].Trim(),
                    connectionStringVariable,
                    StringComparison.Ordinal))
                continue;

            var value = trimmedLine[(separator + 1)..].Trim();
            if (value.Length >= 2 &&
                ((value[0] == '"' && value[^1] == '"') ||
                 (value[0] == '\'' && value[^1] == '\'')))
                value = value[1..^1];

            if (!string.IsNullOrWhiteSpace(value))
                Environment.SetEnvironmentVariable(connectionStringVariable, value);

            break;
        }
    }
}

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        $"Falta la cadena de conexión. Configura {connectionStringVariable} en el entorno o en un archivo .env local.");

// Configuración de la base de datos con Entity Framework
builder.Services.AddDbContext<ServicioComunalDbContext>(options =>
    options.UseSqlServer(connectionString));

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
builder.Services.AddScoped<NotificacionService>();
builder.Services.AddScoped<RecordatorioService>();
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
