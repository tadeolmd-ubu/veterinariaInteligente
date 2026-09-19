using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using veterinariaInteligente.Data;
using veterinariaInteligente.Models;

namespace veterinariaInteligente.Service;

public class DuenioService
{
    private const int MaxNombre = 100;
    private const int MaxTelefono = 15;
    private const int MaxCorreo = 100;
    private const int MaxDireccion = 255;

    private static readonly Regex RegexTelefono =
        new(@"^[0-9+()\-\s]+$", RegexOptions.Compiled);

    private static readonly Regex RegexCorreo =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private readonly AppDbContext _db;

    public DuenioService(AppDbContext db) => _db = db;

    public async Task<List<Duenio>> ObtenerTodosAsync()
    {
        return await _db.Duenios
            .AsNoTracking()
            .Where(d => d.FechaArchivado == null)
            .OrderBy(d => d.Nombre)
            .ToListAsync();
    }

    public async Task<List<Duenio>> ObtenerArchivadosAsync()
    {
        return await _db.Duenios
            .AsNoTracking()
            .Where(d => d.FechaArchivado != null)
            .OrderBy(d => d.Nombre)
            .ToListAsync();
    }

    public async Task<Duenio?> ObtenerPorIdAsync(int id)
    {
        return await _db.Duenios
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Resultado<Duenio>> AgregarAsync(
        string nombre, string telefono, string? correo, string? direccion)
    {
        var error = ValidarCampos(nombre, telefono, correo, direccion);
        if (error is not null)
            return new Resultado<Duenio>(false, error, null);

        nombre = NormalizarTexto(nombre);
        telefono = NormalizarTexto(telefono);
        correo = NormalizarCorreo(correo);
        direccion = NormalizarOpcional(direccion);

        if (await ExisteTelefonoAsync(telefono))
            return new Resultado<Duenio>(false, "Ya existe un dueño con ese teléfono.", null);

        var duenio = new Duenio
        {
            Nombre = nombre.Trim(),
            Telefono = telefono.Trim(),
            Correo = correo?.Trim(),
            Direccion = direccion?.Trim()
        };

        _db.Duenios.Add(duenio);
        await _db.SaveChangesAsync();

        return new Resultado<Duenio>(true, "Dueño creado.", duenio);
    }

    public async Task<Resultado> ActualizarAsync(
        int id, string nombre, string telefono, string? correo, string? direccion)
    {
        var duenio = await BuscarActivoAsync(id);

        if (duenio is null)
            return new Resultado(false, "El dueño no existe.");

        var error = ValidarCampos(nombre, telefono, correo, direccion);
        if (error is not null)
            return new Resultado(false, error);

        nombre = NormalizarTexto(nombre);
        telefono = NormalizarTexto(telefono);
        correo = NormalizarCorreo(correo);
        direccion = NormalizarOpcional(direccion);

        if (await ExisteTelefonoAsync(telefono, id))
            return new Resultado(false, "Ya existe un dueño con ese teléfono.");

        duenio.Nombre = nombre;
        duenio.Telefono = telefono;
        duenio.Correo = correo;
        duenio.Direccion = direccion;

        await _db.SaveChangesAsync();

        return new Resultado(true, "Dueño actualizado.");
    }

    public async Task<Resultado> ArchivarAsync(int id)
    {
        var duenio = await BuscarActivoAsync(id);

        if (duenio is null)
            return new Resultado(false, "El dueño no existe.");

        duenio.FechaArchivado = DateOnly.FromDateTime(DateTime.Today);
        await _db.SaveChangesAsync();

        return new Resultado(true, "Dueño archivado.");
    }

    public async Task<Resultado> RestaurarAsync(int id)
    {
        var duenio = await _db.Duenios
            .FirstOrDefaultAsync(d => d.Id == id && d.FechaArchivado != null);

        if (duenio is null)
            return new Resultado(false, "El dueño no está archivado.");

        duenio.FechaArchivado = null;
        await _db.SaveChangesAsync();

        return new Resultado(true, "Dueño restaurado.");
    }

    private async Task<Duenio?> BuscarActivoAsync(int id)
    {
        return await _db.Duenios
            .FirstOrDefaultAsync(d => d.Id == id && d.FechaArchivado == null);
    }

    private async Task<bool> ExisteTelefonoAsync(string telefono, int? idExcluir = null)
    {
        var query = _db.Duenios
            .Where(d => d.Telefono == telefono && d.FechaArchivado == null);

        if (idExcluir is not null)
            query = query.Where(d => d.Id != idExcluir);

        return await query.AnyAsync();
    }

    private static string? ValidarCampos(
        string nombre, string telefono, string? correo, string? direccion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return "El nombre es obligatorio.";

        if (nombre.Trim().Length > MaxNombre)
            return $"El nombre no puede superar los {MaxNombre} caracteres.";

        if (string.IsNullOrWhiteSpace(telefono))
            return "El teléfono es obligatorio.";

        var telefonoNormalizado = telefono.Trim();
        if (telefonoNormalizado.Length > MaxTelefono)
            return $"El teléfono no puede superar los {MaxTelefono} caracteres.";

        if (!RegexTelefono.IsMatch(telefonoNormalizado))
            return "El teléfono solo puede contener dígitos, espacios, +, -, ( y ).";

        if (telefonoNormalizado.Count(char.IsDigit) < 7)
            return "El teléfono debe tener al menos 7 dígitos.";

        if (!string.IsNullOrWhiteSpace(correo))
        {
            var correoNormalizado = correo.Trim().ToLowerInvariant();
            if (correoNormalizado.Length > MaxCorreo)
                return $"El correo no puede superar los {MaxCorreo} caracteres.";

            if (!RegexCorreo.IsMatch(correoNormalizado))
                return "El correo no tiene un formato válido.";
        }

        if (!string.IsNullOrWhiteSpace(direccion)
            && direccion.Trim().Length > MaxDireccion)
        {
            return $"La dirección no puede superar los {MaxDireccion} caracteres.";
        }

        return null;
    }

    private static string NormalizarTexto(string valor) => valor.Trim();

    private static string? NormalizarOpcional(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static string? NormalizarCorreo(string? correo)
        => string.IsNullOrWhiteSpace(correo)
            ? null
            : correo.Trim().ToLowerInvariant();
}
