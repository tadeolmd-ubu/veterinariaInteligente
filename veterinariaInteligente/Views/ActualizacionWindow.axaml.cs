using Avalonia.Controls;
using Avalonia.Interactivity;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.Views;

public partial class ActualizacionWindow : Window
{
    private ActualizacionDisponible? _actualizacion;
    private bool _instalando;

    public ActualizacionWindow()
    {
        InitializeComponent();
        Closing += (_, e) => e.Cancel = _instalando;
    }

    internal ActualizacionWindow(ActualizacionDisponible actualizacion) : this()
    {
        _actualizacion = actualizacion;
        Estado.Text = $"Nueva versión: {actualizacion.Version.ToString(3)}. ¿Querés instalarla ahora? La aplicación se cerrará y volverá a abrirse.";
    }

    private void PosponerClick(object? sender, RoutedEventArgs e) => Close(false);

    private async void ActualizarClick(object? sender, RoutedEventArgs e)
    {
        if (_actualizacion is null || _instalando) return;
        _instalando = true;
        Actualizar.IsEnabled = Posponer.IsEnabled = false;
        Progreso.IsVisible = true;
        try
        {
            await UpdateService.InstalarAsync(_actualizacion,
                new Progress<string>(message => Estado.Text = message));
            _instalando = false;
            Close(true);
        }
        catch (Exception ex)
        {
            _instalando = false;
            Estado.Text = $"No se pudo actualizar: {ex.Message}";
            Actualizar.IsEnabled = Posponer.IsEnabled = true;
            Progreso.IsVisible = false;
        }
    }
}
