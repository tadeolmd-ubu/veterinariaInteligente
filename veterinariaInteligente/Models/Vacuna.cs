namespace veterinariaInteligente.Models;

public class Vacuna
{ 
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Activo { get; set; }

    public ICollection<AplicacionVacuna> Aplicaciones { get; set; } = [];
}