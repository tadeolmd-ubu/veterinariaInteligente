using veterinariaInteligente.Models;

namespace veterinariaInteligente.ViewModels;

public class ExpedienteViewModel
{
    public ExpedienteViewModel()
    {
        MascotaActual = null!;
        DuenioActual = null!;
    }

    public ExpedienteViewModel(Mascota mascota, Duenio duenio)
    {
        MascotaActual = mascota;
        DuenioActual = duenio;
    }

    public Mascota MascotaActual { get; }
    public Duenio DuenioActual { get; }
}
