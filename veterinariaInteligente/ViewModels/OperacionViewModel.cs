using CommunityToolkit.Mvvm.ComponentModel;

namespace veterinariaInteligente.ViewModels;

public enum TipoMensajeOperacion { Informacion, Exito, Error }

public sealed record MensajeDialogo(string Titulo, string Mensaje, TipoMensajeOperacion Tipo);

// Contrato para Front: suscribir MensajeSolicitado y mostrar el diálogo en la UI.
public abstract class OperacionViewModel : ObservableObject
{
    /// <summary>
    /// Front puede escuchar este evento en MainViewModel (centraliza todas las pantallas).
    /// MensajeOperacion sigue disponible como resumen compatible con el XAML actual.
    /// </summary>
    public event Action<MensajeDialogo>? MensajeSolicitado;
    protected void SolicitarMensaje(MensajeDialogo mensaje) => MensajeSolicitado?.Invoke(mensaje);

    protected string Notificar(string mensaje, bool correcta = false)
    {
        SolicitarMensaje(new(correcta ? "Operación completada" : "Error", mensaje,
            correcta ? TipoMensajeOperacion.Exito : TipoMensajeOperacion.Error));
        return mensaje;
    }
}
