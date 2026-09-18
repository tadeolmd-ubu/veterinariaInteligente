using System.Runtime.InteropServices.JavaScript;

namespace veterinariaInteligente.Models;

public class Mascota
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Raza { get; set; } = string.Empty;
    public string Sexo  { get; set; } = string.Empty;
    public DateOnly? FechaNacimiento  { get; set; }
    public string Color  { get; set; } = string.Empty;
    public string Alergia  { get; set; } = string.Empty;
    public DateOnly? FechaArchivado  { get; set; }
    
    public int IdDuenio { get; set; }
    public Duenio Duenio { get; set; }

    public int IdEstado { get; set; }
    public MascotaEstado Estado { get; set; }
    
    public ICollection<Consulta> Consultas { get; set; } = [];
    public ICollection<AplicacionVacuna> AplicacionesVacunas { get; set; } = [];
    public ICollection<Seguimiento> Seguimientos { get; set; } = [];
    public ICollection<Cita> Citas { get; set; } = [];
    
}