using veterinariaInteligente.Models.Enums;

namespace veterinariaInteligente.Models;

public class Consulta
{
    public int Id { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public decimal? Peso { get; set; }
    public string Observacion { get; set; } = string.Empty;
    public string Tratamiento  { get; set; } = string.Empty;
    public string Diagnostico { get; set; } = string.Empty;
    
    public EstadoConsulta Estado { get; set; }
    
    public int IdMascota { get; set; }
    public Mascota Mascota { get; set; }
    public int IdProfesional { get; set; }
    public Profesional Profesional { get; set; }
    
    public DateTime FechaCreacion  { get; set; }
    public DateTime FechaModificacion { get; set; }
    
    public ICollection<ConsultaVersion> Versiones { get; set; } = []; 
    
    public ICollection<Seguimiento> SeguimientosOriginados { get; set; } = [];
    public ICollection<Seguimiento> SeguimientosRealizados { get; set; } = [];
}