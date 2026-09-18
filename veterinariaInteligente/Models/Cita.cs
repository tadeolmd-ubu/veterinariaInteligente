namespace veterinariaInteligente.Models;
using veterinariaInteligente.Models.Enums;
public class Cita
{
    public int Id { get; set; }
    public DateTime FechaPrevista { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Notas { get; set; }

    public EstadoCita Estado { get; set; }

    public int? IdMascota { get; set; }
    public Mascota? Mascota { get; set; }

    public int? IdDuenio { get; set; }
    public Duenio? Duenio { get; set; }

    public int? IdProfesional { get; set; }
    public Profesional? Profesional { get; set; }
}