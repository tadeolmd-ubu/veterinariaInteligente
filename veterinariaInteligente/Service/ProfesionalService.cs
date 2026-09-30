using Microsoft.EntityFrameworkCore;
using veterinariaInteligente.Data;
using veterinariaInteligente.Models;

namespace veterinariaInteligente.Service;

public class ProfesionalService
{
    private const int MaxNombre = 255;
    private const int MaxCedula = 255;
    private const int ProfesionalActivo = 1;
    private const int ProfesionalInactivo = 0;

    private readonly AppDbContext _db;

    public ProfesionalService(AppDbContext db) => _db = db;

    public async Task<List<Profesional>> ObtenerTodosAsync()
    {
        return await _db.Profesionales
            .AsNoTracking()
            .Where(p => p.Activo == ProfesionalActivo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<List<Profesional>> ObtenerInactivosAsync()
    {
        return await _db.Profesionales
            .AsNoTracking()
            .Where(p => p.Activo == ProfesionalInactivo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<Profesional?> ObtenerPorIdAsync(int id)
    {
        return await _db.Profesionales
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Resultado<Profesional>> AgregarAsync(
        string nombre,
        string? cedula)
    {
        var error = ValidarCampos(nombre, cedula);

        if (error is not null)
            return new Resultado<Profesional>(false, error, null);

        nombre = NormalizarTexto(nombre);
        cedula = NormalizarOpcional(cedula);

        if (cedula is not null &&
            await ExisteCedulaActivaAsync(cedula))
        {
            return new Resultado<Profesional>(
                false,
                "Ya existe un profesional activo con esa cédula.",
                null);
        }

        var profesional = new Profesional
        {
            Nombre = nombre,
            Cedula = cedula,
            Activo = ProfesionalActivo
        };

        _db.Profesionales.Add(profesional);
        await _db.SaveChangesAsync();

        return new Resultado<Profesional>(
            true,
            "Profesional creado.",
            profesional);
    }

    public async Task<Resultado> ActualizarAsync(
        int id,
        string nombre,
        string? cedula)
    {
        var profesional = await _db.Profesionales
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.Activo == ProfesionalActivo);

        if (profesional is null)
        {
            return new Resultado(
                false,
                "El profesional no existe o está inactivo.");
        }

        var error = ValidarCampos(nombre, cedula);

        if (error is not null)
            return new Resultado(false, error);

        nombre = NormalizarTexto(nombre);
        cedula = NormalizarOpcional(cedula);

        if (cedula is not null &&
            await ExisteCedulaActivaAsync(cedula, id))
        {
            return new Resultado(
                false,
                "Ya existe otro profesional activo con esa cédula.");
        }

        profesional.Nombre = nombre;
        profesional.Cedula = cedula;

        await _db.SaveChangesAsync();

        return new Resultado(true, "Profesional actualizado.");
    }

    public async Task<Resultado> DesactivarAsync(int id)
    {
        var profesional = await _db.Profesionales
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.Activo == ProfesionalActivo);

        if (profesional is null)
        {
            return new Resultado(
                false,
                "El profesional no existe o ya está inactivo.");
        }

        profesional.Activo = ProfesionalInactivo;

        await _db.SaveChangesAsync();

        return new Resultado(true, "Profesional desactivado.");
    }

    public async Task<Resultado> ActivarAsync(int id)
    {
        var profesional = await _db.Profesionales
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.Activo == ProfesionalInactivo);

        if (profesional is null)
        {
            return new Resultado(
                false,
                "El profesional no existe o ya está activo.");
        }

        if (!string.IsNullOrWhiteSpace(profesional.Cedula) &&
            await ExisteCedulaActivaAsync(
                profesional.Cedula,
                id))
        {
            return new Resultado(
                false,
                "Ya existe otro profesional activo con esa cédula.");
        }

        profesional.Activo = ProfesionalActivo;

        await _db.SaveChangesAsync();

        return new Resultado(true, "Profesional activado.");
    }

    private async Task<bool> ExisteCedulaActivaAsync(
        string cedula,
        int? idExcluir = null)
    {
        var cedulaNormalizada = cedula.Trim().ToLower();

        var query = _db.Profesionales
            .Where(p =>
                p.Activo == ProfesionalActivo &&
                p.Cedula != null &&
                p.Cedula.ToLower() == cedulaNormalizada);

        if (idExcluir is not null)
            query = query.Where(p => p.Id != idExcluir);

        return await query.AnyAsync();
    }

    private static string? ValidarCampos(
        string nombre,
        string? cedula)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return "El nombre del profesional es obligatorio.";

        if (nombre.Trim().Length > MaxNombre)
            return $"El nombre no puede superar los {MaxNombre} caracteres.";

        if (!string.IsNullOrWhiteSpace(cedula) &&
            cedula.Trim().Length > MaxCedula)
        {
            return $"La cédula no puede superar los {MaxCedula} caracteres.";
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