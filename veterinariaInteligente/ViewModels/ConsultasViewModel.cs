using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using veterinariaInteligente.Models;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.ViewModels;

public partial class ConsultasViewModel : ObservableObject
{
    private readonly ConsultaService? _consultaService;
    private readonly MascotaService? _mascotaService;
    private readonly ProfesionalService? _profesionalService;
    private readonly List<Consulta> _consultasActivas = [];
    private readonly List<Consulta> _consultasArchivadas = [];

    public ConsultasViewModel()
    {
    }

    public ConsultasViewModel(
        ConsultaService consultaService,
        MascotaService mascotaService,
        ProfesionalService profesionalService)
    {
        _consultaService = consultaService;
        _mascotaService = mascotaService;
        _profesionalService = profesionalService;
    }

    public ObservableCollection<Consulta> ConsultasItems { get; } = [];
    public ObservableCollection<Mascota> MascotasItems { get; } = [];
    public ObservableCollection<Profesional> ProfesionalesItems { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PuedeEditar))]
    [NotifyPropertyChangedFor(nameof(PuedeArchivar))]
    [NotifyPropertyChangedFor(nameof(PuedeRestaurar))]
    private Consulta? selectedConsulta;

    [ObservableProperty]
    private Mascota? selectedMascota;

    [ObservableProperty]
    private Profesional? selectedProfesional;

    [ObservableProperty]
    private string busquedaConsultas = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PuedeEditar))]
    [NotifyPropertyChangedFor(nameof(PuedeArchivar))]
    [NotifyPropertyChangedFor(nameof(PuedeRestaurar))]
    private bool mostrandoArchivadas;

    [ObservableProperty]
    private string motivo = string.Empty;

    [ObservableProperty]
    private decimal? peso;

    [ObservableProperty]
    private string observacion = string.Empty;

    [ObservableProperty]
    private string diagnostico = string.Empty;

    [ObservableProperty]
    private string tratamiento = string.Empty;

    [ObservableProperty]
    private string mensajeOperacion = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public bool PuedeEditar => SelectedConsulta is not null && !MostrandoArchivadas;
    public bool PuedeArchivar => SelectedConsulta is not null && !MostrandoArchivadas;
    public bool PuedeRestaurar => SelectedConsulta is not null && MostrandoArchivadas;

    [RelayCommand]
    public Task CargarAsync() => EjecutarSeguroAsync(() => RecargarAsync());

    partial void OnSelectedConsultaChanged(Consulta? value)
    {
        if (value is null)
            return;

        Motivo = value.Motivo;
        Peso = value.Peso;
        Observacion = value.Observacion;
        Diagnostico = value.Diagnostico;
        Tratamiento = value.Tratamiento;
        SelectedMascota = MascotasItems.FirstOrDefault(m => m.Id == value.IdMascota);
        SelectedProfesional = ProfesionalesItems.FirstOrDefault(p => p.Id == value.IdProfesional);
    }

    partial void OnBusquedaConsultasChanged(string value)
    {
        FiltrarConsultas();
    }

    [RelayCommand]
    private async Task AgregarConsultaAsync()
    {
        if (_consultaService is null)
            return;

        if (SelectedMascota is null || SelectedProfesional is null)
        {
            MensajeOperacion = "Seleccioná una mascota y un profesional.";
            return;
        }

        await EjecutarSeguroAsync(async () =>
        {
            var resultado = await _consultaService.AgregarAsync(
                Motivo,
                Peso,
                Observacion,
                Diagnostico,
                Tratamiento,
                SelectedMascota.Id,
                SelectedProfesional.Id);

            MensajeOperacion = resultado.Mensaje;

            if (!resultado.Correcta || resultado.Valor is null)
                return;

            LimpiarFormulario();
            await RecargarAsync(resultado.Valor.Id);
        });
    }

    [RelayCommand]
    private async Task ActualizarConsultaAsync()
    {
        if (_consultaService is null || SelectedConsulta is null)
            return;

        if (SelectedProfesional is null)
        {
            MensajeOperacion = "Seleccioná un profesional.";
            return;
        }

        var consultaId = SelectedConsulta.Id;

        await EjecutarSeguroAsync(async () =>
        {
            var resultado = await _consultaService.ActualizarAsync(
                consultaId,
                Motivo,
                Observacion,
                Diagnostico,
                Tratamiento,
                Peso,
                SelectedProfesional.Id);

            MensajeOperacion = resultado.Mensaje;

            if (resultado.Correcta)
                await RecargarAsync(consultaId);
        });
    }

    [RelayCommand]
    private async Task ArchivarConsultaAsync()
    {
        if (_consultaService is null || SelectedConsulta is null)
            return;

        var consultaId = SelectedConsulta.Id;

        await EjecutarSeguroAsync(async () =>
        {
            var resultado = await _consultaService.ArchivarAsync(consultaId);
            MensajeOperacion = resultado.Mensaje;

            if (resultado.Correcta)
            {
                LimpiarFormulario();
                await RecargarAsync();
            }
        });
    }

    [RelayCommand]
    private async Task RestaurarConsultaAsync()
    {
        if (_consultaService is null || SelectedConsulta is null)
            return;

        var consultaId = SelectedConsulta.Id;

        await EjecutarSeguroAsync(async () =>
        {
            var resultado = await _consultaService.RestaurarAsync(consultaId);
            MensajeOperacion = resultado.Mensaje;

            if (resultado.Correcta)
            {
                LimpiarFormulario();
                await RecargarAsync();
            }
        });
    }

    [RelayCommand]
    private void VerConsultasActivas()
    {
        MostrandoArchivadas = false;
        SelectedConsulta = null;
        FiltrarConsultas();
    }

    [RelayCommand]
    private void VerConsultasArchivadas()
    {
        MostrandoArchivadas = true;
        SelectedConsulta = null;
        FiltrarConsultas();
    }

    [RelayCommand]
    private void NuevaConsulta()
    {
        SelectedConsulta = null;
        LimpiarFormulario();
        MensajeOperacion = string.Empty;
    }

    private async Task RecargarAsync(int? consultaIdSeleccionada = null)
    {
        if (_consultaService is null ||
            _mascotaService is null ||
            _profesionalService is null)
        {
            return;
        }

        var mascotas = await _mascotaService.ObtenerTodosAsync();
        var profesionales = await _profesionalService.ObtenerTodosAsync();
        var consultasActivas = await _consultaService.ObtenerTodosAsync();
        var consultasArchivadas = await _consultaService.ObtenerArchivadasAsync();

        ReemplazarItems(MascotasItems, mascotas.Where(m => m.IdEstado == 1));
        ReemplazarItems(ProfesionalesItems, profesionales);

        _consultasActivas.Clear();
        _consultasActivas.AddRange(consultasActivas);
        _consultasArchivadas.Clear();
        _consultasArchivadas.AddRange(consultasArchivadas);

        FiltrarConsultas(consultaIdSeleccionada);
    }

    private void FiltrarConsultas(int? consultaIdSeleccionada = null)
    {
        var idSeleccionado = consultaIdSeleccionada ?? SelectedConsulta?.Id;
        var termino = BusquedaConsultas.Trim();
        var origen = MostrandoArchivadas ? _consultasArchivadas : _consultasActivas;

        var consultasFiltradas = string.IsNullOrEmpty(termino)
            ? origen
            : origen.Where(c =>
                    c.Motivo.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                    c.Diagnostico.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                    c.Mascota.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                    c.Profesional.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase))
                .ToList();

        ReemplazarItems(ConsultasItems, consultasFiltradas);
        SelectedConsulta = ConsultasItems.FirstOrDefault(c => c.Id == idSeleccionado);
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
        SelectedConsulta = null;
        SelectedMascota = null;
        SelectedProfesional = null;
        Motivo = string.Empty;
        Peso = null;
        Observacion = string.Empty;
        Diagnostico = string.Empty;
        Tratamiento = string.Empty;
    }

    private static void ReemplazarItems<T>(ObservableCollection<T> destino, IEnumerable<T> origen)
    {
        destino.Clear();

        foreach (var item in origen)
            destino.Add(item);
    }
}
