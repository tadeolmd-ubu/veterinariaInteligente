using CommunityToolkit.Mvvm.ComponentModel;
using veterinariaInteligente.Models;

namespace veterinariaInteligente.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly PropietariosViewModel _propietariosViewModel;

    public MainViewModel(PropietariosViewModel propietariosViewModel)
    {
        _propietariosViewModel = propietariosViewModel;
        _propietariosViewModel.ExpedienteSolicitado += MostrarExpediente;
        vistaActual = _propietariosViewModel;
    }

    [ObservableProperty]
    private object vistaActual;

    public Task InicializarAsync() => _propietariosViewModel.CargarAsync();

    private void MostrarExpediente(Mascota mascota, Duenio duenio)
    {
        VistaActual = new ExpedienteViewModel(mascota, duenio);
    }
}
