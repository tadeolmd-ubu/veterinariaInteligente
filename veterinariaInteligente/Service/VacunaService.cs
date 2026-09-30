using Microsoft.EntityFrameworkCore;
using veterinariaInteligente.Data;
using veterinariaInteligente.Models;

namespace veterinariaInteligente.Service;

public class VacunaService
{
    private const int MaxNombre = 100;
    private const int MaxDescripcion = 255;
    private const int idVacunaActiva = 1;
    private const int idVacunaInactiva = 0;

    private readonly AppDbContext _db;

    public VacunaService(AppDbContext db) => _db = db;

    public async Task<List<Vacuna>> ObtenerTodosAsync()
    {
        return await _db.Vacunas
            .AsNoTracking()
            .Where(v => v.Activo == idVacunaActiva)
            .OrderBy(v => v.Nombre)
            .ToListAsync();
    }

    public async Task<List<Vacuna>> ObtenerInactivasAsync()
    {
        return await _db.Vacunas
            .AsNoTracking()
            .Where(v => v.Activo == idVacunaInactiva)
            .OrderBy(v => v.Nombre)
            .ToListAsync();
    }

    public async Task<Vacuna?> ObtenerPorIdAsync(int id)
    {
        return await _db.Vacunas
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Resultado<Vacuna>> AgregarAsync(
        string nombre,
        string? descripcion)
    {
        var error = ValidarCampos(nombre, descripcion);

        if (error is not null)
            return new Resultado<Vacuna>(false, error, null);

        nombre = NormalizarTexto(nombre);
        descripcion = NormalizarOpcional(descripcion);

        var existe = await ExisteNombreActivoAsync(nombre);

        if (existe)
        {
            return new Resultado<Vacuna>(
                false,
                "Ya existe una vacuna activa con ese nombre.",
                null);
        }

        var vacuna = new Vacuna
        {
            Nombre = nombre,
            Descripcion = descripcion,
            Activo = idVacunaActiva
        };

        _db.Vacunas.Add(vacuna);
        await _db.SaveChangesAsync();

        return new Resultado<Vacuna>(
            true,
            "Vacuna creada.",
            vacuna);
    }

    public async Task<Resultado> ActualizarAsync(
        int id,
        string nombre,
        string? descripcion)
    {
        var vacuna = await _db.Vacunas
            .FirstOrDefaultAsync(v =>
                v.Id == id &&
                v.Activo == idVacunaActiva);

        if (vacuna is null)
        {
            return new Resultado(
                false,
                "La vacuna no existe o está inactiva.");
        }

        var error = ValidarCampos(nombre, descripcion);

        if (error is not null)
            return new Resultado(false, error);

        nombre = NormalizarTexto(nombre);
        descripcion = NormalizarOpcional(descripcion);

        var existe = await ExisteNombreActivoAsync(nombre, id);

        if (existe)
        {
            return new Resultado(
                false,
                "Ya existe otra vacuna activa con ese nombre.");
        }

        vacuna.Nombre = nombre;
        vacuna.Descripcion = descripcion;

        await _db.SaveChangesAsync();

        return new Resultado(true, "Vacuna actualizada.");
    }

    public async Task<Resultado> DesactivarAsync(int id)
    {
        var vacuna = await _db.Vacunas
            .FirstOrDefaultAsync(v =>
                v.Id == id &&
                v.Activo == idVacunaActiva);

        if (vacuna is null)
        {
            return new Resultado(
                false,
                "La vacuna no existe o ya está inactiva.");
        }

        vacuna.Activo = idVacunaInactiva;

        await _db.SaveChangesAsync();

        return new Resultado(true, "Vacuna desactivada.");
    }

    public async Task<Resultado> ActivarAsync(int id)
    {
        var vacuna = await _db.Vacunas
            .FirstOrDefaultAsync(v =>
                v.Id == id &&
                v.Activo == idVacunaInactiva);
        if (vacuna is null)
        {
            return new Resultado(
                false,
                "La vacuna no existe o ya está activa.");
        }

        var existe = await ExisteNombreActivoAsync(vacuna.Nombre, id);

        if (existe)
        {
            return new Resultado(
                false,
                "Ya existe una vacuna activa con ese nombre.");
        }

        vacuna.Activo = idVacunaActiva;

        await _db.SaveChangesAsync();

        return new Resultado(true, "Vacuna activada.");
    }

    private async Task<bool> ExisteNombreActivoAsync(
        string nombre,
        int? idExcluir = null)
    {
        var nombreNormalizado = nombre.Trim().ToLower();

        var query = _db.Vacunas
            .Where(v =>
                v.Activo == idVacunaActiva &&
                v.Nombre.ToLower() == nombreNormalizado);

        if (idExcluir is not null)
            query = query.Where(v => v.Id != idExcluir);

        return await query.AnyAsync();
    }

    private static string? ValidarCampos(
        string nombre,
        string? descripcion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return "El nombre de la vacuna es obligatorio.";

        if (nombre.Trim().Length > MaxNombre)
            return $"El nombre no puede superar los {MaxNombre} caracteres.";

        if (!string.IsNullOrWhiteSpace(descripcion) &&
            descripcion.Trim().Length > MaxDescripcion)
        {
            return $"La descripción no puede superar los {MaxDescripcion} caracteres.";
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