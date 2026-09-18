namespace veterinariaInteligente.Models;

public class ConsultaVersion
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Campo { get; set; } = string.Empty;
    public string? ValorAnterior { get; set; } = string.Empty;
    public string? ValorNuevo { get; set; } = string.Empty;
    public int IdConsulta { get; set; }
    public Consulta Consulta { get; set; } = null!;
    public int IdProfesional { get; set; }
    public Profesional Profesional { get; set; } = null!;

}
