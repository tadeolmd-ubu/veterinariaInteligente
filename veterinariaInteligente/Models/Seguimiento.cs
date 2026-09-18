using veterinariaInteligente.Models.Enums;
namespace veterinariaInteligente.Models;

public class Seguimiento
{
    public int Id { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateOnly FechaPrevista { get; set; }
    public DateOnly? FechaRealizada { get; set; }
    public string? Notas { get; set; }

    public EstadoSeguimiento Estado { get; set; }

    public int IdMascota { get; set; }
    public Mascota Mascota { get; set; } = null!;

    public int? IdConsultaOrigen { get; set; }
    public Consulta? ConsultaOrigen { get; set; }

    public int? IdAplicacionVacunaOrigen { get; set; }
    public AplicacionVacuna? AplicacionVacunaOrigen { get; set; }

    public int? IdConsultaRealizacion { get; set; }
    public Consulta? ConsultaRealizacion { get; set; }

}