using Microsoft.EntityFrameworkCore;
using veterinariaInteligente.Data;
using veterinariaInteligente.Models;
using veterinariaInteligente.Models.Enums;

namespace veterinariaInteligente.Service;

public class SeguimientoService
{
    private const int MaxMotivo = 100;
    private const int MaxNotas = 255;
    private const int EstadoMascotaActivaId = 1;

    private readonly AppDbContext _db;

    public SeguimientoService(AppDbContext db) => _db = db;

    public async Task<List<Seguimiento>> ObtenerTodosAsync()
    {
        return await _db.Seguimientos
            .AsNoTracking()
            .Include(s => s.Mascota)
            .Include(s => s.ConsultaOrigen)
            .Include(s => s.AplicacionVacunaOrigen)
            .Include(s => s.ConsultaRealizacion)
            .OrderBy(s => s.FechaPrevista)
            .ToListAsync();
    }

    public async Task<List<Seguimiento>> ObtenerPendientesAsync()
    {
        return await _db.Seguimientos
            .AsNoTracking()
            .Include(s => s.Mascota)
            .Include(s => s.ConsultaOrigen)
            .Include(s => s.AplicacionVacunaOrigen)
            .Where(s => s.Estado == EstadoSeguimiento.Pendiente)
            .OrderBy(s => s.FechaPrevista)
            .ToListAsync();
    }

    public async Task<List<Seguimiento>> ObtenerVencidosAsync(DateOnly fecha)
    {
        return await _db.Seguimientos
            .AsNoTracking()
            .Include(s => s.Mascota)
            .Include(s => s.ConsultaOrigen)
            .Include(s => s.AplicacionVacunaOrigen)
            .Include(s => s.ConsultaRealizacion)
            .Where(s =>
                s.Estado == EstadoSeguimiento.Pendiente &&
                s.FechaPrevista < fecha)
            .OrderBy(s => s.FechaPrevista)
            .ToListAsync();
    }

    public async Task<List<Seguimiento>> ObtenerPorMascotaAsync(int idMascota)
    {
        return await _db.Seguimientos
            .AsNoTracking()
            .Include(s => s.ConsultaOrigen)
            .Include(s => s.AplicacionVacunaOrigen)
            .Include(s => s.ConsultaRealizacion)
            .Where(s => s.IdMascota == idMascota)
            .OrderByDescending(s => s.FechaPrevista)
            .ToListAsync();
    }

