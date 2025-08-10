using Microsoft.EntityFrameworkCore;
using ServicioComunal.Models;

namespace ServicioComunal.Data
{
    public class ServicioComunalDbContext : DbContext
    {
        public ServicioComunalDbContext(DbContextOptions<ServicioComunalDbContext> options) : base(options)
        {
        }

        // DbSets para cada entidad
        public DbSet<Profesor> Profesores { get; set; }
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Grupo> Grupos { get; set; }
        public DbSet<GrupoEstudiante> GruposEstudiantes { get; set; }
        public DbSet<Entrega> Entregas { get; set; }
        public DbSet<Formulario> Formularios { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de la clave compuesta para GrupoEstudiante
            modelBuilder.Entity<GrupoEstudiante>()
                .HasKey(ge => new { ge.EstudianteIdentificacion, ge.GrupoNumero });

            // Configurar claves primarias sin IDENTITY (auto-incremento)
            modelBuilder.Entity<Profesor>()
                .Property(p => p.Identificacion)
                .ValueGeneratedNever();

            modelBuilder.Entity<Estudiante>()
                .Property(e => e.Identificacion)
                .ValueGeneratedNever();

            modelBuilder.Entity<Usuario>()
                .Property(u => u.Identificacion)
                .ValueGeneratedNever();

            modelBuilder.Entity<Grupo>()
                .Property(g => g.Numero)
                .ValueGeneratedNever();

            // Configuración de relaciones para GrupoEstudiante
            modelBuilder.Entity<GrupoEstudiante>()
                .HasOne(ge => ge.Estudiante)
                .WithMany(e => e.GruposEstudiantes)
                .HasForeignKey(ge => ge.EstudianteIdentificacion)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GrupoEstudiante>()
                .HasOne(ge => ge.Grupo)
                .WithMany(g => g.GruposEstudiantes)
                .HasForeignKey(ge => ge.GrupoNumero)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GrupoEstudiante>()
                .HasOne(ge => ge.Profesor)
                .WithMany(p => p.GruposEstudiantes)
                .HasForeignKey(ge => ge.ProfesorIdentificacion)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de relaciones para Entrega
            modelBuilder.Entity<Entrega>()
                .HasOne(e => e.Grupo)
                .WithMany(g => g.Entregas)
                .HasForeignKey(e => e.GrupoNumero)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Entrega>()
                .HasOne(e => e.Profesor)
                .WithMany(p => p.Entregas)
                .HasForeignKey(e => e.ProfesorIdentificacion)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de relaciones para Formulario
            modelBuilder.Entity<Formulario>()
                .HasOne(f => f.Profesor)
                .WithMany(p => p.Formularios)
                .HasForeignKey(f => f.ProfesorIdentificacion)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de relaciones para Notificacion
            modelBuilder.Entity<Notificacion>()
                .HasOne(n => n.Grupo)
                .WithMany(g => g.Notificaciones)
                .HasForeignKey(n => n.GrupoNumero)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notificacion>()
                .HasOne(n => n.Profesor)
                .WithMany(p => p.Notificaciones)
                .HasForeignKey(n => n.ProfesorIdentificacion)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración adicional para Usuario (tabla independiente)
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.NombreUsuario)
                .IsUnique();

            // Usuario es una tabla completamente independiente
            // La relación con Profesor/Estudiante se maneja por código usando Identificacion

            // Configuraciones adicionales si es necesario
            // Por ejemplo, si necesitas configurar longitudes de string específicas:
            // modelBuilder.Entity<Profesor>()
            //     .Property(p => p.Nombre)
            //     .HasMaxLength(100);
        }
    }
}
