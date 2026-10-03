using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using veterinariaInteligente.Models;
using veterinariaInteligente.Models.Enums;
using veterinariaInteligente.ViewModels;

namespace veterinariaInteligente.Views.DesignTime;

public sealed class DesignTimeData
{
    public static readonly PropietariosDesignTime Propietarios = new();
    public static readonly ExpedienteDesignTime Expediente = new();
    public static readonly ConsultasDesignTime Consultas = new();
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

    public new ObservableCollection<Duenio> PropietariosItems { get; }
    public new Duenio? SelectedPropietario { get; set; }

    public new ObservableCollection<Mascota> MascotasItems { get; }
    public new Mascota? SelectedMascota { get; set; }

    public IReadOnlyList<MascotaEstado> MascotaEstadosItems { get; }
    public MascotaEstado? SelectedEstadoNuevaMascota { get; set; }

    public new string BusquedaPropietarios { get; set; }

    public new string NuevoPropietarioNombre { get; }
    public new string? NuevoPropietarioTelefono { get; }
    public new string? NuevoPropietarioCorreo { get; }
    public new string? NuevoPropietarioDireccion { get; }

    public new string NuevaMascotaNombre { get; set; }
    public new string NuevaMascotaRaza { get; set; }
    public new string NuevaMascotaSexo { get => _nuevaMascotaSexo; set => _nuevaMascotaSexo = value; }
    public new string NuevaMascotaColor { get; set; }
    public new string? NuevaMascotaAlergia { get; set; }
    public new DateTimeOffset? NuevaMascotaFechaNacimiento { get => _nuevaMascotaFechaNacimiento; set => _nuevaMascotaFechaNacimiento = value; }
    public new IReadOnlyList<string> SexosMascota => new[] { "Macho", "Hembra" };

    public new ICommand AgregarPropietarioCommand => NoOp;
    public new ICommand ActualizarPropietarioCommand => NoOp;
    public new ICommand ArchivarPropietarioCommand => NoOp;
    public new ICommand RestaurarPropietarioCommand => NoOp;

    public new ICommand AgregarMascotaCommand => NoOp;
    public ICommand ActualizarMascotaCommand => NoOp;
    public new ICommand ArchivarMascotaCommand => NoOp;
    public new ICommand AbrirExpedienteCommand => NoOp;
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

    public new Mascota MascotaActual { get; }
    public new Duenio DuenioActual { get; }
}

public class ConsultasDesignTime : ConsultasViewModel
{
    private static readonly RelayCommand NoOp = new(() => { });

