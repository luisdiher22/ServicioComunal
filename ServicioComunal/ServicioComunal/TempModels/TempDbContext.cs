using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ServicioComunal.TempModels;

public partial class TempDbContext : DbContext
{
    public TempDbContext()
    {
    }

    public TempDbContext(DbContextOptions<TempDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Entrega> Entregas { get; set; }

    public virtual DbSet<Estudiante> Estudiantes { get; set; }

    public virtual DbSet<Formulario> Formularios { get; set; }

    public virtual DbSet<Grupo> Grupos { get; set; }

    public virtual DbSet<GrupoProfesor> GrupoProfesors { get; set; }

    public virtual DbSet<Notificacion> Notificacions { get; set; }

    public virtual DbSet<Profesor> Profesors { get; set; }

    public virtual DbSet<Solicitud> Solicituds { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=ServicioComunalDB;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entrega>(entity =>
        {
            entity.HasKey(e => e.Identificacion);

            entity.ToTable("ENTREGA");

            entity.HasIndex(e => e.FormularioIdentificacion, "IX_ENTREGA_FORMULARIO_Identificacion");

            entity.HasIndex(e => e.GrupoNumero, "IX_ENTREGA_GRUPO_Numero");

            entity.HasIndex(e => e.ProfesorIdentificacion, "IX_ENTREGA_PROFESOR_Identificacion");

            entity.Property(e => e.FormularioIdentificacion).HasColumnName("FORMULARIO_Identificacion");
            entity.Property(e => e.GrupoNumero).HasColumnName("GRUPO_Numero");
            entity.Property(e => e.ProfesorIdentificacion).HasColumnName("PROFESOR_Identificacion");

            entity.HasOne(d => d.FormularioIdentificacionNavigation).WithMany(p => p.Entregas).HasForeignKey(d => d.FormularioIdentificacion);

            entity.HasOne(d => d.GrupoNumeroNavigation).WithMany(p => p.Entregas)
                .HasForeignKey(d => d.GrupoNumero)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.ProfesorIdentificacionNavigation).WithMany(p => p.Entregas).HasForeignKey(d => d.ProfesorIdentificacion);
        });

        modelBuilder.Entity<Estudiante>(entity =>
        {
            entity.HasKey(e => e.Identificacion);

            entity.ToTable("ESTUDIANTE");

            entity.Property(e => e.Identificacion).ValueGeneratedNever();

            entity.HasMany(d => d.GrupoNumeros).WithMany(p => p.EstudianteIdentificacions)
                .UsingEntity<Dictionary<string, object>>(
                    "GrupoEstudiante",
                    r => r.HasOne<Grupo>().WithMany()
                        .HasForeignKey("GrupoNumero")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    l => l.HasOne<Estudiante>().WithMany()
                        .HasForeignKey("EstudianteIdentificacion")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey("EstudianteIdentificacion", "GrupoNumero");
                        j.ToTable("GRUPO_ESTUDIANTE");
                        j.HasIndex(new[] { "GrupoNumero" }, "IX_GRUPO_ESTUDIANTE_GRUPO_Numero");
                        j.IndexerProperty<int>("EstudianteIdentificacion").HasColumnName("ESTUDIANTE_Identificacion");
                        j.IndexerProperty<int>("GrupoNumero").HasColumnName("GRUPO_Numero");
                    });
        });

