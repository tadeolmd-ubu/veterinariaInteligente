using Microsoft.EntityFrameworkCore;
using veterinariaInteligente.Models;

namespace veterinariaInteligente.Data;

public class AppDbContext : DbContext
{
    public DbSet<Duenio> Duenios => Set<Duenio>();

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
    }
}