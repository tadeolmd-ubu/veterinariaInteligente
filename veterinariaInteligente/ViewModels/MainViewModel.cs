using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using veterinariaInteligente.Models;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.ViewModels;

public partial class MainViewModel : OperacionViewModel
{
    private readonly PropietariosViewModel _propietariosViewModel;
    private readonly CitasViewModel _citasViewModel;

    public MainViewModel(
        PropietariosViewModel propietariosViewModel,
        CitasViewModel citasViewModel,
        PendientesViewModel pendientesViewModel,
        VacunasViewModel vacunasViewModel,
        ConsultasViewModel consultasViewModel,
        ConsultaService consultaService,
        ConfiguracionViewModel configuracionViewModel,
        InicioViewModel inicioViewModel)
    {
        _propietariosViewModel = propietariosViewModel;
        _citasViewModel = citasViewModel;
        _pendientesViewModel = pendientesViewModel;
        _vacunasViewModel = vacunasViewModel;
        _consultasViewModel = consultasViewModel;
        _consultaService = consultaService;
        _configuracionViewModel = configuracionViewModel;
        _inicioViewModel = inicioViewModel;
        propietariosViewModel.MensajeSolicitado += SolicitarMensaje;
        citasViewModel.MensajeSolicitado += SolicitarMensaje;
        pendientesViewModel.MensajeSolicitado += SolicitarMensaje;
        vacunasViewModel.MensajeSolicitado += SolicitarMensaje;
        consultasViewModel.MensajeSolicitado += SolicitarMensaje;
        configuracionViewModel.MensajeSolicitado += SolicitarMensaje;
        inicioViewModel.MensajeSolicitado += SolicitarMensaje;
        // Reutilizar la instancia ya provista por la composición actual de App.
        if (configuracionViewModel.ServicioPdf is { } pdf)
        {
            propietariosViewModel.ConfigurarPdf(pdf);
            consultasViewModel.ConfigurarPdf(pdf);
            vacunasViewModel.ConfigurarPdf(pdf);
        }
        _propietariosViewModel.ExpedienteSolicitado += MostrarExpediente;
        vistaActual = _propietariosViewModel;
        tituloVistaActual = "Propietarios y mascotas";
    }

    private readonly ConsultasViewModel _consultasViewModel;
    private readonly ConsultaService _consultaService;
    private readonly PendientesViewModel _pendientesViewModel;
    private readonly VacunasViewModel _vacunasViewModel;
    private readonly ConfiguracionViewModel _configuracionViewModel;
    private readonly InicioViewModel _inicioViewModel;

    [RelayCommand]
    private async Task IrAConfiguracionAsync()
    {
        VistaActual = _configuracionViewModel;
        TituloVistaActual = "Configuración";
        await _configuracionViewModel.CargarAsync();
    }

    [RelayCommand]
    private async Task IrAVacunasAsync()
    {
        VistaActual = _vacunasViewModel;
        TituloVistaActual = "Vacunas";
        await _vacunasViewModel.CargarAsync();
    }

    [ObservableProperty]
    private object vistaActual;

    [ObservableProperty]
    private string tituloVistaActual;

    public Task InicializarAsync() => _propietariosViewModel.CargarAsync();

    [RelayCommand]
    private async Task IrAPropietariosAsync()
    {
        VistaActual = _propietariosViewModel;
        TituloVistaActual = "Propietarios y mascotas";
        await _propietariosViewModel.CargarAsync();
    }

    [RelayCommand]
    private async Task IrACitasAsync()
    {
        VistaActual = _citasViewModel;
        TituloVistaActual = "Citas";
        await _citasViewModel.CargarAsync();
    }

    [RelayCommand]
    private async Task IrAPendientesAsync()
    {
        VistaActual = _pendientesViewModel;
        TituloVistaActual = "Pendientes";
        await _pendientesViewModel.CargarAsync();
    }

    [RelayCommand]
    private async Task IrAConsultasAsync()
    {
        VistaActual = _consultasViewModel;
        TituloVistaActual = "Consultas";
        await _consultasViewModel.CargarAsync();
    }

    [RelayCommand]
    private async Task IrAInicioAsync()
    {
        VistaActual = _inicioViewModel;
        TituloVistaActual = "Inicio";
        await _inicioViewModel.CargarAsync();
    }

    private async void MostrarExpediente(Mascota mascota, Duenio duenio)
    {
        var expedienteViewModel = new ExpedienteViewModel(mascota, duenio, _consultaService,
            _configuracionViewModel.ServicioPdf);
        expedienteViewModel.MensajeSolicitado += SolicitarMensaje;
        VistaActual = expedienteViewModel;
        TituloVistaActual = $"Expediente de {mascota.Nombre}";
        await expedienteViewModel.CargarAsync();
    }
}
