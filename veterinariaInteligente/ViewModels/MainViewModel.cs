using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using veterinariaInteligente.Models;

namespace veterinariaInteligente.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly PropietariosViewModel _propietariosViewModel;
    private readonly CitasViewModel _citasViewModel;

    public MainViewModel(
        PropietariosViewModel propietariosViewModel,
        CitasViewModel citasViewModel)
    {
        _propietariosViewModel = propietariosViewModel;
        _citasViewModel = citasViewModel;
        _propietariosViewModel.ExpedienteSolicitado += MostrarExpediente;
        vistaActual = _propietariosViewModel;
        tituloVistaActual = "Propietarios y mascotas";
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

    private void MostrarExpediente(Mascota mascota, Duenio duenio)
    {
        VistaActual = new ExpedienteViewModel(mascota, duenio);
        TituloVistaActual = $"Expediente de {mascota.Nombre}";
    }
}
