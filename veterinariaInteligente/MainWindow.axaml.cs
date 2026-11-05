using Avalonia.Controls;
using veterinariaInteligente.ViewModels;
using veterinariaInteligente.Views;

namespace veterinariaInteligente;

public partial class MainWindow : Window
{
    private readonly Queue<MensajeDialogo> _mensajesPendientes = new();
    private bool _mostrandoMensaje;
    private MainViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => ConectarViewModel(DataContext as MainViewModel);
    }

    private void ConectarViewModel(MainViewModel? viewModel)
    {
        if (_viewModel is not null)
            _viewModel.MensajeSolicitado -= MostrarMensaje;
        _viewModel = viewModel;
        if (_viewModel is not null)
            _viewModel.MensajeSolicitado += MostrarMensaje;
    }

    private void MostrarMensaje(MensajeDialogo mensaje)
    {
        _mensajesPendientes.Enqueue(mensaje);
        if (!_mostrandoMensaje)
            _ = ProcesarMensajesAsync();
    }

    private async Task ProcesarMensajesAsync()
    {
        _mostrandoMensaje = true;
        try
        {
            while (_mensajesPendientes.TryDequeue(out var mensaje))
                await new MensajeWindow(mensaje).ShowDialog(this);
        }
        finally
        {
            _mostrandoMensaje = false;
        }
    }
}
