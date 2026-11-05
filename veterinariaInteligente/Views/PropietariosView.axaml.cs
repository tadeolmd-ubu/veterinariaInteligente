using Avalonia.Controls;
using System.ComponentModel;
using veterinariaInteligente.Models;
using veterinariaInteligente.ViewModels;
using veterinariaInteligente.Views.DesignTime;

namespace veterinariaInteligente.Views;

public partial class PropietariosView : UserControl
{
    private PropietariosViewModel? _viewModel;
    private PropertyChangedEventHandler? _pdfHandler;
    private PropertyChangedEventHandler? _selectionHandler;
    private PropertyChangedEventHandler? _mascotaHandler;

    public PropietariosView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => ConectarViewModel(DataContext as PropietariosViewModel);

        if (Design.IsDesignMode)
            DataContext = DesignTimeData.Propietarios;
    }

    private void ConectarViewModel(PropietariosViewModel? viewModel)
    {
        if (_viewModel is not null)
        {
            _viewModel.SeleccionPropietarioSolicitada -= SeleccionarPropietarioAsync;
            if (_pdfHandler is not null) _viewModel.PropertyChanged -= _pdfHandler;
            if (_selectionHandler is not null) _viewModel.PropertyChanged -= _selectionHandler;
            if (_mascotaHandler is not null) _viewModel.PropertyChanged -= _mascotaHandler;
        }

        _viewModel = viewModel;
        _pdfHandler = null;
        _selectionHandler = null;
        _mascotaHandler = null;
        if (_viewModel is null) return;
        var currentViewModel = _viewModel;

        currentViewModel.SeleccionPropietarioSolicitada += SeleccionarPropietarioAsync;
        _selectionHandler = (_, e) =>
        {
            if (e.PropertyName != nameof(PropietariosViewModel.SelectedPropietario)) return;
            PropietariosActivosList.SelectedItem = currentViewModel.SelectedPropietario;
            PropietariosArchivadosList.SelectedItem = currentViewModel.SelectedPropietario;
        };
        currentViewModel.PropertyChanged += _selectionHandler;
        _mascotaHandler = (_, e) =>
        {
            if (e.PropertyName != nameof(PropietariosViewModel.SelectedMascota)) return;
            MascotasActivasList.SelectedItem = currentViewModel.SelectedMascota;
            MascotasArchivadasList.SelectedItem = currentViewModel.SelectedMascota;
        };
        currentViewModel.PropertyChanged += _mascotaHandler;
        _pdfHandler = (_, e) =>
        {
            if (e.PropertyName == nameof(PdfViewModel.PdfGenerado))
                _ = StorageDialogHelper.GuardarPdfGeneradoAsync(this, currentViewModel);
        };
        currentViewModel.PropertyChanged += _pdfHandler;
    }

    private void Propietarios_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_viewModel is not null && sender is ListBox { SelectedItem: Duenio propietario })
            _viewModel.SelectedPropietario = propietario;
    }

    private void Mascotas_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_viewModel is not null && sender is ListBox { SelectedItem: Mascota mascota })
            _viewModel.SelectedMascota = mascota;
    }

    private async Task<Duenio?> SeleccionarPropietarioAsync()
    {
        if (_viewModel is null || TopLevel.GetTopLevel(this) is not Window ventana) return null;
        var busquedaActual = _viewModel.BusquedaPropietarios;
        _viewModel.BusquedaPropietarios = string.Empty;
        var propietariosActivos = _viewModel.PropietariosActivos.ToArray();
        _viewModel.BusquedaPropietarios = busquedaActual;
        var selector = new SeleccionPropietarioWindow(propietariosActivos);
        await selector.ShowDialog(ventana);
        return selector.PropietarioSeleccionado;
    }
}
