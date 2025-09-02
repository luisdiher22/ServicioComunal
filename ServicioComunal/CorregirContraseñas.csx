using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ServicioComunal.Data;
using ServicioComunal.Utilities;

// Script temporal para corregir contraseñas
var connectionString = "Data Source=.;Initial Catalog=ServicioComunalDB;Integrated Security=true;TrustServerCertificate=True";

var optionsBuilder = new DbContextOptionsBuilder<ServicioComunalDbContext>();
optionsBuilder.UseSqlServer(connectionString);

using var context = new ServicioComunalDbContext(optionsBuilder.Options);

Console.WriteLine("Verificando contraseñas en la base de datos...");

var usuarios = await context.Usuarios.ToListAsync();
Console.WriteLine($"Total de usuarios encontrados: {usuarios.Count}");

foreach (var usuario in usuarios)
{
    Console.WriteLine($"Usuario: {usuario.NombreUsuario} (ID: {usuario.Identificacion})");
    Console.WriteLine($"  Contraseña length: {usuario.Contraseña?.Length ?? 0}");
    Console.WriteLine($"  Tiene signo $: {usuario.Contraseña?.Contains("$") ?? false}");
    Console.WriteLine($"  Verificación hash: {PasswordHelper.VerifyPassword(usuario.Identificacion.ToString(), usuario.Contraseña ?? "")}");
    
    // Si la contraseña es igual a la cédula (texto plano) o no tiene el formato hash correcto
    if (usuario.Contraseña == usuario.Identificacion.ToString() || 
        string.IsNullOrEmpty(usuario.Contraseña) || 
        !usuario.Contraseña.Contains("$"))
    {
        Console.WriteLine($"  ❌ Corrigiendo contraseña para {usuario.NombreUsuario}");
        usuario.Contraseña = PasswordHelper.HashPassword(usuario.Identificacion.ToString());
        usuario.RequiereCambioContraseña = true;
    }
    else
    {
        Console.WriteLine($"  ✅ Contraseña correcta para {usuario.NombreUsuario}");
    }
    Console.WriteLine();
}

await context.SaveChangesAsync();
Console.WriteLine("Contraseñas corregidas exitosamente.");

// Verificación final para Elena Castro
var elena = await context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario.Contains("elena") || u.Identificacion == 567890123);
if (elena != null)
{
    Console.WriteLine("=== VERIFICACIÓN FINAL ELENA CASTRO ===");
    Console.WriteLine($"Usuario: {elena.NombreUsuario}");
    Console.WriteLine($"Identificación: {elena.Identificacion}");
    Console.WriteLine($"Hash válido: {PasswordHelper.VerifyPassword("567890123", elena.Contraseña)}");
}
