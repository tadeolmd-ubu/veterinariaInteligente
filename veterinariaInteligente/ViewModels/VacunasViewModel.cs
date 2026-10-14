using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using veterinariaInteligente.Models;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.ViewModels;

public partial class VacunasViewModel : PdfViewModel
{
    private readonly VacunaService? _vacunas;
    private readonly AplicacionVacunaService? _aplicaciones;
    private readonly MascotaService? _mascotas;
    private readonly ProfesionalService? _profesionales;
    private List<Vacuna> _catalogo = [];
    private List<AplicacionVacuna> _historial = [];

    public VacunasViewModel() { }
    public VacunasViewModel(VacunaService vacunas, AplicacionVacunaService aplicaciones,
        MascotaService mascotas, ProfesionalService profesionales, PdfService? pdfService = null)
    {
        _vacunas = vacunas;
        _aplicaciones = aplicaciones;
        _mascotas = mascotas;
        _profesionales = profesionales;
        if (pdfService is not null) ConfigurarPdf(pdfService);
    }

    public ObservableCollection<Vacuna> VacunasItems { get; } = [];
    public ObservableCollection<Vacuna> VacunasActivasItems { get; } = [];
    public ObservableCollection<AplicacionVacuna> AplicacionesItems { get; } = [];
    public ObservableCollection<Mascota> MascotasItems { get; } = [];
    public ObservableCollection<Profesional> ProfesionalesItems { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AgregarVacunaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ActualizarVacunaCommand))]
    [NotifyCanExecuteChangedFor(nameof(DesactivarVacunaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ActivarVacunaCommand))]
    private Vacuna? selectedVacuna;
    [ObservableProperty] private Vacuna? selectedVacunaAplicacion;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AgregarAplicacionCommand))]
    [NotifyCanExecuteChangedFor(nameof(ActualizarAplicacionCommand))]
    private AplicacionVacuna? selectedAplicacion;
    [ObservableProperty] private Mascota? selectedMascota;
    [ObservableProperty] private Mascota? filtroMascota;
    [ObservableProperty] private Profesional? selectedProfesional;
    [ObservableProperty] private string nombre = string.Empty;
    [ObservableProperty] private string? descripcion;
    [ObservableProperty] private string? lote;
    [ObservableProperty] private string? observacion;
    [ObservableProperty] private DateTimeOffset? fechaAplicacion = DateTimeOffset.Now;
    [ObservableProperty] private DateTimeOffset? fechaProximaSugerida;
    [ObservableProperty] private bool mostrandoInactivas;
    [ObservableProperty] private string busquedaVacunas = string.Empty;
    [ObservableProperty] private string mensajeOperacion = string.Empty;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AgregarVacunaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ActualizarVacunaCommand))]
    [NotifyCanExecuteChangedFor(nameof(DesactivarVacunaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ActivarVacunaCommand))]
    [NotifyCanExecuteChangedFor(nameof(AgregarAplicacionCommand))]
    [NotifyCanExecuteChangedFor(nameof(ActualizarAplicacionCommand))]
    private bool isBusy;

    private bool PuedeCrearVacuna() => !IsBusy && SelectedVacuna is null;
    private bool PuedeModificarVacuna() => !IsBusy && SelectedVacuna?.Activo == 1;
    private bool PuedeActivarVacuna() => !IsBusy && SelectedVacuna?.Activo == 0;
    private bool PuedeAgregarAplicacion() => !IsBusy && SelectedAplicacion is null;
    private bool PuedeActualizarAplicacion() => !IsBusy && SelectedAplicacion is not null;

    partial void OnSelectedMascotaChanged(Mascota? value) => LimpiarPdf();
    protected override void InformarPdf(string mensaje, bool correcta = false)
        => MensajeOperacion = Notificar(mensaje, correcta);
    [RelayCommand]
    private Task GenerarCartillaPdfAsync() => EjecutarAsync(() =>
        GenerarPdfAsync(SelectedMascota?.Id, "cartilla", (pdf, id) => pdf.GenerarCartillaVacunacionAsync(id)));

    partial void OnSelectedVacunaChanged(Vacuna? value)
    {
        Nombre = value?.Nombre ?? string.Empty;
        Descripcion = value?.Descripcion;
    }
    partial void OnSelectedAplicacionChanged(AplicacionVacuna? value)
    {
        SelectedMascota = MascotasItems.FirstOrDefault(m => m.Id == value?.IdMascota);
        SelectedVacunaAplicacion = VacunasActivasItems.FirstOrDefault(v => v.Id == value?.IdVacuna);
        SelectedProfesional = ProfesionalesItems.FirstOrDefault(p => p.Id == value?.IdProfesional);
        FechaAplicacion = value is null ? DateTimeOffset.Now : AFecha(value.FechaAplicacion);
        FechaProximaSugerida = value?.FechaProximaSugerida is { } fecha ? AFecha(fecha) : null;
        Lote = value?.Lote;
        Observacion = value?.Observacion;
    }
    partial void OnMostrandoInactivasChanged(bool value) => FiltrarVacunas();
    partial void OnBusquedaVacunasChanged(string value) => FiltrarVacunas();
    partial void OnFiltroMascotaChanged(Mascota? value) => FiltrarAplicaciones();

    [RelayCommand]
    public Task CargarAsync() => EjecutarAsync(() => RecargarAsync());

    [RelayCommand(CanExecute = nameof(PuedeCrearVacuna))]
    private Task AgregarVacunaAsync() => EjecutarAsync(async () =>
    {
        if (_vacunas is null || SelectedVacuna is not null) return;
        var resultado = await _vacunas.AgregarAsync(Nombre, Descripcion);
        MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);
        if (!resultado.Correcta) return;
        MostrandoInactivas = false;
        BusquedaVacunas = string.Empty;
        await RecargarAsync(resultado.Valor?.Id);
    });

    [RelayCommand(CanExecute = nameof(PuedeModificarVacuna))]
    private Task ActualizarVacunaAsync() => OperarVacunaAsync(
        v => _vacunas!.ActualizarAsync(v.Id, Nombre, Descripcion), activa: true);

    [RelayCommand(CanExecute = nameof(PuedeModificarVacuna))]
    private Task DesactivarVacunaAsync() => OperarVacunaAsync(
        v => _vacunas!.DesactivarAsync(v.Id), activa: true);

    [RelayCommand(CanExecute = nameof(PuedeActivarVacuna))]
    private Task ActivarVacunaAsync() => OperarVacunaAsync(
        v => _vacunas!.ActivarAsync(v.Id), activa: false);

    private Task OperarVacunaAsync(Func<Vacuna, Task<Resultado>> operacion, bool activa)
    {
        var vacuna = SelectedVacuna;
        if (_vacunas is null || vacuna is null || (vacuna.Activo == 1) != activa)
            return Task.CompletedTask;
        return EjecutarAsync(async () =>
        {
            var resultado = await operacion(vacuna);
            MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);
            if (resultado.Correcta) await RecargarAsync(vacuna.Id);
        });
    }

    [RelayCommand(CanExecute = nameof(PuedeAgregarAplicacion))]
    private Task AgregarAplicacionAsync() => GuardarAplicacionAsync(false);
    [RelayCommand(CanExecute = nameof(PuedeActualizarAplicacion))]
    private Task ActualizarAplicacionAsync() => GuardarAplicacionAsync(true);

    private Task GuardarAplicacionAsync(bool editar) => EjecutarAsync(async () =>
    {
        if (_aplicaciones is null || (editar && SelectedAplicacion is null) ||
            (!editar && SelectedAplicacion is not null)) return;
        if (SelectedVacunaAplicacion is null || FechaAplicacion is null ||
            (!editar && SelectedMascota is null))
        {
            MensajeOperacion = Notificar("Seleccioná una vacuna activa, una mascota y la fecha de aplicación.");
            return;
        }
        if (editar && SelectedProfesional is null)
        {
            MensajeOperacion = Notificar("El servicio requiere un profesional para editar la aplicación.");
            return;
        }
        var fecha = DateOnly.FromDateTime(FechaAplicacion.Value.Date);
        DateOnly? proxima = FechaProximaSugerida is { } f ? DateOnly.FromDateTime(f.Date) : null;
        if (editar)
        {
            var id = SelectedAplicacion!.Id;
            var resultado = await _aplicaciones.ActualizarAsync(id, fecha, Lote, Observacion,
                proxima, SelectedVacunaAplicacion.Id, SelectedProfesional!.Id);
            MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);
            if (resultado.Correcta) await RecargarAsync(aplicacionId: id);
        }
        else
        {
            var resultado = await _aplicaciones.AgregarAsync(fecha, Lote, Observacion,
                proxima, SelectedVacunaAplicacion.Id, SelectedMascota!.Id, SelectedProfesional?.Id);
            MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);
            if (resultado.Correcta)
            {
                FiltroMascota = null;
                await RecargarAsync(aplicacionId: resultado.Valor?.Id);
            }
        }
    });

    [RelayCommand]
    private void NuevaVacuna()
    {
        if (IsBusy) return;
        SelectedVacuna = null;
        Nombre = string.Empty;
        Descripcion = null;
        MensajeOperacion = string.Empty;
    }

    [RelayCommand]
    private void NuevaAplicacion()
    {
        if (IsBusy) return;
        SelectedAplicacion = null;
        OnSelectedAplicacionChanged(null);
        MensajeOperacion = string.Empty;
    }

    private async Task RecargarAsync(int? vacunaId = null, int? aplicacionId = null)
    {
        if (_vacunas is null || _aplicaciones is null || _mascotas is null || _profesionales is null) return;
        var activas = await _vacunas.ObtenerTodosAsync();
        var inactivas = await _vacunas.ObtenerInactivasAsync();
        var aplicaciones = await _aplicaciones.ObtenerTodosAsync();
        var mascotas = await _mascotas.ObtenerTodosAsync();
        var profesionales = await _profesionales.ObtenerTodosAsync();
        var mascotaId = SelectedMascota?.Id;
        var profesionalId = SelectedProfesional?.Id;
        var vacunaAplicacionId = SelectedVacunaAplicacion?.Id;
        _catalogo = [.. activas, .. inactivas];
        _historial = aplicaciones;
        Reemplazar(VacunasActivasItems, activas);
        Reemplazar(MascotasItems, mascotas.Where(m => m.IdEstado == 1));
        Reemplazar(ProfesionalesItems, profesionales);
        SelectedMascota = MascotasItems.FirstOrDefault(m => m.Id == mascotaId);
        SelectedProfesional = ProfesionalesItems.FirstOrDefault(p => p.Id == profesionalId);
        SelectedVacunaAplicacion = VacunasActivasItems.FirstOrDefault(v => v.Id == vacunaAplicacionId);
        FiltrarVacunas(vacunaId);
        FiltrarAplicaciones(aplicacionId);
    }

    private void FiltrarVacunas(int? id = null)
    {
        id ??= SelectedVacuna?.Id;
        var termino = BusquedaVacunas?.Trim() ?? string.Empty;
        Reemplazar(VacunasItems, _catalogo.Where(v => (v.Activo == 0) == MostrandoInactivas &&
            v.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase)));
        SelectedVacuna = VacunasItems.FirstOrDefault(v => v.Id == id);
    }

    private void FiltrarAplicaciones(int? id = null)
    {
        id ??= SelectedAplicacion?.Id;
        Reemplazar(AplicacionesItems, _historial.Where(a =>
            FiltroMascota is null || a.IdMascota == FiltroMascota.Id));
        SelectedAplicacion = AplicacionesItems.FirstOrDefault(a => a.Id == id);
    }

    private async Task EjecutarAsync(Func<Task> operacion)
    {
        if (IsBusy) return;
        IsBusy = true;
        try { await operacion(); }
        catch (Exception ex) { MensajeOperacion = Notificar($"No se pudo completar la operación: {ex.Message}"); }
        finally { IsBusy = false; }
    }

    private static DateTimeOffset AFecha(DateOnly fecha) =>
        new(fecha.ToDateTime(TimeOnly.MinValue));
    private static void Reemplazar<T>(ObservableCollection<T> destino, IEnumerable<T> items)
    {
        destino.Clear();
        foreach (var item in items) destino.Add(item);
    }
}
