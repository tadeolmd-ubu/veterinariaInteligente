using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using veterinariaInteligente.Models;
using veterinariaInteligente.Models.Enums;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.ViewModels;

public partial class CitasViewModel : OperacionViewModel
{
    private readonly CitaService? _citaService;
    private readonly MascotaService? _mascotaService;
    private readonly ProfesionalService? _profesionalService;
    private readonly List<Cita> _citasActivas = [];
    private readonly List<Cita> _citasCanceladas = [];
    private readonly List<Cita> _citasAgenda = [];

    public CitasViewModel()
    {
    }

    public CitasViewModel(
        CitaService citaService,
        MascotaService mascotaService,
        ProfesionalService profesionalService)
    {
        _citaService = citaService;
        _mascotaService = mascotaService;
        _profesionalService = profesionalService;
    }

    public ObservableCollection<Cita> CitasItems { get; } = [];
    public ObservableCollection<Mascota> MascotasItems { get; } = [];
    public ObservableCollection<Profesional> ProfesionalesItems { get; } = [];
    public IReadOnlyList<EstadoCita> EstadosCitaItems { get; } =
    [
        EstadoCita.Pendiente,
        EstadoCita.Confirmada,
        EstadoCita.Atendida,
        EstadoCita.NoAsistio
    ];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AgregarCitaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReactivarCitaCommand))]
    [NotifyPropertyChangedFor(nameof(PuedeReactivar))]
    [NotifyCanExecuteChangedFor(nameof(ActualizarCitaCommand))]
    [NotifyCanExecuteChangedFor(nameof(CambiarEstadoCitaCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelarCitaCommand))]
    [NotifyPropertyChangedFor(nameof(PuedeModificar))]
    [NotifyPropertyChangedFor(nameof(PuedeCancelar))]
    private Cita? selectedCita;

    [ObservableProperty]
    private Mascota? selectedMascota;

    [ObservableProperty]
    private Profesional? selectedProfesional;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ReactivarCitaCommand))]
    private EstadoCita selectedEstado = EstadoCita.Pendiente;

    [ObservableProperty]
    private DateTimeOffset? fechaPrevista = DateTimeOffset.Now.AddDays(1);

    [ObservableProperty]
    private string motivo = string.Empty;

    [ObservableProperty]
    private string? notas;

    [ObservableProperty]
    private string busquedaCitas = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AgregarCitaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReactivarCitaCommand))]
    [NotifyPropertyChangedFor(nameof(PuedeReactivar))]
    [NotifyCanExecuteChangedFor(nameof(ActualizarCitaCommand))]
    [NotifyCanExecuteChangedFor(nameof(CambiarEstadoCitaCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelarCitaCommand))]
    [NotifyPropertyChangedFor(nameof(PuedeModificar))]
    [NotifyPropertyChangedFor(nameof(PuedeCancelar))]
    private bool mostrandoCanceladas;

    [ObservableProperty]
    private bool mostrandoAgenda;

    [ObservableProperty]
    private DateTimeOffset? agendaDesde = DateTimeOffset.Now.Date;

    [ObservableProperty]
    private DateTimeOffset? agendaHasta = DateTimeOffset.Now.Date.AddDays(7);

    [ObservableProperty]
    private string mensajeOperacion = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AgregarCitaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReactivarCitaCommand))]
    [NotifyPropertyChangedFor(nameof(PuedeReactivar))]
    [NotifyCanExecuteChangedFor(nameof(ActualizarCitaCommand))]
    [NotifyCanExecuteChangedFor(nameof(CambiarEstadoCitaCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelarCitaCommand))]
    [NotifyPropertyChangedFor(nameof(PuedeModificar))]
    [NotifyPropertyChangedFor(nameof(PuedeCancelar))]
    private bool isBusy;

    public bool PuedeModificar => !IsBusy && SelectedCita is { Estado: not EstadoCita.Cancelada } && !MostrandoCanceladas;
    public bool PuedeCancelar => !IsBusy && SelectedCita is { Estado: not EstadoCita.Cancelada } && !MostrandoCanceladas;
    private bool PuedeCrear() => !IsBusy && SelectedCita is null && !MostrandoCanceladas;
    public bool PuedeReactivar => !IsBusy && SelectedCita is { Estado: EstadoCita.Cancelada };
    private bool PuedeReactivarAlEstado() => PuedeReactivar && SelectedEstado != EstadoCita.Cancelada;

    [RelayCommand(CanExecute = nameof(PuedeReactivarAlEstado))]
    private Task ReactivarCitaAsync()
    {
        if (!PuedeReactivarAlEstado() || _citaService is null) return Task.CompletedTask;
        var id = SelectedCita!.Id;
        var estado = SelectedEstado;
        return EjecutarSeguroAsync(async () =>
        {
            var resultado = await _citaService.ReactivarAsync(id, estado);
            MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);
            if (!resultado.Correcta) return;
            MostrandoCanceladas = false;
            MostrandoAgenda = false;
            BusquedaCitas = string.Empty;
            await RecargarAsync(id);
        });
    }

    [RelayCommand]
    public Task CargarAsync() => EjecutarSeguroAsync(() => RecargarAsync());

    partial void OnSelectedCitaChanged(Cita? value)
    {
        if (value is null)
        {
            LimpiarFormulario();
            return;
        }

        FechaPrevista = new DateTimeOffset(value.FechaPrevista);
        Motivo = value.Motivo;
        Notas = value.Notas;
        SelectedEstado = value.Estado;
        SelectedMascota = MascotasItems.FirstOrDefault(m => m.Id == value.IdMascota);
        SelectedProfesional = ProfesionalesItems.FirstOrDefault(p => p.Id == value.IdProfesional);
    }

    partial void OnMostrandoCanceladasChanged(bool value) => FiltrarCitas();
    partial void OnMostrandoAgendaChanged(bool value) => FiltrarCitas();

    partial void OnBusquedaCitasChanged(string value)
    {
        FiltrarCitas();
    }

    [RelayCommand(CanExecute = nameof(PuedeCrear))]
    private async Task AgregarCitaAsync()
    {
        if (_citaService is null || !PuedeCrear())
            return;

        if (!TryObtenerDatosFormulario(out var fecha, out var mascotaId))
            return;

        await EjecutarSeguroAsync(async () =>
        {
            var resultado = await _citaService.AgregarAsync(
                fecha,
                Motivo,
                Notas,
                mascotaId,
                SelectedProfesional?.Id);

            MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);

            if (!resultado.Correcta || resultado.Valor is null)
                return;

            MostrandoCanceladas = false;
            MostrandoAgenda = false;
            LimpiarFormulario();
            await RecargarAsync();
            LimpiarFormulario();
        });
    }

    [RelayCommand(CanExecute = nameof(PuedeModificar))]
    private async Task ActualizarCitaAsync()
    {
        if (_citaService is null || SelectedCita is null)
            return;

        if (!TryObtenerDatosFormulario(out var fecha, out var mascotaId))
            return;

        var citaId = SelectedCita.Id;

        await EjecutarSeguroAsync(async () =>
        {
            var resultado = await _citaService.ActualizarAsync(
                citaId,
                fecha,
                Motivo,
                Notas,
                mascotaId,
                SelectedProfesional?.Id);

            MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);

            if (resultado.Correcta)
                await RecargarAsync(citaId);
        });
    }

    [RelayCommand(CanExecute = nameof(PuedeModificar))]
    private async Task CambiarEstadoCitaAsync()
    {
        if (_citaService is null || SelectedCita is null)
            return;

        var citaId = SelectedCita.Id;

        await EjecutarSeguroAsync(async () =>
        {
            var resultado = await _citaService.CambiarEstadoAsync(citaId, SelectedEstado);
            MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);

            if (resultado.Correcta)
                await RecargarAsync(citaId);
        });
    }

    [RelayCommand(CanExecute = nameof(PuedeModificar))]
    private async Task CancelarCitaAsync()
    {
        if (_citaService is null || SelectedCita is null)
            return;

        var citaId = SelectedCita.Id;

        await EjecutarSeguroAsync(async () =>
        {
            var resultado = await _citaService.CancelarAsync(citaId);
            MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);

            if (!resultado.Correcta)
                return;

            LimpiarFormulario();
            await RecargarAsync();
        });
    }

    [RelayCommand]
    private void VerCitasActivas()
    {
        MostrandoCanceladas = false;
        MostrandoAgenda = false;
        SelectedCita = null;
        FiltrarCitas();
    }

    [RelayCommand]
    private void VerCitasCanceladas()
    {
        MostrandoCanceladas = true;
        MostrandoAgenda = false;
        SelectedCita = null;
        FiltrarCitas();
    }

    [RelayCommand]
    private async Task CargarAgendaAsync()
    {
        if (_citaService is null)
            return;

        if (!TryObtenerRangoAgenda(out var desde, out var hasta)) return;

        await EjecutarSeguroAsync(async () =>
        {
            var citas = await _citaService.ObtenerAgendaAsync(
                desde,
                hasta);

            _citasAgenda.Clear();
            _citasAgenda.AddRange(citas);
            MostrandoCanceladas = false;
            MostrandoAgenda = true;
            SelectedCita = null;
            FiltrarCitas();
            MensajeOperacion = Notificar($"Agenda cargada: {citas.Count} cita(s).", true);
        });
    }

    [RelayCommand]
    private void NuevaCita()
    {
        if (IsBusy) return;
        MostrandoCanceladas = false;
        LimpiarFormulario();
        MensajeOperacion = string.Empty;
    }

    private async Task RecargarAsync(int? citaIdSeleccionada = null)
    {
        if (_citaService is null ||
            _mascotaService is null ||
            _profesionalService is null)
        {
            return;
        }

        var mascotas = await _mascotaService.ObtenerTodosAsync();
        var profesionales = await _profesionalService.ObtenerTodosAsync();
        var citasActivas = await _citaService.ObtenerTodosAsync();
        var citasCanceladas = await _citaService.ObtenerCanceladasAsync();

        ReemplazarItems(MascotasItems, mascotas.Where(m => m.IdEstado == 1));
        ReemplazarItems(ProfesionalesItems, profesionales);

        _citasActivas.Clear();
        _citasActivas.AddRange(citasActivas);
        _citasCanceladas.Clear();
        _citasCanceladas.AddRange(citasCanceladas);

        if (MostrandoAgenda)
            await RecargarAgendaAsync();

        FiltrarCitas(citaIdSeleccionada);
    }

    private static DateTime FinDelDia(DateTime fecha) =>
        fecha == DateTime.MaxValue.Date ? DateTime.MaxValue : fecha.AddDays(1).AddTicks(-1);

    private async Task RecargarAgendaAsync()
    {
        if (_citaService is null) return;
        if (!TryObtenerRangoAgenda(out var desde, out var hasta))
        {
            _citasAgenda.Clear();
            return;
        }
        var citas = await _citaService.ObtenerAgendaAsync(desde, hasta);

        _citasAgenda.Clear();
        _citasAgenda.AddRange(citas);
    }

    private void FiltrarCitas(int? citaIdSeleccionada = null)
    {
        var idSeleccionado = citaIdSeleccionada ?? SelectedCita?.Id;
        var termino = BusquedaCitas?.Trim() ?? string.Empty;
        var origen = MostrandoCanceladas
            ? _citasCanceladas
            : MostrandoAgenda
                ? _citasAgenda
                : _citasActivas;

        var citasFiltradas = string.IsNullOrEmpty(termino)
            ? origen
            : origen.Where(c =>
                    c.Motivo.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                    (c.Mascota?.Nombre?.Contains(termino, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Duenio?.Nombre?.Contains(termino, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Profesional?.Nombre?.Contains(termino, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

        ReemplazarItems(CitasItems, citasFiltradas);
        SelectedCita = CitasItems.FirstOrDefault(c => c.Id == idSeleccionado);
    }

    private bool TryObtenerRangoAgenda(out DateTime desde, out DateTime hasta)
    {
        desde = hasta = default;
        if (AgendaDesde is null || AgendaHasta is null)
        {
            MensajeOperacion = Notificar("Seleccioná el inicio y el fin de la agenda.");
            return false;
        }
        // DatePicker representa días de calendario, no instantes convertidos a UTC/local.
        desde = AgendaDesde.Value.Date;
        hasta = FinDelDia(AgendaHasta.Value.Date);
        if (desde <= hasta) return true;
        MensajeOperacion = Notificar("El inicio de la agenda no puede ser posterior al fin.");
        return false;
    }

    private bool TryObtenerDatosFormulario(out DateTime fecha, out int mascotaId)
    {
        fecha = default;
        mascotaId = default;

        if (FechaPrevista is null)
        {
            MensajeOperacion = Notificar("Seleccioná la fecha y hora de la cita.");
            return false;
        }

        if (SelectedMascota is null)
        {
            MensajeOperacion = Notificar("Seleccioná una mascota.");
            return false;
        }

        fecha = FechaPrevista.Value.LocalDateTime;
        mascotaId = SelectedMascota.Id;
        return true;
    }

    private async Task EjecutarSeguroAsync(Func<Task> operacion)
    {
        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            await operacion();
        }
        catch (Exception ex)
        {
            MensajeOperacion = Notificar($"No se pudo completar la operación: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LimpiarFormulario()
    {
        SelectedCita = null;
        SelectedMascota = null;
        SelectedProfesional = null;
        SelectedEstado = EstadoCita.Pendiente;
        FechaPrevista = DateTimeOffset.Now.AddDays(1);
        Motivo = string.Empty;
        Notas = null;
    }

    private static void ReemplazarItems<T>(ObservableCollection<T> destino, IEnumerable<T> origen)
    {
        destino.Clear();

        foreach (var item in origen)
            destino.Add(item);
    }
}
