using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using veterinariaInteligente.Data;
using veterinariaInteligente.Models.Enums;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.Tests;

public class CitaServiceTests
{
    private static async Task<AppDbContext> CrearDbAsync(SqliteConnection connection)
    {
        await connection.OpenAsync();
        var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).Options);
        await db.Database.MigrateAsync();
        return db;
    }

    private static async Task<int> CrearCitaAsync(AppDbContext db, bool cancelar)
    {
        var duenioService = new DuenioService(db);
        var mascotaService = new MascotaService(db);
        var citaService = new CitaService(db);

        var duenio = (await duenioService.AgregarAsync("Ana", "1234567", null, null)).Valor!;
        var estadoId = await db.MascotaEstados
            .Where(e => e.Id == 1)
            .Select(e => e.Id)
            .FirstAsync();

        var mascota = (await mascotaService.AgregarAsync(
            "Firulais", "Labrador", "Macho", null, "Negro", null, estadoId, duenio.Id)).Valor!;

        var cita = (await citaService.AgregarAsync(
            DateTime.Now.AddDays(1), "Control", null, mascota.Id, null)).Valor!;

        if (cancelar)
            Assert.True((await citaService.CancelarAsync(cita.Id)).Correcta);

        return cita.Id;
    }

    private static async Task PonerFechaPasadaAsync(AppDbContext db, int citaId)
    {
        var cita = await db.Citas.FirstAsync(c => c.Id == citaId);
        cita.FechaPrevista = DateTime.Now.AddDays(-1);
        await db.SaveChangesAsync();
    }

    private static async Task<EstadoCita> EstadoDeAsync(AppDbContext db, int citaId)
    {
        return await db.Citas
            .Where(c => c.Id == citaId)
            .Select(c => c.Estado)
            .FirstAsync();
    }

    [Fact]
    public async Task ReactivarAsyncVuelveAPendienteUnaCitaCancelada()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearDbAsync(connection);
        var citaService = new CitaService(db);

        var citaId = await CrearCitaAsync(db, cancelar: true);

        var resultado = await citaService.ReactivarAsync(citaId, EstadoCita.Pendiente);

        Assert.True(resultado.Correcta, resultado.Mensaje);
        Assert.Equal(EstadoCita.Pendiente, await EstadoDeAsync(db, citaId));
    }

    [Fact]
    public async Task ReactivarAsyncPermiteAtendidaConFechaPasada()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearDbAsync(connection);
        var citaService = new CitaService(db);

        var citaId = await CrearCitaAsync(db, cancelar: true);
        await PonerFechaPasadaAsync(db, citaId);

        var resultado = await citaService.ReactivarAsync(citaId, EstadoCita.Atendida);

        Assert.True(resultado.Correcta, resultado.Mensaje);
        Assert.Equal(EstadoCita.Atendida, await EstadoDeAsync(db, citaId));
    }

    [Fact]
    public async Task ReactivarAsyncRechazaPendienteConFechaPasada()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearDbAsync(connection);
        var citaService = new CitaService(db);

        var citaId = await CrearCitaAsync(db, cancelar: true);
        await PonerFechaPasadaAsync(db, citaId);

        var resultado = await citaService.ReactivarAsync(citaId, EstadoCita.Pendiente);

        Assert.False(resultado.Correcta);
        Assert.Equal(EstadoCita.Cancelada, await EstadoDeAsync(db, citaId));
    }

    [Fact]
    public async Task ReactivarAsyncRechazaCitaNoCancelada()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearDbAsync(connection);
        var citaService = new CitaService(db);

        var citaId = await CrearCitaAsync(db, cancelar: false);

        var resultado = await citaService.ReactivarAsync(citaId, EstadoCita.Confirmada);

        Assert.False(resultado.Correcta);
        Assert.Equal(EstadoCita.Pendiente, await EstadoDeAsync(db, citaId));
    }

    [Fact]
    public async Task ReactivarAsyncRechazaDestinoCancelada()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearDbAsync(connection);
        var citaService = new CitaService(db);

        var citaId = await CrearCitaAsync(db, cancelar: true);

        var resultado = await citaService.ReactivarAsync(citaId, EstadoCita.Cancelada);

        Assert.False(resultado.Correcta);
        Assert.Equal(EstadoCita.Cancelada, await EstadoDeAsync(db, citaId));
    }
}
