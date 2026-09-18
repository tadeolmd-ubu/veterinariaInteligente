using Microsoft.EntityFrameworkCore;
using veterinariaInteligente.Models;

namespace veterinariaInteligente.Data;

public class AppDbContext : DbContext
{
    public DbSet<Duenio> Duenios => Set<Duenio>();
    public DbSet<Profesional> Profesionales => Set<Profesional>();
    public DbSet<Mascota> Mascotas => Set<Mascota>();
    public DbSet<Consulta> Consultas => Set<Consulta>();
    
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Duenio>(e =>
        {
            e.ToTable("Duenio");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            e.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(15).IsRequired();
            e.Property(x => x.Correo).HasColumnName("correo").HasMaxLength(100);
            e.Property(x => x.Direccion).HasColumnName("direccion").HasMaxLength(255);
            e.Property(x => x.FechaArchivado).HasColumnName("fechaArchivado");

            e.HasIndex(x => x.Nombre);
            e.HasIndex(x => x.Telefono);
        });
        modelBuilder.Entity<Profesional>(e =>
        {
            e.ToTable("Profesional");
            e.HasKey(x => x.Id);
            
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(255).IsRequired();
            e.Property(x => x.Cedula).HasColumnName("cedula").HasMaxLength(255);
            e.Property(x => x.Activo).HasColumnName("activo");
            
            e.HasIndex(x => x.Nombre);
        });
        modelBuilder.Entity<MascotaEstado>(e =>
        {
            e.ToTable("MascotaEstado");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        });
        modelBuilder.Entity<Mascota>(e =>
        {
            e.ToTable("Mascota");
            e.HasKey(x => x.Id);
            
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            e.Property(x => x.Raza).HasColumnName("raza").HasMaxLength(100);
            e.Property(x => x.Sexo).HasColumnName("sexo").HasMaxLength(15);
            e.Property(x => x.FechaNacimiento).HasColumnName("fechaNacimiento");
            e.Property(x => x.Color).HasColumnName("color").HasMaxLength(50);
            e.Property(x => x.Alergia).HasColumnName("alergia").HasMaxLength(255);
            e.Property(x => x.FechaArchivado).HasColumnName("fechaArchivado");

            e.Property(x => x.IdDuenio).HasColumnName("idDuenio");
            e.HasOne(x => x.Duenio)
                .WithMany(d => d.Mascotas)          // colección inversa en Duenio
                .HasForeignKey(x => x.IdDuenio);

            e.Property(x => x.IdEstado).HasColumnName("idEstado");
            e.HasOne(x => x.Estado)
                .WithMany(m => m.Mascotas)          // colección inversa en MascotaEstado
                .HasForeignKey(x => x.IdEstado);
        });
        modelBuilder.Entity<Consulta>(e =>
        {
            e.ToTable("Consulta");
            e.HasKey(x => x.Id);
            
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Motivo).HasColumnName("motivo").HasMaxLength(500).IsRequired();
            e.Property(x => x.Peso).HasColumnName("peso").HasColumnType("decimal(5,3)");
            e.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500).IsRequired();
            e.Property(x => x.Diagnostico).HasColumnName("diagnostico").HasMaxLength(500).IsRequired();
            e.Property(x => x.Tratamiento).HasColumnName("tratamiento").HasMaxLength(500).IsRequired();
            e.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>().HasMaxLength(25).IsRequired();
            
            e.Property(x => x.IdProfesional).HasColumnName("idProfesional");
            e.HasOne(x => x.Profesional)
                .WithMany(m => m.Consultas)          
                .HasForeignKey(x => x.IdProfesional);
            
            e.Property(x => x.IdMascota).HasColumnName("idMascota");
            e.HasOne(x => x.Mascota)
                .WithMany(m => m.Consultas)
                .HasForeignKey(x => x.IdMascota);
            e.Property(x => x.FechaCreacion).HasColumnName("fechaCreacion").IsRequired();
            e.Property(x => x.FechaModificacion).HasColumnName("fechaModificacion").IsRequired();
        });
    }
}