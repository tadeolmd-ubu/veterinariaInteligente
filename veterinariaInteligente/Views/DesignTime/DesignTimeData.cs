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
    public static readonly CitasDesignTime Citas = new();
    public static readonly PendientesDesignTime Pendientes = new();
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

public class CitasDesignTime : CitasViewModel
{
    private static readonly RelayCommand NoOp = new(() => { });

    public CitasDesignTime()
    {
        var profesional1 = new Profesional { Id = 1, Nombre = "Dra. Ana Ramírez", Cedula = "VET-10482", Activo = 1 };
        var profesional2 = new Profesional { Id = 2, Nombre = "Dr. Luis Mendoza", Cedula = "VET-09176", Activo = 1 };

        var mascota1 = new Mascota { Id = 1, Nombre = "Rocky", Raza = "Labrador", Sexo = "Macho" };
        var mascota2 = new Mascota { Id = 2, Nombre = "Luna", Raza = "Bulldog Francés", Sexo = "Hembra" };
        var mascota3 = new Mascota { Id = 3, Nombre = "Dante", Raza = "Beagle", Sexo = "Macho" };

        MascotasItems = new ObservableCollection<Mascota> { mascota1, mascota2, mascota3 };
        ProfesionalesItems = new ObservableCollection<Profesional> { profesional1, profesional2 };

        CitasItems = new ObservableCollection<Cita>
        {
            new()
            {
                Id = 1,
                FechaPrevista = new DateTime(2026, 10, 4, 10, 30, 0),
                Motivo = "Vacuna anual",
                Notas = "Traer cartilla.",
                Estado = EstadoCita.Confirmada,
                IdMascota = mascota1.Id,
                Mascota = mascota1,
                IdProfesional = profesional1.Id,
                Profesional = profesional1
            },
            new()
            {
                Id = 2,
                FechaPrevista = new DateTime(2026, 10, 4, 11, 0, 0),
                Motivo = "Control dermatológico",
                Notas = null,
                Estado = EstadoCita.Pendiente,
                IdMascota = mascota3.Id,
                Mascota = mascota3,
                IdProfesional = profesional1.Id,
                Profesional = profesional1
            },
            new()
            {
                Id = 3,
                FechaPrevista = new DateTime(2026, 10, 6, 12, 15, 0),
                Motivo = "Consulta por estornudos",
                Notas = "Posible alergia alimentaria.",
                Estado = EstadoCita.NoAsistio,
                IdMascota = mascota2.Id,
                Mascota = mascota2,
                IdProfesional = profesional2.Id,
                Profesional = profesional2
            },
            new()
            {
                Id = 4,
                FechaPrevista = new DateTime(2026, 9, 28, 9, 0, 0),
                Motivo = "Castración",
                Notas = "Cancelada por el dueño.",
                Estado = EstadoCita.Cancelada,
                IdMascota = mascota1.Id,
                Mascota = mascota1,
                IdProfesional = profesional2.Id,
                Profesional = profesional2
            }
        };

        SelectedCita = CitasItems[1];
        SelectedEstado = EstadoCita.Pendiente;
        Motivo = "Control dermatológico";
        Notas = "Revisar zonas afectadas.";
        MostrandoCanceladas = false;
        MostrandoAgenda = false;
        BusquedaCitas = "";

        AgendaDesde = DateTimeOffset.Now.Date;
        AgendaHasta = DateTimeOffset.Now.Date.AddDays(7);
    }

    public new ObservableCollection<Cita> CitasItems { get; }
    public new ObservableCollection<Mascota> MascotasItems { get; }
    public new ObservableCollection<Profesional> ProfesionalesItems { get; }

    public new ICommand AgregarCitaCommand => NoOp;
    public new ICommand ActualizarCitaCommand => NoOp;
    public new ICommand CambiarEstadoCitaCommand => NoOp;
    public new ICommand CancelarCitaCommand => NoOp;
    public new ICommand NuevaCitaCommand => NoOp;
    public new ICommand VerCitasActivasCommand => NoOp;
    public new ICommand VerCitasCanceladasCommand => NoOp;
    public new ICommand CargarAgendaCommand => NoOp;
}

public class PendientesDesignTime : PendientesViewModel
{
    private static readonly RelayCommand NoOp = new(() => { });