    public async Task<Seguimiento?> ObtenerPorIdAsync(int id)
    {
        return await _db.Seguimientos
            .AsNoTracking()
            .Include(s => s.Mascota)
            .Include(s => s.ConsultaOrigen)
            .Include(s => s.AplicacionVacunaOrigen)
            .Include(s => s.ConsultaRealizacion)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Resultado<Seguimiento>> AgregarAsync(
        string motivo,
        DateOnly fechaPrevista,
        string? notas,
        int idMascota,
        int? idConsultaOrigen,
        int? idAplicacionVacunaOrigen)
    {
        var error = ValidarCampos(motivo, fechaPrevista, notas);

        if (error is not null)
            return new Resultado<Seguimiento>(false, error, null);

        var relaciones = await ValidarRelacionesAsync(
            idMascota,
            idConsultaOrigen,
            idAplicacionVacunaOrigen);

        if (!relaciones.Correcta)
        {
            return new Resultado<Seguimiento>(
                false,
                relaciones.Mensaje,
                null);
        }

        var seguimiento = new Seguimiento
        {
            Motivo = NormalizarTexto(motivo),
            FechaPrevista = fechaPrevista,
            Notas = NormalizarOpcional(notas),
            Estado = EstadoSeguimiento.Pendiente,
            IdMascota = idMascota,
            IdConsultaOrigen = idConsultaOrigen,
            IdAplicacionVacunaOrigen = idAplicacionVacunaOrigen
        };

        _db.Seguimientos.Add(seguimiento);
        await _db.SaveChangesAsync();

        return new Resultado<Seguimiento>(
            true,
            "Seguimiento creado.",
            seguimiento);
    }

    public async Task<Resultado> ActualizarAsync(
        int id,
        string motivo,
        DateOnly fechaPrevista,
        string? notas)
    {
        var seguimiento = await BuscarPendienteAsync(id);

        if (seguimiento is null)
        {
            return new Resultado(
                false,
                "El seguimiento no existe o no está pendiente.");
        }

        var error = ValidarCampos(motivo, fechaPrevista, notas);

        if (error is not null)
            return new Resultado(false, error);

        seguimiento.Motivo = NormalizarTexto(motivo);
        seguimiento.FechaPrevista = fechaPrevista;
        seguimiento.Notas = NormalizarOpcional(notas);

        await _db.SaveChangesAsync();

        return new Resultado(true, "Seguimiento actualizado.");
    }

    public async Task<Resultado> MarcarRealizadoAsync(
        int id,
        DateOnly fechaRealizada,
        int? idConsultaRealizacion)
    {
        var seguimiento = await BuscarPendienteAsync(id);

        if (seguimiento is null)
        {
            return new Resultado(
                false,
                "El seguimiento no existe o no está pendiente.");
        }

        if (fechaRealizada == DateOnly.MinValue)
        {
            return new Resultado(
                false,
                "La fecha de realización es obligatoria.");
        }

        if (idConsultaRealizacion is not null)
        {
            var consulta = await _db.Consultas
                .FirstOrDefaultAsync(c =>
                    c.Id == idConsultaRealizacion &&
                    c.IdMascota == seguimiento.IdMascota &&
                    c.Estado != EstadoConsulta.Archivada);

            if (consulta is null)
            {
                return new Resultado(
                    false,
                    "La consulta de realización no existe o no corresponde a la mascota.");
            }
        }

        seguimiento.FechaRealizada = fechaRealizada;
        seguimiento.IdConsultaRealizacion = idConsultaRealizacion;
        seguimiento.Estado = EstadoSeguimiento.Realizado;

        await _db.SaveChangesAsync();

        return new Resultado(true, "Seguimiento marcado como realizado.");
    }

    public async Task<Resultado> CancelarAsync(int id)
    {
        var seguimiento = await BuscarPendienteAsync(id);

        if (seguimiento is null)
        {
            return new Resultado(
                false,
                "El seguimiento no existe o no está pendiente.");
        }

        seguimiento.Estado = EstadoSeguimiento.Cancelado;

        await _db.SaveChangesAsync();

        return new Resultado(true, "Seguimiento cancelado.");
    }

    private async Task<Seguimiento?> BuscarPendienteAsync(int id)
    {
        return await _db.Seguimientos
            .FirstOrDefaultAsync(s =>
                s.Id == id &&
                s.Estado == EstadoSeguimiento.Pendiente);
    }

    private async Task<Resultado> ValidarRelacionesAsync(
        int idMascota,
        int? idConsultaOrigen,
        int? idAplicacionVacunaOrigen)
    {
        var mascota = await _db.Mascotas
            .FirstOrDefaultAsync(m =>
                m.Id == idMascota &&
                m.FechaArchivado == null &&
                m.IdEstado == EstadoMascotaActivaId);

        if (mascota is null)
        {
            return new Resultado(
                false,
                "La mascota no existe, está archivada o no está activa.");
        }

        if (idConsultaOrigen is not null)
        {
            var consulta = await _db.Consultas
                .FirstOrDefaultAsync(c =>
                    c.Id == idConsultaOrigen &&
                    c.IdMascota == idMascota &&
                    c.Estado != EstadoConsulta.Archivada);

            if (consulta is null)
            {
                return new Resultado(
                    false,
                    "La consulta de origen no existe o no corresponde a la mascota.");
            }
        }

        if (idAplicacionVacunaOrigen is not null)
        {
            var aplicacion = await _db.AplicacionVacunas
                .FirstOrDefaultAsync(a =>
                    a.Id == idAplicacionVacunaOrigen &&
                    a.IdMascota == idMascota);

            if (aplicacion is null)
            {
                return new Resultado(
                    false,
                    "La aplicación de vacuna no existe o no corresponde a la mascota.");
            }
        }

        return new Resultado(true, string.Empty);
    }

    private static string? ValidarCampos(
        string motivo,
        DateOnly fechaPrevista,
        string? notas)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            return "El motivo es obligatorio.";

        if (motivo.Trim().Length > MaxMotivo)
            return $"El motivo no puede superar los {MaxMotivo} caracteres.";

        if (fechaPrevista == DateOnly.MinValue)
            return "La fecha prevista es obligatoria.";

        if (!string.IsNullOrWhiteSpace(notas) &&
            notas.Trim().Length > MaxNotas)
        {
            return $"Las notas no pueden superar los {MaxNotas} caracteres.";
        }

        return null;
    }

    private static string NormalizarTexto(string valor)
        => valor.Trim();

    private static string? NormalizarOpcional(string? valor)
        => string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();
}