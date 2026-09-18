namespace veterinariaInteligente.Models;

public class ConfiguracionCliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;

    public int? IdProfesionalDefault { get; set; }
    public Profesional? ProfesionalDefault { get; set; }

}