using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using veterinariaInteligente.Data;
using veterinariaInteligente.Models.Enums;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.Tests;

public class ConsultaServiceTests
{
    private static async Task<AppDbContext> CrearDbAsync(SqliteConnection connection)
    {
        await connection.OpenAsync();
        var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).Options);
        await db.Database.MigrateAsync();
        return db;
    }

    private static async Task<(int ConsultaId, int ProfesionalId)> CrearConsultaConVersionAsync(AppDbContext db)
    {
        var duenioService = new DuenioService(db);
        var mascotaService = new MascotaService(db);
        var profesionalService = new ProfesionalService(db);
        var consultaService = new ConsultaService(db);

        var duenio = (await duenioService.AgregarAsync("Ana", "1234567", null, null)).Valor!;
        var profesional = (await profesionalService.AgregarAsync("Dr. House", null)).Valor!;
        var estadoId = await db.MascotaEstados
            .Where(e => e.Id == 1)
            .Select(e => e.Id)
            .FirstAsync();

        var mascota = (await mascotaService.AgregarAsync(
            "Firulais", "Labrador", "Macho", null, "Negro", null, estadoId, duenio.Id)).Valor!;

        var consulta = (await consultaService.AgregarAsync(
            "Revisión", 12.5m, "Sin novedades", "Sano", "Reposo",
            mascota.Id, profesional.Id)).Valor!;

        var actualizada = await consultaService.ActualizarAsync(
            consulta.Id, "Revisión general", "Sin novedades", "Sano", "Reposo",
            12.5m, profesional.Id);
        Assert.True(actualizada.Correcta, actualizada.Mensaje);

        return (consulta.Id, profesional.Id);
    }

    [Fact]
    public async Task ObtenerVersionesAsyncDevuelveHistorialDeConsultaArchivada()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearDbAsync(connection);

        var consultaService = new ConsultaService(db);
        var (consultaId, _) = await CrearConsultaConVersionAsync(db);

        Assert.True((await consultaService.ArchivarAsync(consultaId)).Correcta);

        var versiones = await consultaService.ObtenerVersionesAsync(consultaId);

        var version = Assert.Single(versiones);
        Assert.Equal("Motivo", version.Campo);
        Assert.Equal("Revisión", version.ValorAnterior);
        Assert.Equal("Revisión general", version.ValorNuevo);

        // ObtenerPorIdAsync sigue excluyendo archivadas a propósito.
        Assert.Null(await consultaService.ObtenerPorIdAsync(consultaId));
    }

    [Fact]
    public async Task ObtenerPorIdAsyncCargaElProfesionalDeLasVersiones()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearDbAsync(connection);

        var consultaService = new ConsultaService(db);
        var (consultaId, profesionalId) = await CrearConsultaConVersionAsync(db);

        var detalle = await consultaService.ObtenerPorIdAsync(consultaId);

        Assert.NotNull(detalle);
        Assert.Equal(EstadoConsulta.Pendiente, detalle!.Estado);
        var version = Assert.Single(detalle.Versiones);
        Assert.NotNull(version.Profesional);
        Assert.Equal(profesionalId, version.Profesional.Id);
        Assert.Equal("Dr. House", version.Profesional.Nombre);
    }
}
