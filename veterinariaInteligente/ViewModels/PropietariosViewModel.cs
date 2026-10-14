using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using veterinariaInteligente.Models;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.ViewModels;

public partial class PropietariosViewModel : PdfViewModel
{
    private const int EstadoMascotaActivaId = 1;

    private readonly DuenioService? _duenioService;
    private readonly MascotaService? _mascotaService;
    private readonly List<Duenio> _propietariosCargados = [];
    private readonly List<Duenio> _propietariosArchivados = [];

    [ObservableProperty]
    private bool mostrandoArchivados;

    partial void OnMostrandoArchivadosChanged(bool value) => FiltrarPropietarios();
    private readonly List<Mascota> _mascotasCargadas = [];
    private readonly List<Mascota> _mascotasArchivadas = [];

    public PropietariosViewModel()
    {
    }

    public PropietariosViewModel(
        DuenioService duenioService,
        MascotaService mascotaService, PdfService? pdfService = null)
    {
        _duenioService = duenioService;
        _mascotaService = mascotaService;
        if (pdfService is not null) ConfigurarPdf(pdfService);
    }

    public ObservableCollection<Duenio> PropietariosItems { get; } = [];
    public ObservableCollection<Mascota> MascotasItems { get; } = [];
    public ObservableCollection<Duenio> PropietariosActivos { get; } = [];
    public ObservableCollection<Duenio> PropietariosArchivados { get; } = [];
    public ObservableCollection<Mascota> MascotasActivas { get; } = [];
    public ObservableCollection<Mascota> MascotasArchivadas { get; } = [];
    [ObservableProperty] private string editarNombre = string.Empty;
    [ObservableProperty] private string editarTelefono = string.Empty;
    [ObservableProperty] private string? editarCorreo;
    [ObservableProperty] private string? editarDireccion;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NombreDuenioMascota))]
    private Duenio? duenioSeleccionadoParaMascota;
    public string NombreDuenioMascota => DuenioSeleccionadoParaMascota?.Nombre ?? string.Empty;
    /// <summary>Front registra un único selector modal; null significa cancelar.</summary>
    public event Func<Task<Duenio?>>? SeleccionPropietarioSolicitada;

    [RelayCommand]
    private Task SeleccionarDuenioMascotaAsync() => EjecutarSeguroAsync(async () =>
    {
        if (SeleccionPropietarioSolicitada is not { } seleccionar) return;
        var duenio = await seleccionar();
        if (duenio is not null && duenio.FechaArchivado is null)
            DuenioSeleccionadoParaMascota = duenio;
    });
    public IReadOnlyList<string> SexosMascota { get; } = ["Macho", "Hembra"];

    public event Action<Mascota, Duenio>? ExpedienteSolicitado;

    partial void OnSelectedMascotaChanged(Mascota? value) => LimpiarPdf();
    protected override void InformarPdf(string mensaje, bool correcta = false)
        => MensajeOperacion = Notificar(mensaje, correcta);
    [RelayCommand]
    private Task GenerarExpedientePdfAsync() => EjecutarSeguroAsync(() =>
        GenerarPdfAsync(SelectedMascota?.Id, "expediente", (pdf, id) => pdf.GenerarExpedienteAsync(id)));

    [ObservableProperty]
    private Duenio? selectedPropietario;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RestaurarMascotaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ArchivarMascotaCommand))]
    private Mascota? selectedMascota;

    [ObservableProperty]
    private string busquedaPropietarios = string.Empty;

    [ObservableProperty]
    private string nuevoPropietarioNombre = string.Empty;

    [ObservableProperty]
    private string nuevoPropietarioTelefono = string.Empty;

    [ObservableProperty]
    private string? nuevoPropietarioCorreo;

    [ObservableProperty]
    private string? nuevoPropietarioDireccion;

    [ObservableProperty]
    private string nuevaMascotaNombre = string.Empty;

    [ObservableProperty]
    private string nuevaMascotaRaza = string.Empty;

    [ObservableProperty]
    private string nuevaMascotaSexo = "Macho";

    [ObservableProperty]
    private string nuevaMascotaColor = string.Empty;

    [ObservableProperty]
    private string? nuevaMascotaAlergia;

    [ObservableProperty]
    private DateTimeOffset? nuevaMascotaFechaNacimiento;

    [ObservableProperty]
    private string mensajeOperacion = string.Empty;

    public Task CargarAsync() => EjecutarSeguroAsync(() => RecargarAsync());

    partial void OnSelectedPropietarioChanged(Duenio? value)
    {
        EditarNombre = value?.Nombre ?? string.Empty;
        EditarTelefono = value?.Telefono ?? string.Empty;
        EditarCorreo = value?.Correo;
        EditarDireccion = value?.Direccion;
        MascotasItems.Clear();
        MascotasActivas.Clear();
        MascotasArchivadas.Clear();
        SelectedMascota = null;

        if (value is null)
            return;

        foreach (var mascota in _mascotasCargadas.Where(m => m.IdDuenio == value.Id))
        {
            MascotasActivas.Add(mascota);
            MascotasItems.Add(mascota);
        }
        foreach (var mascota in _mascotasArchivadas.Where(m => m.IdDuenio == value.Id))
        {
            MascotasArchivadas.Add(mascota);
            MascotasItems.Add(mascota);
        }
    }

    partial void OnBusquedaPropietariosChanged(string value)
    {
        FiltrarPropietarios();
    }

    [RelayCommand]
    private Task AgregarPropietarioAsync() => EjecutarSeguroAsync(AgregarPropietarioInternoAsync);

    private async Task AgregarPropietarioInternoAsync()
    {
        if (_duenioService is null)
            return;

        var resultado = await _duenioService.AgregarAsync(
            NuevoPropietarioNombre,
            NuevoPropietarioTelefono,
            NuevoPropietarioCorreo,
            NuevoPropietarioDireccion);

        MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);

        if (!resultado.Correcta || resultado.Valor is null)
            return;

        LimpiarFormularioPropietario();
        SelectedPropietario = null;
        await RecargarAsync();
    }

    [RelayCommand]
    private Task ActualizarPropietarioAsync() => EjecutarSeguroAsync(ActualizarPropietarioInternoAsync);

    private async Task ActualizarPropietarioInternoAsync()
    {
        if (_duenioService is null || SelectedPropietario is null)
            return;

        var propietario = SelectedPropietario;
        var resultado = await _duenioService.ActualizarAsync(
            propietario.Id,
            EditarNombre,
            EditarTelefono,
            EditarCorreo,
            EditarDireccion);

        MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);

        if (resultado.Correcta)
            await RecargarAsync(propietario.Id);
    }

    [RelayCommand]
    private Task ArchivarPropietarioAsync() => EjecutarSeguroAsync(ArchivarPropietarioInternoAsync);

    private async Task ArchivarPropietarioInternoAsync()
    {
        if (_duenioService is null || SelectedPropietario is null)
            return;

        var resultado = await _duenioService.ArchivarAsync(SelectedPropietario.Id);
        MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);

        if (resultado.Correcta)
            await RecargarAsync();
    }

    [RelayCommand]
    private Task RestaurarPropietarioAsync() => EjecutarSeguroAsync(RestaurarPropietarioInternoAsync);

    private async Task RestaurarPropietarioInternoAsync()
    {
        if (_duenioService is null || SelectedPropietario is null)
            return;

        var id = SelectedPropietario.Id;
        var resultado = await _duenioService.RestaurarAsync(id);
        MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);

        if (resultado.Correcta)
        {
            BusquedaPropietarios = string.Empty;
            await RecargarAsync(id);
        }
    }

    [RelayCommand]
    private Task AgregarMascotaAsync() => EjecutarSeguroAsync(AgregarMascotaInternoAsync);

    private async Task AgregarMascotaInternoAsync()
    {
        if (_mascotaService is null) return;
        if (DuenioSeleccionadoParaMascota is not { FechaArchivado: null } duenio)
        {
            MensajeOperacion = Notificar("Seleccioná un dueño activo para la mascota.");
            return;
        }
        if (NuevaMascotaFechaNacimiento?.Date == DateTime.MinValue.Date)
        {
            MensajeOperacion = Notificar("Seleccioná una fecha de nacimiento válida o dejala vacía.");
            return;
        }

        DateOnly? fechaNacimiento = NuevaMascotaFechaNacimiento is null
            ? null
            : DateOnly.FromDateTime(NuevaMascotaFechaNacimiento.Value.DateTime);

        var resultado = await _mascotaService.AgregarAsync(
            NuevaMascotaNombre,
            NuevaMascotaRaza,
            NuevaMascotaSexo,
            fechaNacimiento,
            NuevaMascotaColor,
            NuevaMascotaAlergia,
            EstadoMascotaActivaId,
            duenio.Id);

        MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);

        if (!resultado.Correcta)
            return;

        var propietarioId = duenio.Id;
        LimpiarFormularioMascota();
        await RecargarAsync(propietarioId);
    }

    private bool PuedeArchivarMascota() => !IsBusy && SelectedMascota is { FechaArchivado: null };
    private bool PuedeRestaurarMascota() => !IsBusy && SelectedMascota?.FechaArchivado is not null;

    [RelayCommand(CanExecute = nameof(PuedeRestaurarMascota))]
    private Task RestaurarMascotaAsync() => EjecutarSeguroAsync(async () =>
    {
        if (_mascotaService is null || SelectedMascota?.FechaArchivado is null) return;
        var id = SelectedMascota.Id;
        var duenioId = SelectedMascota.IdDuenio;
        var resultado = await _mascotaService.RestaurarAsync(id);
        MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);
        if (!resultado.Correcta) return;
        await RecargarAsync(duenioId);
        SelectedMascota = MascotasItems.FirstOrDefault(m => m.Id == id);
    });

    [RelayCommand(CanExecute = nameof(PuedeArchivarMascota))]
    private Task ArchivarMascotaAsync() => EjecutarSeguroAsync(ArchivarMascotaInternoAsync);

    private async Task ArchivarMascotaInternoAsync()
    {
        if (_mascotaService is null || SelectedMascota is null)
            return;

        var propietarioId = SelectedPropietario?.Id;
        var resultado = await _mascotaService.ArchivarAsync(SelectedMascota.Id);
        MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);

        if (resultado.Correcta)
            await RecargarAsync(propietarioId);
    }

    [RelayCommand]
    private void AbrirExpediente()
    {
        if (SelectedMascota is null || SelectedPropietario is null)
            return;

        ExpedienteSolicitado?.Invoke(SelectedMascota, SelectedPropietario);
    }

    private async Task RecargarAsync(int? propietarioIdSeleccionado = null)
    {
        if (_duenioService is null || _mascotaService is null)
            return;

        var propietarios = await _duenioService.ObtenerTodosAsync();
        var mascotas = await _mascotaService.ObtenerTodosAsync();
        var mascotasArchivadas = await _mascotaService.ObtenerArchivadasDeDueniosActivosAsync();
        _mascotasArchivadas.Clear();
        _mascotasArchivadas.AddRange(mascotasArchivadas);
        var archivados = await _duenioService.ObtenerArchivadosAsync();
        _propietariosArchivados.Clear();
        _propietariosArchivados.AddRange(archivados);

        _mascotasCargadas.Clear();
        _mascotasCargadas.AddRange(mascotas);

        _propietariosCargados.Clear();
        _propietariosCargados.AddRange(propietarios);

        foreach (var propietario in _propietariosCargados)
        {
            propietario.Mascotas = _mascotasCargadas
                .Where(m => m.IdDuenio == propietario.Id)
                .ToList();
        }

        FiltrarPropietarios(propietarioIdSeleccionado);
    }

    private void FiltrarPropietarios(int? propietarioIdSeleccionado = null)
    {
        var idSeleccionado = propietarioIdSeleccionado ?? SelectedPropietario?.Id;
        var termino = BusquedaPropietarios?.Trim() ?? string.Empty;
        var origen = _propietariosCargados.Concat(_propietariosArchivados).ToList();

        var propietariosFiltrados = string.IsNullOrEmpty(termino)
            ? origen
            : origen
                .Where(p =>
                    p.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                    p.Telefono.Contains(termino, StringComparison.OrdinalIgnoreCase))
                .ToList();

        PropietariosItems.Clear();
        PropietariosActivos.Clear();
        PropietariosArchivados.Clear();

        foreach (var propietario in propietariosFiltrados)
        {
            if (propietario.FechaArchivado is null)
            {
                PropietariosActivos.Add(propietario);
                PropietariosItems.Add(propietario);
            }
            else
            {
                PropietariosArchivados.Add(propietario);
                if (MostrandoArchivados) PropietariosItems.Add(propietario);
            }
        }

        SelectedPropietario = propietariosFiltrados.FirstOrDefault(p => p.Id == idSeleccionado);
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RestaurarMascotaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ArchivarMascotaCommand))]
    private bool isBusy;
    private async Task EjecutarSeguroAsync(Func<Task> operacion)
    {
        if (IsBusy) return;
        IsBusy = true;
        try { await operacion(); }
        catch (Exception ex) { MensajeOperacion = Notificar($"No se pudo completar la operación: {ex.Message}"); }
        finally { IsBusy = false; }
    }

    private void LimpiarFormularioPropietario()
    {
        NuevoPropietarioNombre = string.Empty;
        NuevoPropietarioTelefono = string.Empty;
        NuevoPropietarioCorreo = null;
        NuevoPropietarioDireccion = null;
    }

    private void LimpiarFormularioMascota()
    {
        NuevaMascotaNombre = string.Empty;
        NuevaMascotaRaza = string.Empty;
        NuevaMascotaSexo = "Macho";
        NuevaMascotaColor = string.Empty;
        NuevaMascotaAlergia = null;
        NuevaMascotaFechaNacimiento = null;
        DuenioSeleccionadoParaMascota = null;
    }
}
