using Microsoft.EntityFrameworkCore;
using veterinariaInteligente.Data;
using veterinariaInteligente.Models;
using veterinariaInteligente.Models.Enums;

namespace veterinariaInteligente.Service;

public class CitaService
{
    private const int MaxMotivo = 100;
    private const int MaxNotas = 255;
    private const int EstadoMascotaActivaId = 1;

    private readonly AppDbContext _db;

    public CitaService(AppDbContext db) => _db = db;

    public async Task<List<Cita>> ObtenerTodosAsync()
    {
        return await _db.Citas
            .AsNoTracking()
            .Include(c => c.Mascota)
            .Include(c => c.Duenio)
            .Include(c => c.Profesional)
            .Where(c => c.Estado != EstadoCita.Cancelada)
            .OrderBy(c => c.FechaPrevista)
            .ToListAsync();
    }

    public async Task<List<Cita>> ObtenerCanceladasAsync()
    {
        return await _db.Citas
            .AsNoTracking()
            .Include(c => c.Mascota)
            .Include(c => c.Duenio)
            .Include(c => c.Profesional)
            .Where(c => c.Estado == EstadoCita.Cancelada)
            .OrderBy(c => c.FechaPrevista)
            .ToListAsync();
    }

    public async Task<Cita?> ObtenerPorIdAsync(int id)
    {
        return await _db.Citas
            .AsNoTracking()
            .Include(c => c.Mascota)
            .Include(c => c.Duenio)
            .Include(c => c.Profesional)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Cita>> ObtenerAgendaAsync(
        DateTime desde,
        DateTime hasta)
    {
        return await _db.Citas
            .AsNoTracking()
            .Include(c => c.Mascota)
            .Include(c => c.Duenio)
            .Include(c => c.Profesional)
            .Where(c =>
                c.FechaPrevista >= desde &&
                c.FechaPrevista <= hasta &&
                c.Estado != EstadoCita.Cancelada)
            .OrderBy(c => c.FechaPrevista)
            .ToListAsync();
    }

    public async Task<Resultado<Cita>> AgregarAsync(
        DateTime fechaPrevista,
        string motivo,
        string? notas,
        int idMascota,
        int? idProfesional)
    {
        var error = ValidarCampos(fechaPrevista, motivo, notas);

        if (error is not null)
            return new Resultado<Cita>(false, error, null);

        var relaciones = await ValidarRelacionesAsync(
            idMascota,
            idProfesional);

        if (!relaciones.Correcta)
            return new Resultado<Cita>(
                false,
                relaciones.Mensaje,
                null);

        var cita = new Cita
        {
            FechaPrevista = fechaPrevista,
            Motivo = NormalizarTexto(motivo),
            Notas = NormalizarOpcional(notas),
            IdMascota = idMascota,
            IdDuenio = relaciones.Valor,
            IdProfesional = idProfesional,
            Estado = EstadoCita.Pendiente
        };

        _db.Citas.Add(cita);
        await _db.SaveChangesAsync();

        return new Resultado<Cita>(
            true,
            "Cita creada.",
            cita);
    }

    public async Task<Resultado> ActualizarAsync(
        int id,
        DateTime fechaPrevista,
        string motivo,
        string? notas,
        int idMascota,
        int? idProfesional)
    {
        var cita = await _db.Citas
            .FirstOrDefaultAsync(c =>
                c.Id == id &&
                c.Estado != EstadoCita.Cancelada);

        if (cita is null)
            return new Resultado(
                false,
                "La cita no existe o está cancelada.");

        var error = ValidarCampos(fechaPrevista, motivo, notas);

        if (error is not null)
            return new Resultado(false, error);

        var relaciones = await ValidarRelacionesAsync(
            idMascota,
            idProfesional);

        if (!relaciones.Correcta)
            return new Resultado(
                false,
                relaciones.Mensaje);

        cita.FechaPrevista = fechaPrevista;
        cita.Motivo = NormalizarTexto(motivo);
        cita.Notas = NormalizarOpcional(notas);
        cita.IdMascota = idMascota;
        cita.IdDuenio = relaciones.Valor;
        cita.IdProfesional = idProfesional;

        await _db.SaveChangesAsync();

        return new Resultado(true, "Cita actualizada.");
    }

    public async Task<Resultado> CambiarEstadoAsync(
        int id,
        EstadoCita estado)
    {
        var cita = await _db.Citas
            .FirstOrDefaultAsync(c =>
                c.Id == id &&
                c.Estado != EstadoCita.Cancelada);

        if (cita is null)
            return new Resultado(
                false,
                "La cita no existe o está cancelada.");

        cita.Estado = estado;

        await _db.SaveChangesAsync();

        return new Resultado(
            true,
            $"Estado cambiado a {estado}.");
    }

    public async Task<Resultado> CancelarAsync(int id)
    {
        var cita = await _db.Citas
            .FirstOrDefaultAsync(c =>
                c.Id == id &&
                c.Estado != EstadoCita.Cancelada);

        if (cita is null)
            return new Resultado(
                false,
                "La cita no existe o ya está cancelada.");

        cita.Estado = EstadoCita.Cancelada;

        await _db.SaveChangesAsync();

        return new Resultado(true, "Cita cancelada.");
    }

    private async Task<Resultado<int?>> ValidarRelacionesAsync(
        int idMascota,
        int? idProfesional)
    {
        var mascota = await _db.Mascotas
            .FirstOrDefaultAsync(m =>
                m.Id == idMascota &&
                m.FechaArchivado == null &&
                m.IdEstado == EstadoMascotaActivaId);

        if (mascota is null)
        {
            return new Resultado<int?>(
                false,
                "La mascota no existe, está archivada o no está activa.",
                null);
        }

        var idDuenio = mascota.IdDuenio;

        var duenio = await _db.Duenios
            .FirstOrDefaultAsync(d =>
                d.Id == idDuenio &&
                d.FechaArchivado == null);

        if (duenio is null)
        {
            return new Resultado<int?>(
                false,
                "El dueño no existe o está archivado.",
                null);
        }

        if (idProfesional is not null)
        {
            var profesional = await _db.Profesionales
                .FirstOrDefaultAsync(p =>
                    p.Id == idProfesional &&
                    p.Activo == 1);

            if (profesional is null)
            {
                return new Resultado<int?>(
                    false,
                    "El profesional no existe o está inactivo.",
                    null);
            }
        }

        return new Resultado<int?>(
            true,
            string.Empty,
            idDuenio);
    }

    private static string? ValidarCampos(
        DateTime fechaPrevista,
        string motivo,
        string? notas)
    {
        if (fechaPrevista <= DateTime.Now)
            return "La fecha de la cita debe ser futura.";

        if (string.IsNullOrWhiteSpace(motivo))
            return "El motivo es obligatorio.";

        if (motivo.Trim().Length > MaxMotivo)
        {
            return $"El motivo no puede superar los {MaxMotivo} caracteres.";
        }

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