using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using veterinariaInteligente.Data;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.Tests;

public class ConfiguracionClienteServiceTests
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
    public async Task ObtenerAsyncDevuelveNullCuandoNoHayConfiguracion()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearDbAsync(connection);

        var configuracion = await new ConfiguracionClienteService(db).ObtenerAsync();

        Assert.Null(configuracion);
    }

    [Fact]
    public async Task GuardarAsyncCreaYActualizaElRegistroUnico()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearDbAsync(connection);
        var service = new ConfiguracionClienteService(db);

        var creada = await service.GuardarAsync("Veterinaria Central", "1234567", "Av. Siempre Viva 742", null);
        Assert.True(creada.Correcta, creada.Mensaje);

        var actualizada = await service.GuardarAsync("Veterinaria Central", "7654321", "Nueva dirección 100", null);
        Assert.True(actualizada.Correcta, actualizada.Mensaje);

        Assert.Equal(1, await db.ConfiguracionesCliente.CountAsync());

        var configuracion = await service.ObtenerAsync();
        Assert.NotNull(configuracion);
        Assert.Equal("Veterinaria Central", configuracion!.Nombre);
        Assert.Equal("7654321", configuracion.Telefono);
        Assert.Equal("Nueva dirección 100", configuracion.Direccion);
    }

    [Fact]
    public async Task GuardarAsyncValidaCampos()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearDbAsync(connection);
        var service = new ConfiguracionClienteService(db);

        Assert.False((await service.GuardarAsync("", "1234567", "Calle 1", null)).Correcta);
        Assert.False((await service.GuardarAsync("Vete", "", "Calle 1", null)).Correcta);
        Assert.False((await service.GuardarAsync("Vete", "abcd", "Calle 1", null)).Correcta);
        Assert.False((await service.GuardarAsync("Vete", "123", "Calle 1", null)).Correcta);
        Assert.False((await service.GuardarAsync("Vete", "1234567", "", null)).Correcta);

        Assert.Equal(0, await db.ConfiguracionesCliente.CountAsync());
    }

    [Fact]
    public async Task GuardarAsyncValidaElProfesionalDefault()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearDbAsync(connection);
        var service = new ConfiguracionClienteService(db);

        var inexistente = await service.GuardarAsync("Vete", "1234567", "Calle 1", 999);
        Assert.False(inexistente.Correcta);

        var profesional = (await new ProfesionalService(db).AgregarAsync("Dr. House", null)).Valor!;

        var conProfesional = await service.GuardarAsync("Vete", "1234567", "Calle 1", profesional.Id);
        Assert.True(conProfesional.Correcta, conProfesional.Mensaje);

        var configuracion = await service.ObtenerAsync();
        Assert.NotNull(configuracion);
        Assert.Equal(profesional.Id, configuracion!.IdProfesionalDefault);
        Assert.NotNull(configuracion.ProfesionalDefault);
        Assert.Equal("Dr. House", configuracion.ProfesionalDefault!.Nombre);
    }
}
