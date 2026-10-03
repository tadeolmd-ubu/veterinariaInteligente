using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using veterinariaInteligente.Models;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly PropietariosViewModel _propietariosViewModel;
    private readonly CitasViewModel _citasViewModel;

    public MainViewModel(
        PropietariosViewModel propietariosViewModel,
        CitasViewModel citasViewModel,
        PendientesViewModel pendientesViewModel,
        ConsultasViewModel consultasViewModel,
        ConsultaService consultaService)
    {
        _propietariosViewModel = propietariosViewModel;
        _citasViewModel = citasViewModel;
        _pendientesViewModel = pendientesViewModel;
        _consultasViewModel = consultasViewModel;
        _consultaService = consultaService;
        _propietariosViewModel.ExpedienteSolicitado += MostrarExpediente;
        vistaActual = _propietariosViewModel;
        tituloVistaActual = "Propietarios y mascotas";
    }

    private readonly ConsultasViewModel _consultasViewModel;
    private readonly ConsultaService _consultaService;
    private readonly PendientesViewModel _pendientesViewModel;

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

    private async void MostrarExpediente(Mascota mascota, Duenio duenio)
    {
        var expedienteViewModel = new ExpedienteViewModel(mascota, duenio, _consultaService);
        VistaActual = expedienteViewModel;
        TituloVistaActual = $"Expediente de {mascota.Nombre}";
        await expedienteViewModel.CargarAsync();
    }
}
