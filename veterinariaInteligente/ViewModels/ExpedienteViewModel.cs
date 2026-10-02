using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using veterinariaInteligente.Models;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.ViewModels;

public partial class ExpedienteViewModel : ObservableObject
{
    private readonly ConsultaService? _consultaService;

    public ExpedienteViewModel()
    {
        MascotaActual = null!;
        DuenioActual = null!;
    }

    public ExpedienteViewModel(Mascota mascota, Duenio duenio)
        : this(mascota, duenio, null)
    {
    }

    public ExpedienteViewModel(
        Mascota mascota,
        Duenio duenio,
        ConsultaService? consultaService)
    {
        MascotaActual = mascota;
        DuenioActual = duenio;
        _consultaService = consultaService;
    }

    public Mascota MascotaActual { get; }
    public Duenio DuenioActual { get; }
    public ObservableCollection<Consulta> ConsultasItems { get; } = [];

    [ObservableProperty]
    private Consulta? selectedConsulta;

    [ObservableProperty]
    private string mensajeOperacion = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [RelayCommand]
    public async Task CargarAsync()
    {
        if (_consultaService is null || IsBusy)
            return;

        IsBusy = true;

        try
        {
            var resultado = await _consultaService.ObtenerExpedienteAsync(MascotaActual.Id);
            MensajeOperacion = resultado.Mensaje;
            ConsultasItems.Clear();

            if (!resultado.Correcta || resultado.Valor is null)
                return;

            foreach (var consulta in resultado.Valor)
                ConsultasItems.Add(consulta);

            SelectedConsulta = ConsultasItems.FirstOrDefault();
        }
        catch (Exception ex)
        {
            MensajeOperacion = $"No se pudo cargar el expediente: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
