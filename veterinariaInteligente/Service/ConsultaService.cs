using Microsoft.EntityFrameworkCore;
using veterinariaInteligente.Data;
using veterinariaInteligente.Models;
using veterinariaInteligente.Models.Enums;

namespace veterinariaInteligente.Service;

public class ConsultaService
{
    private const int MaxMotivo = 500;
    private const int MaxObservacion = 500;
    private const int MaxDiagnostico = 500;
    private const int MaxTratamiento = 500;
    
    private readonly AppDbContext _db;
    
    public ConsultaService(AppDbContext db) => _db = db;

    public async Task<List<Consulta>> ObtenerTodosAsync()
    {
        return await _db.Consultas
            .AsNoTracking()
            .Include(c => c.Mascota)
            .Include(c => c.Profesional)
            .Where(c => c.Estado != EstadoConsulta.Archivada)
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync();
    }

    public async Task<List<Consulta>> ObtenerArchivadasAsync()
    {
        return await _db.Consultas
            .AsNoTracking()
            .Include(c => c.Mascota)
            .Include(c => c.Profesional)
            .Where(c => c.Estado == EstadoConsulta.Archivada)
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync();
    }

    public async Task<Consulta?> ObtenerPorIdAsync(int id)
    {
        return await _db.Consultas
            .AsNoTracking()
            .Include(c => c.Mascota)
            .Include(c => c.Profesional)
            .FirstOrDefaultAsync(c => c.Id == id &&  c.Estado != EstadoConsulta.Archivada);
    }

    public async Task<Resultado<Consulta>> AgregarAsync(string motivo, decimal? peso, string observacion, string diagnostico,
        string tratamiento, int idMascota, int idProfesional, EstadoConsulta estado = EstadoConsulta.Pendiente)
    {
        var error = ValidarCampos(motivo, observacion, diagnostico, tratamiento, peso);
        if (error is not null)
            return new Resultado<Consulta>(false, error, null);

        var mascota = await _db.Mascotas
            .FirstOrDefaultAsync(m => m.Id == idMascota && m.FechaArchivado == null);
        if (mascota is null)
            return new Resultado<Consulta>(false, "La mascota no existe o está archivada.", null);

        var profesional = await _db.Profesionales
            .FirstOrDefaultAsync(p => p.Id == idProfesional && p.Activo == 1);
        if (profesional is null)
            return new Resultado<Consulta>(false, "El profesional no existe o está inactivo.", null);

        
        motivo = NormalizarTexto(motivo);
        diagnostico = NormalizarTexto(diagnostico);
        observacion = NormalizarTexto(observacion);
        tratamiento = NormalizarTexto(tratamiento);

        var nuevaConsulta = new Consulta
        {
            Motivo = motivo,
            Observacion = observacion,
            Diagnostico = diagnostico,
            Tratamiento = tratamiento,
            IdMascota = idMascota,
            Peso =  peso,
            IdProfesional = idProfesional,
            Estado = estado,
            FechaCreacion = DateTime.Now,
            FechaModificacion = DateTime.Now,
        };
        
        _db.Consultas.Add(nuevaConsulta);
        await _db.SaveChangesAsync();

        return new Resultado<Consulta>(true, "Consulta registrada.", nuevaConsulta);
    }
    private static string? ValidarCampos (string motivo, string observacion, string diagnostico, string tratamiento, decimal? peso)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            return "El motivo es obligatorio.";
        if (motivo.Trim().Length > MaxMotivo)
            return $"El motivo no puede superar los {MaxMotivo} caracteres.";

        if (string.IsNullOrWhiteSpace(observacion))
            return "La observacion es obligatorio.";
        if (observacion.Trim().Length > MaxObservacion)
            return $"La observacion no puede superar los {MaxObservacion} caracteres.";
        
        if (string.IsNullOrWhiteSpace(diagnostico))
            return "El diagnostico de la mascota es obligatorio.";
        if (diagnostico.Trim().Length > MaxDiagnostico)
            return $"El diagnositoc no puede superar los {MaxDiagnostico} caracteres.";
        
        if (string.IsNullOrWhiteSpace(tratamiento))
            return "El tratamiento de la mascota es obligatorio.";
        if (tratamiento.Trim().Length > MaxTratamiento)
            return $"El tratamiento no puede superar los {MaxTratamiento} caracteres.";

        if (peso is <= 0)
            return "El peso debe ser un número positivo.";
        if (peso > 99.999m)
            return "El peso no puede superar los 99.999 kg.";
        
        return null;
    }
    private static string NormalizarTexto(string valor) => valor.Trim();
}