    public ConsultasDesignTime()
    {
        var profesional1 = new Profesional { Id = 1, Nombre = "Dra. Ana Ramírez", Cedula = "VET-10482", Activo = 1 };
        var profesional2 = new Profesional { Id = 2, Nombre = "Dr. Luis Mendoza", Cedula = "VET-09176", Activo = 1 };

        var mascota1 = new Mascota { Id = 1, Nombre = "Rocky", Raza = "Labrador", Sexo = "Macho" };
        var mascota2 = new Mascota { Id = 2, Nombre = "Luna", Raza = "Bulldog Francés", Sexo = "Hembra" };
        var mascota3 = new Mascota { Id = 3, Nombre = "Dante", Raza = "Beagle", Sexo = "Macho" };

        MascotasItems = new ObservableCollection<Mascota> { mascota1, mascota2, mascota3 };
        ProfesionalesItems = new ObservableCollection<Profesional> { profesional1, profesional2 };

        var versiones1 = new List<ConsultaVersion>
        {
            new()
            {
                Id = 1,
                Fecha = new DateTime(2026, 9, 20, 11, 40, 0),
                Campo = "Diagnóstico",
                ValorAnterior = null,
                ValorNuevo = "Dermatitis alérgica",
                Profesional = profesional1
            },
            new()
            {
                Id = 2,
                Fecha = new DateTime(2026, 9, 27, 9, 15, 0),
                Campo = "Peso",
                ValorAnterior = "24,8",
                ValorNuevo = "24,2",
                Profesional = profesional1
            }
        };

        var versiones2 = new List<ConsultaVersion>
        {
            new()
            {
                Id = 3,
                Fecha = new DateTime(2026, 10, 1, 17, 5, 0),
                Campo = "Tratamiento",
                ValorAnterior = null,
                ValorNuevo = "Antibiótico + control en 7 días",
                Profesional = profesional2
            }
        };

        ConsultasItems = new ObservableCollection<Consulta>
        {
            new()
            {
                Id = 1,
                Motivo = "Control y seguimiento",
                Peso = 24.2m,
                Observacion = "Evidencia mejoría en zonas afectadas.",
                Diagnostico = "Dermatitis alérgica",
                Tratamiento = "Baños medicados y omega-3.",
                Estado = EstadoConsulta.EnCurso,
                FechaCreacion = new DateTime(2026, 9, 18, 9, 30, 0),
                FechaModificacion = new DateTime(2026, 9, 27, 9, 15, 0),
                IdMascota = mascota1.Id,
                Mascota = mascota1,
                IdProfesional = profesional1.Id,
                Profesional = profesional1,
                Versiones = versiones1
            },
            new()
            {
                Id = 2,
                Motivo = "Vacuna antirrábica",
                Peso = 9.8m,
                Observacion = "Sin reacciones adversas.",
                Diagnostico = "Sana",
                Tratamiento = "Refuerzo en 12 meses.",
                Estado = EstadoConsulta.Finalizada,
                FechaCreacion = new DateTime(2026, 10, 1, 17, 5, 0),
                FechaModificacion = new DateTime(2026, 10, 1, 17, 5, 0),
                IdMascota = mascota2.Id,
                Mascota = mascota2,
                IdProfesional = profesional2.Id,
                Profesional = profesional2,
                Versiones = versiones2
            },
            new()
            {
                Id = 3,
                Motivo = "Vómitos recurrentes",
                Peso = 11.3m,
                Observacion = "Se solicitaron estudios de laboratorio.",
                Diagnostico = "En observación",
                Tratamiento = "Ayuno 12 h y dieta blanda.",
                Estado = EstadoConsulta.Pendiente,
                FechaCreacion = new DateTime(2026, 10, 2, 9, 0, 0),
                FechaModificacion = new DateTime(2026, 10, 2, 9, 0, 0),
                IdMascota = mascota3.Id,
                Mascota = mascota3,
                IdProfesional = profesional2.Id,
                Profesional = profesional2,
                Versiones = new List<ConsultaVersion>()
            },
            new()
            {
                Id = 4,
                Motivo = "Castración",
                Peso = 5.1m,
                Observacion = "Procedimiento completado sin complicaciones.",
                Diagnostico = "Postoperatorio",
                Tratamiento = "Analgésico 5 días.",
                Estado = EstadoConsulta.Archivada,
                FechaCreacion = new DateTime(2026, 8, 12, 10, 20, 0),
                FechaModificacion = new DateTime(2026, 8, 20, 12, 0, 0),
                IdMascota = mascota1.Id,
                Mascota = mascota1,
                IdProfesional = profesional1.Id,
                Profesional = profesional1,
                Versiones = new List<ConsultaVersion>()
            }
        };

        SelectedConsulta = ConsultasItems[0];
        SelectedMascota = mascota1;
        SelectedProfesional = profesional1;
        MostrandoArchivadas = false;
        Peso = "24,2";

        BusquedaConsultas = "";
    }

    public new ObservableCollection<Consulta> ConsultasItems { get; }
    public new ObservableCollection<Mascota> MascotasItems { get; }
    public new ObservableCollection<Profesional> ProfesionalesItems { get; }
    public new string Peso { get; set; }

    public new ICommand AgregarConsultaCommand => NoOp;
    public new ICommand ActualizarConsultaCommand => NoOp;
    public new ICommand ArchivarConsultaCommand => NoOp;
    public new ICommand RestaurarConsultaCommand => NoOp;
    public new ICommand NuevaConsultaCommand => NoOp;
    public new ICommand VerConsultasActivasCommand => NoOp;
    public new ICommand VerConsultasArchivadasCommand => NoOp;
}