using Microsoft.EntityFrameworkCore;
using ServicioComunal.Data;
using ServicioComunal.Services;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios de Entity Framework
builder.Services.AddDbContext<ServicioComunalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Agregar servicios de MVC
builder.Services.AddControllersWithViews();

// Agregar servicios de sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Registrar servicios personalizados
builder.Services.AddScoped<DataSeederService>();
builder.Services.AddScoped<UsuarioService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Usar sesiones
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

// Ejecutar el seeder de datos
using (var scope = app.Services.CreateScope())
{
    var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeederService>();
    await dataSeeder.SeedDataAsync();
}
//Prueba 
app.Run();
