using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using veterinariaInteligente.Data;
using veterinariaInteligente.Service;
using veterinariaInteligente.ViewModels;
using veterinariaInteligente.Views;
using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace veterinariaInteligente;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            string rutaBaseDatos;
            try
            {
                rutaBaseDatos = ResolverRutaBaseDatos();
            }
            catch (Exception ex)
            {
                desktop.MainWindow = new Window
                {
                    Title = "Error al iniciar PawDesk", Width = 600, Height = 200,
                    Content = new TextBlock
                    {
                        Text = $"No se pudo preparar la base de datos: {ex.Message}",
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap, Margin = new Thickness(24)
                    }
                };
                base.OnFrameworkInitializationCompleted();
                return;
            }

            var opciones = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(new SqliteConnectionStringBuilder { DataSource = rutaBaseDatos }.ToString())
                .Options;

            var dbContext = new AppDbContext(opciones);
            var duenioService = new DuenioService(dbContext);
            var mascotaService = new MascotaService(dbContext);
            var profesionalService = new ProfesionalService(dbContext);
            var citaService = new CitaService(dbContext);
            var propietariosViewModel = new PropietariosViewModel(duenioService, mascotaService);
            var citasViewModel = new CitasViewModel(
                citaService,
                mascotaService,
                profesionalService);
            var consultaService = new ConsultaService(dbContext);
            var pendientesViewModel = new PendientesViewModel(
                new SeguimientoService(dbContext), mascotaService, consultaService,
                new AplicacionVacunaService(dbContext));
            var consultasViewModel = new ConsultasViewModel(
                consultaService,
                mascotaService,
                profesionalService);
            var mainViewModel = new MainViewModel(
                propietariosViewModel,
                citasViewModel,
                pendientesViewModel,
                new VacunasViewModel(new VacunaService(dbContext),
                    new AplicacionVacunaService(dbContext), mascotaService, profesionalService),
                consultasViewModel,
                consultaService,
                new ConfiguracionViewModel(profesionalService, new RespaldoService(dbContext),
                    new PdfService(dbContext), mascotaService, consultaService),
                new InicioViewModel(duenioService, mascotaService, citaService,
                    new SeguimientoService(dbContext)));

            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            mainWindow.IsEnabled = false;
            mainWindow.Opened += async (_, _) =>
            {
                try
                {
                    if (File.Exists(rutaBaseDatos) &&
                        (await dbContext.Database.GetPendingMigrationsAsync()).Any())
                    {
                        var respaldo = await new RespaldoService(dbContext).CrearRespaldoAsync(
                            Path.Combine(Path.GetDirectoryName(rutaBaseDatos)!, "Respaldos"));
                        if (!respaldo.Correcta)
                            throw new IOException("No se pudo crear el respaldo previo a la migración.");
                    }
                    await dbContext.Database.MigrateAsync();
                    await mainViewModel.InicializarAsync();
                    mainWindow.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    mainWindow.Content = new TextBlock
                    {
                        Text = $"No se pudo iniciar la base de datos: {ex.Message}",
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        Margin = new Thickness(24)
                    };
                    mainWindow.IsEnabled = true;
                    return;
                }

#if !DEBUG
                try
                {
                    var actualizacion = await UpdateService.BuscarAsync();
                    if (actualizacion is not null && mainWindow.IsVisible &&
                        await new ActualizacionWindow(actualizacion).ShowDialog<bool>(mainWindow))
                        desktop.Shutdown();
                }
                catch (Exception ex)
                {
                    // Network failures must not prevent normal use of the application.
                    Trace.TraceWarning("No se pudo consultar la actualización: {0}", ex.Message);
                }
#endif
            };
            desktop.Exit += (_, _) => dbContext.Dispose();
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static string ResolverRutaBaseDatos()
    {
#if !DEBUG
        var carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VeterinariaInteligente");
        Directory.CreateDirectory(carpeta);
        var ruta = Path.Combine(carpeta, "veterinariaInteligente.db");
        if (!File.Exists(ruta))
        {
            // Import an existing portable installation without modifying its database.
            var anterior = Path.Combine(AppContext.BaseDirectory, "veterinariaInteligente.db");
            if (File.Exists(anterior))
            {
                var temporal = ruta + ".tmp";
                try
                {
                    using (var origen = new SqliteConnection(new SqliteConnectionStringBuilder
                           { DataSource = anterior, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString()))
                    using (var destino = new SqliteConnection(new SqliteConnectionStringBuilder
                           { DataSource = temporal, Pooling = false }.ToString()))
                    {
                        origen.Open();
                        destino.Open();
                        origen.BackupDatabase(destino);
                    }
                    File.Move(temporal, ruta);
                }
                finally
                {
                    File.Delete(temporal);
                }
            }
        }
        return ruta;
#else
        var directorio = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directorio is not null)
        {
            var rutaDirecta = Path.Combine(directorio.FullName, "veterinariaInteligente.db");
            if (File.Exists(rutaDirecta))
                return rutaDirecta;

            var rutaDentroDelProyecto = Path.Combine(
                directorio.FullName,
                "veterinariaInteligente",
                "veterinariaInteligente.db");

            if (File.Exists(rutaDentroDelProyecto))
                return rutaDentroDelProyecto;

            directorio = directorio.Parent;
        }

        return Path.Combine(Directory.GetCurrentDirectory(), "veterinariaInteligente.db");
#endif
    }
}