    public PendientesDesignTime()
    {
        var mascota1 = new Mascota { Id = 1, Nombre = "Rocky", Raza = "Labrador", Sexo = "Macho" };
        var mascota2 = new Mascota { Id = 2, Nombre = "Luna", Raza = "Bulldog Francés", Sexo = "Hembra" };
        var mascota3 = new Mascota { Id = 3, Nombre = "Dante", Raza = "Beagle", Sexo = "Macho" };

        MascotasItems = new ObservableCollection<Mascota> { mascota1, mascota2, mascota3 };
        SelectedMascota = mascota1;

        var consulta1 = new Consulta
        {
            Id = 1,
            Motivo = "Control y seguimiento",
            Estado = EstadoConsulta.EnCurso,
            IdMascota = mascota1.Id,
            Mascota = mascota1,
            Versiones = new List<ConsultaVersion>()
        };
        var consulta2 = new Consulta
        {
            Id = 2,
            Motivo = "Vacuna antirrábica",
            Estado = EstadoConsulta.Finalizada,
            IdMascota = mascota2.Id,
            Mascota = mascota2,
            Versiones = new List<ConsultaVersion>()
        };
        ConsultasItems = new ObservableCollection<Consulta> { consulta1, consulta2 };

        var vacuna1 = new Vacuna { Id = 1, Nombre = "Antirrábica", Activo = 1 };
        var vacuna2 = new Vacuna { Id = 2, Nombre = "Múltiple (triple felina)", Activo = 1 };
        var aplicacion1 = new AplicacionVacuna
        {
            Id = 1,
            FechaAplicacion = new DateOnly(2026, 9, 15),
            IdVacuna = vacuna1.Id,
            Vacuna = vacuna1,
            IdMascota = mascota1.Id,
            Mascota = mascota1
        };
        var aplicacion2 = new AplicacionVacuna
        {
            Id = 2,
            FechaAplicacion = new DateOnly(2026, 9, 20),
            IdVacuna = vacuna2.Id,
            Vacuna = vacuna2,
            IdMascota = mascota2.Id,
            Mascota = mascota2
        };
        AplicacionesVacunaItems = new ObservableCollection<AplicacionVacuna> { aplicacion1, aplicacion2 };

        var hoy = DateTime.Today;

        SeguimientosItems = new ObservableCollection<Seguimiento>
        {
            new()
            {
                Id = 1,
                Motivo = "Control dermatológico",
                FechaPrevista = new DateOnly(hoy.AddDays(2).Year, hoy.AddDays(2).Month, hoy.AddDays(2).Day),
                Notas = "Revisar zonas afectadas.",
                Estado = EstadoSeguimiento.Pendiente,
                IdMascota = mascota1.Id,
                Mascota = mascota1,
                IdConsultaOrigen = consulta1.Id,
                ConsultaOrigen = consulta1
            },
            new()
            {
                Id = 2,
                Motivo = "Refuerzo antirrábica",
                FechaPrevista = DateOnly.FromDateTime(hoy),
                Notas = "Coordinar con dueño.",
                Estado = EstadoSeguimiento.Pendiente,
                IdMascota = mascota2.Id,
                Mascota = mascota2,
                IdAplicacionVacunaOrigen = aplicacion2.Id,
                AplicacionVacunaOrigen = aplicacion2
            },
            new()
            {
                Id = 3,
                Motivo = "Control de peso",
                FechaPrevista = DateOnly.FromDateTime(hoy.AddDays(-3)),
                Notas = null,
                Estado = EstadoSeguimiento.Pendiente,
                IdMascota = mascota3.Id,
                Mascota = mascota3
            },
            new()
            {
                Id = 4,
                Motivo = "Vacuna anual",
                FechaPrevista = DateOnly.FromDateTime(hoy.AddDays(-10)),
                FechaRealizada = DateOnly.FromDateTime(hoy.AddDays(-8)),
                Notas = "Sin reacciones.",
                Estado = EstadoSeguimiento.Realizado,
                IdMascota = mascota1.Id,
                Mascota = mascota1
            },
            new()
            {
                Id = 5,
                Motivo = "Desparasitación",
                FechaPrevista = DateOnly.FromDateTime(hoy.AddDays(-1)),
                Notas = "Cancelada por indecisión del dueño.",
                Estado = EstadoSeguimiento.Cancelado,
                IdMascota = mascota2.Id,
                Mascota = mascota2
            }
        };

        SelectedSeguimiento = SeguimientosItems[0];
        Motivo = "Control dermatológico";
        Notas = "Revisar zonas afectadas.";
        FechaPrevista = DateTimeOffset.Now.Date.AddDays(2);
        FechaRealizada = DateTimeOffset.Now.Date;
        SelectedConsultaOrigen = consulta1;
        SelectedAplicacionVacunaOrigen = aplicacion1;
        BusquedaSeguimientos = "";
        SoloPendientes = true;
        SoloVencidos = false;
    }

    public new ObservableCollection<Seguimiento> SeguimientosItems { get; }
    public new ObservableCollection<Mascota> MascotasItems { get; }
    public new ObservableCollection<Consulta> ConsultasItems { get; }
    public new ObservableCollection<AplicacionVacuna> AplicacionesVacunaItems { get; }

    public new Seguimiento? SelectedSeguimiento { get; set; }
    public new Mascota? SelectedMascota { get; set; }
    public new string Motivo { get; set; }
    public new string? Notas { get; set; }
    public new DateTimeOffset? FechaPrevista { get; set; }
    public new DateTimeOffset? FechaRealizada { get; set; }
    public new string BusquedaSeguimientos { get; set; }
    public new bool SoloPendientes { get; set; }
    public new bool SoloVencidos { get; set; }
    public new Consulta? SelectedConsultaOrigen { get; set; }
    public new bool PuedeModificar => SelectedSeguimiento?.Estado == EstadoSeguimiento.Pendiente;

    public new ICommand CargarCommand => NoOp;
    public new ICommand AgregarSeguimientoCommand => NoOp;
    public new ICommand ActualizarSeguimientoCommand => NoOp;
    public new ICommand MarcarRealizadoCommand => NoOp;
    public new ICommand CancelarSeguimientoCommand => NoOp;
    public new ICommand NuevoSeguimientoCommand => NoOp;
}