using Microsoft.EntityFrameworkCore;
using veterinariaInteligente.Data;
using veterinariaInteligente.Models;

namespace veterinariaInteligente.Service;

public class AplicacionVacunaService
{
    private const int MaxLote = 20;
    private const int MaxObservacion = 255;
    private const int EstadoMascotaActivaId = 1;

    private readonly AppDbContext _db;

    public AplicacionVacunaService(AppDbContext db) => _db = db;

    public async Task<List<AplicacionVacuna>> ObtenerTodosAsync()
    {
        return await _db.AplicacionVacunas
            .AsNoTracking()
            .Include(a => a.Vacuna)
            .Include(a => a.Mascota)
            .Include(a => a.Profesional)
            .OrderByDescending(a => a.FechaAplicacion)
            .ToListAsync();
    }

    public async Task<List<AplicacionVacuna>> ObtenerPorMascotaAsync(
        int idMascota)
    {
        return await _db.AplicacionVacunas
            .AsNoTracking()
            .Include(a => a.Vacuna)
            .Include(a => a.Profesional)
            .Where(a => a.IdMascota == idMascota)
            .OrderByDescending(a => a.FechaAplicacion)
            .ToListAsync();
    }

    public async Task<AplicacionVacuna?> ObtenerPorIdAsync(int id)
    {
        return await _db.AplicacionVacunas
            .AsNoTracking()
            .Include(a => a.Vacuna)
            .Include(a => a.Mascota)
            .Include(a => a.Profesional)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Resultado<AplicacionVacuna>> AgregarAsync(
        DateOnly fechaAplicacion,
        string? lote,
        string? observacion,
        DateOnly? fechaProximaSugerida,
        int idVacuna,
        int idMascota,
        int? idProfesional)
    {
        var error = ValidarCampos(
            fechaAplicacion,
            lote,
            observacion,
            fechaProximaSugerida);

        if (error is not null)
            return new Resultado<AplicacionVacuna>(false, error, null);

        var relaciones = await ValidarRelacionesAsync(
            idVacuna,
            idMascota,
            idProfesional);

        if (!relaciones.Correcta)
        {
            return new Resultado<AplicacionVacuna>(
                false,
                relaciones.Mensaje,
                null);
        }

        var aplicacion = new AplicacionVacuna
        {
            FechaAplicacion = fechaAplicacion,
            Lote = NormalizarOpcional(lote),
            Observacion = NormalizarOpcional(observacion),
            FechaProximaSugerida = fechaProximaSugerida,
            IdVacuna = idVacuna,
            IdMascota = idMascota,
            IdProfesional = idProfesional
        };

        _db.AplicacionVacunas.Add(aplicacion);
        await _db.SaveChangesAsync();

        return new Resultado<AplicacionVacuna>(
            true,
            "Aplicación de vacuna registrada.",
            aplicacion);
    }

    public async Task<Resultado> ActualizarAsync(
        int id,
        DateOnly fechaAplicacion,
        string? lote,
        string? observacion,
        DateOnly? fechaProximaSugerida,
        int idVacuna,
        int idProfesional)
    {
        var aplicacion = await _db.AplicacionVacunas
            .FirstOrDefaultAsync(a => a.Id == id);

        if (aplicacion is null)
        {
            return new Resultado(
                false,
                "La aplicación de vacuna no existe.");
        }

        var error = ValidarCampos(
            fechaAplicacion,
            lote,
            observacion,
            fechaProximaSugerida);

        if (error is not null)
            return new Resultado(false, error);

        var relaciones = await ValidarRelacionesAsync(
            idVacuna,
            aplicacion.IdMascota,
            idProfesional);

        if (!relaciones.Correcta)
            return new Resultado(false, relaciones.Mensaje);

        aplicacion.FechaAplicacion = fechaAplicacion;
        aplicacion.Lote = NormalizarOpcional(lote);
        aplicacion.Observacion = NormalizarOpcional(observacion);
        aplicacion.FechaProximaSugerida = fechaProximaSugerida;
        aplicacion.IdVacuna = idVacuna;
        aplicacion.IdProfesional = idProfesional;

        await _db.SaveChangesAsync();

        return new Resultado(
            true,
            "Aplicación de vacuna actualizada.");
    }

    private async Task<Resultado> ValidarRelacionesAsync(
        int idVacuna,
        int idMascota,
        int? idProfesional)
    {
        var vacuna = await _db.Vacunas
            .FirstOrDefaultAsync(v =>
                v.Id == idVacuna &&
                v.Activo == 1);

        if (vacuna is null)
        {
            return new Resultado(
                false,
                "La vacuna no existe o está inactiva.");
        }

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

        if (idProfesional is not null)
        {
            var profesional = await _db.Profesionales
                .FirstOrDefaultAsync(p =>
                    p.Id == idProfesional &&
                    p.Activo == 1);

            if (profesional is null)
            {
                return new Resultado(
                    false,
                    "El profesional no existe o está inactivo.");
            }
        }

        return new Resultado(true, string.Empty);
    }

    private static string? ValidarCampos(
        DateOnly fechaAplicacion,
        string? lote,
        string? observacion,
        DateOnly? fechaProximaSugerida)
    {
        if (fechaAplicacion == DateOnly.MinValue)
            return "La fecha de aplicación es obligatoria.";

        if (!string.IsNullOrWhiteSpace(lote) &&
            lote.Trim().Length > MaxLote)
        {
            return $"El lote no puede superar los {MaxLote} caracteres.";
        }

        if (!string.IsNullOrWhiteSpace(observacion) &&
            observacion.Trim().Length > MaxObservacion)
        {
            return $"La observación no puede superar los {MaxObservacion} caracteres.";
        }

        if (fechaProximaSugerida is not null &&
            fechaProximaSugerida <= fechaAplicacion)
        {
            return "La próxima fecha sugerida debe ser posterior a la fecha de aplicación.";
        }

        return null;
    }

    private static string? NormalizarOpcional(string? valor)
        => string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();
}