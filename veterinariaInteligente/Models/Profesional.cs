namespace veterinariaInteligente.Models;

public class Profesional
{
    public int Id { get; set; }
    public string Nombre { get; set; } =  string.Empty;
    public string? Cedula { get; set; } = string.Empty;
    public int Activo { get; set; }
    
    public ICollection<Consulta> Consultas { get; set; } = [];
    
}