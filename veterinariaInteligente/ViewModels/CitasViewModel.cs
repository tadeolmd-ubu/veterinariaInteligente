using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using veterinariaInteligente.Models;
using veterinariaInteligente.Models.Enums;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.ViewModels;

public partial class CitasViewModel : ObservableObject
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
    [NotifyPropertyChangedFor(nameof(PuedeModificar))]
    [NotifyPropertyChangedFor(nameof(PuedeCancelar))]
    private Cita? selectedCita;

    [ObservableProperty]
    private Mascota? selectedMascota;

    [ObservableProperty]
    private Profesional? selectedProfesional;

    [ObservableProperty]
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
    private bool isBusy;

    public bool PuedeModificar => SelectedCita is not null && !MostrandoCanceladas;
    public bool PuedeCancelar => SelectedCita is not null && !MostrandoCanceladas;

    [RelayCommand]
    public Task CargarAsync() => EjecutarSeguroAsync(() => RecargarAsync());

    partial void OnSelectedCitaChanged(Cita? value)
    {
        if (value is null)
            return;

        FechaPrevista = new DateTimeOffset(value.FechaPrevista);
        Motivo = value.Motivo;
        Notas = value.Notas;
        SelectedEstado = value.Estado;
        SelectedMascota = MascotasItems.FirstOrDefault(m => m.Id == value.IdMascota);
        SelectedProfesional = ProfesionalesItems.FirstOrDefault(p => p.Id == value.IdProfesional);
    }

    partial void OnBusquedaCitasChanged(string value)
    {
        FiltrarCitas();
    }

    [RelayCommand]
    private async Task AgregarCitaAsync()
    {
        if (_citaService is null)
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

            MensajeOperacion = resultado.Mensaje;

            if (!resultado.Correcta || resultado.Valor is null)
                return;

            MostrandoCanceladas = false;
            MostrandoAgenda = false;
            LimpiarFormulario();
            await RecargarAsync(resultado.Valor.Id);
        });
    }

    [RelayCommand]
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

            MensajeOperacion = resultado.Mensaje;

            if (resultado.Correcta)
                await RecargarAsync(citaId);
        });
    }

    [RelayCommand]
    private async Task CambiarEstadoCitaAsync()
    {
        if (_citaService is null || SelectedCita is null)
            return;

        var citaId = SelectedCita.Id;

        await EjecutarSeguroAsync(async () =>
        {
            var resultado = await _citaService.CambiarEstadoAsync(citaId, SelectedEstado);
            MensajeOperacion = resultado.Mensaje;

            if (resultado.Correcta)
                await RecargarAsync(citaId);
        });
    }

    [RelayCommand]
    private async Task CancelarCitaAsync()
    {
        if (_citaService is null || SelectedCita is null)
            return;

        var citaId = SelectedCita.Id;

        await EjecutarSeguroAsync(async () =>
        {
            var resultado = await _citaService.CancelarAsync(citaId);
            MensajeOperacion = resultado.Mensaje;

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

        if (AgendaDesde is null || AgendaHasta is null)
        {
            MensajeOperacion = "Seleccioná el inicio y el fin de la agenda.";
            return;
        }

        var desde = AgendaDesde.Value.LocalDateTime.Date;
        var hasta = AgendaHasta.Value.LocalDateTime.Date.AddDays(1).AddTicks(-1);

        if (desde > hasta)
        {
            MensajeOperacion = "El inicio de la agenda no puede ser posterior al fin.";
            return;
        }

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
            MensajeOperacion = $"Agenda cargada: {citas.Count} cita(s).";
        });
    }

    [RelayCommand]
    private void NuevaCita()
    {
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

    private async Task RecargarAgendaAsync()
    {
        if (_citaService is null || AgendaDesde is null || AgendaHasta is null)
            return;

        var desde = AgendaDesde.Value.LocalDateTime.Date;
        var hasta = AgendaHasta.Value.LocalDateTime.Date.AddDays(1).AddTicks(-1);
        var citas = await _citaService.ObtenerAgendaAsync(desde, hasta);

        _citasAgenda.Clear();
        _citasAgenda.AddRange(citas);
    }

    private void FiltrarCitas(int? citaIdSeleccionada = null)
    {
        var idSeleccionado = citaIdSeleccionada ?? SelectedCita?.Id;
        var termino = BusquedaCitas.Trim();
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

    private bool TryObtenerDatosFormulario(out DateTime fecha, out int mascotaId)
    {
        fecha = default;
        mascotaId = default;

        if (FechaPrevista is null)
        {
            MensajeOperacion = "Seleccioná la fecha y hora de la cita.";
            return false;
        }

        if (SelectedMascota is null)
        {
            MensajeOperacion = "Seleccioná una mascota.";
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
            MensajeOperacion = $"No se pudo completar la operación: {ex.Message}";
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
