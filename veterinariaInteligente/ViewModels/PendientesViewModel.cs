using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using veterinariaInteligente.Models;
using veterinariaInteligente.Models.Enums;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.ViewModels;

public partial class PendientesViewModel : OperacionViewModel
{
    private readonly SeguimientoService? _seguimientos;
    private readonly MascotaService? _mascotas;
    private readonly ConsultaService? _consultas;
    private readonly AplicacionVacunaService? _aplicaciones;
    private List<Seguimiento> _cargados = [];
    private List<Consulta> _consultasCargadas = [];
    private List<AplicacionVacuna> _aplicacionesCargadas = [];

    public PendientesViewModel() { }

    public PendientesViewModel(SeguimientoService seguimientos, MascotaService mascotas,
        ConsultaService consultas, AplicacionVacunaService aplicaciones)
    {
        _seguimientos = seguimientos;
        _mascotas = mascotas;
        _consultas = consultas;
        _aplicaciones = aplicaciones;
    }

    public ObservableCollection<Seguimiento> SeguimientosItems { get; } = [];
    public ObservableCollection<Mascota> MascotasItems { get; } = [];
    public ObservableCollection<Consulta> ConsultasItems { get; } = [];
    public ObservableCollection<AplicacionVacuna> AplicacionesVacunaItems { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PuedeModificar))]
    [NotifyCanExecuteChangedFor(nameof(ActualizarSeguimientoCommand))]
    [NotifyCanExecuteChangedFor(nameof(MarcarRealizadoCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelarSeguimientoCommand))]
    private Seguimiento? selectedSeguimiento;
    [ObservableProperty] private Mascota? selectedMascota;
    [ObservableProperty] private Consulta? selectedConsultaOrigen;
    // Compatibilidad temporal con el XAML de Front: ya no participa en el flujo.
    [Obsolete("Usar únicamente SelectedConsultaOrigen; eliminar el binding en Front.")]
    public Consulta? SelectedConsultaRealizacion { get => null; set { } }
    [ObservableProperty] private AplicacionVacuna? selectedAplicacionVacunaOrigen;
    [ObservableProperty] private string motivo = string.Empty;
    [ObservableProperty] private string? notas;
    [ObservableProperty] private DateTimeOffset? fechaPrevista = DateTimeOffset.Now;
    [ObservableProperty] private DateTimeOffset? fechaRealizada = DateTimeOffset.Now;
    [ObservableProperty] private string busquedaSeguimientos = string.Empty;
    [ObservableProperty] private bool soloPendientes = true;
    [ObservableProperty] private bool soloVencidos;
    [ObservableProperty] private string mensajeOperacion = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PuedeModificar))]
    [NotifyCanExecuteChangedFor(nameof(ActualizarSeguimientoCommand))]
    [NotifyCanExecuteChangedFor(nameof(MarcarRealizadoCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelarSeguimientoCommand))]
    private bool isBusy;

    public bool PuedeModificar => !IsBusy && SelectedSeguimiento?.Estado == EstadoSeguimiento.Pendiente;

    partial void OnSelectedMascotaChanged(Mascota? value)
    {
        SelectedConsultaOrigen = null;
        SelectedAplicacionVacunaOrigen = null;
        Reemplazar(ConsultasItems, _consultasCargadas.Where(c => c.IdMascota == value?.Id));
        Reemplazar(AplicacionesVacunaItems, _aplicacionesCargadas.Where(a => a.IdMascota == value?.Id));
    }

    partial void OnSelectedSeguimientoChanged(Seguimiento? value)
    {
        if (value is null) return;
        SelectedMascota = MascotasItems.FirstOrDefault(m => m.Id == value.IdMascota);
        Motivo = value.Motivo;
        Notas = value.Notas;
        FechaPrevista = new DateTimeOffset(value.FechaPrevista.ToDateTime(TimeOnly.MinValue));
        FechaRealizada = value.FechaRealizada is { } fecha
            ? new DateTimeOffset(fecha.ToDateTime(TimeOnly.MinValue)) : DateTimeOffset.Now;
        SelectedConsultaOrigen = ConsultasItems.FirstOrDefault(c => c.Id == value.IdConsultaOrigen);
        SelectedAplicacionVacunaOrigen = AplicacionesVacunaItems.FirstOrDefault(a => a.Id == value.IdAplicacionVacunaOrigen);
    }

    partial void OnBusquedaSeguimientosChanged(string value) => Filtrar();
    partial void OnSoloPendientesChanged(bool value) => Filtrar();
    partial void OnSoloVencidosChanged(bool value) => Filtrar();

    [RelayCommand]
    public Task CargarAsync() => EjecutarAsync(() => RecargarAsync());

    [RelayCommand]
    private Task AgregarSeguimientoAsync() => EjecutarAsync(async () =>
    {
        if (_seguimientos is null) return;
        if (SelectedMascota is null || FechaPrevista is null)
        {
            MensajeOperacion = Notificar("Seleccioná una mascota y una fecha prevista.");
            return;
        }
        var resultado = await _seguimientos.AgregarAsync(Motivo,
            DateOnly.FromDateTime(FechaPrevista.Value.Date), Notas, SelectedMascota.Id,
            SelectedConsultaOrigen?.Id, SelectedAplicacionVacunaOrigen?.Id);
        MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);
        if (resultado.Correcta) await RecargarAsync(resultado.Valor?.Id);
    });

    [RelayCommand(CanExecute = nameof(PuedeModificar))]
    private Task ActualizarSeguimientoAsync()
    {
        if (!PuedeModificar) return Task.CompletedTask;
        var id = SelectedSeguimiento!.Id;
        return EjecutarAsync(async () =>
        {
            if (_seguimientos is null) return;
            if (FechaPrevista is null)
            {
                MensajeOperacion = Notificar("Seleccioná una fecha prevista.");
                return;
            }
            var resultado = await _seguimientos.ActualizarAsync(id, Motivo,
                DateOnly.FromDateTime(FechaPrevista.Value.Date), Notas);
            MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);
            if (resultado.Correcta) await RecargarAsync(id);
        });
    }

    [RelayCommand(CanExecute = nameof(PuedeModificar))]
    private Task MarcarRealizadoAsync()
    {
        if (!PuedeModificar) return Task.CompletedTask;
        var id = SelectedSeguimiento!.Id;
        return EjecutarAsync(async () =>
        {
            if (_seguimientos is null) return;
            if (FechaRealizada is null)
            {
                MensajeOperacion = Notificar("Seleccioná una fecha de realización.");
                return;
            }
            var resultado = await _seguimientos.MarcarRealizadoAsync(id,
                DateOnly.FromDateTime(FechaRealizada.Value.Date), null);
            MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);
            if (resultado.Correcta) await RecargarAsync(id);
        });
    }

    [RelayCommand(CanExecute = nameof(PuedeModificar))]
    private Task CancelarSeguimientoAsync()
    {
        if (!PuedeModificar) return Task.CompletedTask;
        var id = SelectedSeguimiento!.Id;
        return EjecutarAsync(async () =>
        {
            if (_seguimientos is null) return;
            var resultado = await _seguimientos.CancelarAsync(id);
            MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);
            if (resultado.Correcta) await RecargarAsync(id);
        });
    }

    [RelayCommand]
    private void NuevoSeguimiento()
    {
        if (IsBusy) return;
        SelectedSeguimiento = null;
        SelectedMascota = null;
        SelectedConsultaOrigen = null;
        SelectedAplicacionVacunaOrigen = null;
        Motivo = string.Empty;
        Notas = null;
        FechaPrevista = DateTimeOffset.Now;
        FechaRealizada = DateTimeOffset.Now;
        MensajeOperacion = string.Empty;
    }

    private async Task RecargarAsync(int? seleccionado = null)
    {
        if (_seguimientos is null || _mascotas is null || _consultas is null || _aplicaciones is null) return;
        var mascotaId = SelectedMascota?.Id;
        var mascotas = await _mascotas.ObtenerTodosAsync();
        var consultas = await _consultas.ObtenerTodosAsync();
        var aplicaciones = await _aplicaciones.ObtenerTodosAsync();
        var seguimientos = await _seguimientos.ObtenerTodosAsync();
        _consultasCargadas = consultas;
        _aplicacionesCargadas = aplicaciones;
        _cargados = seguimientos;
        Reemplazar(MascotasItems, mascotas.Where(m => m.IdEstado == 1));
        SelectedMascota = MascotasItems.FirstOrDefault(m => m.Id == mascotaId);
        Filtrar(seleccionado);
    }

    private void Filtrar(int? seleccionado = null)
    {
        var id = seleccionado ?? SelectedSeguimiento?.Id;
        var termino = BusquedaSeguimientos?.Trim() ?? string.Empty;
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        Reemplazar(SeguimientosItems, _cargados.Where(s =>
            (!(SoloPendientes || SoloVencidos) || s.Estado == EstadoSeguimiento.Pendiente) &&
            (!SoloVencidos || s.FechaPrevista < hoy) &&
            (s.Motivo.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
             (s.Mascota?.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase) ?? false))));
        SelectedSeguimiento = SeguimientosItems.FirstOrDefault(s => s.Id == id);
    }

    private async Task EjecutarAsync(Func<Task> operacion)
    {
        if (IsBusy) return;
        IsBusy = true;
        try { await operacion(); }
        catch (Exception ex) { MensajeOperacion = Notificar($"No se pudo completar la operación: {ex.Message}"); }
        finally { IsBusy = false; }
    }

    private static void Reemplazar<T>(ObservableCollection<T> destino, IEnumerable<T> items)
    {
        destino.Clear();
        foreach (var item in items) destino.Add(item);
    }
}
