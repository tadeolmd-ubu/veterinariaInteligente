using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using veterinariaInteligente.Models;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.ViewModels;

public partial class ConfiguracionViewModel : ObservableObject
{
    private readonly ProfesionalService? _profesionales;
    private readonly RespaldoService? _respaldos;
    private readonly PdfService? _pdf;
    private readonly MascotaService? _mascotas;
    private readonly ConsultaService? _consultas;
    private List<Profesional> _cargados = [];
    private List<Consulta> _consultasCargadas = [];

    public ConfiguracionViewModel() { }
    public ConfiguracionViewModel(ProfesionalService profesionales, RespaldoService respaldos,
        PdfService pdf, MascotaService mascotas, ConsultaService consultas)
    {
        _profesionales = profesionales;
        _respaldos = respaldos;
        _pdf = pdf;
        _mascotas = mascotas;
        _consultas = consultas;
    }

    public ObservableCollection<Profesional> ProfesionalesItems { get; } = [];
    public ObservableCollection<Mascota> MascotasItems { get; } = [];
    public ObservableCollection<Consulta> ConsultasItems { get; } = [];
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ActualizarProfesionalCommand))]
    [NotifyCanExecuteChangedFor(nameof(DesactivarProfesionalCommand))]
    [NotifyCanExecuteChangedFor(nameof(ActivarProfesionalCommand))]
    private Profesional? selectedProfesional;
    [ObservableProperty] private Mascota? selectedMascota;
    [ObservableProperty] private Consulta? selectedConsulta;
    [ObservableProperty] private string nombre = string.Empty;
    [ObservableProperty] private string? cedula;
    [ObservableProperty] private string busquedaProfesionales = string.Empty;
    [ObservableProperty] private bool mostrandoInactivos;
    [ObservableProperty] private string carpetaRespaldo = string.Empty;
    [ObservableProperty] private string? rutaRespaldo;
    // La vista decide dónde guardar/mostrar los bytes devueltos por PdfService.
    [ObservableProperty] private byte[]? pdfGenerado;
    [ObservableProperty] private string? nombreArchivoPdf;
    [ObservableProperty] private string mensajeOperacion = string.Empty;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ActualizarProfesionalCommand))]
    [NotifyCanExecuteChangedFor(nameof(DesactivarProfesionalCommand))]
    [NotifyCanExecuteChangedFor(nameof(ActivarProfesionalCommand))]
    private bool isBusy;

    private bool PuedeModificar() => !IsBusy && SelectedProfesional?.Activo == 1;
    private bool PuedeActivar() => !IsBusy && SelectedProfesional?.Activo == 0;
    partial void OnSelectedProfesionalChanged(Profesional? value)
    {
        Nombre = value?.Nombre ?? string.Empty;
        Cedula = value?.Cedula;
    }
    partial void OnMostrandoInactivosChanged(bool value) => Filtrar();
    partial void OnBusquedaProfesionalesChanged(string value) => Filtrar();
    partial void OnSelectedMascotaChanged(Mascota? value)
    {
        SelectedConsulta = null;
        Reemplazar(ConsultasItems, _consultasCargadas.Where(c => c.IdMascota == value?.Id));
        LimpiarPdf();
    }
    partial void OnSelectedConsultaChanged(Consulta? value) => LimpiarPdf();

    [RelayCommand]
    public Task CargarAsync() => EjecutarAsync(() => RecargarAsync());
    [RelayCommand]
    private Task AgregarProfesionalAsync() => EjecutarAsync(async () =>
    {
        if (_profesionales is null) return;
        var resultado = await _profesionales.AgregarAsync(Nombre, Cedula);
        MensajeOperacion = resultado.Mensaje;
        if (!resultado.Correcta) return;
        MostrandoInactivos = false;
        BusquedaProfesionales = string.Empty;
        await RecargarAsync(resultado.Valor?.Id);
    });
    [RelayCommand(CanExecute = nameof(PuedeModificar))]
    private Task ActualizarProfesionalAsync() => OperarProfesionalAsync(
        id => _profesionales!.ActualizarAsync(id, Nombre, Cedula), true);
    [RelayCommand(CanExecute = nameof(PuedeModificar))]
    private Task DesactivarProfesionalAsync() => OperarProfesionalAsync(
        id => _profesionales!.DesactivarAsync(id), true);
    [RelayCommand(CanExecute = nameof(PuedeActivar))]
    private Task ActivarProfesionalAsync() => OperarProfesionalAsync(
        id => _profesionales!.ActivarAsync(id), false);

    private Task OperarProfesionalAsync(Func<int, Task<Resultado>> accion, bool activo)
    {
        if (_profesionales is null || !(activo ? PuedeModificar() : PuedeActivar()))
            return Task.CompletedTask;
        var id = SelectedProfesional!.Id;
        return EjecutarAsync(async () =>
        {
            var resultado = await accion(id);
            MensajeOperacion = resultado.Mensaje;
            if (resultado.Correcta) await RecargarAsync(id);
        });
    }
    [RelayCommand]
    private void NuevoProfesional()
    {
        if (IsBusy) return;
        SelectedProfesional = null;
        Nombre = string.Empty;
        Cedula = null;
        MensajeOperacion = string.Empty;
    }
    [RelayCommand]
    private Task CrearRespaldoAsync() => EjecutarAsync(async () =>
    {
        RutaRespaldo = null;
        if (_respaldos is null) return;
        var resultado = await _respaldos.CrearRespaldoAsync(CarpetaRespaldo);
        MensajeOperacion = resultado.Mensaje;
        if (resultado.Correcta) RutaRespaldo = resultado.Valor;
    });
    [RelayCommand]
    private Task GenerarConsultaPdfAsync() => GenerarPdfAsync(
        SelectedConsulta?.Id, "consulta", id => _pdf!.GenerarConsultaAsync(id));
    [RelayCommand]
    private Task GenerarExpedientePdfAsync() => GenerarPdfAsync(
        SelectedMascota?.Id, "expediente", id => _pdf!.GenerarExpedienteAsync(id));
    [RelayCommand]
    private Task GenerarCartillaPdfAsync() => GenerarPdfAsync(
        SelectedMascota?.Id, "cartilla", id => _pdf!.GenerarCartillaVacunacionAsync(id));

    private Task GenerarPdfAsync(int? id, string tipo, Func<int, Task<Resultado<byte[]>>> generar)
        => EjecutarAsync(async () =>
        {
            LimpiarPdf();
            if (_pdf is null) return;
            if (id is null)
            {
                MensajeOperacion = "Seleccioná el registro para generar el PDF.";
                return;
            }
            var resultado = await generar(id.Value);
            MensajeOperacion = resultado.Mensaje;
            if (!resultado.Correcta || resultado.Valor is null) return;
            PdfGenerado = resultado.Valor;
            NombreArchivoPdf = $"{tipo}_{id}.pdf";
        });

    private void LimpiarPdf()
    {
        PdfGenerado = null;
        NombreArchivoPdf = null;
    }
    private async Task RecargarAsync(int? id = null)
    {
        if (_profesionales is null || _mascotas is null || _consultas is null) return;
        var activos = await _profesionales.ObtenerTodosAsync();
        var inactivos = await _profesionales.ObtenerInactivosAsync();
        var mascotas = await _mascotas.ObtenerTodosAsync();
        var consultas = await _consultas.ObtenerTodosAsync();
        var mascotaId = SelectedMascota?.Id;
        var consultaId = SelectedConsulta?.Id;
        _cargados = [.. activos, .. inactivos];
        _consultasCargadas = consultas;
        Reemplazar(MascotasItems, mascotas);
        SelectedMascota = MascotasItems.FirstOrDefault(m => m.Id == mascotaId);
        SelectedConsulta = ConsultasItems.FirstOrDefault(c => c.Id == consultaId);
        Filtrar(id);
    }
    private void Filtrar(int? id = null)
    {
        id ??= SelectedProfesional?.Id;
        var texto = BusquedaProfesionales?.Trim() ?? string.Empty;
        Reemplazar(ProfesionalesItems, _cargados.Where(p =>
            (p.Activo == 0) == MostrandoInactivos &&
            (p.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
             (p.Cedula?.Contains(texto, StringComparison.OrdinalIgnoreCase) ?? false))));
        SelectedProfesional = ProfesionalesItems.FirstOrDefault(p => p.Id == id);
    }
    private async Task EjecutarAsync(Func<Task> operacion)
    {
        if (IsBusy) return;
        IsBusy = true;
        try { await operacion(); }
        catch (Exception ex) { MensajeOperacion = $"No se pudo completar la operación: {ex.Message}"; }
        finally { IsBusy = false; }
    }
    private static void Reemplazar<T>(ObservableCollection<T> destino, IEnumerable<T> items)
    {
        destino.Clear();
        foreach (var item in items) destino.Add(item);
    }
}
