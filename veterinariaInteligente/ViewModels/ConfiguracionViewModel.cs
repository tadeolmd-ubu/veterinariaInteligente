using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using veterinariaInteligente.Models;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.ViewModels;

public partial class ConfiguracionViewModel : PdfViewModel
{
    private readonly ProfesionalService? _profesionales;
    private readonly RespaldoService? _respaldos;
    private readonly ConfiguracionClienteService? _configuracion;
    private readonly MascotaService? _mascotas;
    private readonly ConsultaService? _consultas;
    private List<Profesional> _cargados = [];
    private List<Consulta> _consultasCargadas = [];

    public ConfiguracionViewModel() { }
    // Front debe pasar ConfiguracionClienteService como sexto argumento en App.axaml.cs.
    // El argumento opcional conserva la composición existente hasta esa integración.
    public ConfiguracionViewModel(ProfesionalService profesionales, RespaldoService respaldos,
        PdfService pdf, MascotaService mascotas, ConsultaService consultas,
        ConfiguracionClienteService? configuracion = null)
    {
        _profesionales = profesionales;
        _respaldos = respaldos;
        ConfigurarPdf(pdf);
        _configuracion = configuracion;
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
    [ObservableProperty] private string nombreVeterinaria = string.Empty;
    [ObservableProperty] private string telefonoVeterinaria = string.Empty;
    [ObservableProperty] private string direccionVeterinaria = string.Empty;
    [ObservableProperty] private int? idProfesionalDefault;
    /// <summary>Front registra un único FolderPicker; null conserva la carpeta actual.</summary>
    public event Func<Task<string?>>? SeleccionCarpetaRespaldoSolicitada;
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
    public Task CargarAsync() => EjecutarAsync(async () =>
    {
        await RecargarAsync();
        if (_configuracion is not null) await CargarConfiguracionInternaAsync();
    });

    [RelayCommand]
    private Task CargarConfiguracionAsync() => EjecutarAsync(CargarConfiguracionInternaAsync);

    private async Task CargarConfiguracionInternaAsync()
    {
        if (_configuracion is null)
        {
            MensajeOperacion = Notificar("El servicio de configuración no está conectado.");
            return;
        }
        var datos = await _configuracion.ObtenerAsync();
        NombreVeterinaria = datos?.Nombre ?? string.Empty;
        TelefonoVeterinaria = datos?.Telefono ?? string.Empty;
        DireccionVeterinaria = datos?.Direccion ?? string.Empty;
        IdProfesionalDefault = datos?.IdProfesionalDefault;
    }

    [RelayCommand]
    private Task GuardarConfiguracionAsync() => EjecutarAsync(async () =>
    {
        if (_configuracion is null)
        {
            MensajeOperacion = Notificar("El servicio de configuración no está conectado.");
            return;
        }
        var resultado = await _configuracion.GuardarAsync(NombreVeterinaria,
            TelefonoVeterinaria, DireccionVeterinaria, IdProfesionalDefault);
        MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);
        if (resultado.Correcta) await CargarConfiguracionInternaAsync();
    });

    [RelayCommand]
    private Task SeleccionarCarpetaRespaldoAsync() => EjecutarAsync(async () =>
    {
        if (SeleccionCarpetaRespaldoSolicitada is not { } seleccionar) return;
        var carpeta = await seleccionar();
        if (!string.IsNullOrWhiteSpace(carpeta)) CarpetaRespaldo = carpeta;
    });
    [RelayCommand]
    private Task AgregarProfesionalAsync() => EjecutarAsync(async () =>
    {
        if (_profesionales is null) return;
        var resultado = await _profesionales.AgregarAsync(Nombre, Cedula);
        MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);
        if (!resultado.Correcta) return;
        MostrandoInactivos = false;
        BusquedaProfesionales = string.Empty;
        SelectedProfesional = null;
        await RecargarAsync();
        LimpiarProfesional();
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
            MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);
            if (resultado.Correcta) await RecargarAsync(id);
        });
    }
    [RelayCommand]
    private void NuevoProfesional()
    {
        if (IsBusy) return;
        LimpiarProfesional();
        MensajeOperacion = string.Empty;
    }

    private void LimpiarProfesional()
    {
        SelectedProfesional = null;
        Nombre = string.Empty;
        Cedula = null;
    }
    [RelayCommand]
    private Task CrearRespaldoAsync() => EjecutarAsync(async () =>
    {
        RutaRespaldo = null;
        if (_respaldos is null) return;
        var resultado = await _respaldos.CrearRespaldoAsync(CarpetaRespaldo);
        MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);
        if (resultado.Correcta) RutaRespaldo = resultado.Valor;
    });
    [RelayCommand]
    // Adaptadores transitorios para los bindings actuales. Front debe retirar este bloque
    // y usar los comandos PDF de Consultas, Propietarios/Expediente y Vacunas.
    [Obsolete("Usar GenerarConsultaPdfCommand en ConsultasViewModel.")]
    private Task GenerarConsultaPdfAsync() => EjecutarAsync(() => GenerarPdfAsync(
        SelectedConsulta?.Id, "consulta", (pdf, id) => pdf.GenerarConsultaAsync(id)));
    [RelayCommand]
    [Obsolete("Usar GenerarExpedientePdfCommand en PropietariosViewModel/ExpedienteViewModel.")]
    private Task GenerarExpedientePdfAsync() => EjecutarAsync(() => GenerarPdfAsync(
        SelectedMascota?.Id, "expediente", (pdf, id) => pdf.GenerarExpedienteAsync(id)));
    [RelayCommand]
    [Obsolete("Usar GenerarCartillaPdfCommand en VacunasViewModel.")]
    private Task GenerarCartillaPdfAsync() => EjecutarAsync(() => GenerarPdfAsync(
        SelectedMascota?.Id, "cartilla", (pdf, id) => pdf.GenerarCartillaVacunacionAsync(id)));

    protected override void InformarPdf(string mensaje, bool correcta = false)
        => MensajeOperacion = Notificar(mensaje, correcta);
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
        catch (Exception ex) { MensajeOperacion = Notificar($"No se pudo completar la operación: {ex.Message}"); }
        finally { IsBusy = false; }
    }
    private static void Reemplazar<T>(ObservableCollection<T> destino, IEnumerable<T> items)
    {
        destino.Clear();
        foreach (var item in items) destino.Add(item);
    }
}