        modelBuilder.Entity<Formulario>(entity =>
        {
            entity.HasKey(e => e.Identificacion);

            entity.ToTable("FORMULARIO");

            entity.HasIndex(e => e.ProfesorIdentificacion, "IX_FORMULARIO_PROFESOR_Identificacion");

            entity.Property(e => e.ProfesorIdentificacion).HasColumnName("PROFESOR_Identificacion");

            entity.HasOne(d => d.ProfesorIdentificacionNavigation).WithMany(p => p.Formularios)
                .HasForeignKey(d => d.ProfesorIdentificacion)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Grupo>(entity =>
        {
            entity.HasKey(e => e.Numero);

            entity.ToTable("GRUPO");

            entity.HasIndex(e => e.LiderIdentificacion, "IX_GRUPO_LiderIdentificacion");

            entity.Property(e => e.Numero).ValueGeneratedNever();

            entity.HasOne(d => d.LiderIdentificacionNavigation).WithMany(p => p.Grupos).HasForeignKey(d => d.LiderIdentificacion);
        });

        modelBuilder.Entity<GrupoProfesor>(entity =>
        {
            entity.HasKey(e => new { e.GrupoNumero, e.ProfesorIdentificacion });

            entity.ToTable("GRUPO_PROFESOR");

            entity.HasIndex(e => e.ProfesorIdentificacion, "IX_GRUPO_PROFESOR_PROFESOR_Identificacion");

            entity.Property(e => e.GrupoNumero).HasColumnName("GRUPO_Numero");
            entity.Property(e => e.ProfesorIdentificacion).HasColumnName("PROFESOR_Identificacion");

            entity.HasOne(d => d.GrupoNumeroNavigation).WithMany(p => p.GrupoProfesors)
                .HasForeignKey(d => d.GrupoNumero)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.ProfesorIdentificacionNavigation).WithMany(p => p.GrupoProfesors)
                .HasForeignKey(d => d.ProfesorIdentificacion)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.HasKey(e => e.Identificacion);

            entity.ToTable("NOTIFICACION");

            entity.HasIndex(e => e.GrupoNumero, "IX_NOTIFICACION_GRUPO_Numero");

            entity.HasIndex(e => e.ProfesorIdentificacion, "IX_NOTIFICACION_PROFESOR_Identificacion");

            entity.Property(e => e.GrupoNumero).HasColumnName("GRUPO_Numero");
            entity.Property(e => e.ProfesorIdentificacion).HasColumnName("PROFESOR_Identificacion");

            entity.HasOne(d => d.GrupoNumeroNavigation).WithMany(p => p.Notificacions)
                .HasForeignKey(d => d.GrupoNumero)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.ProfesorIdentificacionNavigation).WithMany(p => p.Notificacions)
                .HasForeignKey(d => d.ProfesorIdentificacion)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Profesor>(entity =>
        {
            entity.HasKey(e => e.Identificacion);

            entity.ToTable("PROFESOR");

            entity.Property(e => e.Identificacion).ValueGeneratedNever();
        });

        modelBuilder.Entity<Solicitud>(entity =>
        {
            entity.ToTable("SOLICITUD");

            entity.HasIndex(e => e.EstudianteDestinatarioId, "IX_SOLICITUD_EstudianteDestinatarioId");

            entity.HasIndex(e => e.EstudianteRemitenteId, "IX_SOLICITUD_EstudianteRemitenteId");

            entity.HasIndex(e => e.GrupoNumero, "IX_SOLICITUD_GrupoNumero");

            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.Mensaje).HasMaxLength(500);
            entity.Property(e => e.Tipo).HasMaxLength(50);

            entity.HasOne(d => d.EstudianteDestinatario).WithMany(p => p.SolicitudEstudianteDestinatarios)
                .HasForeignKey(d => d.EstudianteDestinatarioId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.EstudianteRemitente).WithMany(p => p.SolicitudEstudianteRemitentes)
                .HasForeignKey(d => d.EstudianteRemitenteId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.GrupoNumeroNavigation).WithMany(p => p.Solicituds).HasForeignKey(d => d.GrupoNumero);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Identificacion);

            entity.ToTable("USUARIO");

            entity.HasIndex(e => e.Usuario1, "IX_USUARIO_Usuario").IsUnique();

            entity.Property(e => e.Identificacion).ValueGeneratedNever();
            entity.Property(e => e.Contraseña).HasMaxLength(255);
            entity.Property(e => e.Rol).HasMaxLength(20);
            entity.Property(e => e.Usuario1)
                .HasMaxLength(50)
                .HasColumnName("Usuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
