using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using veterinariaInteligente.Data;
using veterinariaInteligente.Models;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.Tests;

public class DeploymentDatabaseTests
{
    [Fact]
    public async Task FirstInstallationCreatesSchemaAndSeedData()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection).Options);

        await db.Database.MigrateAsync();

        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        Assert.Equal(2, await db.MascotaEstados.CountAsync());
        Assert.Empty(await db.Duenios.ToListAsync());
    }

    [Fact]
    public async Task UpgradePreservesExistingOwnerAndCreatesReadableBackup()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VeterinariaDeploymentTest-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(
                new SqliteConnectionStringBuilder
                { DataSource = Path.Combine(folder, "app.db"), Pooling = false }.ToString()).Options;
            await using var db = new AppDbContext(options);
            await db.GetService<IMigrator>().MigrateAsync("20260918174424_AddDomainEntities");
            db.Duenios.Add(new Duenio { Nombre = "Propietario existente", Telefono = "123456" });
            await db.SaveChangesAsync();

            var backup = await new RespaldoService(db).CrearRespaldoAsync(folder);
            Assert.True(backup.Correcta, backup.Mensaje);
            await db.Database.MigrateAsync();

            Assert.Equal("Propietario existente", (await db.Duenios.SingleAsync()).Nombre);
            await using var backedUp = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(new SqliteConnectionStringBuilder
                { DataSource = backup.Valor!, Pooling = false }.ToString()).Options);
            Assert.Equal("Propietario existente", (await backedUp.Duenios.SingleAsync()).Nombre);
            Assert.NotEmpty(await backedUp.Database.GetPendingMigrationsAsync());
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }
}
