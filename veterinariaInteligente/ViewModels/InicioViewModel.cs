using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.ViewModels;

public partial class InicioViewModel : ObservableObject
{
    private readonly DuenioService? _duenios;
    private readonly MascotaService? _mascotas;
    private readonly CitaService? _citas;
    private readonly SeguimientoService? _seguimientos;

    public InicioViewModel() { }
    public InicioViewModel(DuenioService duenios, MascotaService mascotas,
        CitaService citas, SeguimientoService seguimientos)
    {
        _duenios = duenios;
        _mascotas = mascotas;
        _citas = citas;
        _seguimientos = seguimientos;
    }

    [ObservableProperty] private int totalPropietarios;
    [ObservableProperty] private int totalMascotas;
    [ObservableProperty] private int citasHoy;
    [ObservableProperty] private int seguimientosVencidos;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string mensajeOperacion = string.Empty;

    [RelayCommand]
    public async Task CargarAsync()
    {
        if (IsBusy || _duenios is null || _mascotas is null ||
            _citas is null || _seguimientos is null) return;
        IsBusy = true;
        try
        {
            var hoy = DateTime.Today;
            // Los servicios comparten DbContext; las consultas deben ser secuenciales.
            var propietarios = await _duenios.ObtenerTodosAsync();
            var mascotas = await _mascotas.ObtenerTodosAsync();
            var citas = await _citas.ObtenerAgendaAsync(hoy, hoy.AddDays(1).AddTicks(-1));
            var vencidos = await _seguimientos.ObtenerVencidosAsync(DateOnly.FromDateTime(hoy));
            TotalPropietarios = propietarios.Count;
            TotalMascotas = mascotas.Count;
            CitasHoy = citas.Count;
            SeguimientosVencidos = vencidos.Count;
            MensajeOperacion = string.Empty;
        }
        catch (Exception ex)
        {
            TotalPropietarios = TotalMascotas = CitasHoy = SeguimientosVencidos = 0;
            MensajeOperacion = $"No se pudo cargar el resumen: {ex.Message}";
        }
        finally { IsBusy = false; }
    }
}
