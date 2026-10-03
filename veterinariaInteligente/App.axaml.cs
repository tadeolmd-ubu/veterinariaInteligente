using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using veterinariaInteligente.Data;
using veterinariaInteligente.Service;
using veterinariaInteligente.ViewModels;

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
            var rutaBaseDatos = ResolverRutaBaseDatos();

            var opciones = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={rutaBaseDatos}")
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
                consultasViewModel,
                consultaService);

            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            mainWindow.Opened += async (_, _) => await mainViewModel.InicializarAsync();
            desktop.Exit += (_, _) => dbContext.Dispose();
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static string ResolverRutaBaseDatos()
    {
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
    }
}
