using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using veterinariaInteligente.Models;

namespace veterinariaInteligente.Views;

public partial class SeleccionPropietarioWindow : Window
{
    private List<Duenio> _propietarios = [];

    public SeleccionPropietarioWindow()
    {
        InitializeComponent();
        ActualizarLista();
    }

    public SeleccionPropietarioWindow(IEnumerable<Duenio> propietarios)
        : this()
    {
        _propietarios = propietarios.ToList();
        ActualizarLista();
    }

    public Duenio? PropietarioSeleccionado { get; private set; }

    private void Busqueda_TextChanged(object? sender, TextChangedEventArgs e) => ActualizarLista();

    private void ActualizarLista()
    {
        var texto = Busqueda?.Text?.Trim() ?? string.Empty;
        ListaPropietarios.ItemsSource = _propietarios.Where(p =>
            string.IsNullOrEmpty(texto) ||
            p.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
            p.Telefono.Contains(texto, StringComparison.OrdinalIgnoreCase)).ToList();
        SeleccionarButton.IsEnabled = false;
    }

    private void ListaPropietarios_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        => SeleccionarButton.IsEnabled = ListaPropietarios.SelectedItem is Duenio;

    private void ListaPropietarios_DoubleTapped(object? sender, TappedEventArgs e) => Confirmar();

    private void Seleccionar_Click(object? sender, RoutedEventArgs e) => Confirmar();

    private void Confirmar()
    {
        if (ListaPropietarios.SelectedItem is not Duenio propietario) return;
        PropietarioSeleccionado = propietario;
        Close();
    }

    private void Cancelar_Click(object? sender, RoutedEventArgs e) => Close();
}
