using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using veterinariaInteligente.Data;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.Tests;

public class MascotaServiceTests
{
    private static async Task<AppDbContext> CrearDbAsync(SqliteConnection connection)
    {
        await connection.OpenAsync();
        var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).Options);
        await db.Database.MigrateAsync();
        return db;
    }

    [Fact]
    public async Task RestaurarMascotaFuncionaConDuenioArchivado()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearDbAsync(connection);

        var duenioService = new DuenioService(db);
        var mascotaService = new MascotaService(db);

        var duenio = await duenioService.AgregarAsync("Ana", "1234567", null, null);
        Assert.True(duenio.Correcta, duenio.Mensaje);

        var estadoId = await db.MascotaEstados.Select(e => e.Id).FirstAsync();

        var mascota = await mascotaService.AgregarAsync(
            "Firulais", "Labrador", "Macho", null, "Negro", null, estadoId, duenio.Valor!.Id);
        Assert.True(mascota.Correcta, mascota.Mensaje);

        var mascotaId = mascota.Valor!.Id;

        Assert.True((await mascotaService.ArchivarAsync(mascotaId)).Correcta);
        Assert.True((await duenioService.ArchivarAsync(duenio.Valor!.Id)).Correcta);

        var resultado = await mascotaService.RestaurarAsync(mascotaId);

        Assert.True(resultado.Correcta, resultado.Mensaje);
        var fechaArchivado = await db.Mascotas
            .Where(m => m.Id == mascotaId)
            .Select(m => m.FechaArchivado)
            .FirstAsync();
        Assert.Null(fechaArchivado);
    }

    [Fact]
    public async Task ObtenerArchivadasDeDueniosActivosExcluyeDueniosArchivados()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearDbAsync(connection);

        var duenioService = new DuenioService(db);
        var mascotaService = new MascotaService(db);

        var duenioActivo = (await duenioService.AgregarAsync("Ana", "1234567", null, null)).Valor!;
        var duenioArchivado = (await duenioService.AgregarAsync("Beto", "7654321", null, null)).Valor!;

        var estadoId = await db.MascotaEstados.Select(e => e.Id).FirstAsync();

        var mascotaDeActivo = (await mascotaService.AgregarAsync(
            "Firulais", "Labrador", "Macho", null, "Negro", null, estadoId, duenioActivo.Id)).Valor!;
        var mascotaDeArchivado = (await mascotaService.AgregarAsync(
            "Michi", "Siames", "Hembra", null, "Gris", null, estadoId, duenioArchivado.Id)).Valor!;

        await mascotaService.ArchivarAsync(mascotaDeActivo.Id);
        await mascotaService.ArchivarAsync(mascotaDeArchivado.Id);
        await duenioService.ArchivarAsync(duenioArchivado.Id);

        var resultado = await mascotaService.ObtenerArchivadasDeDueniosActivosAsync();

        var unica = Assert.Single(resultado);
        Assert.Equal(mascotaDeActivo.Id, unica.Id);
        Assert.NotNull(unica.Duenio);
        Assert.Equal(duenioActivo.Id, unica.Duenio.Id);
    }
}
