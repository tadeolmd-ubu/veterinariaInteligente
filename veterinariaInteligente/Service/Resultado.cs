namespace veterinariaInteligente.Service;

public record Resultado(bool Correcta, string Mensaje);
public record Resultado<T>(bool Correcta, string Mensaje, T? Valor);
