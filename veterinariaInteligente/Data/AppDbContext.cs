using Microsoft.EntityFrameworkCore;
using veterinariaInteligente.Models;

namespace veterinariaInteligente.Data;

public class AppDbContext : DbContext
{
    public DbSet<Duenio> Duenios => Set<Duenio>();
    public DbSet<Profesional> Profesionales => Set<Profesional>();
    public DbSet<Mascota> Mascotas => Set<Mascota>();
    public DbSet<Consulta> Consultas => Set<Consulta>();
    public DbSet<ConsultaVersion> ConsultaVersiones => Set<ConsultaVersion>();
    public DbSet<Vacuna> Vacunas => Set<Vacuna>();
    public DbSet<AplicacionVacuna> AplicacionVacunas => Set<AplicacionVacuna>();
    
    public DbSet<Seguimiento> Seguimientos => Set<Seguimiento>();
    
    
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
        modelBuilder.Entity<ConsultaVersion>(e =>
        {
            e.ToTable("ConsultaVersion");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Fecha).HasColumnName("fecha").HasColumnType("datetime").IsRequired();
            e.Property(x => x.Campo).HasColumnName("campo").HasMaxLength(100).IsRequired();
            e.Property(x => x.ValorAnterior).HasColumnName("valorAnterior").HasMaxLength(500);
            e.Property(x => x.ValorNuevo).HasColumnName("valorNuevo").HasMaxLength(500);
            
            e.Property(x => x.IdConsulta).HasColumnName("idConsulta");
            e.HasOne(x => x.Consulta)
                .WithMany(c => c.Versiones)     
                .HasForeignKey(x => x.IdConsulta);

            e.Property(x => x.IdProfesional).HasColumnName("idProfesional");
            e.HasOne(x => x.Profesional)
                .WithMany()                      
                .HasForeignKey(x => x.IdProfesional);
        });
        modelBuilder.Entity<Vacuna>(e =>
        {
            e.ToTable("Vacuna");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            e.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(255);
            e.Property(x => x.Activo).HasColumnName("activo");
            e.HasIndex(x => x.Nombre);
        });
        modelBuilder.Entity<AplicacionVacuna>(e =>
        {
            e.ToTable("VacunaAplicacion");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.FechaAplicacion).HasColumnName("fechaAplicacion").IsRequired();
            e.Property(x => x.Lote).HasColumnName("lote").HasMaxLength(20);
            e.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(255);
            e.Property(x => x.FechaProximaSugerida).HasColumnName("fechaProximaSugerida");
            
            e.Property(x => x.IdVacuna).HasColumnName("idVacuna");
            e.HasOne(x => x.Vacuna)
                .WithMany(v => v.Aplicaciones)
                .HasForeignKey(x => x.IdVacuna);

            e.Property(x => x.IdMascota).HasColumnName("idMascota");

            e.HasOne(x => x.Mascota)
                .WithMany(m => m.AplicacionesVacunas)
                .HasForeignKey(x => x.IdMascota);

            e.Property(x => x.IdProfesional).HasColumnName("idProfesional");

            e.HasOne(x => x.Profesional)
                .WithMany(p => p.AplicacionesVacunas)
                .HasForeignKey(x => x.IdProfesional);
        });
        modelBuilder.Entity<Seguimiento>(e =>
        {
            e.ToTable("Seguimiento");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Motivo).HasColumnName("motivo").HasMaxLength(100).IsRequired();
            e.Property(x => x.FechaPrevista).HasColumnName("fechaPrevista").IsRequired();
            e.Property(x => x.FechaRealizada).HasColumnName("fechaRealizada");
            e.Property(x => x.Notas).HasColumnName("notas").HasMaxLength(255);
            e.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>().HasMaxLength(20).IsRequired();
            
            e.Property(x => x.IdMascota).HasColumnName("idMascota");
            e.HasOne(x => x.Mascota)
                .WithMany(m => m.Seguimientos)
                .HasForeignKey(x => x.IdMascota);

            e.Property(x => x.IdConsultaOrigen).HasColumnName("idConsultaOrigen");
            e.HasOne(x => x.ConsultaOrigen)
                .WithMany(c => c.SeguimientosOriginados)
                .HasForeignKey(x => x.IdConsultaOrigen);

            e.Property(x => x.IdAplicacionVacunaOrigen).HasColumnName("idAplicacionVacunaOrigen");
            e.HasOne(x => x.AplicacionVacunaOrigen)
                .WithMany(v => v.SeguimientosOriginados)
                .HasForeignKey(x => x.IdAplicacionVacunaOrigen);

            e.Property(x => x.IdConsultaRealizacion).HasColumnName("idConsultaRealizacion");
            e.HasOne(x => x.ConsultaRealizacion)
                .WithMany(c => c.SeguimientosRealizados)
                .HasForeignKey(x => x.IdConsultaRealizacion);
        });
    }
}
