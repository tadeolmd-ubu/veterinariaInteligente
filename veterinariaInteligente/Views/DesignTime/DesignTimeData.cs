using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using veterinariaInteligente.Models;
using veterinariaInteligente.ViewModels;

namespace veterinariaInteligente.Views.DesignTime;

public sealed class DesignTimeData
{
    public static readonly PropietariosDesignTime Propietarios = new();
    public static readonly ExpedienteDesignTime Expediente = new();
}

public class PropietariosDesignTime : PropietariosViewModel
{
    private static readonly RelayCommand NoOp = new(() => { });
    private string _nuevaMascotaSexo = "Macho";
    private DateTimeOffset? _nuevaMascotaFechaNacimiento = DateTimeOffset.Now;

    public PropietariosDesignTime()
    {
        var estados = new List<MascotaEstado>
        {
            new() { Id = 1, Nombre = "Activo" },
            new() { Id = 2, Nombre = "En tratamiento" },
            new() { Id = 3, Nombre = "Adoptado" },
            new() { Id = 4, Nombre = "Fallecido" }
        };
        MascotaEstadosItems = estados;

        var duenio1 = new Duenio
        {
            Id = 1,
            Nombre = "Ricardo Castro",
            Telefono = "+52 667 123 4567",
            Correo = "ricardo.castro@correo.com",
            Direccion = "Av. Universidad 800, Culiacán",
            Mascotas = new List<Mascota>
            {
                new() { Id = 1, Nombre = "Rocky", Raza = "Labrador", Sexo = "Macho", Color = "Dorado", Alergia = "Pollo", FechaNacimiento = new DateOnly(2021, 5, 12), Estado = estados[0] },
                new() { Id = 2, Nombre = "Luna", Raza = "Bulldog Francés", Sexo = "Hembra", Color = "Negro", Alergia = null, FechaNacimiento = new DateOnly(2023, 2, 1), Estado = estados[0] },
                new() { Id = 3, Nombre = "Dante", Raza = "Beagle", Sexo = "Macho", Color = "Tricolor", Alergia = null, FechaNacimiento = null, Estado = estados[1] }
            }
        };
        var duenio2 = new Duenio
        {
            Id = 2,
            Nombre = "María López",
            Telefono = "+52 667 987 6543",
            Correo = "maria.lopez@correo.com",
            Direccion = "Blvd. Los Ángeles 15, Mazatlán",
            Mascotas = new List<Mascota>()
        };
        var duenio3 = new Duenio
        {
            Id = 3,
            Nombre = "Javier Torres",
            Telefono = "+52 667 555 0102",
            Correo = null,
            Direccion = null,
            Mascotas = new List<Mascota>()
        };

        PropietariosItems = new ObservableCollection<Duenio> { duenio1, duenio2, duenio3 };
        SelectedPropietario = duenio1;

        MascotasItems = new ObservableCollection<Mascota>(duenio1.Mascotas);
        SelectedMascota = duenio1.Mascotas.First();
        SelectedEstadoNuevaMascota = estados[0];

        BusquedaPropietarios = "Ricardo";

        NuevoPropietarioNombre = "Nombre del propietario";
        NuevoPropietarioTelefono = "";
        NuevoPropietarioCorreo = null;
        NuevoPropietarioDireccion = null;

        NuevaMascotaNombre = "Nombre de la mascota";
        NuevaMascotaRaza = "";
        NuevaMascotaColor = "";
        NuevaMascotaAlergia = null;
    }

    public ObservableCollection<Duenio> PropietariosItems { get; }
    public Duenio? SelectedPropietario { get; set; }

    public ObservableCollection<Mascota> MascotasItems { get; }
    public Mascota? SelectedMascota { get; set; }

    public IReadOnlyList<MascotaEstado> MascotaEstadosItems { get; }
    public MascotaEstado? SelectedEstadoNuevaMascota { get; set; }

    public string BusquedaPropietarios { get; set; }

    public string NuevoPropietarioNombre { get; }
    public string? NuevoPropietarioTelefono { get; }
    public string? NuevoPropietarioCorreo { get; }
    public string? NuevoPropietarioDireccion { get; }

    public string NuevaMascotaNombre { get; set; }
    public string NuevaMascotaRaza { get; set; }
    public string NuevaMascotaSexo { get => _nuevaMascotaSexo; set => _nuevaMascotaSexo = value; }
    public string NuevaMascotaColor { get; set; }
    public string? NuevaMascotaAlergia { get; set; }
    public DateTimeOffset? NuevaMascotaFechaNacimiento { get => _nuevaMascotaFechaNacimiento; set => _nuevaMascotaFechaNacimiento = value; }
    public IReadOnlyList<string> SexosMascota => new[] { "Macho", "Hembra" };

    public ICommand AgregarPropietarioCommand => NoOp;
    public ICommand ActualizarPropietarioCommand => NoOp;
    public ICommand ArchivarPropietarioCommand => NoOp;
    public ICommand RestaurarPropietarioCommand => NoOp;

    public ICommand AgregarMascotaCommand => NoOp;
    public ICommand ActualizarMascotaCommand => NoOp;
    public ICommand ArchivarMascotaCommand => NoOp;
    public ICommand AbrirExpedienteCommand => NoOp;
}

public class ExpedienteDesignTime : ExpedienteViewModel
{
    public ExpedienteDesignTime()
    {
        var estado = new MascotaEstado { Id = 1, Nombre = "Activo" };
        MascotaActual = new Mascota
        {
            Id = 1,
            Nombre = "Rocky",
            Raza = "Labrador",
            Sexo = "Macho",
            Color = "Dorado",
            Alergia = "Pollo",
            FechaNacimiento = new DateOnly(2021, 5, 12),
            Estado = estado
        };
        DuenioActual = new Duenio
        {
            Id = 1,
            Nombre = "Ricardo Castro",
            Telefono = "+52 667 123 4567",
            Correo = "ricardo.castro@correo.com",
            Direccion = "Av. Universidad 800, Culiacán"
        };
    }

    public Mascota MascotaActual { get; }
    public Duenio DuenioActual { get; }
}