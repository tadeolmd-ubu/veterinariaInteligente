namespace veterinariaInteligente.Models;

public class AplicacionVacuna
{
    public int Id { get; set; }
    public DateOnly FechaAplicacion { get; set; }
    public string? Lote { get; set; }
    public string? Observacion { get; set; }
    public DateOnly? FechaProximaSugerida { get; set; }

    public int IdVacuna { get; set; }
    public Vacuna Vacuna { get; set; } = null!;

    public int IdMascota { get; set; }
    public Mascota Mascota { get; set; } = null!;

    public int? IdProfesional { get; set; }
    public Profesional? Profesional { get; set; }

}