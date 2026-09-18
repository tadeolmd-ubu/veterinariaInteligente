using System;

namespace veterinariaInteligente.Models;

public class Duenio
{
    public int Id  { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Correo { get; set; } = string.Empty;
    public string? Direccion { get; set; } = string.Empty;
    public DateOnly? FechaArchivado { get; set; }
    
    public ICollection<Mascota> Mascotas { get; set; } = [];
}