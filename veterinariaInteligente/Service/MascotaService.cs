using Microsoft.EntityFrameworkCore;
using veterinariaInteligente.Data;
using veterinariaInteligente.Models;

namespace veterinariaInteligente.Service;

public class MascotaService
{
    private const int MaxNombre = 100;
    private const int MaxRaza = 100;
    private const int MaxSexo = 15;
    private const int MaxColor = 50;
    private const int MaxAlergia = 255;
    
    private readonly AppDbContext _db;

    public MascotaService(AppDbContext db) => _db = db;

    public async Task<List<Mascota>> ObtenerTodosAsync()
    {
        return await _db.Mascotas
            .AsNoTracking()
            .Where(d => d.FechaArchivado == null)
            .OrderBy(d => d.Nombre)
            .ToListAsync();
    }

    public async Task<List<Mascota>> ObtenerArchivadosAsync()
    {
        return await _db.Mascotas
        .AsNoTracking()
        .Where(d => d.FechaArchivado != null)
        .OrderBy(d => d.Nombre)
        .ToListAsync();
    }

    // Mascotas archivadas cuyo dueño sigue activo. Las de un dueño archivado
    // quedan ocultas hasta restaurar al dueño.
    public async Task<List<Mascota>> ObtenerArchivadasDeDueniosActivosAsync()
    {
        return await _db.Mascotas
            .AsNoTracking()
            .Include(m => m.Duenio)
            .Where(m => m.FechaArchivado != null && m.Duenio.FechaArchivado == null)
            .OrderBy(m => m.Nombre)
            .ToListAsync();
    }

    public async Task<Mascota?> ObtenerPorIdAsync(int id)
    {
        return await _db.Mascotas
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id  && d.FechaArchivado == null);
    }
    
    public async Task<Resultado<Mascota>> AgregarAsync(string nombre, string raza, string sexo, DateOnly? fechaNacimiento, string color, string? alergia, int idEstadoMascota, int idDuenio)
    {
        var duenio = await _db.Duenios
            .FirstOrDefaultAsync(d =>
                d.Id == idDuenio &&
                d.FechaArchivado == null);

        var estado = await _db.MascotaEstados.FirstOrDefaultAsync(d => d.Id == idEstadoMascota);
 
        if (duenio is null)
        {
            return new Resultado<Mascota>(
                false,
                "El dueño no existe o está archivado.",
                null);
        }

        if (estado is null)
        {
            return new Resultado<Mascota>(
                false,
                "El estado de la mascota no existe.",
                null);
        }
        
        var error = ValidarCampos(nombre,  raza,  sexo,  color,  alergia);
        if (error is not null)
            return new Resultado<Mascota>(false, error, null);

        nombre = NormalizarTexto(nombre);
        raza = NormalizarTexto(raza);
        color = NormalizarTexto(color);
        alergia = string.IsNullOrWhiteSpace(alergia)
            ? null
            : alergia.Trim();
        
        
        var mascota = new Mascota
        {
            Nombre = nombre.Trim(),
            Raza = raza.Trim(),
            Sexo = sexo.Trim(),
            FechaNacimiento = fechaNacimiento,
            Color = color.Trim(),
            Alergia = alergia?.Trim(),
            IdDuenio = idDuenio,
            IdEstado = idEstadoMascota
        };

        _db.Mascotas.Add(mascota);
        await _db.SaveChangesAsync();

        return new Resultado<Mascota>(true, "Mascota creada.", mascota);
    }
    
    public async Task<Resultado> ActualizarAsync(int id, string nombre, string raza, string sexo, string color, string? alergia, DateOnly? fechaNacimiento)
    {
        var mascota = await BuscarActivoAsync(id);

        if (mascota is null)
            return new Resultado(false, "La mascota no existe.");

        var error = ValidarCampos(nombre,  raza,  sexo,  color,  alergia);
        if (error is not null)
            return new Resultado(false, error);
        
        nombre = NormalizarTexto(nombre);
        raza = NormalizarTexto(raza);
        color = NormalizarTexto(color);
        alergia = string.IsNullOrWhiteSpace(alergia)
            ? null
            : alergia.Trim();
        
        mascota.Sexo = sexo.Trim();
        mascota.Nombre = nombre;
        mascota.Raza = raza;
        mascota.Color = color;
        mascota.Alergia = alergia;
        mascota.FechaNacimiento = fechaNacimiento;

        await _db.SaveChangesAsync();

        return new Resultado(true, "Mascota actualizada.");
    }
    public async Task<Resultado> ArchivarAsync(int id)
    {
        var mascota = await BuscarActivoAsync(id);

        if (mascota is null)
            return new Resultado(false, "La mascota no existe.");

        mascota.FechaArchivado = DateOnly.FromDateTime(DateTime.Today);
        await _db.SaveChangesAsync();

        return new Resultado(true, "Mascota archivada.");
    }
    public async Task<Resultado> RestaurarAsync(int id)
    {
        var mascota = await _db.Mascotas
            .FirstOrDefaultAsync(d => d.Id == id && d.FechaArchivado != null);

        if (mascota is null)
            return new Resultado(false, "La mascota no está archivada.");

        mascota.FechaArchivado = null;
        await _db.SaveChangesAsync();

        return new Resultado(true, "Mascota restaurada.");
    }
    private async Task<Mascota?> BuscarActivoAsync(int id)
    {
        return await _db.Mascotas
            .FirstOrDefaultAsync(d => d.Id == id && d.FechaArchivado == null);
    }
    private static string? ValidarCampos (string nombre, string raza, string sexo, string color, string? alergia)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return "El nombre es obligatorio.";
        if (nombre.Trim().Length > MaxNombre)
            return $"El nombre no puede superar los {MaxNombre} caracteres.";

        if (string.IsNullOrWhiteSpace(raza))
            return "La raza es obligatorio.";
        if (raza.Trim().Length > MaxRaza)
            return $"La Raza no puede superar los {MaxRaza} caracteres.";
        
        if (string.IsNullOrWhiteSpace(sexo))
            return "El sexo de la mascota es obligatorio.";
        if (sexo.Trim().Length > MaxSexo)
            return $"El sexo no puede superar los {MaxSexo} caracteres.";
        
        if (string.IsNullOrWhiteSpace(color))
            return "El color de la mascota es obligatorio.";
        if (color.Trim().Length > MaxColor)
            return $"El color no puede superar los {MaxColor} caracteres.";
        
        if (alergia is not null && alergia.Length > MaxAlergia)
            return $"La alergia no puede superar los {MaxAlergia} caracteres.";
        
        return null;
    }
    private static string NormalizarTexto(string valor) => valor.Trim();
}