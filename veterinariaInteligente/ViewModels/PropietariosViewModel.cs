using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using veterinariaInteligente.Models;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.ViewModels;

public partial class PropietariosViewModel : ObservableObject
{
    private const int EstadoMascotaActivaId = 1;

    private readonly DuenioService? _duenioService;
    private readonly MascotaService? _mascotaService;
    private readonly List<Duenio> _propietariosCargados = [];
    private readonly List<Mascota> _mascotasCargadas = [];

    public PropietariosViewModel()
    {
    }

    public PropietariosViewModel(
        DuenioService duenioService,
        MascotaService mascotaService)
    {
        _duenioService = duenioService;
        _mascotaService = mascotaService;
    }

    public ObservableCollection<Duenio> PropietariosItems { get; } = [];
    public ObservableCollection<Mascota> MascotasItems { get; } = [];
    public IReadOnlyList<string> SexosMascota { get; } = ["Macho", "Hembra"];

    public event Action<Mascota, Duenio>? ExpedienteSolicitado;

    [ObservableProperty]
    private Duenio? selectedPropietario;

    [ObservableProperty]
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

    public Task CargarAsync() => RecargarAsync();

    partial void OnSelectedPropietarioChanged(Duenio? value)
    {
        MascotasItems.Clear();
        SelectedMascota = null;

        if (value is null)
            return;

        foreach (var mascota in _mascotasCargadas.Where(m => m.IdDuenio == value.Id))
        {
            MascotasItems.Add(mascota);
        }

        SelectedMascota = MascotasItems.FirstOrDefault();
    }

    partial void OnBusquedaPropietariosChanged(string value)
    {
        FiltrarPropietarios();
    }

    [RelayCommand]
    private async Task AgregarPropietarioAsync()
    {
        if (_duenioService is null)
            return;

        var resultado = await _duenioService.AgregarAsync(
            NuevoPropietarioNombre,
            NuevoPropietarioTelefono,
            NuevoPropietarioCorreo,
            NuevoPropietarioDireccion);

        MensajeOperacion = resultado.Mensaje;

        if (!resultado.Correcta || resultado.Valor is null)
            return;

        LimpiarFormularioPropietario();
        await RecargarAsync(resultado.Valor.Id);
    }

    [RelayCommand]
    private async Task ActualizarPropietarioAsync()
    {
        if (_duenioService is null || SelectedPropietario is null)
            return;

        var propietario = SelectedPropietario;
        var resultado = await _duenioService.ActualizarAsync(
            propietario.Id,
            propietario.Nombre,
            propietario.Telefono,
            propietario.Correo,
            propietario.Direccion);

        MensajeOperacion = resultado.Mensaje;

        if (resultado.Correcta)
            await RecargarAsync(propietario.Id);
    }

    [RelayCommand]
    private async Task ArchivarPropietarioAsync()
    {
        if (_duenioService is null || SelectedPropietario is null)
            return;

        var resultado = await _duenioService.ArchivarAsync(SelectedPropietario.Id);
        MensajeOperacion = resultado.Mensaje;

        if (resultado.Correcta)
            await RecargarAsync();
    }

    [RelayCommand]
    private async Task RestaurarPropietarioAsync()
    {
        if (_duenioService is null || SelectedPropietario is null)
            return;

        var resultado = await _duenioService.RestaurarAsync(SelectedPropietario.Id);
        MensajeOperacion = resultado.Mensaje;

        if (resultado.Correcta)
            await RecargarAsync(SelectedPropietario.Id);
    }

    [RelayCommand]
    private async Task AgregarMascotaAsync()
    {
        if (_mascotaService is null || SelectedPropietario is null)
            return;

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
            SelectedPropietario.Id);

        MensajeOperacion = resultado.Mensaje;

        if (!resultado.Correcta)
            return;

        var propietarioId = SelectedPropietario.Id;
        LimpiarFormularioMascota();
        await RecargarAsync(propietarioId);
    }

    [RelayCommand]
    private async Task ArchivarMascotaAsync()
    {
        if (_mascotaService is null || SelectedMascota is null)
            return;

        var propietarioId = SelectedPropietario?.Id;
        var resultado = await _mascotaService.ArchivarAsync(SelectedMascota.Id);
        MensajeOperacion = resultado.Mensaje;

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
        var termino = BusquedaPropietarios.Trim();

        var propietariosFiltrados = string.IsNullOrEmpty(termino)
            ? _propietariosCargados
            : _propietariosCargados
                .Where(p =>
                    p.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                    p.Telefono.Contains(termino, StringComparison.OrdinalIgnoreCase))
                .ToList();

        PropietariosItems.Clear();

        foreach (var propietario in propietariosFiltrados)
        {
            PropietariosItems.Add(propietario);
        }

        SelectedPropietario = PropietariosItems
            .FirstOrDefault(p => p.Id == idSeleccionado)
            ?? PropietariosItems.FirstOrDefault();
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
    }
}
