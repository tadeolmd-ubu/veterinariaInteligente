using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using veterinariaInteligente.ViewModels;

namespace veterinariaInteligente.Views;

public partial class MensajeWindow : Window
{
    public MensajeWindow()
    {
        InitializeComponent();
    }

    public MensajeWindow(MensajeDialogo mensaje)
        : this()
    {
        Titulo.Text = mensaje.Titulo;
        TextoMensaje.Text = mensaje.Mensaje;

        if (mensaje.Tipo == TipoMensajeOperacion.Error)
        {
            Icono.Text = "!";
            Icono.Foreground = new SolidColorBrush(Color.Parse("#E4573D"));
        }
        else if (mensaje.Tipo == TipoMensajeOperacion.Informacion)
        {
            Icono.Text = "i";
            Icono.Foreground = new SolidColorBrush(Color.Parse("#2F8FD1"));
        }
    }

    private void Aceptar_Click(object? sender, RoutedEventArgs e) => Close();
}
