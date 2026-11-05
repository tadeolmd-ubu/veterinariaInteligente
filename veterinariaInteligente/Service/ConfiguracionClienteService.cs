using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using veterinariaInteligente.Data;
using veterinariaInteligente.Models;

namespace veterinariaInteligente.Service;

public class ConfiguracionClienteService
{
    private const int MaxNombre = 255;
    private const int MaxTelefono = 15;
    private const int MaxDireccion = 255;
    private const int ProfesionalActivo = 1;

    private static readonly Regex RegexTelefono =
        new(@"^[0-9+()\-\s]+$", RegexOptions.Compiled);

    private readonly AppDbContext _db;

    public ConfiguracionClienteService(AppDbContext db) => _db = db;

    // Registro único; null si todavía no se configuró.
    public async Task<ConfiguracionCliente?> ObtenerAsync()
    {
        return await _db.ConfiguracionesCliente
            .AsNoTracking()
            .Include(c => c.ProfesionalDefault)
            .OrderBy(c => c.Id)
            .FirstOrDefaultAsync();
    }

    // Crea el registro único la primera vez y lo actualiza después.
    public async Task<Resultado<ConfiguracionCliente>> GuardarAsync(
        string nombre,
        string telefono,
        string direccion,
        int? idProfesionalDefault)
    {
        var error = ValidarCampos(nombre, telefono, direccion);
        if (error is not null)
            return new Resultado<ConfiguracionCliente>(false, error, null);

        if (idProfesionalDefault is not null)
        {
            var profesional = await _db.Profesionales
                .FirstOrDefaultAsync(p =>
                    p.Id == idProfesionalDefault &&
                    p.Activo == ProfesionalActivo);

            if (profesional is null)
                return new Resultado<ConfiguracionCliente>(
                    false,
                    "El profesional no existe o está inactivo.",
                    null);
        }

        nombre = nombre.Trim();
        telefono = telefono.Trim();
        direccion = direccion.Trim();

        var configuracion = await _db.ConfiguracionesCliente
            .OrderBy(c => c.Id)
            .FirstOrDefaultAsync();

        var esNueva = configuracion is null;
        if (esNueva)
        {
            configuracion = new ConfiguracionCliente();
            _db.ConfiguracionesCliente.Add(configuracion);
        }

        configuracion!.Nombre = nombre;
        configuracion.Telefono = telefono;
        configuracion.Direccion = direccion;
        configuracion.IdProfesionalDefault = idProfesionalDefault;

        await _db.SaveChangesAsync();

        return new Resultado<ConfiguracionCliente>(
            true,
            esNueva ? "Configuración guardada." : "Configuración actualizada.",
            configuracion);
    }

    private static string? ValidarCampos(
        string nombre,
        string telefono,
        string direccion)
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

        if (string.IsNullOrWhiteSpace(direccion))
            return "La dirección es obligatoria.";

        if (direccion.Trim().Length > MaxDireccion)
            return $"La dirección no puede superar los {MaxDireccion} caracteres.";

        return null;
    }
}
